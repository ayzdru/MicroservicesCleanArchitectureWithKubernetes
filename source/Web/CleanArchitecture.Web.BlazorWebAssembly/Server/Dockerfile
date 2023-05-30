FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Server
# Build and publish a release
RUN dotnet publish /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Server/ -c Release -o /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Server/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Server
COPY --from=build-env /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Server/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Web.BlazorWebAssembly.Server.dll"]