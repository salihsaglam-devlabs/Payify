DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='account' AND column_name='is_commercial'
    ) THEN
ALTER TABLE core.account ADD is_commercial bool NOT NULL DEFAULT false;
END IF;
END $$;
    
DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.account_activity (
	id uuid NOT NULL,
	account_id uuid NOT NULL,
	transfer_type varchar(20) NULL,
	sender varchar(36) NOT NULL,
	transaction_direction varchar(50) NOT NULL,
	receiver varchar(36) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	"year" int4 NOT NULL,
	"month" int4 NOT NULL,
	own_account bool NOT NULL DEFAULT false,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_account_activity PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_account_activity_account_id_year_month ON core.account_activity USING btree (account_id, year, month);
END $$;
    
DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.pricing_commercial (
    id uuid NOT NULL,
    max_distinct_sender_count int4 NOT NULL DEFAULT 0,
    max_distinct_sender_count_with_amount int4 NOT NULL DEFAULT 0,
    max_distinct_sender_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
    pricing_commercial_type varchar(50) NOT NULL,
    activation_date date NOT NULL,
    commission_rate numeric(18, 4) NOT NULL,
    currency_code varchar(3) NOT NULL,
    pricing_commercial_status varchar(50) NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_pricing_commercial PRIMARY KEY (id)
);
END $$;