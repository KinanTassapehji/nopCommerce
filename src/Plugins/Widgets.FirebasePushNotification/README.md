# Widgets.FirebasePushNotification

This plugin integrates nopCommerce with Firebase Cloud Messaging (FCM) HTTP v1 API using a Google Service Account JSON.

## Required configuration values

- **Enable notifications** = enabled
- **Firebase project ID** = required (single project id used for Android, iOS, and Web)
- **Service account JSON** = required
- **Use FCM HTTP v1 API** = enabled

> You do **not** need separate Firebase project IDs per platform.

## Automatic token creation on register/login

The plugin now listens to customer authentication events:

- customer registration
- customer login

If a customer has no Firebase token record yet, the plugin creates a server-side token record automatically.

## Sending test notifications

You do **not** need to create a custom controller to send test notifications.

The plugin provides:

- `POST /Admin/FirebasePushNotification/SendTest`

In nopCommerce Admin, open:

- **Configuration -> Local plugins -> Widgets.FirebasePushNotification -> Configure**

Then use **Send test notification** (Customer ID + Title + Body + optional data JSON + platform).

## Client registration endpoints

Your mobile/web app can register device tokens (authenticated customer):

- `POST /api/plugin/FirebasePushNotification/RegisterToken`
- `POST /api/plugin/FirebasePushNotification/UnregisterToken`

## Web Push / VAPID requirement

Web push in Firebase requires configuring **Web Push certificates (VAPID keys)** in Firebase Console:

- Firebase Console -> Project settings -> Cloud Messaging -> Web Push certificates.

The plugin does **not** generate VAPID keys server-side.
