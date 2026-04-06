IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Merchant'
      AND COLUMN_NAME = 'IsPaymentToMainMerchant'
)
BEGIN
    DECLARE @ConstraintName sysname;
    DECLARE @IsNullable bit;
    DECLARE @DefaultDefinition nvarchar(100);

    SELECT
        @IsNullable = c.is_nullable,
        @ConstraintName = dc.name,
        @DefaultDefinition = dc.definition
    FROM sys.columns c
    LEFT JOIN sys.default_constraints dc
        ON dc.parent_object_id = c.object_id
       AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('Merchant.Merchant')
      AND c.name = 'IsPaymentToMainMerchant';

    IF EXISTS (
        SELECT 1
        FROM Merchant.Merchant
        WHERE IsPaymentToMainMerchant IS NULL
    )
    BEGIN
        UPDATE Merchant.Merchant
        SET IsPaymentToMainMerchant = 0
        WHERE IsPaymentToMainMerchant IS NULL;
    END;

    IF @ConstraintName IS NULL OR @DefaultDefinition <> '(0)'
    BEGIN
        IF @ConstraintName IS NOT NULL
        BEGIN
            DECLARE @sql nvarchar(max);
            SET @sql = 'ALTER TABLE Merchant.Merchant DROP CONSTRAINT [' + @ConstraintName + ']';
            EXEC (@sql);
        END;

        ALTER TABLE Merchant.Merchant
        ADD CONSTRAINT DF_Merchant_IsPaymentToMainMerchant
        DEFAULT (0) FOR IsPaymentToMainMerchant;
    END;

    IF @IsNullable = 1
    BEGIN
        ALTER TABLE Merchant.Merchant
        ALTER COLUMN IsPaymentToMainMerchant BIT NOT NULL;
    END;
END;

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Merchant'
      AND TABLE_NAME = 'Pool'
      AND COLUMN_NAME = 'IsPaymentToMainMerchant'
)
BEGIN
    DECLARE @ConstraintName_Pool sysname;
    DECLARE @IsNullable_Pool bit;
    DECLARE @DefaultDefinition_Pool nvarchar(100);

    SELECT
        @IsNullable_Pool = c.is_nullable,
        @ConstraintName_Pool = dc.name,
        @DefaultDefinition_Pool = dc.definition
    FROM sys.columns c
             LEFT JOIN sys.default_constraints dc
                       ON dc.parent_object_id = c.object_id
                           AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('PF.Merchant.Pool')
      AND c.name = 'IsPaymentToMainMerchant';

    IF EXISTS (
            SELECT 1 FROM PF.Merchant.Pool
            WHERE IsPaymentToMainMerchant IS NULL
        )
    BEGIN
        UPDATE PF.Merchant.Pool
        SET IsPaymentToMainMerchant = 0
        WHERE IsPaymentToMainMerchant IS NULL;
        END;
        
            IF @ConstraintName_Pool IS NULL OR @DefaultDefinition_Pool <> '(0)'
        BEGIN
                IF @ConstraintName_Pool IS NOT NULL
        BEGIN
                    DECLARE @sqlPool nvarchar(max);
                    SET @sqlPool = 'ALTER TABLE PF.Merchant.Pool DROP CONSTRAINT [' + @ConstraintName_Pool + ']';
        EXEC (@sqlPool);
    END;
    
    ALTER TABLE PF.Merchant.Pool
        ADD CONSTRAINT DF_Pool_IsPaymentToMainMerchant
            DEFAULT (0) FOR IsPaymentToMainMerchant;
    END;
    
        IF @IsNullable_Pool = 1
    BEGIN
        ALTER TABLE PF.Merchant.Pool
        ALTER COLUMN IsPaymentToMainMerchant BIT NOT NULL;
    END;
END;

IF EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'PricingProfile'
      AND COLUMN_NAME = 'IsPaymentToMainMerchant'
)
BEGIN
    DECLARE @ConstraintName_PP sysname;
    DECLARE @IsNullable_PP bit;
    DECLARE @DefaultDefinition_PP nvarchar(100);

    SELECT
        @IsNullable_PP = c.is_nullable,
        @ConstraintName_PP = dc.name,
        @DefaultDefinition_PP = dc.definition
    FROM sys.columns c
             LEFT JOIN sys.default_constraints dc
                       ON dc.parent_object_id = c.object_id
                           AND dc.parent_column_id = c.column_id
    WHERE c.object_id = OBJECT_ID('PF.Core.PricingProfile')
      AND c.name = 'IsPaymentToMainMerchant';
    
    IF EXISTS (
            SELECT 1 FROM PF.Core.PricingProfile
            WHERE IsPaymentToMainMerchant IS NULL
        )
    BEGIN
    UPDATE PF.Core.PricingProfile
    SET IsPaymentToMainMerchant = 0
    WHERE IsPaymentToMainMerchant IS NULL;
    END;
    
        IF @ConstraintName_PP IS NULL OR @DefaultDefinition_PP <> '(0)'
    BEGIN
            IF @ConstraintName_PP IS NOT NULL
    BEGIN
                DECLARE @sqlPP nvarchar(max);
                SET @sqlPP = 'ALTER TABLE PF.Core.PricingProfile DROP CONSTRAINT [' + @ConstraintName_PP + ']';
    EXEC (@sqlPP);
    END;
    
    ALTER TABLE PF.Core.PricingProfile
        ADD CONSTRAINT DF_PricingProfile_IsPaymentToMainMerchant
            DEFAULT (0) FOR IsPaymentToMainMerchant;
    END;
    
        IF @IsNullable_PP = 1
    BEGIN
    ALTER TABLE PF.Core.PricingProfile
    ALTER COLUMN IsPaymentToMainMerchant BIT NOT NULL;
    END;
END;