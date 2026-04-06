IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Core')
 EXEC('CREATE SCHEMA Core');

IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'AgreementDocument'
 ) BEGIN
CREATE TABLE Core.AgreementDocument (
                                        Id UNIQUEIDENTIFIER NOT NULL,
    [Name] varchar(50) NOT NULL,
    LastVersion varchar(10) NOT NULL,
    LanguageCode varchar(10) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    ProductType varchar(50) DEFAULT '' NOT NULL,
    CONSTRAINT PkAgreementDocument PRIMARY KEY (Id)
    );
END;


IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'AgreementDocumentVersion'
 ) BEGIN
CREATE TABLE Core.AgreementDocumentVersion (
                                               Id UNIQUEIDENTIFIER NOT NULL,
                                               AgreementDocumentId UNIQUEIDENTIFIER NOT NULL,
    [Content] NVARCHAR NULL,
                                               Title varchar(150) NOT NULL,
                                               LanguageCode varchar(10) NOT NULL,
                                               Version varchar(10) NOT NULL,
                                               IsLatest BIT NOT NULL,
                                               IsForceUpdate BIT NOT NULL,
                                               CreateDate DATETIME2(7) NOT NULL,
                                               UpdateDate DATETIME2(7) NULL,
                                               CreatedBy varchar(50) NOT NULL,
                                               LastModifiedBy varchar(50) NULL,
                                               RecordStatus varchar(50) NOT NULL,
                                               IsOptional BIT DEFAULT 0 NOT NULL,
                                               CONSTRAINT PkAgreementDocumentVersion PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'AgreementDocumentVersion'
 AND CONSTRAINT_NAME = 'FkAgreementDocumentVersionAgreementDocumentAgreementDoc'
 ) BEGIN
ALTER TABLE Core.AgreementDocumentVersion
    ADD CONSTRAINT FkAgreementDocumentVersionAgreementDocumentAgreementDoc
        FOREIGN KEY (AgreementDocumentId)
            REFERENCES Core.AgreementDocument(Id)
            ON DELETE NO ACTION;
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'DeviceInfo'
 ) BEGIN
CREATE TABLE Core.DeviceInfo (
                                 Id UNIQUEIDENTIFIER NOT NULL,
                                 DeviceId varchar(255) NULL,
                                 DeviceType varchar(50) NULL,
                                 DeviceName varchar(255) NULL,
                                 RegistrationToken varchar(1000) NOT NULL,
                                 Manufacturer varchar(20) NULL,
                                 Model varchar(255) NULL,
                                 OperatingSystem varchar(50) NULL,
                                 OperatingSystemVersion varchar(255) NULL,
                                 ScreenResolution varchar(50) NULL,
                                 AppVersion varchar(40) NULL,
                                 AppBuildNumber varchar(255) NULL,
                                 Camera varchar(1000) NULL,
                                 CreateDate DATETIME2(7) NOT NULL,
                                 UpdateDate DATETIME2(7) NULL,
                                 CreatedBy varchar(50) NOT NULL,
                                 LastModifiedBy varchar(50) NULL,
                                 RecordStatus varchar(50) NOT NULL,
                                 CONSTRAINT PkDeviceInfo PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'LoginWhitelist'
 ) BEGIN
CREATE TABLE Core.LoginWhitelist (
                                     Id UNIQUEIDENTIFIER NOT NULL,
                                     PhoneCode varchar(10) NOT NULL,
                                     PhoneNumber varchar(50) NOT NULL,
                                     FirstName varchar(50) NOT NULL,
                                     LastName varchar(50) NOT NULL,
                                     Email varchar(50) NULL,
                                     CreateDate DATETIME2(7) NOT NULL,
                                     UpdateDate DATETIME2(7) NULL,
                                     CreatedBy varchar(50) NOT NULL,
                                     LastModifiedBy varchar(50) NULL,
                                     RecordStatus varchar(50) NOT NULL,
                                     CONSTRAINT PkLoginWhitelist PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'Permission'
 ) BEGIN
