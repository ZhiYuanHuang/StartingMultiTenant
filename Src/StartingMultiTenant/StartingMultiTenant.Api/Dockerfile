#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["StartingMultiTenant.Api/StartingMultiTenant.Api.csproj", "StartingMultiTenant.Api/"]
RUN dotnet restore "StartingMultiTenant.Api/StartingMultiTenant.Api.csproj"
COPY . .
WORKDIR "/src/StartingMultiTenant.Api"
RUN dotnet build "StartingMultiTenant.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StartingMultiTenant.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StartingMultiTenant.Api.dll"]