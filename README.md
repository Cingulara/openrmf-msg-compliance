# openrmf-msg-controls
Messaging service to respond to internal API requests to receive a list of controls. This is all using a NATS Request/Reply scenario. This uses 
an XML file to load an internal memory database of sorts to query and receive data from. Update the database = update the 
XML file and redeploy.

* openrmf.controls

## Running the NATS docker images
* docker run --rm --name nats-main -p 4222:4222 -p 6222:6222 -p 8222:8222 nats:2.1.2-linux
* this is the default and lets you run a NATS server version 2.x (as of 12/2019)
* just runs in memory and no streaming (that is separate)

## What is required
* .NET Core 2.x
* running `dotnet add package NATS.Client` to add the package
* dotnet restore to pull in all required libraries
* The C# NATS client library available at https://github.com/nats-io/csharp-nats

## Making your local Docker image
* make build
* make latest