CREATE TABLE Core.[Permission] (
                                   Id UNIQUEIDENTIFIER NOT NULL,
                                   ClaimType varchar(200) NULL,
    ClaimValue varchar(250) NOT NULL,
    [Module] varchar(200) NULL,
    OperationType varchar(50) NOT NULL,
    NormalizedClaimValue varchar(250) NOT NULL,
    Description varchar(450) NULL,
    DisplayName varchar(200) NULL,
    Display BIT NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    CONSTRAINT PkPermission PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'Role'
 ) BEGIN
CREATE TABLE Core.[Role] (
                             Id UNIQUEIDENTIFIER NOT NULL,
                             CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NOT NULL,
    LastModifiedBy varchar(100) NULL,
    CreatedBy varchar(50) NOT NULL,
    RecordStatus varchar(50) NOT NULL,
    RoleScope varchar(50) NOT NULL,
    [Name] varchar(256) NOT NULL,
    NormalizedName varchar(256) NULL,
    ConcurrencyStamp varchar(450) NULL,
    CanSeeSensitiveData BIT DEFAULT 0 NOT NULL,
    CONSTRAINT PkRole PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'LoginActivity'
 ) BEGIN
CREATE TABLE Core.LoginActivity (
                                    Id UNIQUEIDENTIFIER NOT NULL,
                                    UserId UNIQUEIDENTIFIER NOT NULL,
                                    Ip NVARCHAR NULL,
    [Date] DATETIME2(7) NOT NULL,
    Port NVARCHAR NULL,
    LoginResult varchar(50) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    Channel varchar(300) NULL,
    CONSTRAINT PkLoginActivity PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'Screen'
 ) BEGIN
CREATE TABLE Core.Screen (
                             Id UNIQUEIDENTIFIER NOT NULL,
    [Name] varchar(100) NOT NULL,
    [Module] varchar(100) NOT NULL,
    OperationType varchar(50) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    Icon NVARCHAR NULL,
    Link NVARCHAR NULL,
    ModuleIcon NVARCHAR NULL,
    ModuleLink NVARCHAR NULL,
    ModulePriority INT DEFAULT 0 NOT NULL,
    priority INT DEFAULT 0 NOT NULL,
    CONSTRAINT PkScreen PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'SecurityQuestion'
 ) BEGIN
CREATE TABLE Core.SecurityQuestion (
                                       Id UNIQUEIDENTIFIER NOT NULL,
                                       Question varchar(100) NOT NULL,
                                       LanguageCode varchar(10) NOT NULL,
                                       CreateDate DATETIME2(7) NOT NULL,
                                       UpdateDate DATETIME2(7) NULL,
                                       CreatedBy varchar(50) NOT NULL,
                                       LastModifiedBy varchar(50) NULL,
                                       RecordStatus varchar(50) NOT NULL,
                                       CONSTRAINT PkSecurityQuestion PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'User'
 ) BEGIN
CREATE TABLE Core.[User] (
                             Id UNIQUEIDENTIFIER NOT NULL,
                             FirstName varchar(50) NOT NULL,
    LastName varchar(50) NOT NULL,
    IdentityNumber varchar(50) NULL,
    BirthDate DATETIME2(7) NOT NULL,
    UserType varchar(50) NOT NULL,
    UserStatus varchar(50) NOT NULL,
    PhoneCode varchar(10) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NOT NULL,
    LastModifiedBy varchar(100) NULL,
    CreatedBy varchar(50) NOT NULL,
    PasswordModifiedDate DATETIME2(7) NOT NULL,
    RecordStatus varchar(50) NOT NULL,
    LoginLastActivityId UNIQUEIDENTIFIER NULL,
    UserName varchar(256) NULL,
    NormalizedUserName varchar(256) NULL,
    Email varchar(256) NULL,
    NormalizedEmail varchar(256) NULL,
    EmailConfirmed BIT NOT NULL,
    PasswordHash NVARCHAR NULL,
    SecurityStamp NVARCHAR NULL,
    ConcurrencyStamp NVARCHAR NULL,
    PhoneNumber varchar(50) NOT NULL,
    PhoneNumberConfirmed BIT NOT NULL,
    TwoFactorEnabled BIT NOT NULL,
    LockoutEnd DATETIMEOFFSET(7) NULL,
    LockoutEnabled BIT NOT NULL,
    AccessFailedCount INT NOT NULL,
    CONSTRAINT PkUser PRIMARY KEY (Id)
    );
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'ScreenClaim'
 ) BEGIN
