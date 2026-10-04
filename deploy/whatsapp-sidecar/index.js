// WhatsApp Web sidecar for the TmTm store: sends phone verification codes from the
// store's own WhatsApp number. Copied from the MosqueSms sidecar (D:\Work\mosquesms);
// the only addition is POST /check. Runs as its own service, with its own number and
// session folder - see deploy/whatsapp-sidecar/README.md.
//
// Owns one or more WhatsApp Web sessions (scan each QR once, persisted via
// LocalAuth) and exposes a tiny localhost HTTP API the store calls. With more
// than one number linked, the store spreads codes across them.
//
// Endpoints:
//   GET  /health                 -> { ok: true }
//   GET  /accounts               -> [{ id, state, qr?, phone? }]   state: starting|qr|authenticated|ready|disconnected
//   POST /accounts               -> { ok, id }   adds a new number slot and shows its QR
//   POST /accounts/:id/logout    -> logs out that number, clears its session, removes the slot
//   POST /check                  -> body { accountId, phone } => { ok, onWhatsApp } | { ok:false, error }
//   POST /send                   -> body { accountId, phone, message, media? } => { ok, id } | { ok:false, error }
//
// Run:  npm install && npm start   (then scan the QR shown in Admin > Configuration > Settings > Phone verification)

const fs = require('fs');
const path = require('path');
const express = require('express');
const qrcode = require('qrcode');
const { Client, LocalAuth, MessageMedia } = require('whatsapp-web.js');

const PORT = process.env.PORT || 3000;
// Where the linked sessions are stored. Point this at a PERSISTENT folder/volume
// on the server so a redeploy never forces a re-scan of any QR. LocalAuth keeps
// each number under its own `session-<clientId>` subfolder (and the legacy
// single-number layout under a bare `session` folder).
const DATA_PATH = process.env.WWEBJS_DATA || './.wwebjs_auth';

// Pinned WhatsApp Web build. WhatsApp periodically ships a new web version;
// when the version baked into whatsapp-web.js falls behind, page injection
// hangs ("Runtime.callFunctionOn timed out"), the client never emits a QR, and
// the service crash-loops. Loading a known-good build from the remote cache
// decouples us from WhatsApp's live rollout. If it breaks again, bump
// WA_WEB_VERSION (env, no code change) to a newer snapshot from
// https://github.com/wppconnect-team/wa-version/tree/main/html and restart.
const WA_WEB_VERSION = process.env.WA_WEB_VERSION || '2.3000.1040818981-alpha';
const WA_WEB_REMOTE =
  `https://raw.githubusercontent.com/wppconnect-team/wa-version/main/html/${WA_WEB_VERSION}.html`;

// A linked session can become corrupt (e.g. after a WhatsApp web update) and
// then hang page injection on every restore — the client never emits a QR and
// the slot is wedged indefinitely. We clear a session ONLY as a last resort:
// after init has failed *continuously* for this long. A valid login must never
// be thrown away over a transient WhatsApp/network blip (which recovers within
// a retry or two); only a session that stays unusable for many minutes is
// treated as corrupt and reset, turning a permanent "can't log in" hang into a
// one-time QR re-scan. Tune with WA_WIPE_AFTER_MS.
const WIPE_AFTER_MS = Number(process.env.WA_WIPE_AFTER_MS || 20 * 60 * 1000);

// The legacy single-number layout stored its session in a bare `session` folder
// (LocalAuth with no clientId). We model that as an account with this special
// id and pass NO clientId to LocalAuth so the existing login is restored as-is
// across this upgrade (no re-scan for already-deployed customers).
const LEGACY_ID = 'default';

// All linked numbers, keyed by id. Each entry:
//   { id, client, state, qr, phone, firstFailureAt, destroyed }
// state: starting | qr | authenticated | ready | disconnected
const accounts = new Map();

// LocalAuth requires a clientId matching /^[-_\w]+$/. The legacy account uses
// none (bare `session` folder); every other account stores under `session-<id>`.
function localAuthFor(id) {
  return id === LEGACY_ID
    ? new LocalAuth({ dataPath: DATA_PATH })
    : new LocalAuth({ dataPath: DATA_PATH, clientId: id });
}

// Absolute path of the on-disk session folder backing an account.
function sessionDirFor(id) {
  return id === LEGACY_ID
    ? path.join(DATA_PATH, 'session')
    : path.join(DATA_PATH, `session-${id}`);
}

