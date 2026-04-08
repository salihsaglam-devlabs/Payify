IF NOT EXISTS (
    SELECT 1 
    FROM sys.tables t
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    WHERE t.name = 'DebitAuthorizationFee' AND s.name = 'Core'
)
BEGIN
    CREATE TABLE core.debit_authorization_fee (
    id UNIQUEIDENTIFIER NOT NULL,
    ocean_txn_guid BIGINT NOT NULL,
    type NVARCHAR(MAX),
    amount DECIMAL(18, 2) NOT NULL,
    currency_code INT NOT NULL,
    tax1amount DECIMAL(18, 2),
    tax2amount DECIMAL(18, 2),
    create_date DATETIME2 NOT NULL,
    update_date DATETIME2 NULL,
    created_by NVARCHAR(50) NOT NULL,
    last_modified_by NVARCHAR(50) NULL,
    record_status NVARCHAR(50) NOT NULL,
    CONSTRAINT pk_debit_authorization_fee PRIMARY KEY (id)
);
END
GO