CREATE TABLE Core.ScreenClaim (
                                  Id UNIQUEIDENTIFIER NOT NULL,
                                  ScreenId UNIQUEIDENTIFIER NOT NULL,
                                  ClaimValue NVARCHAR NULL,
                                  CreateDate DATETIME2(7) NOT NULL,
                                  UpdateDate DATETIME2(7) NULL,
                                  CreatedBy varchar(50) NOT NULL,
                                  LastModifiedBy varchar(50) NULL,
                                  RecordStatus varchar(50) NOT NULL,
                                  CONSTRAINT PkScreenClaim PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserAddress'
 ) BEGIN
CREATE TABLE Core.UserAddress (
                                  Id UNIQUEIDENTIFIER NOT NULL,
                                  UserId UNIQUEIDENTIFIER NOT NULL,
                                  CountryId INT NOT NULL,
                                  CityId INT NOT NULL,
                                  DistrictId INT NOT NULL,
                                  Neighbourhood varchar(450) NOT NULL,
                                  Street varchar(450) NOT NULL,
                                  FullAddress varchar(600) NOT NULL,
                                  CreateDate DATETIME2(7) NOT NULL,
                                  UpdateDate DATETIME2(7) NULL,
                                  CreatedBy varchar(50) NOT NULL,
                                  LastModifiedBy varchar(100) NULL,
                                  RecordStatus varchar(50) NOT NULL,
                                  CONSTRAINT PkUserAddress PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'RoleClaim'
 ) BEGIN
CREATE TABLE Core.RoleClaim (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                CreateDate DATETIME2(7) NOT NULL,
                                UpdateDate DATETIME2(7) NOT NULL,
                                LastModifiedBy varchar(100) NULL,
                                CreatedBy varchar(50) NOT NULL,
                                RecordStatus varchar(50) NOT NULL,
                                RoleId UNIQUEIDENTIFIER NOT NULL,
                                ClaimType NVARCHAR NULL,
                                ClaimValue NVARCHAR NULL
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'RoleScreen'
 ) BEGIN
CREATE TABLE Core.RoleScreen (
                                 Id UNIQUEIDENTIFIER NOT NULL,
                                 RoleId UNIQUEIDENTIFIER NOT NULL,
                                 ScreenId UNIQUEIDENTIFIER NOT NULL,
                                 CreateDate DATETIME2(7) NOT NULL,
                                 UpdateDate DATETIME2(7) NULL,
                                 CreatedBy varchar(50) NOT NULL,
                                 LastModifiedBy varchar(50) NULL,
                                 RecordStatus varchar(50) NOT NULL,
                                 CONSTRAINT PkRoleScreen PRIMARY KEY (Id)
);
END;

IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserAgreementDocument'
 ) BEGIN
CREATE TABLE Core.UserAgreementDocument (
                                            Id UNIQUEIDENTIFIER NOT NULL,
                                            UserId UNIQUEIDENTIFIER NOT NULL,
                                            AgreementDocumentVersionId UNIQUEIDENTIFIER NOT NULL,
                                            CreateDate DATETIME2(7) NOT NULL,
                                            UpdateDate DATETIME2(7) NULL,
                                            CreatedBy varchar(50) NOT NULL,
                                            LastModifiedBy varchar(50) NULL,
                                            RecordStatus varchar(50) NOT NULL,
                                            ApprovalChannel varchar(50) NULL,
                                            CONSTRAINT PkUserAgreementDocument PRIMARY KEY (Id)
);
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserClaim'
 ) BEGIN
CREATE TABLE Core.UserClaim (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Description varchar(450) NULL,
                                DisplayName varchar(450) NULL,
                                CreateDate DATETIME2(7) NOT NULL,
                                UpdateDate DATETIME2(7) NOT NULL,
                                LastModifiedBy varchar(100) NULL,
                                CreatedBy varchar(50) NOT NULL,
                                RecordStatus varchar(50) NOT NULL,
                                UserId UNIQUEIDENTIFIER NOT NULL,
                                ClaimType NVARCHAR NULL,
                                ClaimValue NVARCHAR NULL
);
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserDeviceInfo'
 ) BEGIN
