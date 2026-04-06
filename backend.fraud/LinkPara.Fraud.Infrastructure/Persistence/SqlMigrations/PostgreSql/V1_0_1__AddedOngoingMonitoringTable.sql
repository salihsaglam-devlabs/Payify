 DO $$
BEGIN
  IF NOT EXISTS (
     SELECT 1 FROM information_schema.columns 
     WHERE table_schema = 'core'
       AND table_name = 'search_log'
       AND column_name = 'reference_number'
 ) THEN
     ALTER TABLE core.search_log ADD reference_number character varying(50);
 END IF;
 END$$;


 DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
          AND table_name = 'ongoing_monitoring'
    ) THEN
        CREATE TABLE core.ongoing_monitoring (
             id uuid NOT NULL,
             search_name character varying(200) NOT NULL,
             search_type character varying(50) NOT NULL,
             scan_id character varying(50),
             period character varying(50) NOT NULL,
             is_ongoing_list boolean NOT NULL,
             create_date timestamp without time zone NOT NULL,
             update_date timestamp without time zone,
             created_by character varying(50) NOT NULL,
             last_modified_by character varying(50),
             record_status character varying(50) NOT NULL,
             CONSTRAINT pk_ongoing_monitoring PRIMARY KEY (id)
        );
    END IF;
END $$;