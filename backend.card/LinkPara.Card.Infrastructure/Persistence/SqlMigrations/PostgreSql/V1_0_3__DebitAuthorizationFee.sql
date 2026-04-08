DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.debit_authorization_fee (
    id uuid NOT NULL,
    ocean_txn_guid bigint NOT NULL,
    type text,
    amount numeric NOT NULL,
    currency_code integer NOT NULL,
    tax1amount numeric,
    tax2amount numeric,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_debit_authorization_fee PRIMARY KEY (id)
);
END $$;