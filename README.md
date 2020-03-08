# Rate Limit

## Prerequisites
1. [Docker](https://www.docker.com/)

## Steps To Run
1. `dotnet dev-certs https --trust`
2. `docker-compose build`
3. `docker-compose up`
4. Navigate to Swagger (http://localhost:8000/swagger/index.html)

## Configuration
To change the settings on the maximum requests per interval, go to `appsettings.json` and change the values,
where `MaximumTries` is an Integer and `Interval` is a Timestamp.
```
"RateLimitOptions": {
    "MaximumTries": 100,
    "Interval": "01:00:00"
},
```
