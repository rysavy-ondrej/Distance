# Troubleshooting Local Connectivity Problems

This document provides a demonstration of the DISTANCE Diagnostic Tool for troubleshooting local connectivity problems. 
Such problems include:

* Duplicate IP address - the device has the same IP address as an another device within the network.

* Configuration problem - the (static) IP configuration of a device is incorrect - IP address outside the LAN, incorrect mask, gateway server, or DNS server. 

* DHCP or BOOTP issue - the system of dynamic address assignment is not working correctly.

* Physical layer issue - there may be different reasons, for instance, cable is faulty or improperly connected,
  wiring closet cross-connect is faulty or improperly connected, or hardware (interface or port) is faulty - can we duplicate this from packet traces?

* ARP related issues

* Broadcast or multicast flooding

* NetBIOS related errors - Bogus Duplicate Computer Name Error (https://redmondmag.com/articles/2017/02/10/troubleshoot-bogus-duplicate-computer-name-errors.aspx)


## Testing environment

To demonstrate the above-mentioned problems, the virtual testbed was created. The testbed represents a local network with a single gateway router that at the same time serves as DHCP and DNS servers. Different end-host machines are employed in the network:

| Name       | OS             | MAC address       | Note  |
| ---------- | -------------- | ----------------- | ----- |
| windows1   | Windows 7      | 00:0c:29:62:79:a2 |       |
| ubuntu2    | Ubuntu 18.04.2 | 00:0c:29:ac:af:38 |       |
| ubuntu     | Ubuntu 18.04.2 | 00:0c:29:14:fd:e0 | Runs n2disk packet sniffer. |

All machines run in ESXi 6.5 environment, To capture the traffic, the port group is configured in promiscuous mode. 



In the rest of this document, we will show how to create diagnostic procedures for three of four presented problems.


## Detecting Duplicate IP Address
The duplicate address problem is observable in network communication. If two devices have been configured with the same IP address we can see packets sourced from the single IP address but having different hardware addresses. To test we first generate objects 
representing IP-MAC address mapping:

```yaml
derived:
  - name: AddressMapping
    description: Carries information on mapping between ip and mac addresses
    fields:       
      - string ip.addr
      - string eth.addr
rule:
  - name: AddressMappingRule
    code: |
      IpPacket packet = null;
      When()
          .Match(() => packet); 
      Then()
          .Do(ctx => ctx.TryInsert(new AddressMapping { IpAddr = packet.IpSrc, EthAddr = packet.EthSrc }));
```

Then it is easy to check if there are two (or more) MAC addresses associated with a single IP address:

```yaml
events:    
  - name: DuplicateAddressDetected
    severity:     error
    description:  "Duplicate IP address assignment detected. Two or more MAC addresses share the same IP address."
    message:      "Two or more network hosts has assigned the same network address {ip.address}."
    fields:
      - string    ip.address
      - string[]  eth.addresses

rule:
  - name: DuplicateAddressDetectedRule
    code: |
      AddressMapping mapping = null;
      IEnumerable<AddressMapping> conflicts = null;
      When()
          .Match(() => mapping)
          .Query(() => conflicts,
              c => c.Match<AddressMapping>(
                    m => mapping.IpAddr == m.IpAddr,
                    m => mapping.EthAddr != m.EthAddr)
                    .Collect()
                    .Where(x => x.Any()));                   
      Then()
          .Yield(_ => new DuplicateAddressDetected { IpAddress = mapping.IpAddr, EthAddresses = conflicts.Select(c=>c.EthAddr).ToArray() });
```


## Detecting Configuration Problems
Now, we consider the case when there is a host machine with incorrect static IP configuration.
Incorrect configuration can be because:

* No IP address is configured, the host tries DHCP and if this fails it generates a link-local address (169.254.0.0/16).
* IP address which does not belong to the local LAN address space is configured. For instance, LAN uses 192.168.1.0/24, but the host has configured 10.10.10.121/24.
* The host has correct IP address but the mask is incorrect.
* The host has an incorrect gateway's IP address.  

### Identifying gateway
To detect configuration problems it may be useful to identify the IP address of the local gateway. 
Considering that the network contains nodes properly configured, we may try to analyze their communication to detect the hardware address of the gateway. 
The gateway is used to communicate outside the local network. 
It means that packets supposed to be sent outside the local network are forwarded to the gateway router. Thus, 
we may see packets with different destination IP addresses being forwarded to a single hardware address. 


## Detecting DHCP Issues

##  Physical layer issues


## ARP related issues



## Broadcast or multicast flooding


## Reference 
* https://www.cisco.com/en/US/docs/internetworking/troubleshooting/guide/tr1907.html#wp1021095
