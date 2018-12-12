# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure(2) do |config|
  config.vm.box = "centos/7"

  # config.vm.network "forwarded_port", guest: 80, host: 8080
  # config.vm.network "private_network", ip: "192.168.33.10"
  # config.vm.network "public_network"

  config.vm.synced_folder ".", "/distance", type: "sshfs"

  # Provider-specific configuration so you can fine-tune various
  # backing providers for Vagrant. These expose provider-specific options.
  # Example for VirtualBox:

   config.vm.provider "virtualbox" do |vb|
      # Display the VirtualBox GUI when booting the machine
      # vb.gui = true

      # Customize the amount of memory on the VM:
      vb.memory = "1024"
   end
     
  # Install dotnet:
  config.vm.provision "shell", inline: <<-SHELL
     rpm -Uvh https://packages.microsoft.com/config/rhel/7/packages-microsoft-prod.rpm
     yum update
     yum install dotnet-sdk-2.2
  SHELL  
  
  # Install tshark:
  config.vm.provision "shell", inline: <<-SHELL
     (
        yum install -y wireshark tcpflow unzip
     )
  SHELL 
  

  # Compile and install DISTANCE
  config.vm.provision "shell", inline: <<-SHELL
     (
       cd /distance-repo
       ./bootstrap.sh &&
       ./configure --prefix=/opt/distance -q &&
       make -j2 &&
       sudo make install
       cd pyrocksdb
       sudo python setup.py test
       sudo python setup.py install
     )
  SHELL


end