DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='address'
    ) THEN
        ALTER TABLE core.customer ADD address character varying(500) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='city'
    ) THEN
        ALTER TABLE core.customer ADD city character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='city_code'
    ) THEN
       ALTER TABLE core.customer ADD city_code character varying(15) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='country'
    ) THEN
        ALTER TABLE core.customer ADD country character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='country_code'
    ) THEN
        ALTER TABLE core.customer ADD country_code character varying(15) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='tax_number'
    ) THEN
        ALTER TABLE core.customer ADD tax_number character varying(11) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='tax_office'
    ) THEN
        ALTER TABLE core.customer ADD tax_office character varying(50) NULL;
END IF;
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='customer' AND column_name='tax_office_code'
    ) THEN
        ALTER TABLE core.customer ADD tax_office_code character varying(20) NULL;
END IF;
END $$;