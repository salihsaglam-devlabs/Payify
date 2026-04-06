
CREATE INDEX IF NOT EXISTS ix_account_custom_tier_account_id ON "limit".account_custom_tier (account_id);


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT conname FROM pg_constraint
        WHERE conname = 'fk_account_custom_tier_account_account_id'
    ) THEN
        ALTER TABLE "limit".account_custom_tier ADD CONSTRAINT fk_account_custom_tier_account_account_id FOREIGN KEY (account_id) REFERENCES core.account (id) ON DELETE CASCADE;
END IF;
END $$;