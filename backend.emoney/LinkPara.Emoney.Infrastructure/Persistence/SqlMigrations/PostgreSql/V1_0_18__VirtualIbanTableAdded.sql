DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='virtual_iban'
    ) THEN
    ALTER TABLE core.account ADD virtual_iban character varying(30) NULL;
END IF;
END $$;


DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.virtual_iban (
    id uuid NOT NULL,
    iban character varying(30) NOT NULL,
    bank_code integer NOT NULL,
    available boolean NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NULL,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50) NULL,
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_virtual_iban PRIMARY KEY (id)
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_virtual_iban_iban ON core.virtual_iban (iban);
END $$;
    