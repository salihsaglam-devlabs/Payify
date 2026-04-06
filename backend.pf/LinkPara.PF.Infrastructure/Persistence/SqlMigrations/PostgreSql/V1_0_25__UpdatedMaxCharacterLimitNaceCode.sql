CREATE TABLE IF NOT EXISTS merchant.nace (
	id uuid NOT NULL,
	sector_code varchar(2) NOT NULL,
	sector_description varchar(300) NULL,
	profession_code varchar(10) NOT NULL,
	profession_description varchar(300) NOT NULL,
	code varchar(10) NOT NULL,
	description varchar(800) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
    CONSTRAINT ak_nace_code UNIQUE (code),
    CONSTRAINT pk_nace PRIMARY KEY (id)
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'merchant'
          AND table_name = 'merchant'
          AND column_name = 'nace_code'
    ) THEN
        ALTER TABLE merchant.merchant
        ADD COLUMN nace_code varchar(10);
    END IF;
END $$;


CREATE INDEX IF NOT EXISTS ix_merchant_nace_code ON merchant.merchant USING btree (nace_code);