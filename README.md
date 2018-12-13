# Distance

An experimental implementation of a rule-based network troubleshooting tool. It uses NRules (https://github.com/NRules) engine.

## Vargant
The solution provides `Vagrantfile` to be used with Vagrant for creating a virtual machine containing 
necessary dependencies and enabling to smoothly run the tool.
To use Vargant for running the tool apply the following steps:

* Check that you have properly installed Vgrant environment

* In solution folder (where `Vagrantfile` is located) run Vagrant. 
```
$ vargant up
```

* The virtual machine should be created and necessary tools installed. Also, the root distance folder is mounted 
in the virtual machine. It is possible to login to the virtual machine:

```
$ vagrant ssh
```

* The project is located in `/distance` folder. Thus, to compile the solution switch to this folder and run dotnet tool:

```
$ cd /distance
$ dotnet build
```

* To run the tool 

## Usage:

DistanceEngine project is a .NET Core console application that do all the things:

```bash
$ dotnet DistanceEngine.dll [SOURCE-PCAP]
```

The program loads the input pcap, executes `tshark` for decoding packets and applies rules from `DistanceRules`. The output containing 
identified issues is written to `.log` file.

## Dependencies
* NRules
* TShark
* SharpPcap
* PacketDotNet
