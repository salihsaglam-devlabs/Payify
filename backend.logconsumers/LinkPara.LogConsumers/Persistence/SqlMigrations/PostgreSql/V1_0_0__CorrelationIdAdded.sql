
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='audit_log' AND column_name='correlation_id'
    ) THEN
    alter table core.audit_log add correlation_id character varying(100);
END IF;
END $$;


DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='entity_change_log' AND column_name='correlation_id'
    ) THEN
    alter table core.entity_change_log add correlation_id character varying(100);
END IF;
END $$;