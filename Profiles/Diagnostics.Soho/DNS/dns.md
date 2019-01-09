# DNS
DNS is a simple request response protocol. The detectable errors are related to missing or error responses. For a query we can identify the four possible scenarios:

* Ok Response: no error ocurred
* Late Response: the reply is delayed by a significant amount of time
* Error Response: the reply has non zero error code, which signalize variety of DNS errors.
* None Response: no reply received by the client


For Late, Error and None situations, we try to identify the *scope* of the problem. The scope may be the combination of the following:

* Query scope: the problem occurs to the specific query
* Client scope: the problem occurs to the specific client
* Server scope: the problem occurs to the specific server

Thus, for query `q`, client `c` and server `s` we may identify the problem `p`:
```
p(q,c,s)
```

For example:  
```
NoneResponse(q1,192.168.5.122,209.85.51.222) where q1  { n8na.akamai.net,  }.
```


## None Response

* Server does not reply to the specific query.
* Server does not reply to the specific client(s). 
* Server is unreachable.