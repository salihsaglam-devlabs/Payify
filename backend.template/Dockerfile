FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["LinkPara.Template.API/*.csproj", "LinkPara.Template.API/"]
COPY ["LinkPara.Template.Application/*.csproj", "LinkPara.Template.Application/"]
COPY ["LinkPara.Template.Domain/*.csproj", "LinkPara.Template.Domain/"]
COPY ["LinkPara.Template.Infrastructure/*.csproj", "LinkPara.Template.Infrastructure/"]
COPY ["NuGet.config", "."]

ARG NUGET_USER
ARG NUGET_PASSWORD

RUN dotnet restore "LinkPara.Template.API/LinkPara.Template.API.csproj"

COPY . .

WORKDIR "LinkPara.Template.API"
RUN dotnet build "LinkPara.Template.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LinkPara.Template.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 6000
ENTRYPOINT ["dotnet", "LinkPara.Template.API.dll"]
