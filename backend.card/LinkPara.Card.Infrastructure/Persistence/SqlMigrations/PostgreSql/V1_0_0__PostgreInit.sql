DO $$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
        CREATE SCHEMA core;
    END IF;
END $$;
 
DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.customer_wallet_card (
    id uuid NOT NULL,
    banking_customer_no character varying(16),
    wallet_number character varying(50),
    card_number character varying(50),
    product_code character varying(50),
    user_id uuid NOT NULL,
    is_active boolean NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_customer_wallet_card PRIMARY KEY (id)
);
END $$;
 
DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.debit_authorization (
    id uuid NOT NULL,
    correlation_id bigint NOT NULL,
    ocean_txn_guid bigint NOT NULL,
    banking_customer_no text,
    card_no text,
    account_no text,
    account_branch text,
    account_suffix text,
    account_currency integer,
    iban text,
    acquirer_country_code text,
    national_switch_id text,
    acquirer_id text,
    terminal_id text,
    merchant_id text,
    merchant_name text,
    rrn text,
    provision_code text,
    transaction_amount numeric NOT NULL,
    transaction_currency integer NOT NULL,
    billing_amount numeric NOT NULL,
    billing_currency integer NOT NULL,
    replacement_transaction_amount numeric,
    replacement_transaction_currency integer,
    replacement_billing_amount numeric,
    replacement_billing_currency integer,
    request_date bigint NOT NULL,
    request_time bigint NOT NULL,
    mcc text,
    is_simulation boolean NOT NULL,
    is_advice boolean NOT NULL,
    request_type text,
    transaction_type text,
    expiration_time integer,
    channel text,
    terminal_type text,
    banking_ref_no text,
    transaction_source character(1) NOT NULL,
    card_dci character(1) NOT NULL,
    card_brand character(1) NOT NULL,
    entry_type character(1) NOT NULL,
    partial_acceptor boolean,
    transfer_information_type character(1),
    transfer_information_name text,
    transfer_information_card_no text,
    businesss_application_identifier text,
    qr_data text,
    security_level_indicator integer,
    is_return boolean NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_debit_authorization PRIMARY KEY (id)
);
END $$;
