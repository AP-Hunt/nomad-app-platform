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

RUN apt-get update && apt-get install -y zip

RUN wget -q "https://github.com/buildpacks/pack/releases/download/v0.21.1/pack-v0.21.1-linux.tgz" && \
    tar -xzf pack-v0.21.1-linux.tgz && \
    mv ./pack /usr/local/bin/pack

RUN wget -q "https://releases.hashicorp.com/terraform/1.0.10/terraform_1.0.10_linux_amd64.zip" && \
    unzip terraform_1.0.10_linux_amd64.zip && \
    cp ./terraform /usr/local/bin/terraform

COPY . .
RUN dotnet publish -c release -o /app

## Run
FROM mcr.microsoft.com/dotnet/aspnet:5.0-focal AS run
WORKDIR /app

COPY --from=build /app .
COPY --from=build /usr/local/bin/* /usr/local/bin/

ENTRYPOINT ["/app/Api.Worker", "/app/config.json"]
