# TmTm WhatsApp sidecar

Sends the store's phone verification codes (new account, changed number, password reset by
phone) from TmTm's own WhatsApp number.

It is the MosqueSms sidecar (`D:\Work\mosquesms`) with one addition, `POST /check`: a small
Node service that keeps **WhatsApp Web** logged in through **whatsapp-web.js** and a headless
Chrome, and listens on the server's loopback only. The store calls it
(`WhatsAppSidecarClient`); nothing outside the server can.

> **Read before relying on it.** whatsapp-web.js is an *unofficial* automation of WhatsApp Web
> and is against WhatsApp's terms; the number can be restricted or banned. It is used because
> Meta's official API does not serve Syrian businesses. If the number is banned, nobody can
> register or recover a password by phone until another number is linked - so use a
> dedicated number (never someone's personal one), and link a second number as a spare: the
> store spreads codes over every connected number.

## On the server (185.56.151.67)

TmTm's sidecar runs beside the mosque's - a separate service, port, session folder and
number. Port **3210** (the mosque uses 3102, another app 3100).

```bash
# code (from this folder, on your machine)
scp index.js package.json package-lock.json root@185.56.151.67:/var/www/tmtm-whatsapp/
# on the server
cd /var/www/tmtm-whatsapp && npm ci --omit=dev          # first time: downloads its Chrome
install -m 0644 systemd/tmtm-whatsapp.service /etc/systemd/system/      # copy systemd/ up too
install -m 0755 systemd/tmtm-whatsapp-health.sh /usr/local/bin/
install -m 0644 systemd/tmtm-whatsapp-health.service systemd/tmtm-whatsapp-health.timer /etc/systemd/system/
systemctl daemon-reload
systemctl enable --now tmtm-whatsapp.service tmtm-whatsapp-health.timer
curl -s http://127.0.0.1:3210/health        # {"ok":true}
```

- The linked login lives in `/var/www/tmtm-whatsapp-session`, outside the code folder, so
  redeploying the code never logs the number out. Delete it only to start over.
- The unit caps memory like the mosque's (the server froze once, 2026-10-01, before those
  caps). A second headless Chrome is a real load on this shared box - watch `free -m`.
- If WhatsApp ships a web update that breaks injection (no QR, crash loop), set a newer
  `WA_WEB_VERSION` in the unit (see the comment in `index.js`) and restart.

## Linking the number and switching codes on

1. Admin > Configuration > Settings > **Phone verification** (super administrators).
   The sidecar address is `http://127.0.0.1:3210` (set by the migration).
2. **Link a number**, then on the store's phone: WhatsApp > Settings > Linked devices >
   Link a device, and scan the QR. The page refreshes until it says *Connected*.
3. Tick **Require a code** and save. From then on new accounts open only after the code.

Leaving the address empty writes codes to the system log instead of sending them - for
testing on a local machine without WhatsApp.

## Run it locally

```bash
npm ci
PORT=3210 WWEBJS_DATA=./.wwebjs_auth node index.js
```