#Command and Control Server-Client Prototype
This project is a prototype of client-server application that does these things:

##Server
1. Send commands to Client via websocket
2. Receive command results from Client
3. Save client data to DB (EF Core)
4. Register,Login using Microsoft Identity for viewing client data
5. Show data on asp.net website

##Client
1. Receive commands from server
2. Execute it in predefined shell
3. Send result to server



