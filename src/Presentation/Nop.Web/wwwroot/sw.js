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

const CACHE_VERSION = 'v4';
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
    return res || new Response('', { status: 504, statusText: 'Offline' });
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
