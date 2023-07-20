FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p Password1
# Restore as distinct layers
RUN dotnet restore /App/ApiGateways/CleanArchitecture.ApiGateways.Web
# Build and publish a release
RUN dotnet publish /App/ApiGateways/CleanArchitecture.ApiGateways.Web -c Release -o /App/ApiGateways/CleanArchitecture.ApiGateways.Web/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /https/* /https/
COPY --from=build-env /App/ApiGateways/CleanArchitecture.ApiGateways.Web/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.ApiGateways.Web.dll"]