CREATE TABLE Core.UserDeviceInfo (
                                     Id UNIQUEIDENTIFIER NOT NULL,
                                     UserId UNIQUEIDENTIFIER NOT NULL,
                                     IsMainDevice BIT NOT NULL,
                                     DeviceInfoId UNIQUEIDENTIFIER NOT NULL,
                                     CreateDate DATETIME2(7) NOT NULL,
                                     UpdateDate DATETIME2(7) NULL,
                                     CreatedBy varchar(50) NOT NULL,
                                     LastModifiedBy varchar(50) NULL,
                                     RecordStatus varchar(50) NOT NULL,
                                     CONSTRAINT PkUserDeviceInfo PRIMARY KEY (Id)
);
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserLogin'
 ) BEGIN
CREATE TABLE Core.UserLogin (
                                LoginProvider NVARCHAR(450) NOT NULL,
                                ProviderKey NVARCHAR(450) NOT NULL,
                                ProviderDisplayName NVARCHAR(256) NULL,
                                UserId UNIQUEIDENTIFIER NOT NULL,
                                CONSTRAINT PkUserLogin PRIMARY KEY (LoginProvider, ProviderKey)
);
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserLoginLastActivity'
 ) BEGIN
CREATE TABLE Core.UserLoginLastActivity (
                                            Id UNIQUEIDENTIFIER NOT NULL,
                                            LastSucceededLogin DATETIME2(7) NULL,
                                            LastLockedLogin DATETIME2(7) NULL,
                                            LastFailedLogin DATETIME2(7) NULL,
                                            LoginResult varchar(50) NOT NULL,
                                            UserId UNIQUEIDENTIFIER NOT NULL,
                                            CreateDate DATETIME2(7) NOT NULL,
                                            UpdateDate DATETIME2(7) NULL,
                                            CreatedBy varchar(50) NOT NULL,
                                            LastModifiedBy varchar(50) NULL,
                                            RecordStatus varchar(50) NOT NULL,
                                            FailedLoginCount INT DEFAULT 0 NOT NULL,
                                            Channel varchar(300) NULL,
                                            CONSTRAINT PkUserLoginLastActivity PRIMARY KEY (Id)
);
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserPasswordHistory'
 ) BEGIN
CREATE TABLE Core.UserPasswordHistory (
                                          Id UNIQUEIDENTIFIER NOT NULL,
                                          UserId UNIQUEIDENTIFIER NOT NULL,
                                          PasswordHash NVARCHAR NULL,
                                          CreateDate DATETIME2(7) NOT NULL,
                                          UpdateDate DATETIME2(7) NULL,
                                          CreatedBy varchar(50) NOT NULL,
                                          LastModifiedBy varchar(50) NULL,
                                          RecordStatus varchar(50) NOT NULL,
                                          CONSTRAINT PkUserPasswordHistory PRIMARY KEY (Id)
);
END;

 
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserPicture'
 ) BEGIN
CREATE TABLE Core.UserPicture (
                                  Id UNIQUEIDENTIFIER NOT NULL,
                                  Bytes VARBINARY(MAX) NULL,
                                  ContentType NVARCHAR NULL,
                                  UserId UNIQUEIDENTIFIER NOT NULL,
                                  CreateDate DATETIME2(7) NOT NULL,
                                  UpdateDate DATETIME2(7) NULL,
                                  CreatedBy varchar(50) NOT NULL,
                                  LastModifiedBy varchar(50) NULL,
                                  RecordStatus varchar(50) NOT NULL,
                                  CONSTRAINT PkUserPicture PRIMARY KEY (Id)
);
END;
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserRole'
 ) BEGIN
CREATE TABLE Core.UserRole (
                               UserId UNIQUEIDENTIFIER NOT NULL,
                               RoleId UNIQUEIDENTIFIER NOT NULL,
                               CONSTRAINT PkUserRole PRIMARY KEY (UserId, RoleId)
);
END;
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserSecurityAnswer'
 ) BEGIN
