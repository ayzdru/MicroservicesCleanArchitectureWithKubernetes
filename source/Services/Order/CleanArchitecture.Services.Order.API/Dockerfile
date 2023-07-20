FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p Password1
# Restore as distinct layers
RUN dotnet restore /App/Services/Order/CleanArchitecture.Services.Order.API
# Build and publish a release
RUN dotnet publish /App/Services/Order/CleanArchitecture.Services.Order.API -c Release -o /App/Services/Order/CleanArchitecture.Services.Order.API/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /https/* /https/
COPY --from=build-env /App/Services/Order/CleanArchitecture.Services.Order.API/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Services.Order.API.dll"]