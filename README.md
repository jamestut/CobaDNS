# Project CobaDNS
This .NET project act as a DNS client for both authoritative servers and recursive servers. This client cannot perform recursion by itself.

This project is deliberately made to be as simple as possible. As a result, many error-checking codes are deliberately omitted for a more understandable code and easier debugging.

This project was made as a media for learning exactly how DNS system and protocol works.

## Features
* Could ask the target server to perform recursion (RD flag).
* Can use both TCP and UDP transport.
* IPv4 address conversion to **in-addr.arpa** for reverse PTR  lookup.
* Specify resource type when querying (e.g. SOA, A, NS, MX, TXT, and others).
* Print server replies in both Answer Section, Authority Section, and Additional Section.
* Able to query authoritative servers and root servers. However, to query such servers, you have to perform the recursion yourself (e.g. by requesting the same resource on the server address returned by the prior query).

## Usage
To view program usage, simply run the program without any parameters, e.g.: `dotnet CobaDNS.dll`. The DNS server address supplied must be in IPv4 address. Entries in other address format, such as IPv6 or FQDN, are not supported.

Should the output is not verbose enough for your needs, you can view more elaborate data by debugging this program and inspecting its internal data structures.