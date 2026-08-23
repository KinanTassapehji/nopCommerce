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

const CACHE_VERSION = 'v1';
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
  '/wishlist',
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
  if (isExcluded(url.pathname)) return;
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

  /* --- Static assets: stale-while-revalidate ----------------------------- */
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
