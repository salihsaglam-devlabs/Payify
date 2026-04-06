BEGIN;

-- SCHEMA
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'physical') THEN
        CREATE SCHEMA physical;
    END IF;
END $$;

-- core.cost_profile kolonlar
ALTER TABLE core.cost_profile
    ALTER COLUMN vpos_id DROP NOT NULL;

ALTER TABLE core.cost_profile
    ADD COLUMN IF NOT EXISTS physical_pos_id uuid;

ALTER TABLE core.cost_profile
    ADD COLUMN IF NOT EXISTS pos_type varchar(50) NOT NULL DEFAULT 'Virtual';

-- physical.device_inventory
CREATE TABLE IF NOT EXISTS physical.device_inventory (
    id uuid NOT NULL,
    serial_no varchar(200),
    contactless_separator varchar(50) NOT NULL,
    physical_pos_vendor varchar(50) NOT NULL,
    device_model varchar(50) NOT NULL,
    device_status varchar(50) NOT NULL,
    device_type varchar(50) NOT NULL,
    inventory_type varchar(50) NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_device_inventory PRIMARY KEY (id)
);

-- physical.physical_pos
CREATE TABLE IF NOT EXISTS physical.physical_pos (
    id uuid NOT NULL,
    name varchar(200),
    vpos_status varchar(50) NOT NULL,
    acquire_bank_id uuid NOT NULL,
    vpos_type varchar(50) NOT NULL,
    pf_main_merchant_id varchar(200),
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_physical_pos PRIMARY KEY (id)
);

-- FK: physical_pos → acquire_bank
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_physical_pos_acquire_bank_acquire_bank_id'
    ) THEN
        ALTER TABLE physical.physical_pos
        ADD CONSTRAINT fk_physical_pos_acquire_bank_acquire_bank_id
        FOREIGN KEY (acquire_bank_id)
        REFERENCES bank.acquire_bank (id)
        ON DELETE CASCADE;
    END IF;
END $$;

-- merchant.merchant_physical_device
CREATE TABLE IF NOT EXISTS merchant.merchant_physical_device (
    id uuid NOT NULL,
    owner_psp_no varchar(100),
    is_pin_pad boolean NOT NULL,
    connection_type varchar(50) NOT NULL,
    assignment_type varchar(50) NOT NULL,
    fiscal_no varchar(200),
    merchant_id uuid NOT NULL,
    device_inventory_id uuid NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_merchant_physical_device PRIMARY KEY (id)
);

-- FK’ler
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_merchant_physical_device_device_inventory') THEN
        ALTER TABLE merchant.merchant_physical_device
        ADD CONSTRAINT fk_merchant_physical_device_device_inventory
        FOREIGN KEY (device_inventory_id)
        REFERENCES physical.device_inventory (id)
        ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_merchant_physical_device_merchant') THEN
        ALTER TABLE merchant.merchant_physical_device
        ADD CONSTRAINT fk_merchant_physical_device_merchant
        FOREIGN KEY (merchant_id)
        REFERENCES merchant.merchant (id)
        ON DELETE CASCADE;
    END IF;
END $$;

-- physical.physical_pos_currency
CREATE TABLE IF NOT EXISTS physical.physical_pos_currency (
    id uuid NOT NULL,
    physical_pos_id uuid NOT NULL,
    currency_code varchar(10),
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_physical_pos_currency PRIMARY KEY (id)
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_physical_pos_currency_currency') THEN
        ALTER TABLE physical.physical_pos_currency
        ADD CONSTRAINT fk_physical_pos_currency_currency
        FOREIGN KEY (currency_code)
        REFERENCES core.currency (code)
        ON DELETE RESTRICT;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_physical_pos_currency_physical_pos') THEN
        ALTER TABLE physical.physical_pos_currency
        ADD CONSTRAINT fk_physical_pos_currency_physical_pos
        FOREIGN KEY (physical_pos_id)
        REFERENCES physical.physical_pos (id)
        ON DELETE CASCADE;
    END IF;
END $$;

-- merchant.merchant_device_api_key
CREATE TABLE IF NOT EXISTS merchant.merchant_device_api_key (
    id uuid NOT NULL,
    public_key varchar(100) NOT NULL,
    private_key_encrypted varchar(100) NOT NULL,
    merchant_physical_device_id uuid NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_merchant_device_api_key PRIMARY KEY (id)
);

-- merchant.merchant_pyhsical_pos
CREATE TABLE IF NOT EXISTS merchant.merchant_pyhsical_pos (
    id uuid NOT NULL,
    merchant_physical_device_id uuid NOT NULL,
    physical_pos_id uuid NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50),
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_merchant_pyhsical_pos PRIMARY KEY (id)
);

-- INDEXLER
CREATE INDEX IF NOT EXISTS ix_cost_profile_physical_pos_id
    ON core.cost_profile (physical_pos_id);

CREATE UNIQUE INDEX IF NOT EXISTS ix_device_inventory_unique
    ON physical.device_inventory (device_model, physical_pos_vendor, device_type, serial_no);

CREATE INDEX IF NOT EXISTS ix_merchant_device_api_key_device_id
    ON merchant.merchant_device_api_key (merchant_physical_device_id);

CREATE INDEX IF NOT EXISTS ix_merchant_physical_device_device_inventory_id
    ON merchant.merchant_physical_device (device_inventory_id);

CREATE INDEX IF NOT EXISTS ix_merchant_physical_device_merchant_id
    ON merchant.merchant_physical_device (merchant_id);

CREATE INDEX IF NOT EXISTS ix_merchant_pyhsical_pos_device_id
    ON merchant.merchant_pyhsical_pos (merchant_physical_device_id);

CREATE INDEX IF NOT EXISTS ix_merchant_pyhsical_pos_physical_pos_id
    ON merchant.merchant_pyhsical_pos (physical_pos_id);

CREATE INDEX IF NOT EXISTS ix_physical_pos_acquire_bank_id
    ON physical.physical_pos (acquire_bank_id);

CREATE INDEX IF NOT EXISTS ix_physical_pos_currency_currency_code
    ON physical.physical_pos_currency (currency_code);

CREATE INDEX IF NOT EXISTS ix_physical_pos_currency_physical_pos_id
    ON physical.physical_pos_currency (physical_pos_id);

-- FK: cost_profile → physical_pos
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conname = 'fk_cost_profile_physical_pos_physical_pos_id'
    ) THEN
        ALTER TABLE core.cost_profile
        ADD CONSTRAINT fk_cost_profile_physical_pos_physical_pos_id
        FOREIGN KEY (physical_pos_id)
        REFERENCES physical.physical_pos (id)
        ON DELETE RESTRICT;
    END IF;
END $$;

COMMIT;