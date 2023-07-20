FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./

RUN dotnet dev-certs https -ep /https/aspnetapp.pfx -p Password1

# Restore as distinct layers
RUN dotnet restore /App/Services/Identity/CleanArchitecture.Services.Identity.API
# Build and publish a release
RUN dotnet publish /App/Services/Identity/CleanArchitecture.Services.Identity.API -c Release -o /App/Services/Identity/CleanArchitecture.Services.Identity.API/Out



# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /https/* /https/
COPY --from=build-env /App/Services/Identity/CleanArchitecture.Services.Identity.API/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Services.Identity.API.dll"]