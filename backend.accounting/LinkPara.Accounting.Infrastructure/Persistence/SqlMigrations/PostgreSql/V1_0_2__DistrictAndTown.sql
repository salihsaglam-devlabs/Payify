DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='district'
    ) THEN
        ALTER TABLE core.customer ADD district character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='town'
    ) THEN
        ALTER TABLE core.customer ADD town character varying(50) NULL;
END IF;
END $$;
