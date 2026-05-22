#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/CadentManagement.Web.Public/CadentManagement.Web.Public.csproj", "src/CadentManagement.Web.Public/"]
COPY ["src/CadentManagement.Web.Core/CadentManagement.Web.Core.csproj", "src/CadentManagement.Web.Core/"]
COPY ["src/CadentManagement.Application/CadentManagement.Application.csproj", "src/CadentManagement.Application/"]
COPY ["src/CadentManagement.Application.Shared/CadentManagement.Application.Shared.csproj", "src/CadentManagement.Application.Shared/"]
COPY ["src/CadentManagement.Core.Shared/CadentManagement.Core.Shared.csproj", "src/CadentManagement.Core.Shared/"]
COPY ["src/CadentManagement.Core/CadentManagement.Core.csproj", "src/CadentManagement.Core/"]
COPY ["src/CadentManagement.EntityFrameworkCore/CadentManagement.EntityFrameworkCore.csproj", "src/CadentManagement.EntityFrameworkCore/"]
COPY ["src/CadentManagement.GraphQL/CadentManagement.GraphQL.csproj", "src/CadentManagement.GraphQL/"]
RUN dotnet restore "src/CadentManagement.Web.Public/CadentManagement.Web.Public.csproj"
COPY . .
WORKDIR "/src/src/CadentManagement.Web.Public"
RUN dotnet build "CadentManagement.Web.Public.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CadentManagement.Web.Public.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CadentManagement.Web.Public.dll"]
