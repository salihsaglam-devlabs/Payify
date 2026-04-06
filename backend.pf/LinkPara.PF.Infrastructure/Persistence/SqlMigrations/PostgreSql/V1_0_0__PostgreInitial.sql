-- vpos.bank_api_info definition
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'vpos') THEN
        CREATE SCHEMA vpos;
    END IF;
END $EF$;

-- core.contact_person definition
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
        CREATE SCHEMA core;
    END IF;
END $EF$;

-- bank.bank definition
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'bank') THEN
        CREATE SCHEMA bank;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'card') THEN
        CREATE SCHEMA card;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'merchant') THEN
        CREATE SCHEMA merchant;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'api') THEN
        CREATE SCHEMA api;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'hpp') THEN
        CREATE SCHEMA hpp;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'limit') THEN
        CREATE SCHEMA "limit";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'link') THEN
        CREATE SCHEMA link;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'posting') THEN
        CREATE SCHEMA posting;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'submerchant') THEN
        CREATE SCHEMA submerchant;
    END IF;
END $EF$;

-- Drop table

-- DROP TABLE core.contact_person;

CREATE TABLE IF NOT EXISTS core.contact_person (
	id uuid NOT NULL,
	contact_person_type varchar(50) NOT NULL,
	identity_number varchar(11) NULL,
	"name" varchar(100) NOT NULL,
	surname varchar(100) NOT NULL,
	email varchar(256) NOT NULL,
	company_email varchar(256) NULL,
	birth_date timestamp NOT NULL,
	company_phone_number varchar(20) NOT NULL,
	mobile_phone_number varchar(20) NOT NULL,
	mobile_phone_number_second varchar(20) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_contact_person PRIMARY KEY (id)
);

-- core.currency definition

-- Drop table

-- DROP TABLE core.currency;

