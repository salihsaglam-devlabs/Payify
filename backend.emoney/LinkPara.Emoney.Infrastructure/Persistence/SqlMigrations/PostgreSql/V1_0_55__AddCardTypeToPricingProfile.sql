DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema='core' AND table_name='pricing_profile' AND column_name='card_type'
  ) THEN
    ALTER TABLE core.pricing_profile ADD COLUMN card_type varchar(50) NULL DEFAULT '';
  END IF;
END $$;