CREATE TABLE Core.UserSecurityAnswer (
                                         Id UNIQUEIDENTIFIER NOT NULL,
                                         UserId UNIQUEIDENTIFIER NOT NULL,
                                         SecurityQuestionId UNIQUEIDENTIFIER NOT NULL,
                                         AnswerHash varchar(400) NOT NULL,
                                         CreateDate DATETIME2(7) NOT NULL,
                                         UpdateDate DATETIME2(7) NULL,
                                         CreatedBy varchar(50) NOT NULL,
                                         LastModifiedBy varchar(50) NULL,
                                         RecordStatus varchar(50) NOT NULL,
                                         CONSTRAINT PkUserSecurityAnswer PRIMARY KEY (Id)
);
END;
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserSession'
 ) BEGIN
CREATE TABLE Core.UserSession (
                                  Id UNIQUEIDENTIFIER NOT NULL,
                                  UserId UNIQUEIDENTIFIER NOT NULL,
                                  RefreshToken NVARCHAR NULL,
                                  RefreshTokenExpiration DATETIME2(7) NOT NULL,
                                  CreateDate DATETIME2(7) NOT NULL,
                                  UpdateDate DATETIME2(7) NULL,
                                  CreatedBy varchar(50) NOT NULL,
                                  LastModifiedBy varchar(50) NULL,
                                  RecordStatus varchar(50) NOT NULL,
                                  CONSTRAINT PkUserSession PRIMARY KEY (Id)
);
END;
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserToken'
 ) BEGIN
CREATE TABLE Core.UserToken (
                                UserId UNIQUEIDENTIFIER NOT NULL,
                                LoginProvider NVARCHAR NOT NULL,
                                Name NVARCHAR NOT NULL,
                                Value NVARCHAR NULL,
                                CONSTRAINT PkUserToken PRIMARY KEY (UserId, LoginProvider, Name)
);
END;
IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLES
 WHERE TABLE_SCHEMA = 'dbo'
 AND TABLE_NAME = 'MaxId'
 ) BEGIN
CREATE TABLE dbo.MaxId (Id INT NULL);
END;
IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_UserAgreementDocument_AgreementDocumentVersionId' 

      AND object_id = OBJECT_ID('Core.UserAgreementDocument')

) BEGIN
CREATE INDEX Ix_UserAgreementDocument_AgreementDocumentVersionId ON Core.UserAgreementDocument (AgreementDocumentVersionId);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'Ix_UserAgreementDocument_UserId' 
      AND object_id = OBJECT_ID('Core.UserAgreementDocument')
)
BEGIN
CREATE INDEX Ix_UserAgreementDocument_UserId ON Core.UserAgreementDocument (UserId);
END;


IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'Ix_UserClaim_UserId' 
      AND object_id = OBJECT_ID('Core.UserClaim')
) BEGIN
CREATE INDEX Ix_UserClaim_UserId ON Core.UserClaim (UserId);
END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_UserDeviceInfo_DeviceInfoId' 


      AND object_id = OBJECT_ID('Core.UserDeviceInfo')


)


BEGIN


CREATE INDEX Ix_UserDeviceInfo_DeviceInfoId ON Core.UserDeviceInfo (DeviceInfoId);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_UserLogin_UserId' 


      AND object_id = OBJECT_ID('Core.UserLogin')


)


BEGIN


CREATE INDEX Ix_UserLogin_UserId ON Core.UserLogin (UserId);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_UserLoginLastActivity_UserId' 


      AND object_id = OBJECT_ID('Core.UserLoginLastActivity')


)


BEGIN


CREATE UNIQUE INDEX Ix_UserLoginLastActivity_UserId ON Core.UserLoginLastActivity (UserId);


END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_UserPasswordHistory_UserId' 


      AND object_id = OBJECT_ID('Core.UserPasswordHistory')


)


BEGIN


CREATE INDEX Ix_UserPasswordHistory_UserId ON Core.UserPasswordHistory (UserId);


END;


IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserAgreementDocument'
 AND CONSTRAINT_NAME = 'FkUserAgreementDocumentAgreementDocumentVersionAgreemen'
 ) BEGIN
