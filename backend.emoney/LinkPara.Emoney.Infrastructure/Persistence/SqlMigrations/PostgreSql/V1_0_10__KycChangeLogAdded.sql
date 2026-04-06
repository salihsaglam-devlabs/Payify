DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.account_kyc_change (
                                         id uuid NOT NULL,
                                         account_id uuid NOT NULL,
                                         validation_type character varying(50) NOT NULL,
                                         old_kyc_level character varying(50) NOT NULL,
                                         new_kyc_level character varying(50) NOT NULL,
                                         is_upgraded boolean NOT NULL,
                                         error_message text NULL,
                                         create_date timestamp without time zone NOT NULL,
                                         update_date timestamp without time zone NULL,
                                         created_by character varying(50) NOT NULL,
                                         last_modified_by character varying(50) NULL,
                                         record_status character varying(50) NOT NULL,
                                         CONSTRAINT pk_account_kyc_change PRIMARY KEY (id)
);
END $$;