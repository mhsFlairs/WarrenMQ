# Usage
- Check WarrenMQ.Console and WarrenMQ.Api for example
# Configuration

## appsettings.json
```json
"RabbitMQ": {
"HostName": "localhost",
"UserName": "guest",
"Password": "guest",
"Port": 5672,
"VirtualHost": "/",
"ChannelPoolSize": 10
}
```

## Program.cs

- Add `builder.Services.AddRabbitMQ(builder.Configuration);`

# RabbitMQConfig

You can extend the RabbitMQConfig class to add more configurations

```csharp
public class ExtendedRabbitMQConfig : RabbitMQConfig
{
public string Exchange { get; set; }
}
```



