# Distance

An *experimental* implementation of a rule-based network troubleshooting tool written in C# for .net core target. 
It depends on **tshark** (https://www.wireshark.org/docs/man-pages/tshark.html) for decoding packets and **NRules** (https://github.com/NRules) engine for evaluating rules.

## Set up 
The easiest way of running the tool is to use Vagrant for creating a virtual machine and doing all the necessary configuration. To do so, just type 
`vagrant up ` in the root folder of the solution. When using Windows and Hyper-V, it is necessary to run this command with elevated privileges. Execute 
`shell.cmd` to execute the privilege powershell. 



## Usage:

`DistanceEngine` project is a .NET Core console application that do all the things.  When running in the virtual machine, 
`distance` command is available that does all the necesary things to run the `DistanceEngine` application. 
The program offers several commands to build and run the diagnostic engine:

```
Usage: distance [options] [command]

Options:
  -?|-help  Show help information
  -debug    Enable debug output.

Commands:
  build  Build a distance ruleset from the source yaml project.
  run    Run a distance ruleset against the specified input file(s).
```

The `build` command is supposed to be used during development, when new rules are added. It compiles new rules to C# source files to be used by the engine.
The `run` command runs the diagnostic engine for provided packet source file. 

### Run Command
The program loads the input pcap, executes `tshark` for decoding packets and applies rules from `DistanceRules`. 
The output containing identified issues is written to `.log` file and optionally also to console.

```
Usage: distance run [arguments] [options]

Arguments:
  InputPcapFile  An input packet capture file to analyze.

Options:
  -?|-help  Show help information.
```

### Build Command



## Dependencies

| Tool/Library    | Usage        | Homepage                                             | Licence                                |
| --------------- | ------------ | ---------------------------------------------------  | -------------------------------------- |
| NRules          | included     | https://github.com/NRules/NRules                     | MIT Licence                            |
| TShark          | standalone   | https://www.wireshark.org/docs/man-pages/tshark.html | GNU GPL                                |
| SharpPcap       | included     | https://github.com/chmorgan/sharppcap                | GNU Lesser General Public License v3.0 |
| PacketDotNet    | included     | https://github.com/chmorgan/packetnet                | GNU Lesser General Public License v3.0 |
| Newtonsoft.Json | included     | https://www.newtonsoft.com/json                      | MIT Licence                            |
| NLog            | included     | https://nlog-project.org/                            | BSD license                            | 
