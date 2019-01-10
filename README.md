# Distance

An *experimental* implementation of a rule-based network troubleshooting tool written in C# for .net core target. 
It depends on **tshark** (https://www.wireshark.org/docs/man-pages/tshark.html) for decoding packets and **NRules** (https://github.com/NRules) engine for evaluating rules.

## Set up 
The easiest way of running the tool is to use Vagrant. To do so, just type 
`vagrant up ` in the root folder of the solution. When using Windows and Hyper-V, it is necessary to run this command with elevated privileges. Execute 
`shell.cmd` to open powershell with necessary rights. 



## Usage:

The `DistanceEngine` project is a .NET Core console application that do all the things.  When running in the virtual machine, 
`distance` command is available to run the `DistanceEngine` application. 
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
The program loads the input pcap, executes `tshark` for decoding packets and applies diagnostic rules. 
The full diagnostic log output is written to `.log` file. The generated events are written to '.evt' file.

```
Usage: distance run [arguments] [options]

Arguments:
  InputPcapFile  An input packet capture file to analyze.

Options:
  -?|-help                Show help information
  -profile <PROFILENAME>  Specifies assembly that contains a diagnostic profile.
  -parallel <NUMBER>      Sets the degree of parallelism when loading and decoding of input data (-1 means unlimited).
```

To run the diagnostics, it is necessary to specify the diagnostic profile. The profile is identified by the name of the assembly implementing diagnostic rules. 
The application searches for the profile assemblies in the following order:
* Current working directory 
* All directories set in environment variable `DISTANCE_PROFILES`
* Directory of the executable file


### Build Command
Build command is used to generate C# source code from the diagnostic rule definitions. 

```
Usage: distance build [arguments] [options]

Arguments:
  SourceYamlProject  A file with the source yaml ruleset project. Multiple values can be specified.

Options:
  -?|-help  Show help information
```




## Example
The following is an example of running the tool identifying DNS problems:

```
$ vagrant ssh
[vagrant@localhost]$ mkdir pcap
[vagrant@localhost]$ cd pcap
[vagrant@localhost]$ wget www.fit.vutbr.cz/~rysavy/distance.datasets/testbed-16.pcap
[vagrant@localhost]$ distance run -profile Diagnostics.Soho testbed-16.pcap
Loading rules from assembly 'Diagnostics.Soho, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null', Location='/distance/artifacts/Diagnostics.Soho.dll'...ok [00:00:00.0949057].
Compiling rules...ok [00:00:00.0918322].
Creating a session...ok [00:00:00.0150605].
Loading facts, using all thread(s):
  Loading packets from '/home/vagrant/pcaps/testbed-16.pcap', filter='ip'...
  Loading packets from '/home/vagrant/pcaps/testbed-16.pcap', filter='icmp'...
  Loading packets from '/home/vagrant/pcaps/testbed-16.pcap', filter='dns'...
  ok [00:00:04.1976993].
  Inserting 'IcmpPacket' facts (0) to the session.
  ok [00:00:00.0046929].
  ok [00:00:04.8154225].
  Inserting 'IpPacket' facts (22109) to the session.
  ok [00:00:00.1061313].
  ok [00:00:04.0939339].
  Inserting 'DnsPacket' facts (306) to the session.
  ok [00:00:00.0378132].
All facts loaded [00:00:05.1743015].
Waiting for completion.......done [00:00:00.0395075].
Diagnostic Log written to '/home/vagrant/pcaps/testbed-16.log'.
Diagnostic Events written to '/home/vagrant/pcaps/testbed-16.evt'.
[vagrant@localhost pcaps]$
```
The output is written to `testbed-16.log`. Its content consists of lines each giving information about a single issue found in the communication. These issues are low level 
information that is reported by diagnostic rules. 

The results of the diagnostics is stored in `testbed-16.evt` file. This file contains information about the problems found and possible reasons. 

While it is still too early to evaluate the performance, the following table contains some preliminary results measured for the DNS diagnostic ruleset (just 4 rules):

| Input size    | DNS packets        | TShark DNS decoding | Rules Evaluation Time | Detected Issues |
| ------------- | ------------------ | ------------------- | --------------------- | --------------- |
| 16 MB         | 306                | 1.62s               | 0.10s                 | 58              | 
| 32 MB         | 580                | 2.58s               | 0.14s                 | 103             |
| 64 MB         | 2084               | 6.11s               | 0.50s                 | 327             |
| 128 MB        | 4530               | 14.41s              | 1.88s                 | 594             |


## Dependencies

| Tool/Library    | Usage        | Homepage                                             | Licence                                |
| --------------- | ------------ | ---------------------------------------------------  | -------------------------------------- |
| NRules          | included     | https://github.com/NRules/NRules                     | MIT Licence                            |
| TShark          | standalone   | https://www.wireshark.org/docs/man-pages/tshark.html | GNU GPL                                |
| SharpPcap       | included     | https://github.com/chmorgan/sharppcap                | GNU Lesser General Public License v3.0 | 
| PacketDotNet    | included     | https://github.com/chmorgan/packetnet                | GNU Lesser General Public License v3.0 | 
| Newtonsoft.Json | included     | https://www.newtonsoft.com/json                      | MIT Licence                            |
| NLog            | included     | https://nlog-project.org/                            | BSD license                            | 
