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