function createClient(acc) {
  const c = new Client({
    authStrategy: localAuthFor(acc.id),
    // Pin the web build (see WA_WEB_VERSION above) so a WhatsApp rollout can't
    // silently break injection.
    webVersionCache: { type: 'remote', remotePath: WA_WEB_REMOTE },
    puppeteer: {
      headless: true,
      args: ['--no-sandbox', '--disable-setuid-sandbox'],
      // Give the (large) web-page injection room to finish on a busy/cold VPS
      // instead of timing out at puppeteer's 30s default.
      protocolTimeout: 120000,
    },
  });

  c.on('qr', async (qr) => {
    acc.state = 'qr';
    try {
      acc.qr = await qrcode.toDataURL(qr);
    } catch (err) {
      console.error(`[${acc.id}] Failed to render QR:`, err);
      acc.qr = null;
    }
    console.log(`[${acc.id}] QR ready — scan it in Admin > Configuration > Settings > Phone verification.`);
  });

  c.on('authenticated', () => {
    acc.state = 'authenticated';
    acc.qr = null;
    console.log(`[${acc.id}] Authenticated.`);
  });

  c.on('ready', () => {
    acc.state = 'ready';
    acc.qr = null;
    // The linked phone number (digits only), shown in the UI and the report.
    try { acc.phone = c.info && c.info.wid ? c.info.wid.user : null; }
    catch (_) { acc.phone = null; }
    console.log(`[${acc.id}] WhatsApp client is ready (${acc.phone || 'unknown number'}).`);
  });

  c.on('disconnected', (reason) => {
    acc.state = 'disconnected';
    acc.qr = null;
    console.log(`[${acc.id}] Disconnected:`, reason);
  });

  c.on('auth_failure', (msg) => {
    acc.state = 'disconnected';
    console.error(`[${acc.id}] Auth failure:`, msg);
  });

  return c;
}

function wipeSession(id) {
  try {
    fs.rmSync(sessionDirFor(id), { recursive: true, force: true });
    console.warn(`[${id}] Cleared session; a fresh QR will be shown to re-link.`);
  } catch (e) {
    console.error(`[${id}] Failed to clear session dir:`, e && e.message ? e.message : e);
  }
}

// Boots (or reboots) the client for one account, with the same transient-error
// backoff + last-resort corrupt-session wipe as the original single-number
// sidecar, now scoped per account.
function startClient(acc) {
  acc.state = 'starting';
  acc.qr = null;
  acc.client = createClient(acc);
  // Catch a failed/hung initialize so a transient error backs off and retries
  // in-process rather than crashing node. (A successful restore resolves;
  // reaching the QR screen keeps it pending — neither counts as a failure, so
  // waiting for a scan never wipes anything.)
  acc.client.initialize()
    .then(() => { acc.firstFailureAt = null; })
    .catch((err) => {
      if (acc.destroyed) return; // slot was logged out while initializing
      acc.state = 'disconnected';
      if (acc.firstFailureAt == null) acc.firstFailureAt = Date.now();
      const downSec = Math.round((Date.now() - acc.firstFailureAt) / 1000);
      console.error(`[${acc.id}] initialize() failed (down ${downSec}s), retrying in 15s:`,
        err && err.message ? err.message : err);
      try { acc.client.destroy().catch(() => {}); } catch (_) { /* ignore */ }
      if (Date.now() - acc.firstFailureAt >= WIPE_AFTER_MS) {
        // Unusable for too long — treat the session as corrupt and reset it.
        wipeSession(acc.id);
        acc.firstFailureAt = null;
      }
      if (!acc.destroyed) setTimeout(() => startClient(acc), 15000);
    });
}

// Registers an account slot in the map and boots its client.
function addAccount(id) {
  const acc = { id, client: null, state: 'starting', qr: null, phone: null, firstFailureAt: null, destroyed: false };
  accounts.set(id, acc);
  startClient(acc);
  return acc;
}

// On startup, restore every previously-linked number by scanning DATA_PATH for
// its session folders. `session` (no suffix) is the legacy single-number login;
// `session-<id>` are the multi-number slots. Nothing found => start with zero
// accounts (the UI prompts the user to add one).
function restoreAccounts() {
  let entries = [];
  try {
    entries = fs.readdirSync(DATA_PATH, { withFileTypes: true });
  } catch (_) {
    // DATA_PATH does not exist yet — first run, no linked numbers.
    return;
  }
  for (const e of entries) {
    if (!e.isDirectory()) continue;
    if (e.name === 'session') {
      addAccount(LEGACY_ID); // legacy bare-session login
    } else if (e.name.startsWith('session-')) {
      addAccount(e.name.slice('session-'.length));
    }
  }
  if (accounts.size > 0) {
    console.log(`Restoring ${accounts.size} linked number(s): ${[...accounts.keys()].join(', ')}`);
  } else {
    console.log('No linked numbers yet — add one from the Connection panel.');
  }
}

