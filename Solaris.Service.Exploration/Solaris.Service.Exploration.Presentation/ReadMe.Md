# Solaris.Web.CrewApi

## Description

**Solaris.Services.Exploration** is the back-end Service which would normally process data from the robots exploration,
but in this case it will mock their data and mimic their behavior. The service will send back the data it could extract from
the planet and also update the robots state.

## Used Technologies

- .Net Core Console App
- RabbitMQ Client
- NLog logging


## Installing
Hosting machine requires:
- [.Net Core v3.1] (https://dotnet.microsoft.com/download).
- Install an RabbitMQ server

      
RabbitMQ Configurations
In order to support the background running tasks that send messages to RabbitMQ, we need to install it on the local environment, it should be already configured for the other environments (staging/integration).
You can install it and configure it by following these steps:
   - Download & Install RabbitMQ server from  https://www.rabbitmq.com/install-windows.html (This will also install Erlang)
   - Open RabbitMQ Command Prompt and run the following command: ```rabbitmq-plugins enable rabbitmq_management```. This will enable the UI management tools.
   - Download the x-delayed-message RabbitMQ plugin from: http://www.rabbitmq.com/community-plugins.html , unpack it into ```Drive:\{{Instal_Dir}}\plugins```
   - Access the http://localhost:15672 with the default user.

      
## Deployment
Mandatory Environment variables are:
- APPSETTINGS__RABBITMQ__HOST 
- APPSETTINGS__RABBITMQ__PASSWORD
- APPSETTINGS__RABBITMQ__PORT
- APPSETTINGS__RABBITMQ__USERNAME

Other optional Env Variables can be added to override the fields in the appSettings.json file.

## Input and Output
The service will send back through RabbitMQ responses to the other microservices.

## Versioning

GitHub for versioning.