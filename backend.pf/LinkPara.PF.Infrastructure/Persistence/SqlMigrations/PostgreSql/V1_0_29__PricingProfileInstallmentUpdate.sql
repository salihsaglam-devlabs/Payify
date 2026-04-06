DO $$
BEGIN

IF EXISTS (
    SELECT 1 FROM information_schema.tables
    WHERE table_schema = 'core' AND table_name = 'pricing_profile'
)
AND NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema = 'core'
      AND table_name = 'pricing_profile'
      AND column_name = 'profile_settlement_mode'
) THEN
ALTER TABLE core.pricing_profile
    ADD COLUMN profile_settlement_mode VARCHAR(50) NOT NULL DEFAULT 'SingleBlock';
END IF;

IF EXISTS (
    SELECT 1 FROM information_schema.tables
    WHERE table_schema = 'core' AND table_name = 'cost_profile'
)
AND NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema = 'core'
      AND table_name = 'cost_profile'
      AND column_name = 'profile_settlement_mode'
) THEN
ALTER TABLE core.cost_profile
    ADD COLUMN profile_settlement_mode VARCHAR(50) NOT NULL DEFAULT 'SingleBlock';
END IF;

IF NOT EXISTS (
    SELECT 1 FROM information_schema.tables
    WHERE table_schema = 'core' AND table_name = 'cost_profile_installment'
) THEN
    CREATE TABLE core.cost_profile_installment (
        id uuid NOT NULL,
        installment_sequence integer NOT NULL,
        blocked_day_number integer NOT NULL,
        cost_profile_item_id uuid NOT NULL,
        create_date timestamp NOT NULL,
        update_date timestamp,
        created_by VARCHAR(50) NOT NULL,
        last_modified_by VARCHAR(50),
        record_status VARCHAR(50) NOT NULL,
        CONSTRAINT pk_cost_profile_installment PRIMARY KEY (id),
        CONSTRAINT fk_cost_profile_installment_cost_profile_item
            FOREIGN KEY (cost_profile_item_id)
                REFERENCES core.cost_profile_item (id)
                ON DELETE RESTRICT
);
END IF;

IF NOT EXISTS (
    SELECT 1 FROM information_schema.tables
    WHERE table_schema = 'core' AND table_name = 'pricing_profile_installment'
) THEN
    CREATE TABLE core.pricing_profile_installment (
        id uuid NOT NULL,
        installment_sequence integer NOT NULL,
        blocked_day_number integer NOT NULL,
        pricing_profile_item_id uuid NOT NULL,
        create_date timestamp NOT NULL,
        update_date timestamp,
        created_by VARCHAR(50) NOT NULL,
        last_modified_by VARCHAR(50),
        record_status VARCHAR(50) NOT NULL,
        CONSTRAINT pk_pricing_profile_installment PRIMARY KEY (id),
        CONSTRAINT fk_pricing_profile_installment_pricing_profile_item
            FOREIGN KEY (pricing_profile_item_id)
                REFERENCES core.pricing_profile_item (id)
                ON DELETE RESTRICT
);
END IF;

IF NOT EXISTS (
    SELECT 1 FROM pg_indexes
    WHERE schemaname = 'core'
      AND indexname = 'ix_cost_profile_installment_cost_profile_item_id_installment_s'
) THEN
CREATE UNIQUE INDEX ix_cost_profile_installment_cost_profile_item_id_installment_s
    ON core.cost_profile_installment (cost_profile_item_id, installment_sequence);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'core'
          AND indexname = 'ix_pricing_profile_installment_pricing_profile_item_id_install'
    ) THEN
CREATE UNIQUE INDEX ix_pricing_profile_installment_pricing_profile_item_id_install
    ON core.pricing_profile_installment (pricing_profile_item_id, installment_sequence);
END IF;

END $$;