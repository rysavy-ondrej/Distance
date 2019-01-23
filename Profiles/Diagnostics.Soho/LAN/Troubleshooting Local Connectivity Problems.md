# Troubleshooting Local Connectivity Problems

This document provides demonstration of the DISTANCE Diagnostic Tool for troubleshooting local connectivity problems. 
Such problems include:

* Configuration problem - the (static) IP configuration of a device is incorrect. 

* DHCP or BOOTP issue - the system of dynamic address assignment is not working correctly.

* Physical layer issue - there may be different reasons, for instance, cable is faulty or improperly connected,
  wiring closet cross-connect is faulty or improperly connected, or hardware (interface or port) is faulty.

* Duplicate IP address - the device has the same IP address as an another device within the network.


In the rest of this document, we will show how to create diagnostic procedures for three of four presented problems.

## Detecting Duplicate IP Address
The duplicate address problem is observable in the network communication. If two devices have been configured with the same IP address 
we can see packets sourced from the single IP address but having different hardware addresses. To test we first generates objects 
representing IP-MAC address mapping:

```yaml
derived:
  - name: AddressMapping
    description: Carries information on mapping between ip and mac addresses
    fields:       
      - string ip.addr
      - string eth.addr

RULE
```

Then it is easy to check if there are two MAC addresses associated with a single IP address:

```yaml
events:    
  - name: DuplicateAddressDetected
    severity:     error
    description:  "Duplicate IP address assignment detected. Two or more MAC addresses share the same IP address."
    message:      "Two or more network hosts has assigned the same network address {ip.address}."
    fields:
      - string    ip.address
      - string[]  eth.addresses

RULE
```


## Detecting Configuration Problems
Now, we consider the case when there is a host machine with incorrect static IP configuration.
Incorrect configuration can be because:

* No IP address is configured, the host tries DHCP and if this fails it generates a link-local address (169.254.0.0/16).
* IP address which does not belong to the local LAN address space is configured. For instance, LAN uses 192.168.1.0/24, but the host has configured 10.10.10.121/24.
* The host has correct IP address but the mask is incorrect.
* The host has an incorrect gateway's IP address.  

### Identifying gateway
To detect configuration problems it may be useful to identify IP address of the local gateway. 
Considering that the network contains nodes properly configured, we may try to analyze their communication 
to detect the hardware address of the gateway. The gateway is used to communcate outside the local network. 
It means that packets supposed to be sent outside the local network are forwarded to the gateway router. Thus, 
we may see packets with different destination IP addresses being forwared to a single hardware address. 


## Detecting DHCP Issues


## Reference 
* https://www.cisco.com/en/US/docs/internetworking/troubleshooting/guide/tr1907.html#wp1021095
