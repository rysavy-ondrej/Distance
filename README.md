# Distance

An experimental implementation of a rule-based network troubleshooting tool. It uses NRules (https://github.com/NRules) engine.

## Usage:

DistanceEngine project is a .NET Core console application that do all the things:

```bash
$ dotnet DistanceEngine.dll [SOURCE-PCAP]
```

The program loads the input pcap, executes `tshark` for decoding packets and applies rules from `DistanceRules`. The output containing 
identified issues is written to `.log` file.
