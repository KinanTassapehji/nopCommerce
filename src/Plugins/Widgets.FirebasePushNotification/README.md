# Widgets.FirebasePushNotification

Firebase Cloud Messaging (FCM) push notifications for nopCommerce, sent through the
Firebase Admin SDK (HTTP v1) with a Google service account.

## Setup

1. **Service account** - download the service account JSON from Firebase Console ->
   Project settings -> Service accounts. Either drop it next to the app (the file is
   found by its `*firebase-adminsdk*.json` name, exactly as downloaded) or point
   `GOOGLE_APPLICATION_CREDENTIALS` at it. On GCP the metadata server is used instead.
   Without a credential the plugin loads but every send fails silently.
2. **Web app config** - Admin -> Configuration -> Local plugins ->
   Widgets.FirebasePushNotification -> Configure. Fill API key, auth domain, project id,
   messaging sender id and app id from Firebase Console -> Project settings -> General ->
   Your apps -> Web app, and the VAPID key from Cloud Messaging -> Web Push certificates.
   The client script renders nothing while API key or project id is empty.
3. **Enable the widget** - Admin -> Configuration -> Widgets -> enable
   `Widgets.FirebasePushNotification`. The script only renders when the widget is active.
4. **HTTPS** - web push needs a secure context. Only `localhost` is exempt, so on a plain
   `http://` host name the browser refuses to register the service worker and no token is
   ever collected.

One Firebase project id covers Android, iOS and Web.

## How tokens are collected

The widget renders into `body_end_html_tag_before` for logged-in customers, asks for
notification permission once (the choice is remembered in `localStorage`), then posts the
FCM token to the plugin. There is no server-side token creation: a customer has a row in
`FirebaseDeviceToken` only after a real device registered.

Background delivery uses the site's own `wwwroot/sw.js`, which handles the `push` and
`notificationclick` events with the plain Push API - the plugin does not serve a
`firebase-messaging-sw.js` of its own.

Clients (e.g. a mobile wrapper) can register directly, as the authenticated customer:

- `POST /api/plugin/FirebasePushNotification/RegisterToken` - `{ token, platform }`,
  platform is `android`, `ios` or `web`
- `POST /api/plugin/FirebasePushNotification/UnregisterToken` - `{ token }`

Tokens FCM reports as unregistered or invalid are deactivated automatically.

## Sending

- **Automatic** - order placed, paid, processing, complete, cancelled, shipped, delivered
  and ready-for-pickup events each push to the order's customer. These texts are currently
  English only.
- **Broadcast** - Admin -> Third party plugins -> Send Broadcast Notification. Targets one
  searched customer or all customers, filtered by platform, with separate English and
  Arabic title/body picked per customer language, plus optional `string:string` data JSON.
- **Test** - `POST /Admin/FirebasePushNotification/SendTest` (customer id, title, body,
  platform, optional data JSON). There is no form for it on the configuration page yet.