# -*- mode: ruby -*-
# vi: set ft=ruby :

# Need to run these as vagrant user not root:
$user_config_script = <<-SCRIPT
chmod a+x /distance/Scripts/user-config.sh
su -c "source /distance/Scripts/user-config.sh" vagrant
SCRIPT

Vagrant.configure(2) do |config|
  config.vm.box = "centos/7"

  if Vagrant::Util::Platform.windows?
    config.vm.synced_folder ".", "/distance"
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
  
  # Install distance runner:
  config.vm.provision "file", source: "Scripts/distance", destination: "/distance/artifacts/distance"

  # Modify environment for vagrant user:
  config.vm.provision "shell", inline: $user_config_script

end