FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LinkPara.HealthCheck/*.csproj", "LinkPara.HealthCheck/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet restore "LinkPara.HealthCheck/LinkPara.HealthCheck.csproj"

COPY . .

WORKDIR "LinkPara.HealthCheck"
RUN dotnet build "LinkPara.HealthCheck.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkPara.HealthCheck.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5025
ENTRYPOINT ["dotnet", "LinkPara.HealthCheck.dll"]
