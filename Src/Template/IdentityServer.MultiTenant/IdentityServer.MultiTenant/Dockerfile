#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["IdentityServer.MultiTenant/IdentityServer.MultiTenant.csproj", "IdentityServer.MultiTenant/"]
COPY ["../../../../StartingMultiTenant.SDK/src/dotnet/StartingMultiTenantLib/StartingMultiTenantLib/StartingMultiTenantLib.csproj", "../../../../StartingMultiTenant.SDK/src/dotnet/StartingMultiTenantLib/StartingMultiTenantLib/"]
RUN dotnet restore "IdentityServer.MultiTenant/IdentityServer.MultiTenant.csproj"
COPY . .
WORKDIR "/src/IdentityServer.MultiTenant"
RUN dotnet build "IdentityServer.MultiTenant.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IdentityServer.MultiTenant.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityServer.MultiTenant.dll"]