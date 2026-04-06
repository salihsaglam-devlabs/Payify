FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LinkPara.CampaignManagement.API/*.csproj", "LinkPara.CampaignManagement.API/"]
COPY ["LinkPara.CampaignManagement.Application/*.csproj", "LinkPara.CampaignManagement.Application/"]
COPY ["LinkPara.CampaignManagement.Domain/*.csproj", "LinkPara.CampaignManagement.Domain/"]
COPY ["LinkPara.CampaignManagement.Infrastructure/*.csproj", "LinkPara.CampaignManagement.Infrastructure/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet nuget locals all --clear
RUN dotnet restore "LinkPara.CampaignManagement.API/LinkPara.CampaignManagement.API.csproj"

COPY . .

WORKDIR "LinkPara.CampaignManagement.API"

# ARG SONAR_HOST_URL
# ARG SONAR_LOGIN_TOKEN
# ARG APP_NAME
# RUN apt-get update && apt-get install -y unzip
# RUN curl -o sonar-scanner.zip -L https://github.com/SonarSource/sonar-scanner-msbuild/releases/download/7.0.0.95646/sonar-scanner-7.0.0.95646-net.zip && \
#     unzip sonar-scanner.zip -d /sonar-scanner ; \
#     chmod +x /sonar-scanner/sonar-scanner-5.0.1.3006/bin/sonar-scanner && \
#     rm sonar-scanner.zip
# RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll begin /k:"$APP_NAME" /d:sonar.host.url=$SONAR_HOST_URL /d:sonar.login=$SONAR_LOGIN_TOKEN
RUN dotnet build "LinkPara.CampaignManagement.API.csproj" -c Release -o /app/build
# RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll end /d:sonar.login=$SONAR_LOGIN_TOKEN

FROM build AS publish
RUN dotnet publish "LinkPara.CampaignManagement.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 6000
ENTRYPOINT ["dotnet", "LinkPara.CampaignManagement.API.dll"]
