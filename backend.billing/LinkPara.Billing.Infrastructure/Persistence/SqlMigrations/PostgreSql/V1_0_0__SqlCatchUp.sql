DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='bill_date'
    ) THEN
    ALTER TABLE core.transaction  ALTER COLUMN bill_date TYPE date;
    ALTER TABLE core.transaction ALTER COLUMN bill_date DROP NOT NULL;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='transaction' AND column_name='bill_due_date'
    ) THEN
    ALTER TABLE core.transaction ALTER COLUMN bill_due_date TYPE date;    
END IF;
END $$;
    
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='timeout_transaction' AND column_name='bill_date'
    ) THEN
ALTER TABLE core.transaction  ALTER COLUMN bill_date TYPE date;
ALTER TABLE core.transaction ALTER COLUMN bill_date DROP NOT NULL;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='timeout_transaction' AND column_name='bill_due_date'
    ) THEN
ALTER TABLE core.transaction ALTER COLUMN bill_due_date TYPE date;
END IF;
END $$;
    
DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='reconciliation' AND table_name='institution_detail' AND column_name='bill_date'
    ) THEN
ALTER TABLE core.transaction  ALTER COLUMN bill_date TYPE date;
ALTER TABLE core.transaction ALTER COLUMN bill_date DROP NOT NULL;
END IF;
END $$;

DO $$
BEGIN
  IF EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='reconciliation' AND table_name='institution_detail' AND column_name='bill_due_date'
    ) THEN
ALTER TABLE core.transaction ALTER COLUMN bill_due_date TYPE date;
END IF;
END $$;
    