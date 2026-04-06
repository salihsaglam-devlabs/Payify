IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = 'Core'
      AND TABLE_NAME = 'UserAgreementDocument'
      AND COLUMN_NAME = 'ApprovalChannel'
)
BEGIN
    ALTER TABLE Core.UserAgreementDocument ADD ApprovalChannel VARCHAR(50);
END