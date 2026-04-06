DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
CREATE SCHEMA core;
END IF;
END $EF$;

CREATE TABLE IF NOT EXISTS core.integration_log (
                                                    id uuid NOT NULL,
                                                    transaction_monitoring_id uuid NOT NULL,
                                                    request text NULL,
                                                    response text NULL,
                                                    is_success bool NOT NULL,
                                                    create_date timestamp NOT NULL,
                                                    update_date timestamp NULL,
                                                    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_integration_log PRIMARY KEY (id)
    );


CREATE TABLE IF NOT EXISTS core.search_log (
                                               id uuid NOT NULL,
                                               search_name varchar(200) NOT NULL,
    birth_year varchar(10) NULL,
    search_type varchar(50) NOT NULL,
    match_status varchar(50) NOT NULL,
    match_rate int4 NOT NULL,
    is_black_list bool NOT NULL,
    blacklist_name varchar(500) NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    expire_date timestamp NULL,
    client_ip_address varchar(50) NULL,
    CONSTRAINT pk_search_log PRIMARY KEY (id)
    );

CREATE TABLE IF NOT EXISTS core.transaction_monitoring (
                                                           id uuid NOT NULL,
                                                           "module" varchar(100) NOT NULL,
    command_name varchar(200) NOT NULL,
    transfer_request text NOT NULL,
    command_json text NOT NULL,
    sender_number varchar(50) NULL,
    receiver_number varchar(50) NULL,
    amount numeric(18, 4) NOT NULL,
    currency_code varchar(10) NOT NULL,
    transaction_id varchar(50) NOT NULL,
    total_score int4 NOT NULL,
    monitoring_status varchar(50) NOT NULL,
    risk_level varchar(50) NOT NULL,
    transaction_date timestamp NOT NULL,
    error_code varchar(100) NULL,
    error_message varchar(300) NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    receiver_name varchar(100) NULL,
    sender_name varchar(100) NULL,
    client_ip_address varchar(50) NULL,
    CONSTRAINT pk_transaction_monitoring PRIMARY KEY (id)
    );

CREATE TABLE IF NOT EXISTS core.triggered_rule (
                                                   id uuid NOT NULL,
                                                   rule_key varchar(300) NOT NULL,
    score int4 NOT NULL,
    transaction_monitoring_id uuid NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_triggered_rule PRIMARY KEY (id)
    );


CREATE TABLE IF NOT EXISTS core.triggered_rule_set_key (
                                                           id uuid NOT NULL,
                                                           operation varchar(50) NOT NULL,
    "level" varchar(50) NOT NULL,
    rule_set_key varchar(50) NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_triggered_rule_set_key PRIMARY KEY (id)
    );

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'triggered_rule'
          AND constraint_name = 'fk_triggered_rule_transaction_monitoring_transaction_monitorin'
    ) THEN
ALTER TABLE "core".triggered_rule
    ADD CONSTRAINT fk_triggered_rule_transaction_monitoring_transaction_monitorin
        FOREIGN KEY (transaction_monitoring_id)
            REFERENCES core.transaction_monitoring(id)
            ON DELETE CASCADE;
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'integration_log'
          AND constraint_name = 'fk_integration_log_transaction_monitoring_transaction_monitori'
    ) THEN
ALTER TABLE "core".integration_log
    ADD CONSTRAINT fk_integration_log_transaction_monitoring_transaction_monitori
        FOREIGN KEY (transaction_monitoring_id)
            REFERENCES core.transaction_monitoring(id)
            ON DELETE CASCADE;
END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS pk_triggered_rule_set_key ON core.triggered_rule_set_key USING btree (id);

CREATE UNIQUE INDEX IF NOT EXISTS pk_search_log ON core.search_log USING btree (id);

CREATE INDEX IF NOT EXISTS ix_integration_log_transaction_monitoring_id ON core.integration_log USING btree (transaction_monitoring_id);

CREATE UNIQUE INDEX IF NOT EXISTS pk_integration_log ON core.integration_log USING btree (id);

CREATE INDEX IF NOT EXISTS ix_triggered_rule_transaction_monitoring_id ON core.triggered_rule USING btree (transaction_monitoring_id);

CREATE UNIQUE INDEX IF NOT EXISTS pk_triggered_rule ON core.triggered_rule USING btree (id);

CREATE UNIQUE INDEX IF NOT EXISTS pk_transaction_monitoring ON core.transaction_monitoring USING btree (id);