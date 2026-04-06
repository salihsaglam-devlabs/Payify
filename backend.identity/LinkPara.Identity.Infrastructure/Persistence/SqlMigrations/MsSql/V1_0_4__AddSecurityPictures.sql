IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'SecurityPicture'
) BEGIN
CREATE TABLE Core.SecurityPicture (
    Id UNIQUEIDENTIFIER NOT NULL,
    Name varchar(100) NOT NULL,
    Bytes VARBINARY(MAX) NOT NULL,
    ContentType varchar(50) NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    CONSTRAINT PkSecurityPicture PRIMARY KEY (Id)
);
END;


IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = 'Core' AND TABLE_NAME = 'UserSecurityPicture'
) BEGIN
CREATE TABLE Core.UserSecurityPicture (
    Id UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    SecurityPictureId UNIQUEIDENTIFIER NOT NULL,
    CreateDate DATETIME2(7) NOT NULL,
    UpdateDate DATETIME2(7) NULL,
    CreatedBy varchar(50) NOT NULL,
    LastModifiedBy varchar(50) NULL,
    RecordStatus varchar(50) NOT NULL,
    CONSTRAINT PkUserSecurityPicture PRIMARY KEY (Id)
);
END;


IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IxUserSecurityPictureUserId'
      AND object_id = OBJECT_ID('Core.UserSecurityPicture')
) BEGIN
CREATE INDEX IxUserSecurityPictureUserId ON Core.UserSecurityPicture (UserId);
END;


IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IxUserSecurityPictureSecurityPictureId'
      AND object_id = OBJECT_ID('Core.UserSecurityPicture')
) BEGIN
CREATE INDEX IxUserSecurityPictureSecurityPictureId ON Core.UserSecurityPicture (SecurityPictureId);
END;


IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'UserSecurityPicture'
      AND CONSTRAINT_NAME = 'FkUserSecurityPictureSecurityPictureSecurityPictureId'
) BEGIN
ALTER TABLE Core.UserSecurityPicture
    ADD CONSTRAINT FkUserSecurityPictureSecurityPictureSecurityPictureId
        FOREIGN KEY (SecurityPictureId)
            REFERENCES Core.SecurityPicture(Id)
            ON DELETE CASCADE;
END;