CREATE TABLE IF NOT EXISTS core.currency (
	id uuid NOT NULL,
	code varchar(10) NOT NULL,
	"name" varchar(50) NOT NULL,
	symbol varchar(5) NOT NULL,
	"number" int4 NOT NULL,
	currency_type varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT ak_currency_code UNIQUE (code),
	CONSTRAINT pk_currency PRIMARY KEY (id)
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_currency_code ON core.currency USING btree (code);

-- core.customer definition

-- Drop table

-- DROP TABLE core.customer;

CREATE TABLE IF NOT EXISTS core.customer (
	id uuid NOT NULL,
	customer_status varchar(50) NOT NULL,
	company_type varchar(50) NOT NULL,
	commercial_title varchar(100) NOT NULL,
	trade_registration_number varchar(16) NOT NULL,
	tax_administration varchar(200) NOT NULL,
	tax_number varchar(11) NOT NULL,
	mersis_number varchar(16) NULL,
	country int4 NOT NULL,
	country_name varchar(200) NOT NULL,
	city int4 NOT NULL,
	city_name varchar(200) NOT NULL,
	district int4 NOT NULL,
	district_name varchar(200) NOT NULL,
	postal_code varchar(5) NOT NULL,
	address varchar(256) NOT NULL,
	contact_person_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	customer_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	customer_number int4 NOT NULL DEFAULT 0,
	CONSTRAINT pk_customer PRIMARY KEY (id),
	CONSTRAINT fk_customer_contact_person_contact_person_id FOREIGN KEY (contact_person_id) REFERENCES core.contact_person(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_customer_contact_person_id ON core.customer USING btree (contact_person_id);



-- Drop table

-- DROP TABLE bank.bank;

CREATE TABLE IF NOT EXISTS bank.bank (
	id uuid NOT NULL,
	code int4 NOT NULL,
	"name" varchar(100) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT ak_bank_code UNIQUE (code),
	CONSTRAINT pk_bank PRIMARY KEY (id)
);


-- bank.bank_backup definition

-- Drop table

-- DROP TABLE bank.bank_backup;

CREATE TABLE IF NOT EXISTS bank.bank_backup (
	id uuid NULL,
	code int4 NULL,
	"name" varchar(100) NULL,
	create_date timestamp NULL,
	update_date timestamp NULL,
	created_by varchar(50) NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NULL
);


-- bank.health_check_transaction definition

-- Drop table

-- DROP TABLE bank.health_check_transaction;

CREATE TABLE IF NOT EXISTS bank.health_check_transaction (
	id uuid NOT NULL,
	transaction_type varchar(50) NOT NULL,
	transaction_status varchar(50) NOT NULL,
	acquire_bank_code int4 NOT NULL,
	bank_transaction_date timestamp NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_health_check_transaction PRIMARY KEY (id)
);


-- bank.acquire_bank definition

-- Drop table

-- DROP TABLE bank.acquire_bank;

CREATE TABLE IF NOT EXISTS bank.acquire_bank (
	id uuid NOT NULL,
	bank_code int4 NOT NULL,
	end_of_day_hour int4 NOT NULL,
	end_of_day_minute int4 NOT NULL,
	accept_amex bool NOT NULL,
	has_submerchant_integration bool NOT NULL,
	card_network varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	restrict_own_card_not_on_us bool NOT NULL DEFAULT false,
	CONSTRAINT pk_acquire_bank PRIMARY KEY (id),
	CONSTRAINT fk_acquire_bank_bank_bank_id FOREIGN KEY (bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_acquire_bank_bank_code ON bank.acquire_bank USING btree (bank_code);


-- bank.api_key definition

-- Drop table

-- DROP TABLE bank.api_key;

CREATE TABLE IF NOT EXISTS bank.api_key (
	id uuid NOT NULL,
	acquire_bank_id uuid NOT NULL,
	"key" varchar(50) NULL,
	mapping_name varchar(50) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	category varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_api_key PRIMARY KEY (id),
	CONSTRAINT fk_api_key_acquire_bank_acquire_bank_id FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_api_key_acquire_bank_id ON bank.api_key USING btree (acquire_bank_id);


-- bank.health_check definition

-- Drop table

-- DROP TABLE bank.health_check;

CREATE TABLE IF NOT EXISTS bank.health_check (
	id uuid NOT NULL,
	acquire_bank_id uuid NOT NULL,
	last_check_date timestamp NOT NULL,
	total_transaction_count int4 NOT NULL,
	fail_transaction_count int4 NOT NULL,
	fail_transaction_rate int4 NOT NULL,
	health_check_type varchar(50) NOT NULL,
	is_health_check_allowed bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	allowed_check_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	CONSTRAINT pk_health_check PRIMARY KEY (id),
	CONSTRAINT fk_health_check_acquire_bank_acquire_bank_id FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_health_check_acquire_bank_id ON bank.health_check USING btree (acquire_bank_id);


-- bank."limit" definition

-- Drop table

-- DROP TABLE bank."limit";

CREATE TABLE IF NOT EXISTS bank."limit" (
	id uuid NOT NULL,
	acquire_bank_id uuid NOT NULL,
	monthly_limit_amount numeric(18, 4) NOT NULL,
	margin_ratio int4 NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	bank_limit_type varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	last_valid_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	is_expired bool NOT NULL DEFAULT false,
	CONSTRAINT pk_limit PRIMARY KEY (id),
	CONSTRAINT fk_limit_acquire_bank_acquire_bank_id FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_limit_acquire_bank_id ON bank."limit" USING btree (acquire_bank_id);


-- bank."transaction" definition

-- Drop table

-- DROP TABLE bank."transaction";

CREATE TABLE IF NOT EXISTS bank."transaction" (
	id uuid NOT NULL,
	transaction_type varchar(50) NOT NULL,
	transaction_status varchar(50) NOT NULL,
	order_id varchar(50) NULL,
	amount numeric(18, 4) NOT NULL,
	point_amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	installment_count int4 NOT NULL,
	card_number varchar(50) NULL,
	is_reverse bool NOT NULL,
	reverse_date timestamp NOT NULL,
	is3ds bool NOT NULL,
	issuer_bank_code int4 NOT NULL,
	acquire_bank_code int4 NOT NULL,
	merchant_code varchar(200) NULL,
	sub_merchant_code varchar(200) NULL,
	bank_order_id varchar(50) NULL,
	rrn_number varchar(50) NULL,
	approval_code varchar(50) NULL,
	bank_response_code varchar(50) NULL,
	bank_response_description varchar(1000) NULL,
	bank_transaction_date timestamp NOT NULL,
	transaction_start_date timestamp NOT NULL,
	transaction_end_date timestamp NOT NULL,
	vpos_id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	stan varchar(50) NULL,
	CONSTRAINT pk_transaction PRIMARY KEY (id),
	CONSTRAINT fk_transaction_bank_acquire_bank_code FOREIGN KEY (acquire_bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT,
	CONSTRAINT fk_transaction_bank_issuer_bank_code FOREIGN KEY (issuer_bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_transaction_acquire_bank_code ON bank.transaction USING btree (acquire_bank_code);
CREATE INDEX IF NOT EXISTS ix_transaction_issuer_bank_code ON bank.transaction USING btree (issuer_bank_code);

-- merchant.response_code definition

-- Drop table

-- DROP TABLE merchant.response_code;

CREATE TABLE IF NOT EXISTS merchant.response_code (
	id uuid NOT NULL,
	response_code varchar(10) NOT NULL,
	description varchar(100) NOT NULL,
	display_message_tr varchar(500) NOT NULL,
	display_message_en varchar(500) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_response_code PRIMARY KEY (id)
);

-- bank.response_code definition

-- Drop table

-- DROP TABLE bank.response_code;

CREATE TABLE IF NOT EXISTS bank.response_code (
	id uuid NOT NULL,
	bank_code int4 NOT NULL,
	response_code varchar(50) NOT NULL,
	description varchar(256) NOT NULL,
	process_timeout_management bool NOT NULL,
	merchant_response_code_id uuid NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_response_code PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_response_code_bank_code ON bank.response_code USING btree (bank_code);
CREATE INDEX IF NOT EXISTS ix_response_code_merchant_response_code_id1 ON bank.response_code USING btree (merchant_response_code_id);


-- bank.response_code foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'bank'
          AND table_name = 'response_code'
          AND constraint_name = 'fk_response_code_bank_bank_code'
    ) THEN
        ALTER TABLE bank.response_code
        ADD CONSTRAINT fk_response_code_bank_bank_code
        FOREIGN KEY (bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'bank'
          AND table_name = 'response_code'
          AND constraint_name = 'fk_response_code_merchant_response_code_merchant_response_code'
    ) THEN
        ALTER TABLE bank.response_code
        ADD CONSTRAINT fk_response_code_merchant_response_code_merchant_response_code
        FOREIGN KEY (merchant_response_code_id)
        REFERENCES merchant.response_code(id);
    END IF;
END $$;

-- vpos.vpos definition

-- Drop table

-- DROP TABLE vpos.vpos;

CREATE TABLE IF NOT EXISTS vpos.vpos (
	id uuid NOT NULL,
	"name" varchar(100) NULL,
	vpos_status varchar(50) NOT NULL,
	acquire_bank_id uuid NOT NULL,
	security_type varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	vpos_type varchar(50) NOT NULL DEFAULT ''::character varying,
	blockage_code int4 NULL,
	is_on_us_vpos bool NOT NULL DEFAULT false,
	CONSTRAINT pk_vpos PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_vpos_acquire_bank_id ON vpos.vpos USING btree (acquire_bank_id);

-- card.bin_backup definition

-- Drop table

-- DROP TABLE card.bin_backup;

CREATE TABLE IF NOT EXISTS card.bin_backup (
	id uuid NULL,
	bin_number varchar(10) NULL,
	card_brand varchar(50) NULL,
	card_type varchar(50) NULL,
	card_sub_type varchar(50) NULL,
	card_network varchar(50) NULL,
	country int4 NULL,
	country_name varchar(200) NULL,
	is_virtual bool NULL,
	bank_code int4 NULL,
	create_date timestamp NULL,
	update_date timestamp NULL,
	created_by varchar(50) NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NULL
);


-- card.loyalty_backup definition

-- Drop table

-- DROP TABLE card.loyalty_backup;

CREATE TABLE IF NOT EXISTS card.loyalty_backup (
	id uuid NULL,
	"name" varchar(50) NULL,
	bank_code int4 NULL,
	create_date timestamp NULL,
	update_date timestamp NULL,
	created_by varchar(50) NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NULL
);


-- card.bin definition

-- Drop table

-- DROP TABLE card.bin;

CREATE TABLE IF NOT EXISTS card.bin (
	id uuid NOT NULL,
	bin_number varchar(10) NOT NULL,
	card_brand varchar(50) NOT NULL,
	card_type varchar(50) NOT NULL,
	card_sub_type varchar(50) NOT NULL,
	card_network varchar(50) NOT NULL,
	country int4 NOT NULL,
	country_name varchar(200) NOT NULL,
	is_virtual bool NOT NULL,
	bank_code int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_bin PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_bin_bank_code ON card.bin USING btree (bank_code);
CREATE UNIQUE INDEX IF NOT EXISTS ix_bin_bin_number ON card.bin USING btree (bin_number);


-- card.loyalty definition

-- Drop table

-- DROP TABLE card.loyalty;

CREATE TABLE IF NOT EXISTS card.loyalty (
	id uuid NOT NULL,
	"name" varchar(50) NOT NULL,
	bank_code int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_loyalty PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_loyalty_bank_code ON card.loyalty USING btree (bank_code);


-- card.loyalty_exception definition

-- Drop table

-- DROP TABLE card.loyalty_exception;

CREATE TABLE IF NOT EXISTS card.loyalty_exception (
	id uuid NOT NULL,
	bank_code int4 NOT NULL,
	counter_bank_code int4 NOT NULL,
	allow_on_us bool NOT NULL,
	allow_installment bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	allow_point bool NOT NULL DEFAULT false,
	CONSTRAINT pk_loyalty_exception PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_loyalty_exception_bank_code ON card.loyalty_exception USING btree (bank_code);
CREATE INDEX IF NOT EXISTS ix_loyalty_exception_counter_bank_code ON card.loyalty_exception USING btree (counter_bank_code);


-- card."token" definition

-- Drop table

-- DROP TABLE card."token";

CREATE TABLE IF NOT EXISTS card."token" (
	id uuid NOT NULL,
	"token" varchar(50) NOT NULL,
	expiry_date timestamp NOT NULL,
	merchant_id uuid NOT NULL,
	cvv_encrypted varchar(300) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	card_number_encrypted varchar(300) NOT NULL DEFAULT ''::character varying,
	expire_date_encrypted varchar(300) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_token PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_token_expiry_date ON card.token USING btree (expiry_date);
CREATE INDEX IF NOT EXISTS ix_token_merchant_id ON card.token USING btree (merchant_id);


-- card.bin foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'card'
          AND table_name = 'bin'
          AND constraint_name = 'fk_bin_bank_bank_code'
    ) THEN
        ALTER TABLE card.bin
        ADD CONSTRAINT fk_bin_bank_bank_code
        FOREIGN KEY (bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;



-- card.loyalty foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'card'
          AND table_name = 'loyalty'
          AND constraint_name = 'fk_loyalty_bank_bank_code'
    ) THEN
        ALTER TABLE card.loyalty
        ADD CONSTRAINT fk_loyalty_bank_bank_code
        FOREIGN KEY (bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;


-- card.loyalty_exception foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'card'
          AND table_name = 'loyalty_exception'
          AND constraint_name = 'fk_loyalty_exception_bank_bank_code'
    ) THEN
        ALTER TABLE card.loyalty_exception
        ADD CONSTRAINT fk_loyalty_exception_bank_bank_code
        FOREIGN KEY (bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'card'
          AND table_name = 'loyalty_exception'
          AND constraint_name = 'fk_loyalty_exception_bank_counter_bank_code'
    ) THEN
        ALTER TABLE card.loyalty_exception
        ADD CONSTRAINT fk_loyalty_exception_bank_counter_bank_code
        FOREIGN KEY (counter_bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;


-- card."token" foreign keys


-- merchant.merchant_content definition

-- Drop table

-- DROP TABLE merchant.merchant_content;

CREATE TABLE IF NOT EXISTS merchant.merchant_content (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	"name" varchar(50) NOT NULL,
	content_source varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_merchant_content PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_content_record_status ON merchant.merchant_content USING btree (record_status);


-- merchant.merchant definition

-- Drop table

-- DROP TABLE merchant.merchant;

CREATE TABLE IF NOT EXISTS merchant.merchant (
	id uuid NOT NULL,
	"name" varchar(150) NOT NULL,
	"number" varchar(15) NOT NULL,
	merchant_status varchar(50) NOT NULL,
	application_channel varchar(50) NOT NULL,
	integration_mode varchar(50) NOT NULL,
	mcc_code varchar(4) NULL,
	customer_id uuid NOT NULL,
	"language" varchar(100) NULL,
	web_site_url varchar(150) NOT NULL,
	monthly_turnover numeric(18, 4) NOT NULL,
	phone_code varchar(15) NOT NULL,
	agreement_date timestamp NOT NULL,
	sales_person_id uuid NULL,
	payment_due_day int4 NOT NULL,
	is3d_required bool NOT NULL,
	is_document_required bool NOT NULL,
	is_manuel_payment3d_required bool NOT NULL,
	half_secure_allowed bool NOT NULL,
	installment_allowed bool NOT NULL,
	international_card_allowed bool NOT NULL,
	pre_authorization_allowed bool NOT NULL,
	financial_transaction_allowed bool NOT NULL,
	payment_allowed bool NOT NULL,
	reject_reason varchar(256) NULL,
	parameter_value varchar(100) NULL,
	pricing_profile_number varchar(6) NULL,
	merchant_pool_id uuid NOT NULL,
	merchant_integrator_id uuid NULL,
	contact_person_id uuid NULL,
	global_merchant_id varchar(8) NULL,
	annulment_code varchar(2) NULL,
	annulment_description varchar(300) NULL,
	annulment_date timestamp NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	is_post_auth_amount_higher_allowed bool NOT NULL DEFAULT false,
	iks_notification_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	annulment_id varchar(6) NULL,
	annulment_additional_info varchar(300) NULL,
	is_annulment bool NULL,
	is_link_payment3d_required bool NOT NULL DEFAULT false,
	payment_return_allowed bool NOT NULL DEFAULT false,
	payment_reverse_allowed bool NOT NULL DEFAULT false,
	is_hosted_payment3d_required bool NOT NULL DEFAULT false,
	is_return_approved bool NOT NULL DEFAULT false,
	is_cvv_payment_allowed bool NOT NULL DEFAULT false,
	is_excess_return_allowed bool NOT NULL DEFAULT false,
	hosting_tax_no varchar(11) NULL,
	posting_payment_channel varchar(50) NOT NULL DEFAULT 'BankAccount'::character varying,
	merchant_type varchar(50) NOT NULL DEFAULT ''::character varying,
	is_invoice_commission_reflected bool NOT NULL DEFAULT false,
	parent_merchant_id uuid NULL,
	parent_merchant_name varchar(150) NULL,
	parent_merchant_number varchar(15) NULL,
	CONSTRAINT pk_merchant PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_contact_person_id ON merchant.merchant USING btree (contact_person_id);
CREATE INDEX IF NOT EXISTS ix_merchant_customer_id ON merchant.merchant USING btree (customer_id);
CREATE INDEX IF NOT EXISTS ix_merchant_mcc_code ON merchant.merchant USING btree (mcc_code);
CREATE INDEX IF NOT EXISTS ix_merchant_merchant_integrator_id ON merchant.merchant USING btree (merchant_integrator_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_merchant_merchant_pool_id ON merchant.merchant USING btree (merchant_pool_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'card'
          AND table_name = 'token'
          AND constraint_name = 'fk_token_merchant_merchant_id'
    ) THEN
        ALTER TABLE card."token"
        ADD CONSTRAINT fk_token_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- merchant.counter definition

-- Drop table

-- DROP TABLE merchant.counter;

CREATE TABLE IF NOT EXISTS merchant.counter (
	id uuid NOT NULL,
	number_counter int4 NOT NULL GENERATED BY DEFAULT AS IDENTITY( INCREMENT BY 1 MINVALUE 1 MAXVALUE 2147483647 START 1 CACHE 1 NO CYCLE),
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_counter PRIMARY KEY (id)
);
CREATE UNIQUE INDEX IF NOT EXISTS ix_counter_number_counter ON merchant.counter USING btree (number_counter);


-- merchant.deduction_transaction definition

-- Drop table

-- DROP TABLE merchant.deduction_transaction;

CREATE TABLE IF NOT EXISTS merchant.deduction_transaction (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	posting_balance_id uuid NOT NULL,
	merchant_deduction_id uuid NOT NULL,
	deduction_type varchar(50) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_deduction_transaction PRIMARY KEY (id)
);


-- merchant.integrator definition

-- Drop table

-- DROP TABLE merchant.integrator;

CREATE TABLE IF NOT EXISTS merchant.integrator (
	id uuid NOT NULL,
	"name" varchar(100) NOT NULL,
	commission_rate numeric(4, 2) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_integrator PRIMARY KEY (id)
);


-- merchant.mcc definition

-- Drop table

-- DROP TABLE merchant.mcc;

CREATE TABLE IF NOT EXISTS merchant.mcc (
	id uuid NOT NULL,
	code varchar(4) NOT NULL,
	"name" varchar(256) NOT NULL,
	max_individual_installment_count int4 NOT NULL,
	max_corporate_installment_count int4 NOT NULL,
	description varchar(300) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT ak_mcc_code UNIQUE (code),
	CONSTRAINT pk_mcc PRIMARY KEY (id)
);

-- merchant.pool definition

-- Drop table

-- DROP TABLE merchant.pool;

CREATE TABLE IF NOT EXISTS merchant.pool (
	id uuid NOT NULL,
	merchant_pool_status varchar(50) NOT NULL,
	merchant_name text NULL,
	company_type varchar(50) NOT NULL,
	commercial_title varchar(100) NOT NULL,
	web_site_url varchar(100) NOT NULL,
	monthly_turnover numeric(18, 4) NOT NULL,
	postal_code varchar(5) NOT NULL,
	address varchar(256) NOT NULL,
	phone_code varchar(15) NOT NULL,
	country int4 NOT NULL,
	country_name varchar(200) NOT NULL,
	city int4 NOT NULL,
	city_name varchar(200) NOT NULL,
	district int4 NOT NULL,
	district_name varchar(200) NOT NULL,
	tax_administration varchar(200) NOT NULL,
	tax_number varchar(11) NOT NULL,
	trade_registration_number varchar(16) NOT NULL,
	iban varchar(26) NOT NULL,
	bank_code int4 NOT NULL,
	currency_code varchar(10) NULL,
	reject_reason varchar(256) NULL,
	parameter_value varchar(100) NULL,
	channel varchar(50) NULL,
	email varchar(256) NOT NULL,
	company_email varchar(256) NOT NULL,
	authorized_person_identity_number varchar(11) NULL,
	authorized_person_name varchar(100) NOT NULL,
	authorized_person_surname varchar(100) NOT NULL,
	authorized_person_birth_date timestamp NOT NULL,
	authorized_person_company_phone_number varchar(20) NOT NULL,
	authorized_person_mobile_phone_number varchar(20) NOT NULL,
	authorized_person_mobile_phone_number_second varchar(20) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	posting_payment_channel varchar(50) NOT NULL DEFAULT 'BankAccount'::character varying,
	wallet_number varchar(26) NULL,
	merchant_type varchar(50) NOT NULL DEFAULT ''::character varying,
	is_invoice_commission_reflected bool NOT NULL DEFAULT false,
	parent_merchant_id uuid NULL,
	parent_merchant_name varchar(150) NULL,
	parent_merchant_number varchar(15) NULL,
	CONSTRAINT pk_pool PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_pool_bank_code ON merchant.pool USING btree (bank_code);
CREATE INDEX IF NOT EXISTS ix_pool_currency_code ON merchant.pool USING btree (currency_code);


-- merchant.merchant_logo definition

-- Drop table

-- DROP TABLE merchant.merchant_logo;

CREATE TABLE IF NOT EXISTS merchant.merchant_logo (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	bytes bytea NULL,
	content_type varchar(100) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	file_name text NULL,
	CONSTRAINT pk_merchant_logo PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_logo_merchant_id ON merchant.merchant_logo USING btree (merchant_id);


-- merchant.merchant_pre_application definition

-- Drop table

-- DROP TABLE merchant.merchant_pre_application;

CREATE TABLE IF NOT EXISTS merchant.merchant_pre_application (
	id uuid NOT NULL,
	full_name varchar(50) NOT NULL,
	email varchar(50) NOT NULL,
	phone_number text NOT NULL,
	monthly_turnover varchar(50) NOT NULL,
	application_status varchar(50) NOT NULL,
	website varchar(50) NOT NULL,
	consent_confirmation bool NOT NULL,
	kvkk_confirmation bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	responsible_person text NULL,
	product_types varchar(50) NOT NULL DEFAULT ARRAY[]::integer[],
	CONSTRAINT pk_merchant_pre_application PRIMARY KEY (id)
);


-- merchant.merchant_statement definition

-- Drop table

-- DROP TABLE merchant.merchant_statement;

CREATE TABLE IF NOT EXISTS merchant.merchant_statement (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	mail_address varchar(50) NOT NULL,
	statement_start_date timestamp NOT NULL,
	statement_end_date timestamp NOT NULL,
	pdf_path varchar(256) NULL,
	file_name varchar(50) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	merchant_name varchar(200) NULL,
	statement_month int4 NOT NULL DEFAULT 0,
	statement_year int4 NOT NULL DEFAULT 0,
	description varchar(300) NULL,
	receipt_number varchar(50) NULL,
	statement_status varchar(50) NOT NULL DEFAULT ''::character varying,
	statement_type varchar(50) NOT NULL DEFAULT ''::character varying,
	excel_path varchar(256) NULL,
	CONSTRAINT pk_merchant_statement PRIMARY KEY (id)
);





-- merchant.merchant_content_version definition

-- Drop table

-- DROP TABLE merchant.merchant_content_version;

CREATE TABLE IF NOT EXISTS merchant.merchant_content_version (
	id uuid NOT NULL,
	merchant_content_id uuid NOT NULL,
	title varchar(150) NOT NULL,
	"content" text NULL,
	language_code varchar(10) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_merchant_content_version PRIMARY KEY (id),
	CONSTRAINT fk_merchant_content_version_merchant_content_merchant_content_ FOREIGN KEY (merchant_content_id) REFERENCES merchant.merchant_content(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_merchant_content_version_merchant_content_id ON merchant.merchant_content_version USING btree (merchant_content_id);


-- merchant.merchant_pre_application_history definition

-- Drop table

-- DROP TABLE merchant.merchant_pre_application_history;

CREATE TABLE IF NOT EXISTS merchant.merchant_pre_application_history (
	id uuid NOT NULL,
	merchant_pre_application_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	user_id uuid NOT NULL,
	user_name varchar(50) NOT NULL,
	operation_type varchar(50) NOT NULL,
	operation_date timestamp NOT NULL,
	operation_note text NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_merchant_pre_application_history PRIMARY KEY (id),
	CONSTRAINT fk_merchant_pre_application_history_merchant_pre_application_m FOREIGN KEY (merchant_pre_application_id) REFERENCES merchant.merchant_pre_application(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_merchant_pre_application_history_merchant_pre_application_id ON merchant.merchant_pre_application_history USING btree (merchant_pre_application_id);


-- merchant.api_key definition

-- Drop table

-- DROP TABLE merchant.api_key;

CREATE TABLE IF NOT EXISTS merchant.api_key (
	id uuid NOT NULL,
	public_key varchar(100) NOT NULL,
	private_key_encrypted varchar(100) NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_api_key PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_api_key_merchant_id ON merchant.api_key USING btree (merchant_id);


-- merchant.bank_account definition

-- Drop table

-- DROP TABLE merchant.bank_account;

CREATE TABLE IF NOT EXISTS merchant.bank_account (
	id uuid NOT NULL,
	iban varchar(26) NOT NULL,
	bank_code int4 NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_bank_account PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_bank_account_bank_code ON merchant.bank_account USING btree (bank_code);
CREATE INDEX IF NOT EXISTS ix_bank_account_merchant_id ON merchant.bank_account USING btree (merchant_id);


-- merchant.blockage definition

-- Drop table

-- DROP TABLE merchant.blockage;

CREATE TABLE IF NOT EXISTS merchant.blockage (
	id uuid NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	blockage_amount numeric(18, 4) NOT NULL,
	remaining_amount numeric(18, 4) NOT NULL,
	merchant_blockage_status varchar(50) NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_blockage PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_blockage_merchant_id ON merchant.blockage USING btree (merchant_id);


-- merchant.blockage_detail definition

-- Drop table

-- DROP TABLE merchant.blockage_detail;

CREATE TABLE IF NOT EXISTS merchant.blockage_detail (
	id uuid NOT NULL,
	posting_date date NOT NULL,
	merchant_id uuid NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	blockage_amount numeric(18, 4) NOT NULL,
	remaining_amount numeric(18, 4) NOT NULL,
	blockage_status varchar(50) NOT NULL,
	merchant_blockage_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_blockage_detail PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_blockage_detail_merchant_blockage_id ON merchant.blockage_detail USING btree (merchant_blockage_id);
CREATE INDEX IF NOT EXISTS ix_blockage_detail_merchant_id ON merchant.blockage_detail USING btree (merchant_id);


-- merchant.business_partner definition

-- Drop table

-- DROP TABLE merchant.business_partner;

CREATE TABLE IF NOT EXISTS merchant.business_partner (
	id uuid NOT NULL,
	first_name varchar(100) NOT NULL,
	last_name varchar(100) NOT NULL,
	email varchar(256) NOT NULL,
	phone_number varchar(50) NOT NULL,
	identity_number varchar(20) NOT NULL,
	birth_date timestamp NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_business_partner PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_business_partner_merchant_id ON merchant.business_partner USING btree (merchant_id);


-- merchant."document" definition

-- Drop table

-- DROP TABLE merchant."document";

CREATE TABLE IF NOT EXISTS merchant."document" (
	id uuid NOT NULL,
	document_id uuid NOT NULL,
	document_type_id uuid NOT NULL,
	document_name varchar(256) NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	merchant_transaction_id uuid NULL,
	CONSTRAINT pk_document PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_document_merchant_id ON merchant.document USING btree (merchant_id);


-- merchant.email definition

-- Drop table

-- DROP TABLE merchant.email;

CREATE TABLE IF NOT EXISTS merchant.email (
	id uuid NOT NULL,
	email varchar(256) NOT NULL,
	email_type varchar(50) NOT NULL,
	report_allowed bool NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_email PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_email_merchant_id ON merchant.email USING btree (merchant_id);


-- merchant.history definition

-- Drop table

-- DROP TABLE merchant.history;

CREATE TABLE IF NOT EXISTS merchant.history (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	permission_operation_type varchar(50) NOT NULL,
	new_data varchar(1000) NOT NULL,
	old_data varchar(1000) NOT NULL,
	detail varchar(256) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	created_name_by varchar(256) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_history PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_history_merchant_id ON merchant.history USING btree (merchant_id);


-- merchant.merchant_deduction definition

-- Drop table

-- DROP TABLE merchant.merchant_deduction;

CREATE TABLE IF NOT EXISTS merchant.merchant_deduction (
	id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	total_deduction_amount numeric(18, 4) NOT NULL,
	remaining_deduction_amount numeric(18, 4) NOT NULL,
	deduction_type varchar(50) NOT NULL,
	deduction_status varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	currency int4 NOT NULL DEFAULT 0,
	execution_date date NOT NULL DEFAULT '-infinity'::date,
	merchant_due_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	posting_balance_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	conversation_id varchar(50) NULL,
	deduction_amount_with_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	sub_merchant_deduction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	CONSTRAINT pk_merchant_deduction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_deduction_merchant_id ON merchant.merchant_deduction USING btree (merchant_id);


-- merchant.merchant_due definition

-- Drop table

-- DROP TABLE merchant.merchant_due;

CREATE TABLE IF NOT EXISTS merchant.merchant_due (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	last_execution_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	total_execution_count int4 NOT NULL DEFAULT 0,
	due_profile_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	CONSTRAINT pk_merchant_due PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_due_due_profile_id ON merchant.merchant_due USING btree (due_profile_id);
CREATE INDEX IF NOT EXISTS ix_merchant_due_merchant_id ON merchant.merchant_due USING btree (merchant_id);


-- merchant.merchant_return_pool definition

-- Drop table

-- DROP TABLE merchant.merchant_return_pool;

CREATE TABLE IF NOT EXISTS merchant.merchant_return_pool (
	id uuid NOT NULL,
	action_date timestamp NOT NULL,
	action_user uuid NOT NULL,
	return_status varchar(50) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	order_id varchar(50) NOT NULL,
	conversation_id varchar(50) NOT NULL,
	client_ip_address varchar(50) NOT NULL,
	language_code text NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	bank_code int4 NOT NULL DEFAULT 0,
	bank_name varchar(100) NOT NULL DEFAULT ''::character varying,
	bank_status bool NULL,
	card_number varchar(50) NULL,
	reject_description varchar(400) NULL,
	currency_code varchar(10) NULL,
	reject_reason varchar(400) NULL,
	bank_response_code varchar(50) NULL,
	bank_response_description varchar(1000) NULL,
	CONSTRAINT pk_merchant_return_pool PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_return_pool_merchant_id ON merchant.merchant_return_pool USING btree (merchant_id);



-- merchant.score definition

-- Drop table

-- DROP TABLE merchant.score;

CREATE TABLE IF NOT EXISTS merchant.score (
	id uuid NOT NULL,
	has_score_card bool NOT NULL,
	score_card_score int4 NULL,
	has_findeks_risk_report bool NOT NULL,
	findeks_score int4 NULL,
	alexa_rank varchar(10) NULL,
	google_rank varchar(10) NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_score PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_score_merchant_id ON merchant.score USING btree (merchant_id);


-- merchant."transaction" definition

-- Drop table

-- DROP TABLE merchant."transaction";

CREATE TABLE IF NOT EXISTS merchant."transaction" (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
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
	CONSTRAINT pk_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_transaction_acquire_bank_code1 ON merchant.transaction USING btree (acquire_bank_code);
CREATE INDEX IF NOT EXISTS ix_transaction_batch_status_record_status ON merchant.transaction USING btree (batch_status, record_status);
CREATE INDEX IF NOT EXISTS ix_transaction_issuer_bank_code1 ON merchant.transaction USING btree (issuer_bank_code);
CREATE INDEX IF NOT EXISTS ix_transaction_merchant_id ON merchant.transaction USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_transaction_posting_item_id ON merchant.transaction USING btree (posting_item_id);
CREATE INDEX IF NOT EXISTS ix_transaction_transaction_date ON merchant.transaction USING btree (transaction_date);


-- merchant."user" definition

-- Drop table

-- DROP TABLE merchant."user";

CREATE TABLE IF NOT EXISTS merchant."user" (
	id uuid NOT NULL,
	user_id uuid NOT NULL,
	"name" varchar(100) NOT NULL,
	surname varchar(100) NOT NULL,
	email varchar(100) NOT NULL,
	mobile_phone_number varchar(20) NOT NULL,
	role_id varchar(50) NULL,
	role_name varchar(150) NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	birth_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	CONSTRAINT pk_user PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_user_merchant_id ON merchant."user" USING btree (merchant_id);


-- merchant.vpos definition

-- Drop table

-- DROP TABLE merchant.vpos;

CREATE TABLE IF NOT EXISTS merchant.vpos (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	vpos_id uuid NOT NULL,
	sub_merchant_code varchar(50) NULL,
	priority int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	is_terminal_notification bool NOT NULL DEFAULT false,
	"password" varchar(50) NULL,
	terminal_no varchar(20) NULL,
	api_key varchar(50) NULL,
	provider_key varchar(50) NULL,
	CONSTRAINT pk_vpos PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_vpos_merchant_id ON merchant.vpos USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_vpos_vpos_id ON merchant.vpos USING btree (vpos_id);


-- merchant.wallet definition

-- Drop table

-- DROP TABLE merchant.wallet;

CREATE TABLE IF NOT EXISTS merchant.wallet (
	id uuid NOT NULL,
	wallet_number varchar(26) NOT NULL,
	merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_wallet PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_wallet_merchant_id ON merchant.wallet USING btree (merchant_id);


DO $$
BEGIN
    -- merchant.api_key foreign key
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'api_key' AND constraint_name = 'fk_api_key_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.api_key
        ADD CONSTRAINT fk_api_key_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- merchant.bank_account foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_bank_bank_code'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_bank_bank_code
        FOREIGN KEY (bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

   

END $$; 


-- core.due_profile definition

-- Drop table

-- DROP TABLE core.due_profile;

CREATE TABLE IF NOT EXISTS core.due_profile (
	id uuid NOT NULL,
	due_type varchar(50) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	occurence_interval varchar(50) NOT NULL,
	is_default bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	title varchar(250) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_due_profile PRIMARY KEY (id)
);


-- core.on_us_payment definition

-- Drop table

-- DROP TABLE core.on_us_payment;

CREATE TABLE IF NOT EXISTS core.on_us_payment (
	id uuid NOT NULL,
	status varchar(50) NOT NULL,
	payment_status varchar(50) NOT NULL,
	webhook_status varchar(50) NOT NULL,
	merchant_id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	merchant_name varchar(150) NOT NULL,
	merchant_number varchar(15) NOT NULL,
	"name" varchar(150) NULL,
	surname varchar(150) NULL,
	email varchar(256) NULL,
	phone_code varchar(10) NOT NULL,
	phone_number varchar(30) NOT NULL,
	wallet_number varchar(10) NULL,
	emoney_reference_number varchar(256) NULL,
	emoney_transaction_id uuid NOT NULL,
	expiry_date timestamp NOT NULL,
	webhook_retry_count int4 NOT NULL,
	callback_url varchar(250) NOT NULL,
	order_id varchar(50) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_on_us_payment PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_on_us_payment_expiry_date ON core.on_us_payment USING btree (expiry_date);
CREATE INDEX IF NOT EXISTS ix_on_us_payment_merchant_transaction_id ON core.on_us_payment USING btree (merchant_transaction_id);
CREATE INDEX IF NOT EXISTS ix_on_us_payment_payment_status ON core.on_us_payment USING btree (payment_status);
CREATE INDEX IF NOT EXISTS ix_on_us_payment_status ON core.on_us_payment USING btree (status);
CREATE INDEX IF NOT EXISTS ix_on_us_payment_webhook_status ON core.on_us_payment USING btree (webhook_status);




-- core.pricing_profile definition

-- Drop table

-- DROP TABLE core.pricing_profile;

CREATE TABLE IF NOT EXISTS core.pricing_profile (
	id uuid NOT NULL,
	"name" varchar(50) NULL,
	pricing_profile_number varchar(6) NULL,
	activation_date timestamp NOT NULL,
	profile_status varchar(50) NOT NULL,
	currency_code varchar(10) NULL,
	per_transaction_fee numeric(4, 2) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	profile_type varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_pricing_profile PRIMARY KEY (id),
	CONSTRAINT fk_pricing_profile_currency_currency_id FOREIGN KEY (currency_code) REFERENCES core.currency(code) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_pricing_profile_currency_code ON core.pricing_profile USING btree (currency_code);


-- core.pricing_profile_item definition

-- Drop table

-- DROP TABLE core.pricing_profile_item;

CREATE TABLE IF NOT EXISTS core.pricing_profile_item (
	id uuid NOT NULL,
	profile_card_type varchar(50) NOT NULL,
	installment_number int4 NOT NULL,
	installment_number_end int4 NOT NULL,
	commission_rate numeric(4, 2) NOT NULL,
	blocked_day_number int4 NOT NULL,
	is_active bool NOT NULL,
	pricing_profile_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	parent_merchant_commission_rate numeric NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_pricing_profile_item PRIMARY KEY (id),
	CONSTRAINT fk_pricing_profile_item_pricing_profile_pricing_profile_id FOREIGN KEY (pricing_profile_id) REFERENCES core.pricing_profile(id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS ix_pricing_profile_item_pricing_profile_id ON core.pricing_profile_item USING btree (pricing_profile_id);


-- core.cost_profile definition

-- Drop table

-- DROP TABLE core.cost_profile;

CREATE TABLE IF NOT EXISTS core.cost_profile (
	id uuid NOT NULL,
	"name" varchar(50) NULL,
	activation_date timestamp NOT NULL,
	point_commission numeric(4, 2) NOT NULL,
	service_commission numeric(4, 2) NOT NULL,
	profile_status varchar(50) NOT NULL,
	vpos_id uuid NOT NULL,
	currency_code varchar(10) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_cost_profile PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_cost_profile_currency_code ON core.cost_profile USING btree (currency_code);
CREATE INDEX IF NOT EXISTS ix_cost_profile_vpos_id ON core.cost_profile USING btree (vpos_id);


-- core.cost_profile_item definition

-- Drop table

-- DROP TABLE core.cost_profile_item;

CREATE TABLE IF NOT EXISTS core.cost_profile_item (
	id uuid NOT NULL,
	card_transaction_type varchar(50) NOT NULL,
	profile_card_type varchar(50) NOT NULL,
	installment_number int4 NOT NULL,
	installment_number_end int4 NOT NULL,
	commission_rate numeric(4, 2) NOT NULL,
	blocked_day_number int4 NOT NULL,
	is_active bool NOT NULL,
	installment_support bool NULL,
	cost_profile_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_cost_profile_item PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_cost_profile_item_cost_profile_id ON core.cost_profile_item USING btree (cost_profile_id);


-- core.three_d_verification definition

-- Drop table

-- DROP TABLE core.three_d_verification;

CREATE TABLE IF NOT EXISTS core.three_d_verification (
	id uuid NOT NULL,
	transaction_type varchar(50) NOT NULL,
	order_id varchar(50) NULL,
	card_token varchar(50) NOT NULL,
	installment_count int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	point_amount numeric(18, 4) NOT NULL,
	current_step varchar(50) NOT NULL,
	callback_url varchar(250) NULL,
	merchant_id uuid NOT NULL,
	issuer_bank_code int4 NOT NULL,
	acquire_bank_code int4 NOT NULL,
	merchant_code varchar(50) NOT NULL,
	sub_merchant_code varchar(50) NOT NULL,
	bin_number varchar(8) NULL,
	currency int4 NOT NULL,
	session_expiry_date timestamp NOT NULL,
	bank_commission_amount numeric(18, 4) NOT NULL,
	bank_commission_rate numeric(4, 2) NOT NULL,
	md varchar(256) NULL,
	md_status varchar(2) NULL,
	md_error_message varchar(256) NULL,
	xid varchar(50) NULL,
	eci varchar(50) NULL,
	cavv varchar(50) NULL,
	payer_txn_id varchar(100) NULL,
	txn_stat varchar(50) NULL,
	three_d_status varchar(50) NULL,
	hash_key varchar(100) NULL,
	bank_transaction_date timestamp NOT NULL,
	bank_response_code varchar(40) NULL,
	bank_response_description varchar(256) NULL,
	vpos_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	sub_merchant_terminal_no varchar(20) NOT NULL DEFAULT ''::character varying,
	conversation_id varchar(50) NULL,
	bank_blocked_day_number int4 NOT NULL DEFAULT 0,
	bank_packet varchar(500) NULL,
	CONSTRAINT pk_three_d_verification PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_three_d_verification_vpos_id ON core.three_d_verification USING btree (vpos_id);


-- core.time_out_transaction definition

-- Drop table

-- DROP TABLE core.time_out_transaction;

CREATE TABLE IF NOT EXISTS core.time_out_transaction (
	id uuid NOT NULL,
	timeout_transaction_status varchar(50) NOT NULL,
	transaction_type varchar(50) NOT NULL,
	card_number varchar(50) NULL,
	original_order_id varchar(50) NOT NULL,
	conversation_id varchar(50) NULL,
	order_id varchar(50) NULL,
	amount numeric(18, 4) NOT NULL,
	sub_merchant_code varchar(200) NULL,
	transaction_date timestamp NOT NULL,
	merchant_id uuid NOT NULL,
	acquire_bank_code int4 NOT NULL,
	vpos_id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	bank_transaction_id uuid NOT NULL,
	currency int4 NOT NULL,
	language_code varchar(100) NULL,
	retry_count int4 NOT NULL,
	next_try_time timestamp NULL,
	pos_error_code varchar(20) NULL,
	pos_error_message varchar(1000) NULL,
	error_code varchar(10) NULL,
	error_message varchar(256) NULL,
	response_code varchar(10) NULL,
	response_message varchar(256) NULL,
	description varchar(256) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	client_ip_address varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_time_out_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_time_out_transaction_acquire_bank_code ON core.time_out_transaction USING btree (acquire_bank_code);
CREATE INDEX IF NOT EXISTS ix_time_out_transaction_merchant_id ON core.time_out_transaction USING btree (merchant_id);


-- core.cost_profile foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'cost_profile'
          AND constraint_name = 'fk_cost_profile_currency_currency_id'
    ) THEN
        ALTER TABLE core.cost_profile
        ADD CONSTRAINT fk_cost_profile_currency_currency_id
        FOREIGN KEY (currency_code)
        REFERENCES core.currency(code)
        ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'cost_profile'
          AND constraint_name = 'fk_cost_profile_vpos_vpos_id'
    ) THEN
        ALTER TABLE core.cost_profile
        ADD CONSTRAINT fk_cost_profile_vpos_vpos_id
        FOREIGN KEY (vpos_id)
        REFERENCES vpos.vpos(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- core.cost_profile_item foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'cost_profile_item'
          AND constraint_name = 'fk_cost_profile_item_cost_profile_cost_profile_id'
    ) THEN
        ALTER TABLE core.cost_profile_item
        ADD CONSTRAINT fk_cost_profile_item_cost_profile_cost_profile_id
        FOREIGN KEY (cost_profile_id)
        REFERENCES core.cost_profile(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- core.three_d_verification foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'three_d_verification'
          AND constraint_name = 'fk_three_d_verification_vpos_vpos_id'
    ) THEN
        ALTER TABLE core.three_d_verification
        ADD CONSTRAINT fk_three_d_verification_vpos_vpos_id
        FOREIGN KEY (vpos_id)
        REFERENCES vpos.vpos(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- core.time_out_transaction foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'time_out_transaction'
          AND constraint_name = 'fk_time_out_transaction_bank_acquire_bank_code'
    ) THEN
        ALTER TABLE core.time_out_transaction
        ADD CONSTRAINT fk_time_out_transaction_bank_acquire_bank_code
        FOREIGN KEY (acquire_bank_code)
        REFERENCES bank.bank(code)
        ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'time_out_transaction'
          AND constraint_name = 'fk_time_out_transaction_merchant_merchant_id'
    ) THEN
        ALTER TABLE core.time_out_transaction
        ADD CONSTRAINT fk_time_out_transaction_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;



-- Drop table

-- DROP TABLE vpos.bank_api_info;

CREATE TABLE IF NOT EXISTS vpos.bank_api_info (
	id uuid NOT NULL,
	vpos_id uuid NOT NULL,
	key_id uuid NOT NULL,
	value varchar(150) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_bank_api_info PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_bank_api_info_key_id ON vpos.bank_api_info USING btree (key_id);
CREATE INDEX IF NOT EXISTS ix_bank_api_info_vpos_id ON vpos.bank_api_info USING btree (vpos_id);


-- vpos.currency definition

-- Drop table

-- DROP TABLE vpos.currency;

CREATE TABLE IF NOT EXISTS vpos.currency (
	id uuid NOT NULL,
	vpos_id uuid NOT NULL,
	currency_code varchar(10) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_currency PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_currency_currency_code ON vpos.currency USING btree (currency_code);
CREATE INDEX IF NOT EXISTS ix_currency_vpos_id ON vpos.currency USING btree (vpos_id);

-------------------------------------------------

DO $$
BEGIN
    -- merchant.api_key foreign key
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'api_key' AND constraint_name = 'fk_api_key_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.api_key
        ADD CONSTRAINT fk_api_key_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- merchant.bank_account foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_bank_bank_code'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_bank_bank_code
        FOREIGN KEY (bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- vpos.bank_api_info foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_api_key_key_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_api_key_key_id
        FOREIGN KEY (key_id) REFERENCES bank.api_key(id) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.currency foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_currency_currency_code'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_currency_currency_code
        FOREIGN KEY (currency_code) REFERENCES core.currency(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.vpos foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'vpos' AND constraint_name = 'fk_vpos_acquire_bank_acquire_bank_id'
    ) THEN
        ALTER TABLE vpos.vpos
        ADD CONSTRAINT fk_vpos_acquire_bank_acquire_bank_id
        FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE;
    END IF;

END $$;


-- hpp.hosted_payment definition

-- Drop table

-- DROP TABLE hpp.hosted_payment;

CREATE TABLE IF NOT EXISTS hpp.hosted_payment (
	id uuid NOT NULL,
	tracking_id varchar(24) NOT NULL,
	hpp_status varchar(50) NOT NULL,
	hpp_payment_status varchar(50) NOT NULL,
	webhook_status varchar(50) NOT NULL,
	order_id varchar(24) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	is3d_required bool NOT NULL,
	callback_url varchar(250) NOT NULL,
	"name" varchar(150) NULL,
	surname varchar(150) NULL,
	email varchar(256) NOT NULL,
	phone_number varchar(10) NOT NULL,
	client_ip_address varchar(50) NOT NULL,
	language_code varchar(2) NOT NULL,
	expiry_date timestamp NOT NULL,
	merchant_id uuid NOT NULL,
	merchant_name varchar(150) NOT NULL,
	merchant_number varchar(15) NOT NULL,
	webhook_retry_count int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	page_view_type varchar(50) NOT NULL DEFAULT ''::character varying,
	return_url varchar(250) NULL,
	enable_installments bool NOT NULL DEFAULT false,
	commission_from_customer bool NOT NULL DEFAULT false,
	CONSTRAINT pk_hosted_payment PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_hosted_payment_expiry_date ON hpp.hosted_payment USING btree (expiry_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_hosted_payment_tracking_id ON hpp.hosted_payment USING btree (tracking_id);
CREATE INDEX IF NOT EXISTS ix_hosted_payment_webhook_status ON hpp.hosted_payment USING btree (webhook_status);


-- hpp.hosted_payment_transaction definition

-- Drop table

-- DROP TABLE hpp.hosted_payment_transaction;

CREATE TABLE IF NOT EXISTS hpp.hosted_payment_transaction (
	id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	tracking_id varchar(24) NOT NULL,
	transaction_type varchar(50) NOT NULL,
	transaction_date timestamp NOT NULL,
	hpp_payment_status varchar(50) NOT NULL,
	order_id varchar(24) NULL,
	amount numeric(18, 4) NOT NULL,
	installment_count int4 NOT NULL,
	currency int4 NOT NULL,
	is3d_required bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	three_d_session_id varchar(200) NULL,
	CONSTRAINT pk_hosted_payment_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_hosted_payment_transaction_tracking_id ON hpp.hosted_payment_transaction USING btree (tracking_id);
CREATE INDEX IF NOT EXISTS ix_hosted_payment_transaction_transaction_date ON hpp.hosted_payment_transaction USING btree (transaction_date);


-- hpp.hosted_payment_installment definition

-- Drop table

-- DROP TABLE hpp.hosted_payment_installment;

CREATE TABLE IF NOT EXISTS hpp.hosted_payment_installment (
	id uuid NOT NULL,
	hosted_payment_id uuid NOT NULL,
	installment int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	amount numeric(18, 4) NULL,
	card_network varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_hosted_payment_installment PRIMARY KEY (id),
	CONSTRAINT fk_hosted_payment_installment_hosted_payment_hosted_payment_id FOREIGN KEY (hosted_payment_id) REFERENCES hpp.hosted_payment(id) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_hosted_payment_installment_hosted_payment_id ON hpp.hosted_payment_installment USING btree (hosted_payment_id);

-- "limit".merchant_daily_usage definition

-- Drop table

-- DROP TABLE "limit".merchant_daily_usage;

CREATE TABLE IF NOT EXISTS "limit".merchant_daily_usage (
	id uuid NOT NULL,
	"date" timestamp NOT NULL,
	count int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	merchant_id uuid NOT NULL,
	currency varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	transaction_limit_type varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_merchant_daily_usage PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_daily_usage_merchant_id ON "limit".merchant_daily_usage USING btree (merchant_id);


-- "limit".merchant_limit definition

-- Drop table

-- DROP TABLE "limit".merchant_limit;

CREATE TABLE IF NOT EXISTS "limit".merchant_limit (
	id uuid NOT NULL,
	transaction_limit_type varchar(50) NOT NULL,
	"period" varchar(50) NOT NULL,
	limit_type varchar(50) NOT NULL,
	max_piece int4 NULL,
	max_amount numeric(18, 4) NULL,
	merchant_id uuid NOT NULL,
	currency varchar(50) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_merchant_limit PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_limit_merchant_id ON "limit".merchant_limit USING btree (merchant_id);


-- "limit".merchant_monthly_usage definition

-- Drop table

-- DROP TABLE "limit".merchant_monthly_usage;

CREATE TABLE IF NOT EXISTS "limit".merchant_monthly_usage (
	id uuid NOT NULL,
	"date" timestamp NOT NULL,
	count int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	merchant_id uuid NOT NULL,
	currency varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	transaction_limit_type varchar(50) NOT NULL DEFAULT ''::character varying,
	CONSTRAINT pk_merchant_monthly_usage PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_merchant_monthly_usage_merchant_id ON "limit".merchant_monthly_usage USING btree (merchant_id);


-- "limit".merchant_daily_usage foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'limit'
          AND table_name = 'merchant_daily_usage'
          AND constraint_name = 'fk_merchant_daily_usage_merchant_merchant_id'
    ) THEN
        ALTER TABLE "limit".merchant_daily_usage
        ADD CONSTRAINT fk_merchant_daily_usage_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- "limit".merchant_limit foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'limit'
          AND table_name = 'merchant_limit'
          AND constraint_name = 'fk_merchant_limit_merchant_merchant_id'
    ) THEN
        ALTER TABLE "limit".merchant_limit
        ADD CONSTRAINT fk_merchant_limit_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;


-- "limit".merchant_monthly_usage foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'limit'
          AND table_name = 'merchant_monthly_usage'
          AND constraint_name = 'fk_merchant_monthly_usage_merchant_merchant_id'
    ) THEN
        ALTER TABLE "limit".merchant_monthly_usage
        ADD CONSTRAINT fk_merchant_monthly_usage_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;

-- link.link definition

-- Drop table

-- DROP TABLE link.link;

CREATE TABLE IF NOT EXISTS link.link (
	id uuid NOT NULL,
	link_status varchar(50) NOT NULL,
	link_type varchar(50) NOT NULL,
	expiry_date timestamp NOT NULL,
	current_usage_count int4 NOT NULL,
	max_usage_count int4 NOT NULL,
	order_id varchar(24) NULL,
	link_amount_type varchar(50) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	commission_from_customer bool NOT NULL,
	is3d_required bool NOT NULL,
	merchant_name varchar(150) NOT NULL DEFAULT ''::character varying,
	merchant_number varchar(15) NOT NULL DEFAULT ''::character varying,
	product_name varchar(100) NOT NULL DEFAULT ''::character varying,
	product_description varchar(400) NOT NULL DEFAULT ''::character varying,
	return_url varchar(150) NULL,
	is_name_required bool NOT NULL,
	is_email_required bool NOT NULL,
	is_phone_number_required bool NOT NULL,
	is_address_required bool NOT NULL,
	is_note_required bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	link_payment_status varchar(50) NOT NULL DEFAULT ''::character varying,
	link_code varchar(24) NOT NULL DEFAULT ''::character varying,
	merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	sub_merchant_id uuid NULL,
	sub_merchant_name varchar(150) NULL DEFAULT ''::character varying,
	sub_merchant_number varchar(15) NULL DEFAULT ''::character varying,
	CONSTRAINT pk_link PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_link_expiry_date ON link.link USING btree (expiry_date);
CREATE UNIQUE INDEX IF NOT EXISTS ix_link_link_code ON link.link USING btree (link_code);


-- link.link_customer definition

-- Drop table

-- DROP TABLE link.link_customer;

CREATE TABLE IF NOT EXISTS link.link_customer (
	id uuid NOT NULL,
	link_transaction_id uuid NOT NULL,
	"name" varchar(100) NULL,
	email varchar(100) NULL,
	phone_number varchar(30) NULL,
	address varchar(256) NULL,
	note varchar(256) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_link_customer PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_link_customer_link_transaction_id ON link.link_customer USING btree (link_transaction_id);


-- link.link_transaction definition

-- Drop table

-- DROP TABLE link.link_transaction;

CREATE TABLE IF NOT EXISTS link.link_transaction (
	id uuid NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	link_code varchar(24) NOT NULL,
	link_payment_status varchar(50) NOT NULL,
	link_type varchar(50) NOT NULL,
	order_id varchar(24) NULL,
	amount numeric(18, 4) NOT NULL,
	currency int4 NOT NULL,
	commission_from_customer bool NOT NULL,
	is3d_required bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	customer_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	commission_amount numeric NOT NULL DEFAULT 0.0,
	installment_count int4 NOT NULL DEFAULT 0,
	transaction_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	transaction_type varchar(50) NOT NULL DEFAULT ''::character varying,
	three_d_session_id varchar(200) NULL,
	CONSTRAINT pk_link_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_link_transaction_link_code ON link.link_transaction USING btree (link_code);
CREATE INDEX IF NOT EXISTS ix_link_transaction_transaction_date ON link.link_transaction USING btree (transaction_date);


-- link.link_installment definition

-- Drop table

-- DROP TABLE link.link_installment;

CREATE TABLE IF NOT EXISTS link.link_installment (
	id uuid NOT NULL,
	installment int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	link_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	CONSTRAINT pk_link_installment PRIMARY KEY (id),
	CONSTRAINT fk_link_installment_link_link_id FOREIGN KEY (link_id) REFERENCES link.link(id) ON DELETE RESTRICT
);
CREATE INDEX IF NOT EXISTS ix_link_installment_link_id ON link.link_installment USING btree (link_id);

-- posting.batch_status definition

-- Drop table

-- DROP TABLE posting.batch_status;

CREATE TABLE IF NOT EXISTS posting.batch_status (
	id uuid NOT NULL,
	posting_batch_level varchar(50) NOT NULL,
	batch_summary varchar(200) NOT NULL,
	is_critical_error bool NOT NULL,
	posting_date date NOT NULL,
	start_time timestamp NOT NULL,
	finish_time timestamp NOT NULL,
	batch_status varchar(50) NOT NULL,
	batch_order int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_batch_status PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_batch_status_posting_date ON posting.batch_status USING btree (posting_date);


-- posting.item definition

-- Drop table

-- DROP TABLE posting.item;

CREATE TABLE IF NOT EXISTS posting.item (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	error_count int4 NOT NULL,
	total_count int4 NOT NULL,
	posting_date date NOT NULL,
	batch_status varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_item PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_item_merchant_id ON posting.item USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_item_merchant_id_posting_date ON posting.item USING btree (merchant_id, posting_date);


-- posting.posting_additional_transaction definition

-- Drop table

-- DROP TABLE posting.posting_additional_transaction;

CREATE TABLE IF NOT EXISTS posting.posting_additional_transaction (
	id uuid NOT NULL,
	acquire_bank_code int4 NOT NULL DEFAULT 0,
	amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	amount_without_bank_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	amount_without_commissions numeric(18, 4) NOT NULL DEFAULT 0.0,
	b_trans_status varchar(50) NOT NULL DEFAULT ''::character varying,
	bank_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	bank_commission_rate numeric(4, 2) NOT NULL DEFAULT 0.0,
	batch_status varchar(50) NOT NULL DEFAULT ''::character varying,
	blockage_status varchar(50) NOT NULL DEFAULT ''::character varying,
	card_number varchar(50) NOT NULL DEFAULT ''::character varying,
	conversation_id varchar(50) NOT NULL DEFAULT ''::character varying,
	create_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	created_by varchar(50) NOT NULL DEFAULT ''::character varying,
	currency int4 NOT NULL DEFAULT 0,
	installment_count int4 NOT NULL DEFAULT 0,
	last_modified_by varchar(50) NULL,
	merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	merchant_transaction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	old_payment_date date NOT NULL DEFAULT '-infinity'::date,
	order_id varchar(50) NULL,
	payment_date date NOT NULL DEFAULT '-infinity'::date,
	pf_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	pf_commission_rate numeric(4, 2) NOT NULL DEFAULT 0.0,
	pf_net_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	pf_per_transaction_fee numeric(18, 4) NOT NULL DEFAULT 0.0,
	point_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	posting_balance_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	posting_bank_balance_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	posting_date date NOT NULL DEFAULT '-infinity'::date,
	pricing_profile_item_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	pricing_profile_number text NOT NULL DEFAULT ''::text,
	record_status varchar(50) NOT NULL DEFAULT ''::character varying,
	transaction_date date NOT NULL DEFAULT '-infinity'::date,
	transaction_end_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	transaction_start_date timestamp NOT NULL DEFAULT '-infinity'::timestamp without time zone,
	transaction_type varchar(50) NOT NULL DEFAULT ''::character varying,
	update_date timestamp NULL,
	vpos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	merchant_deduction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	related_posting_balance_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	sub_merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	sub_merchant_name varchar(150) NULL,
	sub_merchant_number varchar(15) NULL,
	easy_sub_merchant_name varchar(150) NULL,
	easy_sub_merchant_number varchar(15) NULL,
	amount_without_parent_merchant_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_posting_additional_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_posting_additional_transaction_batch_status_record_status ON posting.posting_additional_transaction USING btree (batch_status, record_status);


-- posting."transaction" definition

-- Drop table

-- DROP TABLE posting."transaction";

CREATE TABLE IF NOT EXISTS posting."transaction" (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	transaction_type varchar(50) NOT NULL,
	transaction_date date NOT NULL,
	posting_date date NOT NULL,
	payment_date date NOT NULL,
	old_payment_date date NOT NULL,
	card_number varchar(50) NOT NULL,
	order_id varchar(50) NULL,
	installment_count int4 NOT NULL,
	currency int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	point_amount numeric(18, 4) NOT NULL,
	bank_commission_rate numeric(4, 2) NOT NULL,
	bank_commission_amount numeric(18, 4) NOT NULL,
	amount_without_bank_commission numeric(18, 4) NOT NULL,
	pf_commission_rate numeric(4, 2) NOT NULL,
	pf_per_transaction_fee numeric(18, 4) NOT NULL,
	pf_commission_amount numeric(18, 4) NOT NULL,
	pf_net_commission_amount numeric(18, 4) NOT NULL,
	amount_without_commissions numeric(18, 4) NOT NULL,
	pricing_profile_number text NOT NULL,
	batch_status varchar(50) NOT NULL,
	blockage_status varchar(50) NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	posting_bank_balance_id uuid NOT NULL,
	posting_balance_id uuid NOT NULL,
	acquire_bank_code int4 NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	pricing_profile_item_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	transaction_end_date timestamp NOT NULL DEFAULT '-infinity'::date,
	transaction_start_date timestamp NOT NULL DEFAULT '-infinity'::date,
	vpos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	b_trans_status varchar(50) NOT NULL DEFAULT ''::character varying,
	conversation_id varchar(50) NOT NULL DEFAULT ''::character varying,
	amount_without_parent_merchant_commission numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_commission_rate numeric(18, 4) NOT NULL DEFAULT 0.0,
	parent_merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	easy_sub_merchant_name varchar(150) NULL,
	easy_sub_merchant_number varchar(15) NULL,
	merchant_deduction_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	related_posting_balance_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	sub_merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	sub_merchant_name varchar(150) NULL,
	sub_merchant_number varchar(15) NULL,
	CONSTRAINT pk_transaction PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_transaction_batch_status_record_status1 ON posting.transaction USING btree (batch_status, record_status);
CREATE UNIQUE INDEX IF NOT EXISTS ix_transaction_merchant_transaction_id ON posting.transaction USING btree (merchant_transaction_id);


-- posting.balance definition

-- Drop table

-- DROP TABLE posting.balance;

CREATE TABLE IF NOT EXISTS posting.balance (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	transaction_date date NOT NULL,
	posting_date date NOT NULL,
	payment_date date NOT NULL,
	currency int4 NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	total_point_amount numeric(18, 4) NOT NULL,
	total_bank_commission_amount numeric(18, 4) NOT NULL,
	total_amount_without_bank_commission numeric(18, 4) NOT NULL,
	total_pf_commission_amount numeric(18, 4) NOT NULL,
	total_pf_net_commission_amount numeric(18, 4) NOT NULL,
	total_amount_without_commissions numeric(18, 4) NOT NULL,
	total_due_amount numeric(18, 4) NOT NULL,
	total_paying_amount numeric(18, 4) NOT NULL,
	total_chargeback_amount numeric(18, 4) NOT NULL,
	total_suspicious_amount numeric(18, 4) NOT NULL,
	money_transfer_payment_date date NOT NULL,
	money_transfer_status varchar(50) NOT NULL,
	money_transfer_reference_id uuid NOT NULL,
	retry_count int4 NOT NULL,
	batch_status varchar(50) NOT NULL,
	blockage_status varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	posting_balance_type varchar(50) NOT NULL DEFAULT ''::character varying,
	iban varchar(26) NULL,
	money_transfer_bank_code int4 NOT NULL DEFAULT 0,
	money_transfer_bank_name varchar(50) NULL,
	transaction_source_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	b_trans_status varchar(50) NOT NULL DEFAULT ''::character varying,
	accounting_status varchar(50) NOT NULL DEFAULT ''::character varying,
	total_excess_return_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	total_negative_balance_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	old_payment_date date NOT NULL DEFAULT '-infinity'::date,
	transaction_count int4 NOT NULL DEFAULT 0,
	posting_payment_channel varchar(50) NOT NULL DEFAULT ''::character varying,
	wallet_number varchar(26) NULL,
	parent_merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	total_parent_merchant_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	total_submerchant_deduction_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_balance PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_balance_merchant_id ON posting.balance USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_balance_money_transfer_status_b_trans_status ON posting.balance USING btree (money_transfer_status, b_trans_status);


-- posting.bank_balance definition

-- Drop table

-- DROP TABLE posting.bank_balance;

CREATE TABLE IF NOT EXISTS posting.bank_balance (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	acquire_bank_code int4 NOT NULL,
	posting_date date NOT NULL,
	payment_date date NOT NULL,
	currency int4 NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	total_point_amount numeric(18, 4) NOT NULL,
	total_bank_commission_amount numeric(18, 4) NOT NULL,
	total_amount_without_bank_commission numeric(18, 4) NOT NULL,
	total_pf_commission_amount numeric(18, 4) NOT NULL,
	total_pf_net_commission_amount numeric(18, 4) NOT NULL,
	total_amount_without_commissions numeric(18, 4) NOT NULL,
	total_paying_amount numeric(18, 4) NOT NULL,
	batch_status varchar(50) NOT NULL,
	blockage_status varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	transaction_date date NOT NULL DEFAULT '-infinity'::date,
	posting_balance_id uuid NULL,
	accounting_status varchar(50) NOT NULL DEFAULT ''::character varying,
	old_payment_date date NOT NULL DEFAULT '-infinity'::date,
	total_return_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	transaction_count int4 NOT NULL DEFAULT 0,
	parent_merchant_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000'::uuid,
	total_parent_merchant_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_bank_balance PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_bank_balance_merchant_id ON posting.bank_balance USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_bank_balance_posting_balance_id ON posting.bank_balance USING btree (posting_balance_id);


-- posting.posting_bill definition

-- Drop table

-- DROP TABLE posting.posting_bill;

CREATE TABLE IF NOT EXISTS posting.posting_bill (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	total_amount numeric(18, 4) NOT NULL,
	total_pf_commission_amount numeric(18, 4) NOT NULL,
	total_paying_amount numeric(18, 4) NOT NULL,
	total_due_amount numeric(18, 4) NOT NULL,
	client_reference_id uuid NOT NULL,
	bill_url text NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	bill_date date NOT NULL DEFAULT '-infinity'::date,
	bill_month int4 NOT NULL DEFAULT 0,
	currency int4 NOT NULL DEFAULT 0,
	bill_year int4 NOT NULL DEFAULT 0,
	total_bank_commission_amount numeric(18, 4) NOT NULL DEFAULT 0.0,
	CONSTRAINT pk_posting_bill PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_posting_bill_merchant_id_bill_month_bill_year ON posting.posting_bill USING btree (merchant_id, bill_month, bill_year);


-- posting.transfer_error definition

-- Drop table

-- DROP TABLE posting.transfer_error;

CREATE TABLE IF NOT EXISTS posting.transfer_error (
	id uuid NOT NULL,
	posting_date timestamp NOT NULL,
	merchant_transaction_id uuid NOT NULL,
	merchant_id uuid NULL,
	transfer_error_category varchar(50) NOT NULL,
	error_message varchar(500) NOT NULL,
	stack_trace varchar(2000) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_transfer_error PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_transfer_error_merchant_id ON posting.transfer_error USING btree (merchant_id);
CREATE INDEX IF NOT EXISTS ix_transfer_error_merchant_transaction_id ON posting.transfer_error USING btree (merchant_transaction_id);


DO $$
BEGIN
    -- merchant.api_key foreign key
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'api_key' AND constraint_name = 'fk_api_key_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.api_key
        ADD CONSTRAINT fk_api_key_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- merchant.bank_account foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_bank_bank_code'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_bank_bank_code
        FOREIGN KEY (bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- vpos.bank_api_info foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_api_key_key_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_api_key_key_id
        FOREIGN KEY (key_id) REFERENCES bank.api_key(id) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.currency foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_currency_currency_code'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_currency_currency_code
        FOREIGN KEY (currency_code) REFERENCES core.currency(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.vpos foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'vpos' AND constraint_name = 'fk_vpos_acquire_bank_acquire_bank_id'
    ) THEN
        ALTER TABLE vpos.vpos
        ADD CONSTRAINT fk_vpos_acquire_bank_acquire_bank_id
        FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE;
    END IF;

    -- posting.balance foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'merchant_return_pool' AND constraint_name = 'fk_merchant_return_pool_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.merchant_return_pool
        ADD CONSTRAINT fk_merchant_return_pool_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.bank_balance foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'bank_balance' AND constraint_name = 'fk_bank_balance_balance_posting_balance_id'
    ) THEN
        ALTER TABLE posting.bank_balance
        ADD CONSTRAINT fk_bank_balance_balance_posting_balance_id
        FOREIGN KEY (posting_balance_id) REFERENCES posting.balance(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'bank_balance' AND constraint_name = 'fk_bank_balance_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.bank_balance
        ADD CONSTRAINT fk_bank_balance_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.posting_bill foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'posting_bill' AND constraint_name = 'fk_posting_bill_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.posting_bill
        ADD CONSTRAINT fk_posting_bill_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.transfer_error foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'transfer_error' AND constraint_name = 'fk_transfer_error_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.transfer_error
        ADD CONSTRAINT fk_transfer_error_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'transfer_error' AND constraint_name = 'fk_transfer_error_transaction_merchant_transaction_id'
    ) THEN
        ALTER TABLE posting.transfer_error
        ADD CONSTRAINT fk_transfer_error_transaction_merchant_transaction_id
        FOREIGN KEY (merchant_transaction_id) REFERENCES merchant."transaction"(id) ON DELETE CASCADE;
    END IF;

END $$;


	-- submerchant.daily_usage definition


-- submerchant.sub_merchant definition

-- Drop table

-- DROP TABLE submerchant.sub_merchant;

CREATE TABLE IF NOT EXISTS submerchant.sub_merchant (
	id uuid NOT NULL,
	"name" varchar(150) NOT NULL,
	"number" varchar(15) NOT NULL,
	merchant_type varchar(50) NOT NULL,
	merchant_id uuid NOT NULL,
	city int4 NOT NULL,
	city_name text NULL,
	is_manuel_payment_page_allowed bool NOT NULL,
	is_link_payment_page_allowed bool NOT NULL,
	pre_authorization_allowed bool NOT NULL,
	payment_reverse_allowed bool NOT NULL,
	payment_return_allowed bool NOT NULL,
	installment_allowed bool NOT NULL,
	is3d_required bool NOT NULL,
	is_excess_return_allowed bool NOT NULL,
	international_card_allowed bool NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	parameter_value varchar(100) NULL,
	reject_reason varchar(256) NULL,
	is_on_us_payment_page_allowed bool NOT NULL DEFAULT false,
	CONSTRAINT pk_sub_merchant PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_sub_merchant_merchant_id ON submerchant.sub_merchant USING btree (merchant_id);


-- Drop table

-- DROP TABLE submerchant.daily_usage;

CREATE TABLE IF NOT EXISTS submerchant.daily_usage (
	id uuid NOT NULL,
	"date" timestamp NOT NULL,
	count int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	sub_merchant_id uuid NOT NULL,
	currency varchar(50) NOT NULL,
	transaction_limit_type varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_daily_usage PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_daily_usage_sub_merchant_id ON submerchant.daily_usage USING btree (sub_merchant_id);


-- submerchant."document" definition

-- Drop table

-- DROP TABLE submerchant."document";

CREATE TABLE IF NOT EXISTS submerchant."document" (
	id uuid NOT NULL,
	document_id uuid NOT NULL,
	document_type_id uuid NOT NULL,
	document_name varchar(256) NOT NULL,
	sub_merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_document PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_document_sub_merchant_id ON submerchant.document USING btree (sub_merchant_id);


-- submerchant."limit" definition

-- Drop table

-- DROP TABLE submerchant."limit";

CREATE TABLE IF NOT EXISTS submerchant."limit" (
	id uuid NOT NULL,
	transaction_limit_type varchar(50) NOT NULL,
	"period" varchar(50) NOT NULL,
	limit_type varchar(50) NOT NULL,
	max_piece int4 NULL,
	max_amount numeric(18, 4) NULL,
	sub_merchant_id uuid NOT NULL,
	currency varchar(50) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_limit PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_limit_sub_merchant_id ON submerchant."limit" USING btree (sub_merchant_id);


-- submerchant.monthly_usage definition

-- Drop table

-- DROP TABLE submerchant.monthly_usage;

CREATE TABLE IF NOT EXISTS submerchant.monthly_usage (
	id uuid NOT NULL,
	"date" timestamp NOT NULL,
	count int4 NOT NULL,
	amount numeric(18, 4) NOT NULL,
	sub_merchant_id uuid NOT NULL,
	currency varchar(50) NOT NULL,
	transaction_limit_type varchar(50) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_monthly_usage PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_monthly_usage_sub_merchant_id ON submerchant.monthly_usage USING btree (sub_merchant_id);




-- submerchant."user" definition

-- Drop table

-- DROP TABLE submerchant."user";

CREATE TABLE IF NOT EXISTS submerchant."user" (
	id uuid NOT NULL,
	user_id uuid NOT NULL,
	"name" varchar(100) NOT NULL,
	surname varchar(100) NOT NULL,
	birth_date timestamp NOT NULL,
	email varchar(100) NOT NULL,
	mobile_phone_number varchar(20) NOT NULL,
	role_id varchar(50) NULL,
	role_name varchar(150) NULL,
	sub_merchant_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	identity_number varchar(50) NULL,
	CONSTRAINT pk_user PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_user_sub_merchant_id ON submerchant."user" USING btree (sub_merchant_id);


-- submerchant.daily_usage foreign keys

 DO $$
BEGIN
    -- merchant.api_key foreign key
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'api_key' AND constraint_name = 'fk_api_key_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.api_key
        ADD CONSTRAINT fk_api_key_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- merchant.bank_account foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_bank_bank_code'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_bank_bank_code
        FOREIGN KEY (bank_code) REFERENCES bank.bank(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'bank_account' AND constraint_name = 'fk_bank_account_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.bank_account
        ADD CONSTRAINT fk_bank_account_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- vpos.bank_api_info foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_api_key_key_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_api_key_key_id
        FOREIGN KEY (key_id) REFERENCES bank.api_key(id) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'bank_api_info' AND constraint_name = 'fk_bank_api_info_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.bank_api_info
        ADD CONSTRAINT fk_bank_api_info_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.currency foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_currency_currency_code'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_currency_currency_code
        FOREIGN KEY (currency_code) REFERENCES core.currency(code) ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'currency' AND constraint_name = 'fk_currency_vpos_vpos_id'
    ) THEN
        ALTER TABLE vpos.currency
        ADD CONSTRAINT fk_currency_vpos_vpos_id
        FOREIGN KEY (vpos_id) REFERENCES vpos.vpos(id) ON DELETE CASCADE;
    END IF;

    -- vpos.vpos foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'vpos' AND table_name = 'vpos' AND constraint_name = 'fk_vpos_acquire_bank_acquire_bank_id'
    ) THEN
        ALTER TABLE vpos.vpos
        ADD CONSTRAINT fk_vpos_acquire_bank_acquire_bank_id
        FOREIGN KEY (acquire_bank_id) REFERENCES bank.acquire_bank(id) ON DELETE CASCADE;
    END IF;

    -- posting.balance foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'merchant' AND table_name = 'merchant_return_pool' AND constraint_name = 'fk_merchant_return_pool_merchant_merchant_id'
    ) THEN
        ALTER TABLE merchant.merchant_return_pool
        ADD CONSTRAINT fk_merchant_return_pool_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.bank_balance foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'bank_balance' AND constraint_name = 'fk_bank_balance_balance_posting_balance_id'
    ) THEN
        ALTER TABLE posting.bank_balance
        ADD CONSTRAINT fk_bank_balance_balance_posting_balance_id
        FOREIGN KEY (posting_balance_id) REFERENCES posting.balance(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'bank_balance' AND constraint_name = 'fk_bank_balance_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.bank_balance
        ADD CONSTRAINT fk_bank_balance_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.posting_bill foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'posting_bill' AND constraint_name = 'fk_posting_bill_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.posting_bill
        ADD CONSTRAINT fk_posting_bill_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- posting.transfer_error foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'transfer_error' AND constraint_name = 'fk_transfer_error_merchant_merchant_id'
    ) THEN
        ALTER TABLE posting.transfer_error
        ADD CONSTRAINT fk_transfer_error_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'posting' AND table_name = 'transfer_error' AND constraint_name = 'fk_transfer_error_transaction_merchant_transaction_id'
    ) THEN
        ALTER TABLE posting.transfer_error
        ADD CONSTRAINT fk_transfer_error_transaction_merchant_transaction_id
        FOREIGN KEY (merchant_transaction_id) REFERENCES merchant."transaction"(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.daily_usage foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'daily_usage' AND constraint_name = 'fk_daily_usage_sub_merchant_sub_merchant_id'
    ) THEN
        ALTER TABLE submerchant.daily_usage
        ADD CONSTRAINT fk_daily_usage_sub_merchant_sub_merchant_id
        FOREIGN KEY (sub_merchant_id) REFERENCES submerchant.sub_merchant(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.document foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'document' AND constraint_name = 'fk_document_sub_merchant_sub_merchant_id'
    ) THEN
        ALTER TABLE submerchant."document"
        ADD CONSTRAINT fk_document_sub_merchant_sub_merchant_id
        FOREIGN KEY (sub_merchant_id) REFERENCES submerchant.sub_merchant(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.limit foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'limit' AND constraint_name = 'fk_limit_sub_merchant_sub_merchant_id'
    ) THEN
        ALTER TABLE submerchant."limit"
        ADD CONSTRAINT fk_limit_sub_merchant_sub_merchant_id
        FOREIGN KEY (sub_merchant_id) REFERENCES submerchant.sub_merchant(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.monthly_usage foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'monthly_usage' AND constraint_name = 'fk_monthly_usage_sub_merchant_sub_merchant_id'
    ) THEN
        ALTER TABLE submerchant.monthly_usage
        ADD CONSTRAINT fk_monthly_usage_sub_merchant_sub_merchant_id
        FOREIGN KEY (sub_merchant_id) REFERENCES submerchant.sub_merchant(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.sub_merchant foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'sub_merchant' AND constraint_name = 'fk_sub_merchant_merchant_merchant_id'
    ) THEN
        ALTER TABLE submerchant.sub_merchant
        ADD CONSTRAINT fk_sub_merchant_merchant_merchant_id
        FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;

    -- submerchant.user foreign keys
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'submerchant' AND table_name = 'user' AND constraint_name = 'fk_user_sub_merchant_sub_merchant_id'
    ) THEN
        ALTER TABLE submerchant."user"
        ADD CONSTRAINT fk_user_sub_merchant_sub_merchant_id
        FOREIGN KEY (sub_merchant_id) REFERENCES submerchant.sub_merchant(id) ON DELETE CASCADE;
    END IF;

END $$;

	-- api.log definition

-- Drop table

-- DROP TABLE api.log;

CREATE TABLE IF NOT EXISTS api.log (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	payment_type varchar(50) NOT NULL,
	request text NULL,
	response text NULL,
	error_code varchar(10) NOT NULL,
	error_message varchar(256) NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_log PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_log_merchant_id ON api.log USING btree (merchant_id);


-- api.response_code definition

-- Drop table

-- DROP TABLE api.response_code;

CREATE TABLE IF NOT EXISTS api.response_code (
	id uuid NOT NULL,
	response_code varchar(10) NOT NULL,
	description varchar(256) NOT NULL,
	merchant_response_code_id uuid NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_response_code PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_response_code_merchant_response_code_id ON api.response_code USING btree (merchant_response_code_id);


-- api.validation_log definition

-- Drop table

-- DROP TABLE api.validation_log;

CREATE TABLE IF NOT EXISTS api.validation_log (
	id uuid NOT NULL,
	merchant_id uuid NOT NULL,
	transaction_type varchar(50) NOT NULL,
	error_code varchar(10) NOT NULL,
	error_message varchar(256) NOT NULL,
	amount numeric(18, 4) NOT NULL,
	point_amount numeric(18, 4) NOT NULL,
	card_token varchar(50) NULL,
	currency varchar(50) NULL,
	installment_count int4 NOT NULL,
	three_d_session_id varchar(200) NULL,
	conversation_id varchar(50) NULL,
	original_reference_number varchar(50) NULL,
	client_ip_address varchar(50) NULL,
	language_code varchar(100) NULL,
	api_name varchar(200) NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_validation_log PRIMARY KEY (id)
);
CREATE INDEX IF NOT EXISTS ix_validation_log_merchant_id ON api.validation_log USING btree (merchant_id);


-- api.log foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'api'
          AND table_name = 'log'
          AND constraint_name = 'fk_log_merchant_merchant_id'
    ) THEN
       ALTER TABLE api.log ADD CONSTRAINT fk_log_merchant_merchant_id FOREIGN KEY (merchant_id) REFERENCES merchant.merchant(id) ON DELETE CASCADE;
    END IF;
END $$;


-- api.response_code foreign keys
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'api'
          AND table_name = 'response_code'
          AND constraint_name = 'fk_response_code_merchant_response_code_merchant_response_code'
    ) THEN
        ALTER TABLE api.response_code
        ADD CONSTRAINT fk_response_code_merchant_response_code_merchant_response_code
        FOREIGN KEY (merchant_response_code_id)
        REFERENCES merchant.response_code(id);
    END IF;
END $$;


-- api.validation_log foreign keys

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'api'
          AND table_name = 'validation_log'
          AND constraint_name = 'fk_validation_log_merchant_merchant_id'
    ) THEN
        ALTER TABLE api.validation_log
        ADD CONSTRAINT fk_validation_log_merchant_merchant_id
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant(id)
        ON DELETE CASCADE;
    END IF;
END $$;
