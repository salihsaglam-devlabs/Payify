DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'merchant'
          AND table_name   = 'merchant_pyhsical_pos'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'merchant'
          AND table_name   = 'merchant_physical_pos'
    )
    THEN
ALTER TABLE merchant.merchant_pyhsical_pos
    RENAME TO merchant_physical_pos;
END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='posting'
          AND table_name='transaction'
          AND column_name='transaction_source'
    )
    AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='posting'
          AND table_name='transaction'
          AND column_name='pf_transaction_source'
    )
    THEN
ALTER TABLE posting.transaction
    RENAME COLUMN transaction_source TO pf_transaction_source;
END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='merchant'
          AND table_name='transaction'
          AND column_name='transaction_source'
    )
    AND NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema='merchant'
          AND table_name='transaction'
          AND column_name='pf_transaction_source'
    )
    THEN
ALTER TABLE merchant.transaction
    RENAME COLUMN transaction_source TO pf_transaction_source;
END IF;
END$$;

ALTER TABLE posting.transaction
    ADD COLUMN IF NOT EXISTS merchant_physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE merchant.transaction
    ADD COLUMN IF NOT EXISTS end_of_day_status varchar(50) NOT NULL DEFAULT 'Pending';

ALTER TABLE merchant.transaction
    ADD COLUMN IF NOT EXISTS merchant_physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE merchant.transaction
    ADD COLUMN IF NOT EXISTS physical_pos_eod_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE merchant.transaction
    ADD COLUMN IF NOT EXISTS physical_pos_old_eod_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE bank.transaction
    ADD COLUMN IF NOT EXISTS end_of_day_status varchar(50) NOT NULL DEFAULT 'Pending';

ALTER TABLE bank.transaction
    ADD COLUMN IF NOT EXISTS merchant_physical_pos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE bank.transaction
    ADD COLUMN IF NOT EXISTS physical_pos_eod_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE merchant.merchant_physical_pos
    ADD COLUMN IF NOT EXISTS pos_merchant_id varchar(50);

ALTER TABLE merchant.merchant_physical_pos
    ADD COLUMN IF NOT EXISTS pos_terminal_id varchar(50);

CREATE TABLE IF NOT EXISTS physical.end_of_day (
    id uuid NOT NULL,
    merchant_id uuid NOT NULL,
    batch_id character varying(50) NOT NULL,
    pos_merchant_id character varying(50) NOT NULL,
    pos_terminal_id character varying(50) NOT NULL,
    date timestamp without time zone NOT NULL,
    sale_count integer NOT NULL,
    void_count integer NOT NULL,
    refund_count integer NOT NULL,
    installment_sale_count integer NOT NULL,
    failed_count integer NOT NULL,
    sale_amount numeric(18,4) NOT NULL,
    void_amount numeric(18,4) NOT NULL,
    refund_amount numeric(18,4) NOT NULL,
    installment_sale_amount numeric(18,4) NOT NULL,
    currency character varying(10) NOT NULL,
    institution_id integer NOT NULL,
    vendor character varying(20),
    serial_number character varying(200) NOT NULL,
    status character varying(50) NOT NULL DEFAULT 'Pending',
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_end_of_day PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS physical.reconciliation_transaction (
    id uuid NOT NULL,
    payment_id character varying(50) NOT NULL,
    batch_id character varying(50) NOT NULL,
    date timestamp without time zone NOT NULL,
    type character varying(50) NOT NULL,
    status character varying(20) NOT NULL,
    currency character varying(10) NOT NULL,
    merchant_id character varying(50) NOT NULL,
    terminal_id character varying(50) NOT NULL,
    amount numeric(18,4) NOT NULL,
    point_amount numeric(18,4) NOT NULL,
    installment integer NOT NULL,
    masked_card_no character varying(50) NOT NULL,
    bin_number character varying(10) NOT NULL,
    provision_no character varying(50),
    issuer_bank_id character varying(20) NOT NULL,
    acquirer_response_code character varying(10),
    institution_id integer NOT NULL,
    vendor character varying(20),
    rrn character varying(50),
    stan character varying(50),
    pos_entry_mode character varying(20),
    pin_entry_info character varying(20),
    bank_ref character varying(50) NOT NULL,
    original_ref character varying(50),
    pf_merchant_id uuid NOT NULL,
    conversation_id character varying(50) NOT NULL,
    client_ip_address character varying(50),
    serial_number character varying(200) NOT NULL,
    reconciliation_status character varying(50) NOT NULL DEFAULT 'Pending',
    merchant_transaction_id uuid NOT NULL,
    unacceptable_transaction_id uuid NOT NULL,
    physical_pos_eod_id uuid NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_reconciliation_transaction PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS physical.unacceptable_transaction (
    id uuid NOT NULL,
    payment_id character varying(50) NOT NULL,
    batch_id character varying(50) NOT NULL,
    date timestamp without time zone NOT NULL,
    type character varying(50) NOT NULL,
    status character varying(20) NOT NULL,
    currency character varying(10) NOT NULL,
    merchant_id character varying(50) NOT NULL,
    terminal_id character varying(50) NOT NULL,
    amount numeric(18,4) NOT NULL,
    point_amount numeric(18,4) NOT NULL,
    installment integer NOT NULL,
    masked_card_no character varying(50) NOT NULL,
    bin_number character varying(10) NOT NULL,
    provision_no character varying(50),
    issuer_bank_id character varying(20) NOT NULL,
    acquirer_response_code character varying(10),
    institution_id integer NOT NULL,
    vendor character varying(20),
    rrn character varying(50),
    stan character varying(50),
    pos_entry_mode character varying(20),
    pin_entry_info character varying(20),
    bank_ref character varying(50) NOT NULL,
    original_ref character varying(50),
    pf_merchant_id uuid NOT NULL,
    conversation_id character varying(50) NOT NULL,
    client_ip_address character varying(50),
    serial_number character varying(200) NOT NULL,
    gateway character varying(20),
    error_code character varying(20),
    error_message character varying(300),
    current_status character varying(50) NOT NULL,
    physical_pos_eod_id uuid NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50),
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_unacceptable_transaction PRIMARY KEY (id)
);     