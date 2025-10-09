#!/bin/bash
set -euo pipefail

# === Configuration ===
DEPLOY_USER="deploy"
APP_DIR="/srv/app"
REPO_URL="https://github.com/PROxZIMA/ContributionAPI.git"
SERVICE_NAME="contribution-api"

echo ">>> Creating user $DEPLOY_USER"
if ! id "$DEPLOY_USER" &>/dev/null; then
  useradd -m -s /bin/bash -G sudo "$DEPLOY_USER"
  echo "$DEPLOY_USER ALL=(ALL) NOPASSWD:ALL" >/etc/sudoers.d/$DEPLOY_USER
  chmod 440 /etc/sudoers.d/$DEPLOY_USER
fi

# Add your public SSH key
mkdir -p /home/$DEPLOY_USER/.ssh
# !!!!! REPLACE THE BELOW SSH KEY WITH YOUR OWN !!!!!
cat > /home/$DEPLOY_USER/.ssh/authorized_keys <<'EOF'
ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABAQC2RKIVsWPoImlIEZHQ1H7NJn0s/0XmMeADBg/nN4yW0uPLAbG8VVElaDnSrhr+wEWh/FMwk/L1YdeCf8wx7HiMMn6v+XexJa9PZ7Ww3JjR+LSErjnJ7EJLEriRBQwpqKPkJ0qvHqu8XW+1d145PsosPaEdnaBbXHZ2EFkrVjJG1nGp9Juya8R4k3bJ7LbA/kUpWWaXP/dHScl78eumBoQegVugfYwl+RT30myax60l4qCW3ZxQfNSjPZOEIENMOFW5ChxgYhGbb+fRvH8DG8w2C274//uZcq3GDreXle39lorgPhvuPg0K+DvSv3ZMwGroj/Khb6sBP/3mA0Zkj0Ph ssh-key-2025-10-07
EOF
chown -R $DEPLOY_USER:$DEPLOY_USER /home/$DEPLOY_USER/.ssh
chmod 700 /home/$DEPLOY_USER/.ssh
chmod 600 /home/$DEPLOY_USER/.ssh/authorized_keys

echo ">>> Updating system and installing base packages"
apt-get update -y
apt-get upgrade -y
apt-get install -y apt-transport-https ca-certificates curl gnupg lsb-release ufw git

echo ">>> Installing Docker"
mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo "deb [arch=arm64 signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" > /etc/apt/sources.list.d/docker.list
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
usermod -aG docker "$DEPLOY_USER"
systemctl enable --now docker

echo ">>> Configuring firewall"
ufw allow 22
ufw allow 80
ufw allow 443
ufw --force enable

echo ">>> Cloning repository into $APP_DIR"
mkdir -p "$APP_DIR"
chown $DEPLOY_USER:$DEPLOY_USER "$APP_DIR"
cd "$APP_DIR"
sudo -u $DEPLOY_USER git clone "$REPO_URL" . || true
chown -R $DEPLOY_USER:$DEPLOY_USER "$APP_DIR"

echo ">>> Creating secrets directory"
mkdir -p "$APP_DIR/.secrets"
chown $DEPLOY_USER:$DEPLOY_USER "$APP_DIR/.secrets"
chmod 700 "$APP_DIR/.secrets"

echo ">>> Creating GHCR login helper script"
cat > /usr/local/bin/ghcr-login.sh <<'EOF'
#!/bin/bash
set -e
TOKEN_FILE="/home/deploy/.ghcr_token"
GOOGLE_CREDS_FILE="/home/deploy/.google_credentials_b64"

if [ -f "$TOKEN_FILE" ]; then
  GHCR_TOKEN=$(cat "$TOKEN_FILE")
  echo "$GHCR_TOKEN" | docker login ghcr.io -u deploy --password-stdin || true
  echo "Successfully logged into GHCR"
fi

if [ -f "$GOOGLE_CREDS_FILE" ]; then
  echo "Setting up Google Application Credentials..."
  cat "$GOOGLE_CREDS_FILE" | base64 --decode > /srv/app/.secrets/google-credentials.json
  chown deploy:deploy /srv/app/.secrets/google-credentials.json
  chmod 644 /srv/app/.secrets/google-credentials.json  # Make readable by all users
  echo "Google credentials configured successfully"
fi
EOF
chmod +x /usr/local/bin/ghcr-login.sh
chown root:root /usr/local/bin/ghcr-login.sh

echo ">>> Creating systemd service for $SERVICE_NAME"
cat > /etc/systemd/system/${SERVICE_NAME}.service <<'EOF'
[Unit]
Description=ContributionAPI Docker Compose Services
Requires=docker.service
After=network-online.target docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
User=deploy
Group=deploy
WorkingDirectory=/srv/app
ExecStartPre=/usr/local/bin/ghcr-login.sh
ExecStartPre=/usr/bin/git pull origin master
ExecStart=/usr/bin/docker compose pull
ExecStartPost=/usr/bin/docker compose up -d
ExecStop=/usr/bin/docker compose down
ExecReload=/usr/bin/docker compose pull
ExecReload=/usr/bin/docker compose up -d
TimeoutStartSec=300
Restart=on-failure
RestartSec=10

[Install]
WantedBy=multi-user.target
EOF

echo ">>> Creating update helper script"
cat > /usr/local/bin/update-contribution-api.sh <<'EOF'
#!/bin/bash
set -e
echo "Updating ContributionAPI deployment..."
cd /srv/app
git pull origin master
/usr/local/bin/ghcr-login.sh
docker compose pull
docker compose up -d
docker image prune -f
echo "ContributionAPI updated successfully!"
EOF
chmod +x /usr/local/bin/update-contribution-api.sh
chown root:root /usr/local/bin/update-contribution-api.sh

echo ">>> Creating Caddy log directory"
mkdir -p /var/log/caddy
chown $DEPLOY_USER:$DEPLOY_USER /var/log/caddy

echo ">>> Reloading systemd and enabling service"
systemctl daemon-reload
systemctl enable --now ${SERVICE_NAME}.service

echo "✅ Setup complete: ContributionAPI deployed and managed by systemd."
echo ">>> To update ContributionAPI, run: /usr/local/bin/update-contribution-api.sh"
