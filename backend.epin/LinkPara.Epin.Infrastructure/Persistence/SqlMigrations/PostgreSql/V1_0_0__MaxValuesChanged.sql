
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='reconciliation_detail' AND column_name='product_name'
    ) THEN
    ALTER TABLE core.reconciliation_detail ADD product_name character varying(300) NULL;
END IF;
END $$;


DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='brand' AND column_name='summary'
    ) THEN
    ALTER TABLE core.brand ALTER COLUMN summary TYPE text;
END IF;
END $$;


DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='brand' AND column_name='image'
    ) THEN
    ALTER TABLE core.brand ALTER COLUMN image TYPE text;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='brand' AND column_name='description'
    ) THEN
    ALTER TABLE core.brand ALTER COLUMN description TYPE text;
END IF;
END $$;