ALTER TABLE Core.UserAgreementDocument
    ADD CONSTRAINT FkUserAgreementDocumentAgreementDocumentVersionAgreemen
        FOREIGN KEY (AgreementDocumentVersionId)
            REFERENCES Core.AgreementDocumentVersion(Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserAgreementDocument'
 AND CONSTRAINT_NAME = 'FkUserAgreementDocumentUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserAgreementDocument
    ADD CONSTRAINT FkUserAgreementDocumentUsersUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserClaim'
 AND CONSTRAINT_NAME = 'FkUserClaimUserUserId'
 ) BEGIN
ALTER TABLE Core.UserClaim
    ADD CONSTRAINT FkUserClaimUserUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserDeviceInfo'
 AND CONSTRAINT_NAME = 'FkUserDeviceInfoDeviceInfoDeviceInfoId'
 ) BEGIN
ALTER TABLE Core.UserDeviceInfo
    ADD CONSTRAINT FkUserDeviceInfoDeviceInfoDeviceInfoId
        FOREIGN KEY (DeviceInfoId)
            REFERENCES Core.DeviceInfo(Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserLogin'
 AND CONSTRAINT_NAME = 'FkUserLoginUserUserId'
 ) BEGIN
ALTER TABLE Core.UserLogin
    ADD CONSTRAINT FkUserLoginUserUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserLoginLastActivity'
 AND CONSTRAINT_NAME = 'FkUserLoginLastActivityUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserLoginLastActivity
    ADD CONSTRAINT FkUserLoginLastActivityUsersUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserPasswordHistory'
 AND CONSTRAINT_NAME = 'FkUserPasswordHistoryUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserPasswordHistory
    ADD CONSTRAINT FkUserPasswordHistoryUsersUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserPicture_UserId'
 ) BEGIN
CREATE INDEX Ix_UserPicture_UserId ON Core.UserPicture (UserId);
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserRole_RoleId'
 ) BEGIN
CREATE INDEX Ix_UserRole_RoleId ON Core.UserRole (RoleId);
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserSecurityAnswer_SecurityQuestionId'
 ) BEGIN
CREATE INDEX Ix_UserSecurityAnswer_SecurityQuestionId ON Core.UserSecurityAnswer (SecurityQuestionId);
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserSecurityAnswer_UserId'
 ) BEGIN
CREATE INDEX Ix_UserSecurityAnswer_UserId ON Core.UserSecurityAnswer (UserId);
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserSession_RefreshTokenExpiration'
 ) BEGIN
CREATE INDEX Ix_UserSession_RefreshTokenExpiration ON Core.UserSession (RefreshTokenExpiration);
END;
IF NOT EXISTS (
 SELECT 1
 FROM sys.indexes
 WHERE name = 'Ix_UserSession_UserId'
 ) BEGIN
CREATE INDEX Ix_UserSession_UserId ON Core.UserSession (UserId);
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserPicture'
 AND CONSTRAINT_NAME = 'FkUserPictureUserUserId'
 ) BEGIN
ALTER TABLE Core.UserPicture
    ADD CONSTRAINT FkUserPictureUserUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserRole'
 AND CONSTRAINT_NAME = 'FkUserRoleUserUserId'
 ) BEGIN
ALTER TABLE Core.UserRole
    ADD CONSTRAINT FkUserRoleUserUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserRole'
 AND CONSTRAINT_NAME = 'FkUserRoleRoleRoleId'
 ) BEGIN
ALTER TABLE Core.UserRole
    ADD CONSTRAINT FkUserRoleRoleRoleId FOREIGN KEY (RoleId) REFERENCES Core.[Role](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserSecurityAnswer'
 AND CONSTRAINT_NAME = 'FkUserSecurityAnswerUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserSecurityAnswer
    ADD CONSTRAINT FkUserSecurityAnswerUsersUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserSecurityAnswer'
 AND CONSTRAINT_NAME = 'FkUserSecurityAnswerSecurityQuestionSecurityQuestionId'
 ) BEGIN
ALTER TABLE Core.UserSecurityAnswer
    ADD CONSTRAINT FkUserSecurityAnswerSecurityQuestionSecurityQuestionId FOREIGN KEY (SecurityQuestionId) REFERENCES Core.SecurityQuestion(Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserSession'
 AND CONSTRAINT_NAME = 'FkUserSessionUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserSession
    ADD CONSTRAINT FkUserSessionUsersUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'UserToken'
 AND CONSTRAINT_NAME = 'FkUserTokenUserUserId'
 ) BEGIN
