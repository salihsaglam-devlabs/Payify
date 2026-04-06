DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.onus_payment_request (
    id uuid NOT NULL,
    merchant_name varchar(150) NOT NULL,
    merchant_number varchar(15) NOT NULL,
    amount numeric(18,4) NOT NULL,
    currency varchar(10) NOT NULL,
    status varchar(20)  NOT NULL,
    phone_code varchar(5) NOT NULL,
    phone_number varchar(15) NOT NULL,
    wallet_number varchar(20),
    wallet_id uuid NOT NULL,
    transaction_id uuid NOT NULL,
    expire_date timestamp without time zone NOT NULL,
    request_date timestamp without time zone NOT NULL,
    merchant_transaction_Id varchar(20),
    error_code varchar(30),
    error_message varchar(256),
    cancel_description varchar(300),
    conversation_id varchar(100),
    order_id varchar(50),    
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    chargeback_description varchar(300),
    transaction_date timestamp without time zone,
    user_id uuid NOT NULL, 
    user_name character varying(200) 
);
END $$;
