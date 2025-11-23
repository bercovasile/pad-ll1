#!/bin/sh
set -eu


ANNOUNCE_IP="$1"
CONF_FILE="/data/redis.conf"


mkdir -p /data
# write a fresh config (use > not >> to avoid appending on restarts)
cat > "$CONF_FILE" <<EOF
port 6379
cluster-enabled yes
cluster-config-file nodes.conf
cluster-node-timeout 5000
appendonly no
loglevel notice
requirepass ${SUPER_SECRET_PASSWORD}
masterauth ${SUPER_SECRET_PASSWORD}
protected-mode no
cluster-announce-ip $ANNOUNCE_IP
cluster-announce-port 6379
cluster-announce-bus-port 16379
dir /data
EOF


# start redis-server from the config file
exec redis-server "$CONF_FILE"