FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["LinkPara.ApiGateway.Card/*.csproj", "LinkPara.ApiGateway.Card/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet restore "LinkPara.ApiGateway.Card/LinkPara.ApiGateway.Card.csproj"

COPY . .

WORKDIR "LinkPara.ApiGateway.Card"

#ARG SONAR_HOST_URL
#ARG SONAR_LOGIN_TOKEN
#ARG APP_NAME
#RUN apt-get update && apt-get install -y unzip
#RUN curl -o sonar-scanner.zip -L https://github.com/SonarSource/sonar-scanner-msbuild/releases/download/8.0.3.99785/sonar-scanner-8.0.3.99785-net.zip && \
#    unzip sonar-scanner.zip -d /sonar-scanner ; \
#    chmod +x /sonar-scanner/sonar-scanner-5.0.1.3006/bin/sonar-scanner && \
#    rm sonar-scanner.zip
#RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll begin /k:"$APP_NAME" /d:sonar.host.url=$SONAR_HOST_URL /d:sonar.login=$SONAR_LOGIN_TOKEN
RUN dotnet build "LinkPara.ApiGateway.Card.csproj" -c Release -o /app/build
#RUN dotnet /sonar-scanner/SonarScanner.MSBuild.dll end /d:sonar.login=$SONAR_LOGIN_TOKEN

FROM build AS publish
RUN dotnet publish "LinkPara.ApiGateway.Card.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app

# �htiya� paketleri (sen zaten bunlar� istiyordun)
RUN apk --no-cache upgrade \
 && apk add --no-cache ca-certificates tzdata icu-libs curl

# Zaman dilimi istersen (opsiyonel, apt'li sat�rlar�n Alpine kar��l���)
RUN ln -snf /usr/share/zoneinfo/Europe/Istanbul /etc/localtime \
 && echo "Europe/Istanbul" > /etc/timezone

COPY --from=publish /app/publish .

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5021
ENTRYPOINT ["dotnet", "LinkPara.ApiGateway.Card.dll"]
