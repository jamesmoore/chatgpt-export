FROM mcr.microsoft.com/dotnet/sdk:latest AS build-env
ARG VERSION
LABEL version=$VERSION
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore

# Build and publish a release
RUN dotnet publish ./ChatGPTExport -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ChatGPTExport.dll"]