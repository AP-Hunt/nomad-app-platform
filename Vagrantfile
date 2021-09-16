# -*- mode: ruby -*-
# vi: set ft=ruby :


instances = [

  {
    :hostname => "client-1",
    :ip => "192.168.33.11"
  },
  {
    :hostname => "client-2",
    :ip => "192.168.33.12"
  },
]

Vagrant.configure("2") do |config|
  config.vm.define "server-1" do |node|
    node.vm.box = "ubuntu/bionic64"
    node.vm.network "private_network", ip: "192.168.33.10"
    node.vm.synced_folder ".", "/vagrant"

    node.vm.provider "virtualbox" do |vb|
      vb.gui = false
      vb.memory = "2048"
    end
  end


  instances.each do |instance|
    config.vm.define instance[:hostname] do |node|
      node.vm.box = "ubuntu/bionic64"
      node.vm.network "private_network", ip: instance[:ip]
      node.vm.synced_folder ".", "/vagrant"

      node.vm.provider "virtualbox" do |vb|
        vb.gui = false
        vb.memory = "2048"
      end
    end
  end

  config.vm.provision "ansible" do |ansible|
    ansible.playbook = "config/ansible/playbook.yml"
    ansible.groups = {
      "servers" => ["server-1"],
      "clients" => ["client-1", "client-2"]
    }
  end
end
