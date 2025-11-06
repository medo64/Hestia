# Hestia

Small web server that will prompt user for disk password (if there are
any encrypted LUKS partitions present). Once password is entered, it
will decrypt partitions and restart docker. All subsequent web requests
will be forwarded to a custom address.

This is essentially just scratching my itch when dealing with encrypted
appliance-like deployments.

To install as a service:
~~~sh
cat << EOF | tee /etc/systemd/system/hestia.service
[Unit]
Description=Hestia
After=network.target

[Service]
Type=simple
ExecStart=/usr/bin/hestia
Restart=on-failure
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable hestia
systemctl start hestia
~~~
