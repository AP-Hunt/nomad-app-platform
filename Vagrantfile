# -*- mode: ruby -*-
# vi: set ft=ruby :


instances = [
  {
    :hostname => "node-1",
    :ip => "192.168.33.10"
  },
  {
    :hostname => "node-2",
    :ip => "192.168.33.11"
  },
  {
    :hostname => "node-3",
    :ip => "192.168.33.12"
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
          ansible.groups = {
            "consul_servers" => ["node-1", "node-2", "node-3"],
          }

          host_vars = {}

          instances.each do |i|
            host_vars[i[:hostname]] = {
              "private_ip" => i[:ip]
            }
          end

          ansible.host_vars = host_vars
        end
      end
    end
  end
end
