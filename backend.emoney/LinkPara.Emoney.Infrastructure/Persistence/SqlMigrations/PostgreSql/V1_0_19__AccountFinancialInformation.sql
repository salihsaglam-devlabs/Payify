
DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.account_financial_information (
    id uuid NOT NULL,
    income_source character varying(50) NOT NULL,
    income_information character varying(50) NOT NULL,
    monthly_transaction_volume character varying(50) NOT NULL,
    monthly_transaction_count character varying(20) NOT NULL,
    account_id uuid NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NULL,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50) NULL,
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_account_financial_information PRIMARY KEY (id),
    CONSTRAINT fk_account_financial_information_account_account_id FOREIGN KEY (account_id) REFERENCES core.account (id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_account_financial_information_account_id ON core.account_financial_information (account_id);
END $$;
    