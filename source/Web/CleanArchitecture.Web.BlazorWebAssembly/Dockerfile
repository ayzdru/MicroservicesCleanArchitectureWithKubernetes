FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore /App/Server/
# Build and publish a release
RUN dotnet publish /App/Server/ -c Release -o /App/Server/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App/Server
COPY --from=build-env /App/Server/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Web.BlazorWebAssembly.Server.dll"]