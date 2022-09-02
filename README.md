# Seelan's Tyres

## Badges

[![Build Status](https://dev.azure.com/Shaylen/Personal/_apis/build/status/ShaylenReddy42.Seelans-Tyres?branchName=master)](https://dev.azure.com/Shaylen/Personal/_build/latest?definitionId=5&branchName=master)

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-black.svg)](https://sonarcloud.io/summary/new_code?id=ShaylenReddy42_Seelans-Tyres)

## What's the purpose of this project?

This project is a rewrite of my [original college project](https://bitbucket.org/Shaylen/seelans-tyres/src/master/) which, compared to this, was very poorly written and insecure. Originally it was written with PHP as the backend, this project is written using ASP.NET Core 6

This project is also rewritten to utilize my Azure and DevOps skills. As of right now, the solution is not cloud-native yet, it isn't even in its final architecture

This project is a proof of concept so the payment system isn't integrated

## Current Architecture

Currently, there's an MVC project that acts as the frontend with a WebApi for data access from a SQL Server database. Both projects utilize the same database. It's created using the code-first approach of Entity Framework Core. Right now, the whole solution has to be deployed as one, even though I added some error handling in the MVC project so it can work without the api, but it obviously cannot function well and there's other errors it will encounter regarding being unauthorized to use the api once it does come online

## Desired Architecture

The WebApi will be split into microservices that has its own databases using Domain-Driven Design. It seems that each controller in the api can be its own service. This will allow most of the site to work even if one service is down and be cloud-native with the site and its services hosted on Azure Kubernetes Services

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
* [Sending Email in C# using FluentEmail](https://www.youtube.com/watch?v=qSeO9886nRM)
  * Author: [IAmTimCorey](https://www.youtube.com/user/IAmTimCorey)
* [Microsoft Docs](https://docs.microsoft.com/en-us/)
* [Stack Overflow](https://stackoverflow.com/) [Obviously]

## Required local setup to build

* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/)
* [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) 6.0.400
* [CMake](https://cmake.org/download/) 3.21.4 or later
* [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)

## Build instructions

* Ensure to set environment variables listed in the `RequiredEnvironmentVariables.txt` file
  * Press the Win Key and search for `Edit environment variables for your account`
* Run `build.cmd` and it will create an ef core bundle to apply migrations, publish each runnable project and copies them to a publish dir at the root. Run each project and use the site however you like