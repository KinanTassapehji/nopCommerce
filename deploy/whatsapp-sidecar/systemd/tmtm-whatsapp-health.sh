#!/usr/bin/env bash
# Health watchdog for TmTm's WhatsApp sidecar (copied from the mosque's).
#
# Restart=always only catches a process that EXITS; this catches one that is alive
# but frozen (hung event loop / wedged Chrome): it polls /health and restarts the
# service after 3 misses in a row. A restart restores the linked number from disk,
# it does not log it out. Runs every 60s from tmtm-whatsapp-health.timer.
set -u

SVC="${SVC:-tmtm-whatsapp.service}"
URL="${URL:-http://127.0.0.1:3210/health}"
STATE="${STATE:-/run/tmtm-whatsapp-health.fails}"
MAX_FAILS="${MAX_FAILS:-3}"

if curl -fs --max-time 8 "$URL" >/dev/null 2>&1; then
  echo 0 > "$STATE"
  exit 0
fi

n=$(( $(cat "$STATE" 2>/dev/null || echo 0) + 1 ))
echo "$n" > "$STATE"
logger -t tmtm-whatsapp-health "health check failed ($n/$MAX_FAILS) for $URL"

if [ "$n" -ge "$MAX_FAILS" ]; then
  logger -t tmtm-whatsapp-health "restarting $SVC after $n consecutive failures"
  systemctl restart "$SVC"
  echo 0 > "$STATE"
fi