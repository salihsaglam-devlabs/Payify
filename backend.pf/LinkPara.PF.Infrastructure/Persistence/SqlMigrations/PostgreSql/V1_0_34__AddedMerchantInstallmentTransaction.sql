DO $$
BEGIN

CREATE TABLE IF NOT EXISTS merchant.installment_transaction (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	conversation_id varchar(50) NOT NULL,
	ip_address varchar(50) NULL,
	transaction_type varchar(50) NOT NULL,
	transaction_status varchar(50) NOT NULL,
	order_id varchar(50) NULL,
	amount numeric(18, 4) NOT NULL,
	point_amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	installment_count int4 NOT NULL,
	bin_number varchar(10) NOT NULL,
	card_number varchar(50) NOT NULL,
	has_cvv bool NOT NULL,
	has_expiry_date bool NOT NULL,
	is_international bool NOT NULL,
	is_amex bool NOT NULL,
	is_reverse bool NOT NULL,
	reverse_date timestamp NOT NULL,
	is_return bool NOT NULL,
	return_date timestamp NOT NULL,
	return_amount numeric(18, 4) NOT NULL,
	returned_transaction_id varchar(50) NULL,
	is_pre_close bool NOT NULL,
	pre_close_date timestamp NOT NULL,
	pre_close_transaction_id varchar(50) NULL,
	is3ds bool NOT NULL,
	three_d_session_id varchar(200) NULL,
	bank_commission_rate numeric(4, 2) NOT NULL,
	bank_commission_amount numeric(18, 4) NOT NULL,
	issuer_bank_code int4 NOT NULL,
	acquire_bank_code int4 NOT NULL,
	card_transaction_type varchar(50) NULL,
	integration_mode varchar(50) NOT NULL,
	response_code varchar(20) NULL,
	response_description varchar(1000) NULL,
	transaction_start_date timestamp NOT NULL,
	transaction_end_date timestamp NOT NULL,
	vpos_id uuid NOT NULL,
	language_code varchar(100) NULL,
	batch_status varchar(50) NOT NULL,
	card_type varchar(50) NOT NULL,
	transaction_date date NOT NULL,
	is_chargeback bool NOT NULL,
	is_suspecious bool NOT NULL,
	suspecious_description varchar(500) NULL,
	merchant_customer_name varchar(200) NULL,
	merchant_customer_phone_number varchar(30) NULL,
	description varchar(300) NULL,
	card_holder_name varchar(200) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	return_status varchar(50) NOT NULL DEFAULT ''::character varying,
	created_name_by varchar(200) NULL,
	amount_without_bank_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	amount_without_commissions numeric(18, 4) NOT NULL DEFAULT 0.0,
	bsmv_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	pf_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	pf_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	pf_net_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	pricing_profile_item_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	provision_number varchar(50) NULL,
	vpos_name varchar(100) NULL,
	bank_payment_date date NOT NULL DEFAULT '-infinity'::date,
	pf_payment_date timestamp NOT NULL DEFAULT '-infinity'::date,
	is_manual_return bool NOT NULL DEFAULT false,
	posting_item_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	point_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	point_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	is_on_us_payment bool NOT NULL DEFAULT false,
	merchant_customer_phone_code varchar(10) NULL,
	pf_per_transaction_fee numeric(18, 4) NOT NULL DEFAULT 0.0,
	service_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	service_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	blockage_status varchar(50) NOT NULL DEFAULT ''::character varying,
	last_chargeback_activity_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	sub_merchant_id uuid NULL,
	sub_merchant_name varchar(150) NULL,
	sub_merchant_number varchar(15) NULL,
	amount_without_parent_merchant_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_installment_transaction PRIMARY KEY (id)
);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_acquire_bank_code ON merchant.installment_transaction (acquire_bank_code);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_batch_status_record_status ON merchant.installment_transaction (batch_status, record_status);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_issuer_bank_code ON merchant.installment_transaction (issuer_bank_code);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_merchant_id ON merchant.installment_transaction (merchant_id);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_posting_item_id ON merchant.installment_transaction (posting_item_id);

CREATE INDEX IF NOT EXISTS ix_installment_transaction_transaction_date ON merchant.installment_transaction (transaction_date);
END $$;

-- pricing_profile tablosundan kolon düşür
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'pricing_profile' 
        AND column_name = 'profile_settlement_mode'
    ) THEN
        ALTER TABLE core.pricing_profile DROP COLUMN profile_settlement_mode;
    END IF;
END $$;

-- transaction tablosuna kolon ekle
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'merchant' 
        AND table_name = 'transaction' 
        AND column_name = 'is_per_installment'
    ) THEN
        ALTER TABLE merchant.transaction ADD is_per_installment boolean NOT NULL DEFAULT FALSE;
    END IF;
END $$;

-- three_d_verification tablosuna cost_profile_item_id ekle
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'three_d_verification' 
        AND column_name = 'cost_profile_item_id'
    ) THEN
        ALTER TABLE core.three_d_verification ADD cost_profile_item_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $$;

-- three_d_verification tablosuna is_per_installment ekle
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' 
        AND table_name = 'three_d_verification' 
        AND column_name = 'is_per_installment'
    ) THEN
        ALTER TABLE core.three_d_verification ADD is_per_installment boolean NOT NULL DEFAULT FALSE;
    END IF;
END $$;