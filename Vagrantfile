# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure(2) do |config|
  config.vm.box = "centos/7"

  if Vagrant::Util::Platform.windows?
	# default windows share (SMB)
    config.vm.synced_folder ".", "/distance", type: "rsync"
  elsif Vagrant.has_plugin?("vagrant-sshfs") then
	  config.vm.synced_folder ".", "/distance", type: "sshfs"
  else
    config.vm.synced_folder ".", "/distance", type: "rsync"
  end

  config.vm.provider "virtualbox" do |vb|
      vb.memory = "1024"
  end
  config.vm.provider "hyperv" do |hv|
      hv.memory = "1024"
  end
     
  # Install dotnet:
  config.vm.provision "shell", path: "Scripts/install-dotnet.sh"
  
  # Install tshark:
  config.vm.provision "shell", path: "Scripts/install-tshark.sh" 
  
end