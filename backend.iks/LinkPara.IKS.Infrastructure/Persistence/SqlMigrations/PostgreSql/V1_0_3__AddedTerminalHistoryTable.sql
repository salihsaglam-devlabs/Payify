-- Sütun ekleme kontrolleri
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal' AND column_name = 'created_name_by'
    ) THEN
        ALTER TABLE core.iks_terminal ADD created_name_by text;
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal' AND column_name = 'terminal_status'
    ) THEN
        ALTER TABLE core.iks_terminal ADD terminal_status character varying(50) NOT NULL DEFAULT '';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal' AND column_name = 'vpos_id'
    ) THEN
        ALTER TABLE core.iks_terminal ADD vpos_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $$;

-- Tablo oluşturma kontrolü
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'core' AND table_name = 'iks_terminal_history'
    ) THEN
        CREATE TABLE core.iks_terminal_history (
            id uuid NOT NULL,
            merchant_id uuid NOT NULL,
            vpos_id uuid NOT NULL,
            reference_code character varying(12) NOT NULL,
            new_data character varying(1000),
            old_data character varying(1000),
            changed_field character varying(100),
            response_code character varying(3),
            response_code_explanation character varying(2000),
            query_date timestamp without time zone NOT NULL,
            terminal_record_type character varying(50) NOT NULL,
            create_date timestamp without time zone NOT NULL,
            update_date timestamp without time zone,
            created_by character varying(50) NOT NULL,
            last_modified_by character varying(50),
            record_status character varying(50) NOT NULL,
            CONSTRAINT pk_iks_terminal_history PRIMARY KEY (id)
        );
    END IF;
END $$;
