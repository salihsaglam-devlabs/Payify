IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Merchant' AND TABLE_NAME = 'Nace')
BEGIN
CREATE TABLE Merchant.Nace (
	Id uniqueidentifier NOT NULL,
	SectorCode varchar(2) NOT NULL,
	SectorDescription varchar(300) NULL,
	ProfessionCode varchar(10) NOT NULL,
	ProfessionDescription varchar(300) NOT NULL,
	Code varchar(10) NOT NULL,
	Description varchar(800) NOT NULL,
	CreateDate datetime2 NOT NULL,
	UpdateDate datetime2 NULL,
	CreatedBy varchar(50) NOT NULL,
	LastModifiedBy varchar(50) NULL,
	RecordStatus varchar(50) NOT NULL,
	CONSTRAINT Ak_NaceCode UNIQUE (Code),
	CONSTRAINT Pk_Nace PRIMARY KEY (Id)
);
END

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'NaceCode'
)
BEGIN
    ALTER TABLE Merchant.Merchant ADD NaceCode VARCHAR(10);
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Merchant_NaceCode'
      AND object_id = OBJECT_ID('[PF].[Merchant].[Merchant]')
)
BEGIN
    CREATE INDEX IX_Merchant_NaceCode
    ON PF.Merchant.Merchant (NaceCode);
END;