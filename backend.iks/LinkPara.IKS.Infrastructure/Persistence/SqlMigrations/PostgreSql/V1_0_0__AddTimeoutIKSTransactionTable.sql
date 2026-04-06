
DO $$
BEGIN

CREATE TABLE IF NOT EXISTS core.timeout_iks_transaction (
    id uuid NOT NULL,
    operation text NOT NULL,
    response_code character varying(20) NOT NULL,
    is_success boolean NOT NULL,
    request_details text NULL,
    response_details text NULL,
    timeout_return_details text NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone NULL,
    created_by character varying(50) NOT NULL,
    last_modified_by character varying(50) NULL,
    record_status character varying(50) NOT NULL,
    CONSTRAINT pk_timeout_iks_transaction PRIMARY KEY (id)
);

END $$;



