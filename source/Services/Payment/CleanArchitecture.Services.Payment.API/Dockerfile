FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore /App/Services/Payment/CleanArchitecture.Services.Payment.API
# Build and publish a release
RUN dotnet publish /App/Services/Payment/CleanArchitecture.Services.Payment.API -c Release -o /App/Services/Payment/CleanArchitecture.Services.Payment.API/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/Services/Payment/CleanArchitecture.Services.Payment.API/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Services.Payment.API.dll"]