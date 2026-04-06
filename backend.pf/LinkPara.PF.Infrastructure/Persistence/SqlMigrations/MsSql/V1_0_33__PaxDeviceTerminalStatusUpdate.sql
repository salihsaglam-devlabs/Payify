IF COL_LENGTH('Merchant.MerchantPhysicalPos', 'DeviceTerminalLastActivity') IS NULL
BEGIN
ALTER TABLE Merchant.MerchantPhysicalPos
    ADD DeviceTerminalLastActivity DATETIME2 NOT NULL
    CONSTRAINT DF_MerchantPhysicalPos_DeviceTerminalLastActivity DEFAULT ('1900-01-01T00:00:00');
END;

IF COL_LENGTH('Merchant.MerchantPhysicalPos', 'DeviceTerminalStatus') IS NULL
BEGIN
ALTER TABLE Merchant.MerchantPhysicalPos
    ADD DeviceTerminalStatus NVARCHAR(50) NOT NULL
        CONSTRAINT DF_MerchantPhysicalPos_DeviceTerminalStatus DEFAULT ('Unknown');
END;