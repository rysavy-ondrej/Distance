# Distance

An experimental implementation of a rule-based network troubleshooting tool. It uses NRules (https://github.com/NRules) engine.

## Vagrant
The solution provides `Vagrantfile` to be used with Vagrant for creating a virtual machine containing 
necessary dependencies and enabling to smoothly run the tool. To use Vagrant for running the tool apply the following steps:

* Check that you have properly installed Vagrant environment. In solution folder (where `Vagrantfile` is located) run: 

```bash
$ vargant up
```

* The virtual machine should be created and necessary tools installed. Also, the root distance folder is mounted 
in the virtual machine. It is possible to login to the virtual machine:

```bash
$ vagrant ssh
```

* The project is located in `/distance` folder. Thus, to compile the solution switch to this folder and execute a build script. Note
that it would be also possble use dotnet tool directly, but the build script contains some other actions necessary to run the tool properly.

```bash
$ cd /distance
$ ./build
```

* The `build` script should build the entire solution and prepare the environment, e.g., it creates an executable script distance for running the tool:

```bash
$ distance some.pcap
```

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
