/* =============================================================================
   tmtm — service worker

   Caching policy is deliberately conservative, because this is a storefront:

   - Only same-origin GET requests are ever touched. Every cart, checkout,
     login and account mutation is a POST and passes straight through.
   - HTML is NEVER cached. Store pages carry per-customer state (cart totals,
     prices, antiforgery tokens, "Welcome, <name>"). Serving a cached copy
     would show one visitor another visitor's state or a stale price.
     Navigations are network-first and fall back to a static offline page.
   - Only versioned, public, static assets are cached: the CSS/JS bundles
     (content-hashed by WebOptimizer), theme assets, icons and images.
   - Sensitive paths are excluded outright as defence in depth.

   Bump CACHE_VERSION to invalidate everything on the next activation.
   ============================================================================= */

const CACHE_VERSION = 'v5';
const STATIC_CACHE = `tmtm-static-${CACHE_VERSION}`;
const OFFLINE_URL = '/offline.html';

/* Cached up-front so the offline page works on the very first disconnection. */
const PRECACHE = [
  OFFLINE_URL,
  '/icons/icons_0/android-chrome-192x192.png',
  '/icons/icons_0/favicon-32x32.png'
];

/* Never touched by the cache, even for GET. */
const EXCLUDED = [
  '/admin',
  '/cart',
  '/checkout',
  '/onepagecheckout',
  '/customer',
  '/order',

  '/compareproducts',
  '/login',
  '/logout',
  '/register',
  '/passwordrecovery',
  '/download',
  '/eucookielawaccept',
  '/setlanguage',
  '/setcurrency',
  '/settaxtype',
  '/addproducttocart',
  '/checkoutattributechange',
  '/productattributechange',
  '/subscribenewsletter',
  '/sw.js'
];

/* Prefixes that are safe to cache: public, versioned, non-personalised. */
const CACHEABLE_PREFIXES = ['/css/', '/js/', '/lib/', '/lib_npm/', '/icons/', '/themes/', '/images/'];

/* Offline, a thumbnail that was never cached draws as a broken-image icon and
   makes the whole page look failed. Serve the store's own mark on a brand tint
   instead: the layout keeps its shape and still reads as this store. */
const OFFLINE_IMAGE = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 250 250" role="img" aria-label="tmtm">
  <rect width="250" height="250" rx="24" fill="#e6f4f4"/>
  <g fill="#02787e" opacity=".22">
    <path fill-rule="evenodd" d="M247.57 189.36C246.52 182.21 235.86 176.04 224.0 176.04L96.67 176.09C84.31 176.09 74.62 182.32 73.87 189.35L247.57 189.36Z"/>
    <path d="M127.77 209.3C127.77 218.01 120.71 225.07 112.0 225.07C103.29 225.07 96.22 218.01 96.22 209.3C96.22 200.59 103.29 193.52 112.0 193.52C120.71 193.52 127.77 200.59 127.77 209.3Z"/>
    <path d="M223.14 209.28C223.14 217.99 216.08 225.05 207.38 225.05C198.67 225.05 191.61 217.99 191.61 209.28C191.61 200.58 198.67 193.52 207.38 193.52C216.08 193.52 223.14 200.58 223.14 209.28Z"/>
    <path fill-rule="evenodd" d="M191.7 104.23L191.33 133.76L191.05 167.52L167.92 167.47L168.26 134.4L168.21 103.96C168.2 98.3 164.12 93.75 158.76 92.55C153.97 91.48 149.25 91.57 144.43 92.26C136.93 93.33 133.53 98.69 133.35 106.33L137.44 167.48L114.31 167.49L111.84 130.11L110.45 108.59C109.35 91.57 115.69 75.3 132.86 70.62C138.81 68.99 144.73 68.56 150.95 68.72C162.19 68.42 173.64 70.96 180.02 81.05C186.72 70.68 198.39 68.43 210.36 68.71C223.87 68.45 237.27 71.5 244.39 83.54C247.66 89.05 249.57 95.18 249.63 101.59L249.71 109.44L240.5 167.51L217.38 167.48L226.42 109.05C227.36 101.0 224.28 93.82 216.32 92.4C211.56 91.54 206.78 91.5 201.97 92.38C196.26 93.43 191.78 98.1 191.7 104.23Z"/>
    <path fill-rule="evenodd" d="M87.35 143.33L103.4 143.49L104.74 167.54L86.13 167.38C78.38 167.32 71.26 163.97 66.05 158.34C61.39 153.06 59.01 146.85 57.87 139.85L43.74 52.97C43.17 49.47 39.31 47.7 36.48 47.69L11.79 47.58C4.65 47.54 0.0 40.48 1.46 33.69C2.5 28.9 6.2 24.66 11.48 24.65L39.92 24.64C49.52 24.64 57.8 30.19 62.6 38.22C65.11 42.46 66.26 46.92 67.04 51.77L69.79 68.68L100.04 68.69L100.98 91.79L73.48 91.91L80.92 137.53C81.42 140.59 84.34 143.3 87.35 143.33Z"/>
  </g>
