CREATE TABLE IF NOT EXISTS core.account_iban (
	id uuid NOT NULL,
	identity_no text NULL,
	iban text NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_account_iban PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_account_iban_iban_identity_no ON core.account_iban USING btree (iban, identity_no);
