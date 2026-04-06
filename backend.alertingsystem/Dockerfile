FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LinkPara.AlertingSystem/*.csproj", "LinkPara.AlertingSystem/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet restore "LinkPara.AlertingSystem/LinkPara.AlertingSystem.csproj"

COPY . .

WORKDIR "LinkPara.AlertingSystem"
RUN dotnet build "LinkPara.AlertingSystem.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkPara.AlertingSystem.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5140
ENTRYPOINT ["dotnet", "LinkPara.AlertingSystem.dll"]
