DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.chargeback (
    id uuid NOT NULL,
    transaction_id uuid NOT NULL,
    transaction_type varchar(50) NOT NULL,
    amount numeric(18,4) NOT NULL,
    currency varchar(10) NOT NULL,
    wallet_number varchar(20)  NOT NULL,
    status varchar(20) NOT NULL,
    description varchar(500) ,
    user_id uuid NOT NULL,
    merchant_id varchar(50) ,
    merchant_name varchar(200) NOT NULL,
    transaction_date timestamp without time zone NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    user_name varchar(200) NOT NULL, 
    order_id varchar(50) NOT NULL
);
END $$;
