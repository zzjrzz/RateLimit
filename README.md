# Rate Limit
[![Build Status](https://travis-ci.com/zzjrzz/RateLimit.svg?branch=master)](https://travis-ci.com/zzjrzz/RateLimit)

## Prerequisites
1. [Docker](https://www.docker.com/)
2. xUnit
3. .NET Core 3.1

## Steps To Run
1. `dotnet dev-certs https --trust`
2. `docker-compose build`
3. `docker-compose up`
4. Navigate to rate limited API call (http://localhost:8000/api/account)
5. Navigate to Swagger (http://localhost:8000/swagger/index.html)

## Configuration
To change the settings on the maximum requests per interval, go to `appsettings.json` and change the values,
where `MaximumTries` is an Integer, `Interval` is a Timestamp `Endpoints` is an array of string routes for endpoints to rate limit.
```
"RateLimitOptions": {
    "MaximumTries": 100,
    "Interval": "01:00:00",
    "Endpoints": ["/api/account"]
},
```
## Assumptions
- In-memory cache will be available therefore no fallback option has been implemented. Ideally this can be swapped out for a database store.
- Cache use is limited and should not exceed normal memory needs therefore no limiting on size is implemented.
- Client-specific limiting quotas are implemented through IP address strategy. It should also be simple to extended for User Session / Auth-based rate limiting.
- Mixing different limiting strategies and rules for different endpoints not implemented.
