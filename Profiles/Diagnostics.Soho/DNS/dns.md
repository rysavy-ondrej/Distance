# DNS
## Basic cases
DNS is a simple request response protocol. The detectable errors are related to delayed,  missing or error responses. For a query we can identify the four possible basic cases:

* Ok Response: no error ocurred, response exists and has zero error code
* Late Response: the reply is delayed by a significant amount of time
* Error(code) Response: the reply has non zero error code, which signalize variety of DNS errors.
* None Response: no reply sent by the server to the client's query 

For error response, we distinguish subcases depending on the error code in the response:
| Code                  | Description                   |
| --------------------- | ----------------------------- |
| ERR_NAME_NOT_RESOLVED | Domain name does not exist.   |
| . . .                 | . . .                         | 

## Scope
The scope of the error may be one of the following:

* Query Scope: the problem occurs to the specific query only
* Client Scope: the problem occurs to the specific client only 
* Server Scope: the problem occurs to the specific server only


## Quantifier
Also, each case has also a quantifier:
* All: the problem occurs in all instances of the case 
* Some: the problem occurs only in some instances of the case

## Error Matrix
We can define an error matrix for `late`, `error` and `none` scenarios:

| NONE | Query | Client | Server  |
| ---- | ----- | ------ | ------- |
| All  |   E1  |   E2   |   E3    |
| Some |   E4  |   E5   |   E6    |

* E1 - all queries Q have none response
* E2 - all queries by client C have none response
* E3 - all queries to server S have none response
* E4 - some queries Q have not a response
* E5 - some queries by client C have not a response
* E6 - some queries to server S have not a response

The errors are not independent. Obviously E3 subsumes all other errors. 

For some quantifier, there are other possible combinations, for instance:

*Some queries Q have late responses while some have not any response.* 

## Permanent vs. transient errors
Some problems may be transient, which means that they disappear later.  