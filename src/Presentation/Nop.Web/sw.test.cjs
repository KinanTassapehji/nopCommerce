/* Self-check for the push payload handling in wwwroot/sw.js.
   Run: node src/Presentation/Nop.Web/sw.test.cjs
   Not under wwwroot on purpose — that directory is served publicly.

   ponytail: stubs just enough of the ServiceWorkerGlobalScope to load the real
   file. Guards the notification/data fallback, which is the part that fails
   silently (a wrong key ships a push that shows "Notification" with no body). */

const assert = require('node:assert');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const handlers = {};
let shown = null;

const scope = {
  location: { origin: 'https://example.test' },
  addEventListener: (type, fn) => { handlers[type] = fn; },
  registration: {
    showNotification: (title, opts) => { shown = { title, ...opts }; },
    /* Unused here, but sw.js reads it at install/activate time. */
    navigationPreload: { enable: async () => {} }
  },
  clients: { matchAll: async () => [], openWindow: async () => {} },
  skipWaiting: () => {},
  caches: { open: async () => ({ addAll: async () => {}, match: async () => null }), keys: async () => [] }
};
scope.self = scope;

const src = fs.readFileSync(path.join(__dirname, 'wwwroot', 'sw.js'), 'utf8');
vm.createContext(scope);
vm.runInContext(src, scope, { filename: 'sw.js' });

assert.ok(handlers.push, 'sw.js registered no push handler');

const push = payload => {
  shown = null;
  const waits = [];
  handlers.push({ data: { json: () => payload }, waitUntil: p => waits.push(p) });
  return shown;
};

// HTTP v1 `notification` block is preferred.
let n = push({ notification: { title: 'Order shipped', body: 'On its way' }, data: {} });
assert.strictEqual(n.title, 'Order shipped');
assert.strictEqual(n.body, 'On its way');

// Falls back to `data`, which is what FirebaseNotificationService mirrors into.
n = push({ data: { title: 'From data', body: 'Body from data' } });
assert.strictEqual(n.title, 'From data');
assert.strictEqual(n.body, 'Body from data');

// Data-only push with neither key still shows something rather than throwing.
n = push({ data: {} });
assert.strictEqual(n.title, 'Notification');
assert.strictEqual(n.body, '');

// Click target comes from data.url, then click_action, then '/'.
assert.strictEqual(push({ data: { url: '/order/details/5' } }).data.url, '/order/details/5');
assert.strictEqual(push({ data: { click_action: '/x' } }).data.url, '/x');
assert.strictEqual(push({ data: {} }).data.url, '/');

// A push with no payload at all must not throw or show an empty notification.
shown = null;
handlers.push({ data: null, waitUntil: () => {} });
assert.strictEqual(shown, null, 'empty push should show nothing');

console.log('sw.js push handling: all checks passed');
