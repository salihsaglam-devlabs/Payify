DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='is_open_banking_permit'
    ) THEN
ALTER TABLE core.account ADD is_open_banking_permit boolean NOT NULL DEFAULT FALSE ;
END IF;
END $$;

DO $$
BEGIN

CREATE TABLE IF NOT EXISTS core.changed_balance_log (
    id uuid NOT NULL,
    consent_id text NULL,
    account_id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    has_balance_changed boolean NOT NULL,
    last_event_time timestamp without time zone NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NULL,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50) NULL,
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_changed_balance_log PRIMARY KEY (id),
    CONSTRAINT fk_changed_balance_log_account_account_id FOREIGN KEY (account_id) REFERENCES core.account (id) ON DELETE CASCADE,
    CONSTRAINT fk_changed_balance_log_wallet_wallet_id FOREIGN KEY (wallet_id) REFERENCES core.wallet (id) ON DELETE RESTRICT
);
END $$;

DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.payment_order (
    id uuid NOT NULL,
    consent_number character varying(50) NULL,
    consent_create_time timestamp without time zone NOT NULL,
    yos_code character varying(50) NULL,
    amount numeric(18,2) NOT NULL,
    currency_code character varying(10) NULL,
    sender_title character varying(300) NULL,
    sender_wallet_number character varying(50) NULL,
    receiver_title character varying(300) NULL,
    receiver_wallet_number character varying(50) NULL,
    receiver_iban character varying(50) NULL,
    is_success boolean NOT NULL,
    transaction_id uuid NOT NULL,
    transaction_time timestamp without time zone NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NULL,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50) NULL,
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_payment_order PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_changed_balance_log_account_id ON core.changed_balance_log (account_id);

CREATE INDEX IF NOT EXISTS ix_changed_balance_log_wallet_id ON core.changed_balance_log (wallet_id);
END $$;