ALTER TABLE Core.UserToken
    ADD CONSTRAINT FkUserTokenUserUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;
DROP INDEX IF EXISTS Core.Ix_AgreementDocument_RecordStatus;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_AgreementDocument_RecordStatus' 


      AND object_id = OBJECT_ID('Core.AgreementDocument')


)


BEGIN


CREATE INDEX Ix_AgreementDocument_RecordStatus ON Core.AgreementDocument (RecordStatus);


END;


DROP INDEX IF EXISTS Core.Ix_LoginWhitelist_PhoneNumber;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_LoginWhitelist_PhoneNumber' 


      AND object_id = OBJECT_ID('Core.LoginWhitelist')


)


BEGIN


CREATE UNIQUE INDEX Ix_LoginWhitelist_PhoneNumber ON Core.LoginWhitelist (PhoneNumber);


END;


DROP INDEX IF EXISTS Core.Ix_AgreementDocumentVersion_AgreementDocumentId;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_AgreementDocumentVersion_AgreementDocumentId' 


      AND object_id = OBJECT_ID('Core.AgreementDocumentVersion')


)


BEGIN


CREATE INDEX Ix_AgreementDocumentVersion_AgreementDocumentId ON Core.AgreementDocumentVersion (AgreementDocumentId);


END;



DROP INDEX IF EXISTS Core.Ix_DeviceInfo_DeviceId;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_DeviceInfo_DeviceId' 


      AND object_id = OBJECT_ID('Core.DeviceInfo')


)


BEGIN


CREATE INDEX Ix_DeviceInfo_DeviceId ON Core.DeviceInfo (DeviceId);


END;


DROP INDEX IF EXISTS Core.Ix_Permission_ClaimValue;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_Permission_ClaimValue' 


      AND object_id = OBJECT_ID('Core.[Permission]')


)


BEGIN


CREATE UNIQUE INDEX Ix_Permission_ClaimValue ON Core.[Permission] (ClaimValue);


END;



DROP INDEX IF EXISTS Core.[RoleNameIndex];
DROP INDEX IF EXISTS Core.Ix_RoleName;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'RoleNameIndex' 
      AND object_id = OBJECT_ID('Core.[Role]')
) BEGIN
CREATE UNIQUE INDEX [RoleNameIndex] ON Core.[Role] (NormalizedName);
END;


IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_RoleName' 


      AND object_id = OBJECT_ID('Core.[Role]')


)


BEGIN


CREATE UNIQUE INDEX Ix_RoleName ON Core.[Role] ([Name]);


END;




IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'EmailIndex' 
      AND object_id = OBJECT_ID('Core.[User]')
) BEGIN
CREATE INDEX [EmailIndex] ON Core.[User] (NormalizedEmail);
END;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'UserNameIndex' 
      AND object_id = OBJECT_ID('Core.[User]')
) BEGIN
CREATE UNIQUE INDEX [UserNameIndex] ON Core.[User] (NormalizedUserName);
END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_Email' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE INDEX Ix_User_Email ON Core.[User] (Email);

END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_IdentityNumber' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE UNIQUE INDEX Ix_User_IdentityNumber ON Core.[User] (IdentityNumber) WHERE IdentityNumber IS NOT NULL;

END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_PhoneNumber' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE INDEX Ix_User_PhoneNumber ON Core.[User] (PhoneNumber);

END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_UserName' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE UNIQUE INDEX Ix_User_UserName ON Core.[User] (UserName);

END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_UserStatus' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE INDEX Ix_User_UserStatus ON Core.[User] (UserStatus);

END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_User_UserType' 

      AND object_id = OBJECT_ID('Core.[User]')

)

BEGIN

CREATE INDEX Ix_User_UserType ON Core.[User] (UserType);

END;


DROP INDEX IF EXISTS Core.Ix_ScreenClaim_ScreenId;

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'Ix_ScreenClaim_ScreenId' 
      AND object_id = OBJECT_ID('Core.ScreenClaim')
) BEGIN
CREATE INDEX Ix_ScreenClaim_ScreenId ON Core.ScreenClaim (ScreenId);
END;




IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'ScreenClaim' AND CONSTRAINT_NAME = 'FkScreenClaimScreenScreenId'
 ) BEGIN
ALTER TABLE Core.ScreenClaim
    ADD CONSTRAINT FkScreenClaimScreenScreenId FOREIGN KEY (ScreenId) REFERENCES Core.Screen(Id) ON DELETE NO ACTION;
END;


DROP INDEX IF EXISTS Core.Ix_LoginActivity_UserId;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_LoginActivity_UserId' 


      AND object_id = OBJECT_ID('Core.LoginActivity')


)


BEGIN


CREATE INDEX Ix_LoginActivity_UserId ON Core.LoginActivity (UserId);


END;


IF NOT EXISTS (
 SELECT 1
 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core'
 AND TABLE_NAME = 'LoginActivity'
 AND CONSTRAINT_NAME = 'FkLoginActivityUsersUserId'
 ) BEGIN
ALTER TABLE Core.LoginActivity
    ADD CONSTRAINT FkLoginActivityUsersUserId
        FOREIGN KEY (UserId)
            REFERENCES Core.[User](Id)
 ON DELETE NO ACTION;
END;


DROP INDEX IF EXISTS Core.ix_user_address_user_id;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'ix_user_address_user_id' 


      AND object_id = OBJECT_ID('Core.UserAddress')


)


BEGIN


CREATE INDEX ix_user_address_user_id ON Core.UserAddress (UserId);


END;


IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserAddress' AND CONSTRAINT_NAME = 'FkUserAddressUsersUserId'
 ) BEGIN
ALTER TABLE Core.UserAddress
    ADD CONSTRAINT FkUserAddressUsersUserId FOREIGN KEY (UserId) REFERENCES Core.[User](Id) ON DELETE NO ACTION;
END;

DROP INDEX IF EXISTS Core.Ix_RoleClaim_RoleId;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_RoleClaim_RoleId' 


      AND object_id = OBJECT_ID('Core.RoleClaim')


)


BEGIN


CREATE INDEX Ix_RoleClaim_RoleId ON Core.RoleClaim (RoleId);


END;


IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'RoleClaim' AND CONSTRAINT_NAME = 'FkRoleClaimRoleRoleId'
 ) BEGIN
ALTER TABLE Core.RoleClaim
    ADD CONSTRAINT FkRoleClaimRoleRoleId FOREIGN KEY (RoleId) REFERENCES Core.[Role](Id) ON DELETE NO ACTION;
END;


DROP INDEX IF EXISTS Core.Ix_RoleScreen_RoleId;
DROP INDEX IF EXISTS Core.Ix_RoleScreen_ScreenId;

IF NOT EXISTS (


    SELECT 1 FROM sys.indexes 


    WHERE name = 'Ix_RoleScreen_RoleId' 


      AND object_id = OBJECT_ID('Core.RoleScreen')


)


BEGIN


CREATE INDEX Ix_RoleScreen_RoleId ON Core.RoleScreen (RoleId);


END;

IF NOT EXISTS (

    SELECT 1 FROM sys.indexes 

    WHERE name = 'Ix_RoleScreen_ScreenId' 

      AND object_id = OBJECT_ID('Core.RoleScreen')

)

BEGIN

CREATE INDEX Ix_RoleScreen_ScreenId ON Core.RoleScreen (ScreenId);

END;


IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'RoleScreen' AND CONSTRAINT_NAME = 'FkRoleScreenRolesRoleId'
 ) BEGIN
ALTER TABLE Core.RoleScreen
    ADD CONSTRAINT FkRoleScreenRolesRoleId FOREIGN KEY (RoleId) REFERENCES Core.[Role](Id) ON DELETE NO ACTION;
END;

 IF NOT EXISTS (
 SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
 WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'RoleScreen' AND CONSTRAINT_NAME = 'FkRoleScreenScreenScreenId'
 ) BEGIN
ALTER TABLE Core.RoleScreen
    ADD CONSTRAINT FkRoleScreenScreenScreenId FOREIGN KEY (ScreenId) REFERENCES Core.Screen(Id) ON DELETE NO ACTION;
END;

