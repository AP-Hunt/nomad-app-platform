## Build
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build

WORKDIR /source

COPY *.sln .
COPY Api.Config/*.fsproj ./Api.Config/
COPY Api.Domain/*.fsproj ./Api.Domain/
COPY Api.Domain.Test/*.fsproj ./Api.Domain.Test/
COPY Api.Domain.Persistence/*.fsproj ./Api.Domain.Persistence/
COPY Api.Web/*.fsproj ./Api.Web/
COPY Api.Web.Test/*.fsproj ./Api.Web.Test/
COPY Api.Worker/*.fsproj ./Api.Worker/

RUN dotnet restore --force

COPY . .
RUN dotnet publish -c release -o /app

## Run
FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS run
WORKDIR /app

COPY --from=build /app .
ENTRYPOINT ["/app/Api.Web", "/app/config.json"]
