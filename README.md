# SDC440 RSVP Project

This repo contains TWO separate projects:

## 1) RSVPApp (MAUI Client)
Path: `RSVPApp/`
Cross-platform MAUI app (MacCatalyst/iOS/Android).

## 2) RSVPApi (ASP.NET Core Web API)
Path: `RSVPApi/`
Minimal API backend used by RSVPApp.

### Run API
dotnet run --project RSVPApi/RSVPApi.csproj --urls "http://0.0.0.0:5049"

### Swagger
http://localhost:5049/swagger
(or your LAN IP: http://192.168.x.x:5049/swagger)