</svg>`;

const MAX_ENTRIES = 120;

self.addEventListener('install', event => {
  event.waitUntil((async () => {
    const cache = await caches.open(STATIC_CACHE);
    // Individually, so one 404 cannot fail the whole install.
    await Promise.all(PRECACHE.map(url =>
      cache.add(new Request(url, { cache: 'reload' })).catch(() => {})
    ));
    await self.skipWaiting();
  })());
});

self.addEventListener('activate', event => {
  event.waitUntil((async () => {
    if (self.registration.navigationPreload) {
      await self.registration.navigationPreload.enable();
    }
    const keys = await caches.keys();
    await Promise.all(keys.filter(k => k.startsWith('tmtm-') && k !== STATIC_CACHE)
                          .map(k => caches.delete(k)));
    await self.clients.claim();
  })());
});

self.addEventListener('message', event => {
  if (event.data === 'SKIP_WAITING') self.skipWaiting();
});

function isExcluded(pathname) {
  const p = pathname.toLowerCase();
  return EXCLUDED.some(x => p === x || p.startsWith(x + '/') || p.startsWith(x + '?'));
}

function isCacheableAsset(pathname) {
  const p = pathname.toLowerCase();
  return CACHEABLE_PREFIXES.some(prefix => p.startsWith(prefix));
}

/* Keep the runtime cache from growing without bound. */
async function trim(cache) {
  const keys = await cache.keys();
  if (keys.length <= MAX_ENTRIES) return;
  for (const k of keys.slice(0, keys.length - MAX_ENTRIES)) await cache.delete(k);
}

self.addEventListener('fetch', event => {
  const req = event.request;

  if (req.method !== 'GET') return;                       // never intercept mutations

  let url;
  try { url = new URL(req.url); } catch { return; }
  if (url.origin !== self.location.origin) return;        // never intercept third parties
  if (req.headers.has('range')) return;                   // let media range requests through

  /* --- Navigations: network-first, offline page as the only fallback ------ */
  if (req.mode === 'navigate') {
    event.respondWith((async () => {
      try {
        const preload = await event.preloadResponse;
        if (preload) return preload;
        return await fetch(req);
      } catch {
        const cache = await caches.open(STATIC_CACHE);
        const offline = await cache.match(OFFLINE_URL);
        return offline || new Response('You are offline.', {
          status: 503,
          headers: { 'Content-Type': 'text/plain; charset=utf-8' }
        });
      }
    })());
    return;
  }

  /* --- Static assets: stale-while-revalidate -----------------------------
     EXCLUDED is checked here, not above: it exists to keep personalised
     responses out of the cache, and the navigation branch above caches
     nothing. Checking it earlier only stripped the offline page from /cart,
     /customer and /checkout — the pages a shopper is most likely to be on
     when the connection drops. */
  if (isExcluded(url.pathname)) return;
  if (!isCacheableAsset(url.pathname)) return;

  event.respondWith((async () => {
    const cache = await caches.open(STATIC_CACHE);
    const hit = await cache.match(req);

    const network = fetch(req).then(res => {
      // Only store complete, successful, basic responses.
      if (res && res.status === 200 && res.type === 'basic') {
        cache.put(req, res.clone()).then(() => trim(cache)).catch(() => {});
      }
      return res;
    }).catch(() => null);

    if (hit) { event.waitUntil(network); return hit; }

    const res = await network;
    if (res) return res;

    if (req.destination === 'image') {
      return new Response(OFFLINE_IMAGE, {
        headers: { 'Content-Type': 'image/svg+xml; charset=utf-8', 'Cache-Control': 'no-store' }
      });
    }
    return new Response('', { status: 504, statusText: 'Offline' });
  })());
});


/* =============================================================================
   Push notifications (Firebase Cloud Messaging)

   ponytail: handled with the plain Push API instead of importScripts()-ing the
   Firebase SDK. FCM delivers a standard web-push payload, so the SDK in the
   worker would only buy us onBackgroundMessage() — at the cost of pulling two
   CDN scripts into the worker and duplicating the plugin's Firebase config
   here, where it cannot read the settings out of the database.

   The page still uses the Firebase SDK (it needs getToken); it hands this
   worker's registration to getToken({serviceWorkerRegistration}), which is why
   there is exactly one worker on scope '/' rather than a second one at
   /firebase-messaging-sw.js fighting this one for it.
   ============================================================================= */

const NOTIFICATION_ICON = '/icons/icons_0/android-chrome-192x192.png';

self.addEventListener('push', event => {
  if (!event.data) return;

  let payload = {};
  try {
    payload = event.data.json();
  } catch {
    payload = { notification: { body: event.data.text() } };
  }

  /* HTTP v1 sends `notification`; the plugin mirrors title/body into `data`. */
  const n = payload.notification || {};
  const d = payload.data || {};
  const title = n.title || d.title || 'Notification';

  event.waitUntil(self.registration.showNotification(title, {
    body: n.body || d.body || '',
    icon: n.icon || NOTIFICATION_ICON,
    badge: NOTIFICATION_ICON,
    /* Collapses repeat pushes about the same order into one notification. */
    tag: d.tag || d.orderId || undefined,
    data: { url: d.url || d.click_action || n.click_action || '/' }
  }));
});

self.addEventListener('notificationclick', event => {
  event.notification.close();

  const target = new URL((event.notification.data && event.notification.data.url) || '/', self.location.origin);

  event.waitUntil((async () => {
    const clientList = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });

    /* Reuse an open tab on this origin rather than piling up new ones. */
    for (const client of clientList) {
      if (new URL(client.url).origin === target.origin && 'focus' in client) {
        await client.focus();
        if (client.url !== target.href && 'navigate' in client) await client.navigate(target.href);
        return;
      }
    }

    if (self.clients.openWindow) await self.clients.openWindow(target.href);
  })());
});
