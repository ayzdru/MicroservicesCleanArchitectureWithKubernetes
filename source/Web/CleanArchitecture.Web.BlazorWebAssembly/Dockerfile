FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore /App/Web/CleanArchitecture.Web.BlazorWebAssembly
# Build and publish a release
RUN dotnet publish /App/Web/CleanArchitecture.Web.BlazorWebAssembly/ -c Release -o /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App/Web/CleanArchitecture.Web.BlazorWebAssembly
COPY --from=build-env /App/Web/CleanArchitecture.Web.BlazorWebAssembly/Out .
ENTRYPOINT ["dotnet", "CleanArchitecture.Web.BlazorWebAssembly.Server.dll"]