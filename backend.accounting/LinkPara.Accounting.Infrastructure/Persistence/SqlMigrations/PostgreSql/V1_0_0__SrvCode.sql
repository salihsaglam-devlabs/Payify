
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='template' AND column_name='srv_code'
    ) THEN
    ALTER TABLE core.template ADD srv_code character varying(50) NULL;
END IF;
END $$;




