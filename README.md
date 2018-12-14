# Distance

An *experimental* implementation of a rule-based network troubleshooting tool written in C# for .net core target. 
It depends on **tshark** (https://www.wireshark.org/docs/man-pages/tshark.html) for decoding packets and **NRules** (https://github.com/NRules) engine for evaluating rules.

## Set up 
The easiest way of running the tool is to use Vagrant. To do so, just type 
`vagrant up ` in the root folder of the solution. When using Windows and Hyper-V, it is necessary to run this command with elevated privileges. Execute 
`shell.cmd` to open powershell with necessary rights. 



## Usage:

The `DistanceEngine` project is a .NET Core console application that do all the things.  When running in the virtual machine, 
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
Not implemented yet :(.



## Example
The following is an example of running the tool:

```
$ vargant ssh
[vagrant@localhost]$ mkdir pcap
[vagrant@localhost]$ cd pcap
[vagrant@localhost]$ wget www.fit.vutbr.cz/~rysavy/distance.datasets/testbed-16.pcap
[vagrant@localhost]$ distance run testbed-16.pcap
Loading and decoding packets from '/home/vagrant/pcaps/testbed-16.pcap'...ok [00:00:01.4510746].
Loading rules from assembly 'DistanceRules, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'...ok [00:00:00.0679699].
Compiling rules...ok [00:00:00.0764481].
Creating a session...ok [00:00:00.0151823].
Inserting facts (306) to the session...ok [00:00:00.0392679].
Waiting for completion......done [00:00:00.0707294].
Diagnostic output written to '/home/vagrant/pcaps/testbed-16.log'.
[vagrant@localhost pcaps]$
```

While it is still too early to evaluate performance here are some preliminary results measured for DNS diagnostic ruleset:

| Input size    | DNS packets        | TShark DNS decoding | Rules Evaluation Time | Detected Issues |
| ------------- | ------------------ | ------------------- | --------------------- | --------------- |
| 16 MB         | 306                | 1.62s               | 0.10s                 | 58              | 
| 32 MB         | 580                | 2.58s               | 0.14s                 | 103             |
| 64 MB         | 2084               | 6.11s               | 0.50s                 | 327             |
| 128 MB        | 4530               | 14.41s              | 1.88s                 | 594             |

Again, this is just for a quick info on the computation costs as the DNS rule set contains only 4 relatively simple rules.
## Dependencies

| Tool/Library    | Usage        | Homepage                                             | Licence                                |
| --------------- | ------------ | ---------------------------------------------------  | -------------------------------------- |
| NRules          | included     | https://github.com/NRules/NRules                     | MIT Licence                            |
| TShark          | standalone   | https://www.wireshark.org/docs/man-pages/tshark.html | GNU GPL                                |
| SharpPcap       | included     | https://github.com/chmorgan/sharppcap                | GNU Lesser General Public License v3.0 |
| PacketDotNet    | included     | https://github.com/chmorgan/packetnet                | GNU Lesser General Public License v3.0 |
| Newtonsoft.Json | included     | https://www.newtonsoft.com/json                      | MIT Licence                            |
| NLog            | included     | https://nlog-project.org/                            | BSD license                            | 
