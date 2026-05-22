#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/CadentManagement.Migrator/CadentManagement.Migrator.csproj", "src/CadentManagement.Migrator/"]
COPY ["src/CadentManagement.EntityFrameworkCore/CadentManagement.EntityFrameworkCore.csproj", "src/CadentManagement.EntityFrameworkCore/"]
COPY ["src/CadentManagement.Core/CadentManagement.Core.csproj", "src/CadentManagement.Core/"]
COPY ["src/CadentManagement.Core.Shared/CadentManagement.Core.Shared.csproj", "src/CadentManagement.Core.Shared/"]
RUN dotnet restore "src/CadentManagement.Migrator/CadentManagement.Migrator.csproj"
COPY . .
WORKDIR "/src/src/CadentManagement.Migrator"
RUN dotnet build "CadentManagement.Migrator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CadentManagement.Migrator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CadentManagement.Migrator.dll", "-s"]
