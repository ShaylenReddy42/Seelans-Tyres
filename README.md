# Seelan's Tyres

## Badges

[![Build Status](https://dev.azure.com/Shaylen/Personal/_apis/build/status/ShaylenReddy42.Seelans-Tyres?branchName=master)](https://dev.azure.com/Shaylen/Personal/_build/latest?definitionId=5&branchName=master)

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-black.svg)](https://sonarcloud.io/summary/new_code?id=ShaylenReddy42_Seelans-Tyres)

## What's the purpose of this project?

This project is a rewrite of my [original college project](https://bitbucket.org/Shaylen/seelans-tyres/src/master/) which, compared to this, was very poorly written and insecure

Originally it was written with PHP as the backend, this is written using ASP.NET Core 6

It's also rewritten to utilize my Azure and DevOps skills since I had earned Microsoft's Azure Certifications and needed a way to prove my skills with a project

As of right now, the solution is not cloud-native yet

Since this project is a proof-of-concept, the payment system isn't integrated

## Architecture

### Where it started

In the beginning, it was a distributed monolith with a frontend and backend API with a single database shared between both the projects

### Where it is

The API has been broken down into microservices which then invited a backend-for-frontend to have a single point of entry into the microservices from the frontend

So far, the solution comprises of 6 projects and isn't even done

### Where it's going

The solution looks complete but it isn't, data synchronization between the microservices hasn't been implemented yet

To solve this, a messaging service will come into play and invite workers into the solution, restructuring the entire solution

The workers will listen in on dedicated queues for updates and update their copy of the data

## Features

* Utilizes some functionality of ASP.NET Core Identity
* Email service that sends emails with FluentEmail using embedded razor templates
  * Emails sent:
    * A receipt when a user completes an order
    * Sends the user a token to reset their password
* JWT Bearer Authentication for the api
* The api makes use of AutoMapper to map between database entities and models returned to the client
* The api utilizes the repository pattern to access the database
* The application contains an admin portal protected by ASP.NET Core Identity. This, however, may be removed into its own project because it isn't a good idea to use the same login for customers and admins even if it is virtually impossible for a customer to access it since it would require them to have an Administrator role which is never given to a customer
* Secrets Management
* Versioning with Git

## Resources that helped me skill up to rewrite this project

* [Building a Web App with ASP.NET Core 5, MVC, Entity Framework Core, Bootstrap, and Angular](https://www.pluralsight.com/courses/aspnetcore-mvc-efcore-bootstrap-angular-web)
  * Author: [Shawn Wildermuth](https://app.pluralsight.com/profile/author/shawn-wildermuth)
* [ASP.NET Core 6 Web API Fundamentals](https://www.pluralsight.com/courses/asp-dot-net-core-6-web-api-fundamentals)
  * Author: [Kevin Dockx](https://app.pluralsight.com/profile/author/kevin-dockx)
* [ASP.NET Core 6 Fundamentals](https://www.pluralsight.com/courses/asp-dot-net-core-6-fundamentals)
  * Author: [Gill Cleeren](https://app.pluralsight.com/profile/author/gill-cleeren)
* [ASP.NET Microservices [Path]](https://www.pluralsight.com/paths/net-microservices)
  * Authors:
    * [Antonio Goncalves](https://www.pluralsight.com/authors/antonio-goncalves)
    * [Roland Guijt](https://www.pluralsight.com/authors/roland-guijt)
    * [Gill Cleeren](https://www.pluralsight.com/authors/gill-cleeren)
    * [Neil Morrissey](https://www.pluralsight.com/authors/neil-morrissey)
    * [Kevin Dockx](https://www.pluralsight.com/authors/kevin-dockx)
    * [Mark Heath](https://www.pluralsight.com/authors/mark-heath)
    * [Marcel de Vries](https://www.pluralsight.com/authors/marcel-devries)
    * [Steve Gordan](https://www.pluralsight.com/authors/steve-gordon)
    * [Rag Dhiman](https://www.pluralsight.com/authors/rag-dhiman)
* [Your Microservices Transition](https://app.pluralsight.com/courses/your-microservices-transition)
  * Author: [Rag Dhiman](https://www.pluralsight.com/authors/rag-dhiman)
* [Microservices Architecture: The Design Principles](https://app.pluralsight.com/courses/microservices-design-principles)
  * Author: [Rag Dhiman](https://www.pluralsight.com/authors/rag-dhiman)
* [Securing ASP.NET Core 3 with OAuth2 and OpenID Connect](https://www.pluralsight.com/courses/securing-aspnet-core-3-oauth2-openid-connect)
  * Author: [Kevin Dockx](https://app.pluralsight.com/profile/author/kevin-dockx)
* [Cryptography in .NET 6](https://www.pluralsight.com/courses/dot-net-6-cryptography)
  * Author: [Stephen Haunts](https://app.pluralsight.com/profile/author/stephen-haunts)
* [Logging and Monitoring in ASP.NET Core 6](https://www.pluralsight.com/courses/logging-monitoring-aspdotnet-core-6)
  * Author: [Erik Dahl](https://app.pluralsight.com/profile/author/erik-dahl)
* [Sending Email in C# using FluentEmail](https://www.youtube.com/watch?v=qSeO9886nRM)
  * Author: [IAmTimCorey](https://www.youtube.com/user/IAmTimCorey)
* [Microsoft Docs](https://docs.microsoft.com/en-us/)
* [Stack Overflow](https://stackoverflow.com/) [Obviously]

## Required local setup to build and run

* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
* [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 6.0.400
* [CMake](https://cmake.org/download/) 3.21.4 or later
* [An Instance of SQL Server 2019 Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
* [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Build instructions

* NOTE: If you use `build-with-docker.cmd`, I recommend you pull these images beforehand:
  * [elasticsearch:7.17.6](https://hub.docker.com/_/elasticsearch/)
  * [kibana:7.17.6](https://hub.docker.com/_/kibana/)
  * [mcr.microsoft.com/dotnet/aspnet:6.0](https://mcr.microsoft.com/en-us/product/dotnet/aspnet/about)
  * [mcr.microsoft.com/dotnet/sdk:6.0](https://mcr.microsoft.com/en-us/product/dotnet/sdk/about)
  * [mcr.microsoft.com/mssql/server:2019-latest](https://hub.docker.com/_/microsoft-mssql-server)
* If you use `build-with-loggingsinks.cmd`, pull only these images beforehand:
  * [elasticsearch:7.17.6](https://hub.docker.com/_/elasticsearch/)
  * [kibana:7.17.6](https://hub.docker.com/_/kibana/)

* Ensure to set environment variables listed in the `RequiredEnvironmentVariables.txt` file
  * Press the Win Key and search for `Edit environment variables for your account`
  * When setting this for the first time, restart your computer
* All build scripts are in the `scripts` folder
* In order to build, I've provided three options for you:
  * `build.cmd` which is the regular build
  * `build-with-docker.cmd` just to see what it looks like orchestrated with docker compose
  * `build-with-loggingsinks.cmd` which is the regular build + elasticsearch and kibana orchestrated with docker compose

* Once you run either `build.cmd` or `build-with-loggingsinks.cmd`, CMake will generate `run-all.cmd`
* Thereafter, run `run-all.cmd` which will start all applications minimized and launch the site, simulating orchestration

## Demos

### Setting up Kibana and demonstrating distributed tracing

[![Kibana Thumbnail](docs/demos/kibana-thumbnail.png)](https://drive.google.com/file/d/1aVM_LosrVu2S6ZnN9Vc6IdDNI87_28la/view "Setting Up Kibana and Demonstrating Distributed Tracing")