#LinkPara.Identity.API

##EF Migrations
- ###Migration add

    **MigrationName** değiştirilmelidir.

      dotnet ef migrations add MigrationName -o Persistence/Migrations -s ../LinkPara.Identity.API

- ###Database Update
      dotnet ef database update -s ../LinkPara.Identity.API