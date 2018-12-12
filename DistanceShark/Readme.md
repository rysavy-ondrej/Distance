# Ndx.TShark
This library enables to use TSHARK from NDX framework.

## Usage
The following snippets demonstrate the basic usage scenarios for TShark protocol decoder.
Thera are two supported options to deploy TShark packet processor:
* LINQ style approach - in this way the TSharp processor transforms input packets to output data.
* Dataflow TPL approach - in this way the Microsoft TPL Dataflow model is used to transform input data to output decoded information.

Both methods start with the common step:

1. Create TShark decoder process by specifying a set of fields to extract from each frame:
```csharp
var fields = new[] {
  // fields for http request
  "http.request.method", "http.request.uri", "http.request.version", "http.host",
  "http.user_agent", "http.accept", "http.accept_language", "http.accept_encoding",
  "http.connection", "http.referer", "http.request.full_uri", "http.request_number",
  // fields for http response
  "http.response.code", "http.response.code.desc",  "http.content_type",
  "http.content_encoding", "http.server",  "http.content_length",
  "http.date", "http.response_number",
  "dns.a", "dns.cname", "dns.id", "dns.ns",
};
var tsharkProcess = new TSharkFieldDecoderProcess(fields);
```

Alternatively, it is possible to let TShark provide relevant fields for the protocol:
```csharp
var tsharkProcess = new TSharkProtocolDecoderProcess(new [] { "http" });
```

### Enumerable
2. Use decoder process in LINQ expression to decode the collection of frames:
```csharp
var frames = PcapReader.ReadFile(@"C:\Temp\NAS-SSH-154.0.166.83.cap");
var packets = frames.Take(1000).Decode(tsharkProcess).Where(x=>x.FrameProtocols.Contains("http"));
foreach (var packet in packets)
{
    Console.WriteLine(packet);
}
```
In this example, only first 1000 frames are decoded and the output is filtered to 
obtain only HTTP packets. Packet decoding is performed by ```Decode``` operation
which is defined as extension method on ```IEnumerable<RawFrame>``` type.
### Dataflow

2. Create Dataflow block that process frames and produces output. Use ```ActionBlock```
to process extracted data from packets. Here, the extracted content is just printed.
```csharp
var tsharkBlock = new TSharkBlock(tsharkProcess);
var consumer = new ActionBlock<PacketFields>( p => Console.WriteLine(p) );
tsharkBlock.LinkTo(consumer, new DataflowLinkOptions() { PropagateCompletion = true });
```

3. Pupm frames to processing engine, for instance by reading frames from the PCAP file.
```csharp
var frames = Captures.PcapReader.ReadFile(path);
tsharkBlock.Consume(frames);
consumer.Completion.Wait();
```

## TShark Notes
TShark can be used to provide parsed information in form of line delimited JSON 
output as follows:
```
tshark -r source.pcap -T ek
```

### Decode using specified protocol parser

```-d <layer type>==<selector>,<decode-as protocol>```

This switch can be applied if the specific decoder should be used to parse the conversation content.
For example: ```-d tcp.port==8888,http``` will decode any traffic running over TCP port 8888 as HTTP.

### Specifying output fields
By default, TShark parses each frame and provide all available information. 
When only specific fields are in the interest, the output can specified using -e flag:

```-e <field>```

Add a field to the list of fields to display if -T fields is selected. This option can be used multiple times on the command line.
For example, to print relevant information from HTTP requests:
* ```http.request.method```
* ```http.request.uri```
* ```http.request.version```
* ```http.host```
* ```http.user_agent```
* ```http.accept```
* ```http.accept_language```
* ```http.accept_encoding```
* ```http.connection```
* ```http.referer```
* ```http.request.full_uri```
Similarly, information from HTTP response may be obtained from the following fields:
* ```http.response.code```
* ```http.content_type```
* ```http.content_encoding```
* ```http.server```
* ```http.cache_control```
* ```http.content_length```
* ```http.date```
* ```http.file_data```


### Packet sources for TSHARK
#### STDIN
It is possible to run TSHARK in a mode that it reads input PCAP from STDIN:

```
tshark -r - 
```
#### NamedPipes
It is also possible to send data to TSHARK using named pipes.

```
tshark -i \\.\pipe\tsharkpipe
```
