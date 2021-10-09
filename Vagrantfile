# -*- mode: ruby -*-
# vi: set ft=ruby :


instances = [
  {
    :hostname => "node-1",
    :ip => "192.168.33.10",
    :cidrAllocation => "10.0.1.0/24",
    :roles => [
      :consul_server,
      :nomad_server,
      :etcd_member,
      :calico_node
    ]
  },
  {
    :hostname => "node-2",
    :ip => "192.168.33.11",
    :cidrAllocation => "10.0.2.0/24",
    :roles => [
      :consul_server,
      :nomad_client,
      :etcd_member,
      :calico_node
    ]
  },
  {
    :hostname => "node-3",
    :ip => "192.168.33.12",
    :cidrAllocation => "10.0.3.0/24",
    :roles => [
      :consul_server,
      :nomad_client,
      :etcd_member,
      :calico_node
    ]
  },
]

Vagrant.configure("2") do |config|
  instances.each do |instance|
    config.vm.define instance[:hostname] do |node|
      node.vm.box = "ubuntu/bionic64"
      node.vm.network "private_network", ip: instance[:ip]
      node.vm.synced_folder ".", "/vagrant"

      node.vm.provider "virtualbox" do |vb|
        vb.gui = false
        vb.memory = "2048"
      end

      # Configure ONLY the last client machine to use Ansible as provisioner
      # and tell it to target everything.
      # This lets ansible provision all machines simultaneously.
      if instance[:hostname] == instances[instances.length-1][:hostname]
        node.vm.provision "ansible" do |ansible|
          ansible.compatibility_mode = "2.0"

          ansible.limit = "all"
          ansible.playbook = "config/ansible/playbook.yml"
          ansible.groups = build_ansible_groups(instances)

          host_vars = {}

          instances.each do |i|
            host_vars[i[:hostname]] = {
              "private_ip" => i[:ip],
              "cidr_allocation" => i[:cidrAllocation]
            }
          end

          ansible.host_vars = host_vars
        end
      end
    end
  end
end

def build_ansible_groups(instances)
  return {
    "consul_servers" => filter_by_role(instances, :consul_server),
    "nomad_servers" => filter_by_role(instances, :nomad_server),
    "nomad_clients" => filter_by_role(instances, :nomad_client),
    "etcd_members" => filter_by_role(instances, :etcd_member),
    "calico_nodes" => filter_by_role(instances, :calico_node)
  }
end

def filter_by_role(is, role)
  return is.select { |i|  i[:roles].include?(role) }.map{|i| i[:hostname]}
end