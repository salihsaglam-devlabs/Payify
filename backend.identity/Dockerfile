FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LinkPara.Identity.API/*.csproj", "LinkPara.Identity.API/"]
COPY ["LinkPara.Identity.Application/*.csproj", "LinkPara.Identity.Application/"]
COPY ["LinkPara.Identity.Domain/*.csproj", "LinkPara.Identity.Domain/"]
COPY ["LinkPara.Identity.Infrastructure/*.csproj", "LinkPara.Identity.Infrastructure/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet nuget locals all --clear
RUN dotnet restore "LinkPara.Identity.API/LinkPara.Identity.API.csproj"

COPY . .

WORKDIR "LinkPara.Identity.API"

#ARG SONAR_HOST_URL
#ARG SONAR_LOGIN_TOKEN
#ARG APP_NAME
#RUN apt-get update && apt-get install -y unzip
#RUN curl -o sonar-scanner.zip -L https://github.com/SonarSource/sonar-scanner-msbuild/releases/download/7.0.0.95646/sonar-scanner-7.0.0.95646-net.zip && \
#    unzip sonar-scanner.zip -d /sonar-scanner ; \
#    chmod +x /sonar-scanner/sonar-scanner-5.0.1.3006/bin/sonar-scanner && \
#    rm sonar-scanner.zip
#RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll begin /k:"$APP_NAME" /d:sonar.host.url=$SONAR_HOST_URL /d:sonar.login=$SONAR_LOGIN_TOKEN
RUN dotnet build "LinkPara.Identity.API.csproj" -c Release -o /app/build
#RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll end /d:sonar.login=$SONAR_LOGIN_TOKEN


FROM build AS publish
RUN dotnet publish "LinkPara.Identity.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5020
ENTRYPOINT ["dotnet", "LinkPara.Identity.API.dll"]
