# Overview
This library simplifies RabbitMQ usage by managing connections and channels efficiently, ensuring one connection per instance and channel pooling per thread, making it thread-safe for multi-threaded applications. Check out the [documentation](https://www.rabbitmq.com/docs/connections) for more information on RabbitMQ connections and channels and https://www.cloudamqp.com/blog/part1-rabbitmq-best-practice.html for best practices.

# Publishing
## Local Nuget Package
 - Clone the repository
 - Open CMD/PowerShell
 - Run `dotnet pack -c Release -o ./directory`

## Nuget
 - Clone the repository
 - Open CMD/PowerShell
 - Run `dotnet pack -c Release -o ./directory`
 - Run `nuget push ./directory/WarrenMQ.1.0.0.nupkg -Source https://api.nuget.org/v3/index.json -ApiKey <API_KEY>`

# Installation
## Direct Reference

- Clone the repository
- Open your solution in preferred IDE
- Reference the WarrenMQ project in your project
- Build the solution

## Local Nuget Package

- Follow the steps in the Publishing section to create a local nuget package
- Open your solution in preferred IDE
- Right click on the project you want to add the package to
- Click on Manage Nuget Packages
- Click on the gear icon on the top right
- Click on the + icon
- Click on the browse tab
- Click on the browse button
- Navigate to the directory where the nuget package was created
- Click on the package
- Click on the Add Package button
- Build the solution

## Nuget 

- Run `Install-Package WarrenMQ -Version 1.0.0` in the Nuget Package Manager Console


# Configuration

Refer to [ReadMe.md](WarrenMQ/ReadMe.md) 
