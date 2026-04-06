

    DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'physical'
          AND table_name = 'device_inventory_history'
    ) THEN
        CREATE TABLE physical.device_inventory_history (
          id uuid NOT NULL,
          device_inventory_id uuid NOT NULL,
          device_history_type character varying(50) NOT NULL,
          new_data character varying(500),
          old_data character varying(500),
          detail character varying(256),
          created_name_by character varying(256),
          create_date timestamp without time zone NOT NULL,
          update_date timestamp without time zone,
          created_by character varying(50) NOT NULL,
          last_modified_by character varying(50),
          record_status character varying(50) NOT NULL,
          CONSTRAINT pk_device_inventory_history PRIMARY KEY (id),
          CONSTRAINT fk_device_inventory_history_device_inventory_device_inventory_
        FOREIGN KEY (device_inventory_id)
        REFERENCES physical.device_inventory (id)
        ON DELETE CASCADE
        );
    END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'physical'
          AND tablename = 'device_inventory_history'
          AND indexname = 'ix_device_inventory_history_device_inventory_id'
    ) THEN
        CREATE INDEX ix_device_inventory_history_device_inventory_id ON physical.device_inventory_history USING btree (device_inventory_id);
    END IF;
END $$;

