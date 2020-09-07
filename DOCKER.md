# dtr.asseco.rs/asseco/offer

Based on microsoft/dotnet:2.2-aspnetcore-runtime-alpine image

## Description

The Dockerfile builds from "microsoft/dotnet:2.2-sdk" see https://hub.docker.com/r/microsoft/dotnet/

## Running inside Docker container

Use the following command in order to run Offer service in Docker container:

```
docker run --name offer -eDATABASE_HOST=172.16.89.235 -eDATABASE_TYPE=Postgres -eDATABASE_PORT=30172 -eDATABASE_USER=do -eDATABASE_NAME=digital_origination -eDATABASE_PASS=31d13d013d013 -eBROKER_HOST=172.16.89.235 -eBROKER_USER=admin -eBROKER_PASS=admin -eBROKER_PORT=30745 -eENVIRONMENT_NAME=dev7 -eTZ=Europe/Belgrade -eAUTH_URL=http://172.16.89.235:30807/v1/authentication -eAUTH_AUDIENCE=offer -eCLIENT_SECRET=secret -ePROXY_URL=http://172.16.89.235:30807 -eASPNETCORE_ENVIRONMENT=Development -eFORWARDED_CUSTOM_HEADERS=X-Asee-Task-ID -eDEBUG_BASE_HOST=172.16.89.235 -p8145:80 -d registry.asseco.rs/asseco/offer:latest-before-bundle
```

## Environment variables
- **ASPNETCORE_ENVIRONMENT** -> Default is: Production. If running container to Debug use: Development
- **DEBUG_BASE_HOST** -> If running in Debug specify this variable in order to forward all Http request to some environment instead of internal calls
- **DEBUG_BASE_PROTOCOL** -> Default: http. If environment that is used for sending Http requests is secured then use https. This variables is not case sensitive.
- **ENVIRONMENT_NAME** -> Specific for Kubernetes Deployment if defined calls to internal services are prefixed with Environment name
- **TZ** -> Allows specifing timezone for Container
- **CLIENT_SECRET** -> Secret in authentication service used for authenticating client: offer
- **FORWARDED_CUSTOM_HEADERS** -> Comma seperated list of Headers that are forwarded to Message Events as Pascal Case headers.
- **DATABASE_NAME** variable is used to define database name
- **DATABASE_USER** variable is used to define username for database
- **DATABASE_PASS** variable is used to define password for database. It supports secret usage by DATABASE_PASS_FILE
- **DATABASE_PORT** variable is used to define port for database
- **DATABASE_TYPE** variable is used to define type for database. Possibilities (Postgres, Oracle, MSSQL)
- **DATABASE_HOST** variable is used to define hostname for database
- **BROKER_USER** variable is used to define username for broker
- **BROKER_PASS** variable is used to define password for broker
- **BROKER_HOST** variable is used to define hostname for broker
- **AUTH_URL** variable is used to define URL to OPENID Connect authentication host
- **AUTH_AUDIENCE** variable is used to define authentication audience. Only tokens with specified Audience can reach the service
- **PROXY_URL** variable is used to define URL to proxy
