# IPv6 Troubleshooting
This document deals with IPv6 troubleshooting based on the analysis of capture file.

## Address Validation
Correct address assignment is important for the propoer function of IPv6 deployment. 

*IPv6-capable nodes follow a process of default address selection (RFC 3484). If there is something wrong with the prefix policy within the operating system it could cause mysterious behavior. This could affect either source address selection or destination address selection. On a Microsoft system we can use the "netsh interface ipv6 show prefixpolicies" command to view the policy table. On a BSD system we can use the ip6addrctl command and on a Solaris system we can use the ipaddrsel command to view the policy table.* [https://www.networkworld.com/article/2229298/cisco-subnet/cisco-subnet-troubleshooting-ipv6-networks-and-systems.html]


## ICMPv6
The ICMPv6 is a set of protocols that control IPv6 communication. Analysis of the ICMPv6 messages
is the fruitful source of information.

### NDP
The Neigbor Discovery Protocol serves for advertising the information necessary for LAN communication, mainly mapping IPv6 addresses to local addresses. 
 
 *Because IPv6 doesn't use broadcast, the NDP ICMPv6 messages use multicast to map Layer-2 addresses (MAC Addresses) to IPv6 addresses. We can use ping to verify IPv6 connectivity to the other nodes on a LAN and then check the neighbor cache (like the IPv4 ARP cache). On a Windows host we can use the command "netsh interface ipv6 show neighbors". On a Linux system we can use the "ip neighbor show" command. On a BSD system the command is "ndp -a" and on a Solaris system the command "netstat -p -f inet6" will show you its neighbor cache. On both a Cisco router and a Juniper router, the command is "show ipv6 neighbors".*
 [https://www.networkworld.com/article/2229298/cisco-subnet/cisco-subnet-troubleshooting-ipv6-networks-and-systems.html]

Some facts about NDP:

* There are five NDP messages: RouterSolicitation, RouterAdvertisement, NeighborSolicitation, NeighborAdvertisement, Redirection.
* Requests are sent to IPv6 multicast address `FF02::1:FFxx:xxxx`. This is mapped to LAN multicast `3333:FFxx:xxxx`.
* Replies are unicasted.

Because the traffic is sent to the specific Ethernet multicast address it is usally handled as a broadcast on the common switches. 
However, if switches are capable of doing MLD snooping they can avoid broadcast. However, for a large number of hosts, switches may have 
a problem as the number of multicasts groups roughly equals to the number of hosts!

#### Messages:

* Router advertisement (RA)—Messages sent to announce the presence of the router, advertise prefixes, assist in address configuration, and share other link information such as MTU size and hop limit. 
* Router solicitation (RS)—Messages sent by IPv6 nodes when they come online to solicit immediate router advertisements from the router. 
* Neighbor solicitation (NS)—Messages used for duplicate address detection and to test reachability of neighbors. A host can verify that its address is unique by sending a neighbor solicitation message destined to the new address.
* Neighbor advertisement (NA)—Messages used for duplicate address detection and to test reachability of neighbors. Neighbor advertisements are sent in response to neighbor solicitation messages.

#### IPv6 Verification

1. Checking the address assignmement - static, SLAAC, DHCPv6. 
Do we have only local address or also global address configuration. Is it valid? 
2. Can the node communicate with router?
3. Check the connectivity to DNS resolver. Is it using IPv6 or IPv4? 
4. Are there any AAAA records in DNS?
5. Is it possible to perform IPv6 communication for the retrieved AAAA records?
#### Possible problems:

1. Sending NS but missing NA - check if this is not part of address assginment where the NS is used to verify the uniqueness of the selected address. 
2. Analyze RA to see whether the router suggests to use SLAAC or DHCP. Check the client's reaction.
3. 



#### References:

* https://www.networkworld.com/article/2229298/cisco-subnet/cisco-subnet-troubleshooting-ipv6-networks-and-systems.html

### IPv6 Path MTU Discovery

*Another problem that could be encountered on dual-protocol networks is links with reduced Maximum Transmission Unit (MTU) size. This can happen if the IPv6 packets have encountered a tunnel and the tunnel overhead has reduced the MTU size. If the IPv6 packets are placed inside a 6in4 tunnel within IPv4 Protocol 41 packets then the MTU size will be reduced by 20 bytes (the IPv4 header size). Because IPv6 routers do not perform fragmentation it is required that the router drop the IPv6 packet and send back an ICMPv6 Packet-Too-Big message indicating the preferred MTU size. The IPv6-capable source must then perform Path MTU Discovery (PMTUD) and then fragment the packet into the proper size. Using ping with various packet sizes can reveal if there is an MTU size reduction along the traffic path. You can perform a "ping -l 1500 2001:DB8:DEAD:C0DE::1" and then verify the ICMPv6 packet too big response with the embedded ideal packet size.*[https://www.networkworld.com/article/2229298/cisco-subnet/cisco-subnet-troubleshooting-ipv6-networks-and-systems.html]

## DHCPv6
