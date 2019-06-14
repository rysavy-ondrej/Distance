# Troubleshooting Local Connectivity Problems

This document provides a demonstration of the DISTANCE Diagnostic Tool for troubleshooting local connectivity problems. 
Such problems include:

* Address configuration problem - the (static) IP configuration of a device is incorrect - IP address outside the LAN, incorrect mask, gateway server, or DNS server. 
  Also, the device can have the same IP address as an another device within the network. 

* DHCP or BOOTP issue - the system of dynamic address configuration is not working correctly.

* ARP related issues - address resolution protocol provides the mapping between network and link addresses which is crucial for correct network function.

* Broadcast or multicast flooding can cause some serious problem within the network as it may consume significant amount of bandwidth or host resources.

* NetBIOS related errors - Bogus Duplicate Computer Name Error (https://redmondmag.com/articles/2017/02/10/troubleshoot-bogus-duplicate-computer-name-errors.aspx)

* Physical layer issue - there may be different reasons, for instance, cable is faulty or improperly connected,
  wiring closet cross-connect is faulty or improperly connected, or hardware (interface or port) is faulty. The questions is whether capture file contains enough evidence to 
  identify such problems.

To demonstrate the above-mentioned problems and prepare datasets for analysis of possible evidence, the virtual testbed was created. 
Then each issue is analyzed and provided with its description and other information necessary to design a detection rule. Detection rules. 
Detection rules express knowledge gained during the analysis of issues done by an expert.  

## Testing environment

The testbed represents a local network with a single gateway router that at the same time serves as DHCP and DNS servers. Different end-host machines are employed in the network:

| Name       | OS             | Interface | MAC address       | Note  |
| ---------- | -------------- | ----------| ----------------- | ----- |
| windows1   | Windows 7      | Eth0      | 00:0c:29:62:79:a2 |       |
| windows2   | Windows 7      | Eth0      | 00:0c:29:ac:af:38 |       |
| ubuntu     | Ubuntu 18.04.2 | ens192    | 00:0c:29:14:fd:ea | Runs packet sniffer. |

All machines run in ESXi 6.5 environment, To capture the traffic, the port group is configured in promiscuous mode thus we can see entire local communication.  
Sniffing the traffic is done by the following command:
```
user@ubuntu1: sudo tcpdump -i ens192 -w <output-file>
```

## Issues

### Address configuration problems

| Id          | lan.1 |
| ----------- | --|
| Name        | Duplicate IP Address  |
| Description | If two devices have been configured with the same IP address we can see packets sourced from the single IP address but having different hardware addresses. |
| Example     | For instance, hosts with mac addresses aa:aa:aa:aa:aa:aa and bb:bb:bb:bb:bb:bb both use 192.168.1.113/24. |
| Evidence    | We can see IP packets that for the same source (destination) IP address contains different source (destination) ethernet addresses. |
| Event       | IpAddressConflict(string ip.address, string[] eth.addresses) |
| Pcap        | ip_conflict.pcap |
| Expected    | `ERROR|DISTANCE.EVTS|IpAddressConflict: Two or more network hosts has assigned the same network address 192.168.114.100: 00:0c:29:62:79:a2,00:0c:29:ac:af:38.`|
| Reference   | |


| Id          | lan.2 |
| ----------- | --|
| Name        | IP address mismatch  |
| Description | The IP address of a host does not belong to the local LAN address space configured. |
| Example     | For instance, LAN uses 192.168.1.0/24, but the host has configured 10.10.10.121/24. |
| Evidence    | We can see unanswered ARP requests as the host tries to find the network gateway. Also, the invalid IP address appears only as the source address in IP packets. |
| Event       | IpAddressMismatch(string ip.address, string neth.address) |
| Pcap        | ip_mismatch.pcap |
| Reference   | |

| Id          | lan.3 |
| ----------- | --|
| Name        | Link-local address in use  |
| Description | No IP address is configured, the host tries DHCP and if this fails it generates a link-local address (169.254.0.0/16). |
| Example     | Host uses one of the link-local address automatically assigned if dynamic configuration fails.|
| Event       | LocalAddressInUse(string ip.address, string eth.address) |
| Pcap        | local_ip_use.pcap |
| Reference   | |

| Id          | lan.4   |
| ----------- | -- |
| Name        | Invalid Network Mask   |
| Description | The host has correct IP address but the mask is incorrect. This may cause that some remote hosts are unreachable.  |
| Example     |    |
| Event       |    |
| Pcap        | invallid_mask.pcap   |
| Reference   |    |

| Id          | lan.5   |
| ----------- | -- |
| Name        | Invalid gateway address   |
| Description | The host has configured invalid or unreachable local gateway. The provided IP address for the gateway is not correct. |
| Example     | The correct gateway address is 192.168.99.1 but the host uses 192.168.99.254.   |
| Event       |    |
| Pcap        | invalid_gateway_address.pcap   |
| Reference   |    |


* The host has an incorrect gateway's IP address. 

### ARP Relate errors


### NetBIOS related errors

## Notes
To support rules certain information needs to be known, for instance, LAN prefix or correct gateway address. Such information can be either provided
by the administrator or in some cases it may be deduced from the capture file. 

### Guessing LAN prefix


### Identifying gateway
To detect configuration problems it may be useful to identify the IP address of the local gateway. 
Considering that the network contains nodes properly configured, we may try to analyze their communication to detect the hardware address of the gateway. 
The gateway is used to communicate outside the local network. 
It means that packets supposed to be sent outside the local network are forwarded to the gateway router. Thus, 
we may see packets with different destination IP addresses being forwarded to a single hardware address. 

## TEMPLATES


| Id          |    |
| ----------- | -- |
| Name        |    |
| Description |    |
| Example     |    |
| Event       |    |
| Pcap        |    |
| Reference   |    |

## Reference 
* https://www.cisco.com/en/US/docs/internetworking/troubleshooting/guide/tr1907.html#wp1021095
