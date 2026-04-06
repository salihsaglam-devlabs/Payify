DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.card_topup_request (
    id uuid NOT NULL,
    amount DECIMAL(18, 2),
    commission_rate DECIMAL(18, 2),
    commission_total DECIMAL(18, 2),
    bsmv_total DECIMAL(18, 2),
    fee DECIMAL(18, 4),
    currency varchar(50),
    card_brand varchar(50),
    card_type varchar(50),
    card_number varchar(50),
    error_code varchar(10),
    error_message varchar(256),
    threed_session_id varchar(200),
    provision_number varchar(50),
    order_id varchar(50),
    conversation_id varchar(100),
    wallet_id uuid,
    wallet_number varchar(50),
    cancel_description varchar(300),
    name varchar(150),
    status varchar(50) ,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL
);
END $$;

DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
      FROM information_schema.columns
      WHERE table_schema='core' AND table_name='transaction' AND column_name='card_topup_request_id'
  ) THEN
    ALTER TABLE core.transaction ADD card_topup_request_id uuid NULL;
  END IF;
END $$;
