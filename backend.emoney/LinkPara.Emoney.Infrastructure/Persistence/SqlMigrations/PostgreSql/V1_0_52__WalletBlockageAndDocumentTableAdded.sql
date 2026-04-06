DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.wallet_blockage (
    id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    wallet_number varchar(20) NOT NULL,
    account_name varchar(100) NOT NULL,
    wallet_currency_code varchar(5) NOT NULL,
    cash_blockage_amount numeric(18,2) NOT NULL,
    credit_blockage_amount numeric(18,2) NOT NULL,    
    operation_type varchar(50)  NOT NULL,
    blockage_status varchar(20) NOT NULL,
    blockage_description varchar(1000) ,
    blockage_start_date timestamp without time zone NOT NULL,
    blockage_end_date timestamp without time zone ,    
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL
);
END $$;

DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.wallet_blockage_document (   
    id uuid NOT NULL,
    wallet_blockage_id uuid NOT NULL,
    wallet_id uuid NOT NULL,
    document_id uuid NOT NULL,
    document_type_id uuid NOT NULL,
    description varchar(1000),
    file_name varchar(200)  NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) ,
    record_status varchar(50)NOT NULL
);
END $$;