// Short unique id for a freshly added account (used as the LocalAuth clientId
// and the session-<id> folder name).
function newAccountId() {
  return 'n' + Date.now().toString(36) + Math.floor(Math.random() * 1e4).toString(36);
}

restoreAccounts();

const app = express();
// Allow an image (sent as base64) to fit in the JSON body; the default ~100 KB
// limit is far too small. The UI caps uploads at 5 MB, base64 inflates ~33%.
app.use(express.json({ limit: '20mb' }));

app.get('/health', (_req, res) => res.json({ ok: true }));

// All linked numbers and their current state (QR included only while pending).
app.get('/accounts', (_req, res) => {
  const list = [...accounts.values()].map((a) => ({
    id: a.id,
    state: a.state,
    qr: a.state === 'qr' ? a.qr : null,
    phone: a.phone || null,
  }));
  res.json(list);
});

// Add a new number slot and start its client; its QR then appears via /accounts.
app.post('/accounts', (_req, res) => {
  const id = newAccountId();
  addAccount(id);
  res.json({ ok: true, id });
});

// Log out one number, clear its saved session, and remove the slot.
app.post('/accounts/:id/logout', async (req, res) => {
  const acc = accounts.get(req.params.id);
  if (!acc) return res.status(404).json({ ok: false, error: 'unknown_account' });
  acc.destroyed = true; // stop the retry loop from rebooting this slot
  try {
    try { await acc.client.logout(); } catch (e) { /* may throw if not fully ready */ }
    try { await acc.client.destroy(); } catch (e) { /* ignore */ }
    wipeSession(acc.id);
    accounts.delete(acc.id);
    return res.json({ ok: true });
  } catch (err) {
    console.error(`[${acc.id}] Logout failed:`, err);
    return res.status(500).json({ ok: false, error: String(err && err.message ? err.message : err) });
  }
});

// Whether a number has WhatsApp, so the store can refuse it at registration
// instead of creating an account that can never receive its code.
app.post('/check', async (req, res) => {
  const { accountId, phone } = req.body || {};

  const acc = accountId ? accounts.get(accountId) : null;
  if (!acc) {
    return res.status(404).json({ ok: false, error: 'unknown_account' });
  }
  if (acc.state !== 'ready') {
    return res.status(409).json({ ok: false, error: 'not_ready' });
  }
  if (!phone) {
    return res.status(400).json({ ok: false, error: 'missing_phone' });
  }

  try {
    const numberId = await acc.client.getNumberId(phone);
    return res.json({ ok: true, onWhatsApp: !!numberId });
  } catch (err) {
    console.error(`[${acc.id}] Check failed:`, err);
    return res.status(500).json({ ok: false, error: String(err && err.message ? err.message : err) });
  }
});

app.post('/send', async (req, res) => {
  // `media` is optional: { data (base64), mimetype, filename }. When present the
  // image is sent and `message` (if any) becomes its caption. `accountId`
  // selects which linked number sends this message.
  const { accountId, phone, message, media } = req.body || {};

  const acc = accountId ? accounts.get(accountId) : null;
  if (!acc) {
    return res.status(404).json({ ok: false, error: 'unknown_account' });
  }
  if (acc.state !== 'ready') {
    return res.status(409).json({ ok: false, error: 'not_ready' });
  }
  if (!phone || (!message && !media)) {
    return res.status(400).json({ ok: false, error: 'missing_phone_or_message' });
  }

  try {
    // phone is expected already normalized to digits only, e.g. 9639XXXXXXXX
    const numberId = await acc.client.getNumberId(phone);
    if (!numberId) {
      return res.json({ ok: false, error: 'not_on_whatsapp' });
    }
    let sent;
    if (media && media.data) {
      const m = new MessageMedia(media.mimetype, media.data, media.filename);
      sent = await acc.client.sendMessage(numberId._serialized, m, { caption: message || undefined });
    } else {
      sent = await acc.client.sendMessage(numberId._serialized, message);
    }
    // whatsapp-web.js sendMessage returns undefined when the message was
    // dispatched but its model couldn't be fetched back (Msg.get miss right
    // after send). Treat that as sent-with-unknown-id, not a crash — the old
    // `sent.id` deref threw "Cannot read properties of undefined (reading 'id')"
    // and, worse, marked already-sent rows as failed (inviting a duplicate resend).
    return res.json({ ok: true, id: sent && sent.id ? sent.id._serialized : null });
  } catch (err) {
    console.error(`[${acc.id}] Send failed:`, err);
    return res.status(500).json({ ok: false, error: String(err && err.message ? err.message : err) });
  }
});

app.listen(PORT, '127.0.0.1', () => {
  console.log(`WhatsApp sidecar listening on http://127.0.0.1:${PORT}`);
});
