#!/usr/bin/env bash

MODE=$1
NEW_HOSTNAME=$2
export BIND_IP=$3

NOMAD_SYSTEMD_FILE=""
CONSUL_SYSTEMD_FILE=""
case $MODE in
"server")
  NOMAD_SYSTEMD_FILE="/vagrant/config/nomad/nomad.server.systemd"
  CONSUL_SYSTEMD_FILE="/vagrant/config/consul/consul.server.systemd"
  ;;

"client")
  NOMAD_SYSTEMD_FILE="/vagrant/config/nomad/nomad.client.systemd"
  CONSUL_SYSTEMD_FILE="/vagrant/config/consul/consul.client.systemd"
  ;;

*)
  echo "Mode must be 'server' or 'client'"
  exit 1
esac

progress () {
    echo "==== $1 ===="
}

############################
progress "Installing packages"
apt-get update
apt-get install -y \
    apt-transport-https \
    build-essential \
    ca-certificates \
    curl \
    lsb-release \
    make \
    resolvconf \
    zip

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo \
    "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu \
    $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io

############################
progress "Setting hostname"
hostname "${NEW_HOSTNAME}"
echo "${NEW_HOSTNAME}" > /etc/hostname

############################
progress "Installing Nomad, Consul, Etcd"
cd /vagrant/
make install

############################
progress "Configuring Nomad"
# Courtesy of https://learn.hashicorp.com/tutorials/nomad/production-deployment-guide-vm-with-consul#configure-systemd
cat "${NOMAD_SYSTEMD_FILE}" | envsubst > /etc/systemd/system/nomad.service

systemctl daemon-reload
systemctl start nomad
systemctl enable nomad

############################
progress "Configuring Consul"
# Courtesy of https://learn.hashicorp.com/tutorials/consul/deployment-guide#configure-systemd
cat "${CONSUL_SYSTEMD_FILE}" | envsubst > /etc/systemd/system/consul.service

systemctl daemon-reload
systemctl start consul
systemctl enable consul

############################
progress "Configuring Etcd"
cp /vagrant/config/etcd/etcd.systemd /etc/systemd/system/etcd.service

systemctl daemon-reload
systemctl start etcd
systemctl enable etcd

############################
progress "Configuring Docker"

cat <<EOF > /etc/docker/daemon.json
{ "ipv6": true, "fixed-cidr-v6": "fd00:1:1::/64", "insecure-registries": ["registry.service.apps.internal:5000"]}
EOF

systemctl reload docker.service
systemctl restart docker.service
systemctl enable docker.service
systemctl enable containerd.service

usermod -aG docker vagrant

############################
progress "Installing Pack CLI"
add-apt-repository ppa:cncf-buildpacks/pack-cli
apt-get update
apt-get install pack-cli

############################
progress "Configuring DNS resolution to use Consul"
cat <<EOF > /etc/resolvconf/resolv.conf.d/head
nameserver 127.0.0.1
EOF
iptables --table nat --append OUTPUT --destination localhost --protocol udp --match udp --dport 53 --jump REDIRECT --to-ports 8600
iptables --table nat --append OUTPUT --destination localhost --protocol tcp --match tcp --dport 53 --jump REDIRECT --to-ports 8600

systemctl restart resolvconf.service
systemctl restart systemd-resolved
