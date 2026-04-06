DO $$ 
BEGIN
    CREATE TABLE IF NOT EXISTS core.call_center_notification_log (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        first_name varchar(50) NOT NULL,
        last_name varchar(50) NOT NULL,
        phone_number varchar(20) NOT NULL,
        confirmation_type varchar(50) NOT NULL,
        status varchar(50) NOT NULL,
        expire_date timestamp NOT NULL,
        error_message varchar(300) NULL,
        create_date timestamp NOT NULL,
        update_date timestamp NULL,
        created_by varchar(50) NOT NULL,
        last_modified_by varchar(50) NULL,
        record_status varchar(50) NOT NULL
    );

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'pk_call_center_notification_log'
    ) THEN
        ALTER TABLE core.call_center_notification_log 
        ADD CONSTRAINT pk_call_center_notification_log PRIMARY KEY (id);
    END IF;

END $$;
