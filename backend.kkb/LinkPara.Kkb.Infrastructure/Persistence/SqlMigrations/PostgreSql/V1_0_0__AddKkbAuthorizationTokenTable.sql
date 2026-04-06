

CREATE TABLE IF NOT EXISTS core.kkb_authorization_token
(
    id uuid NOT NULL,
    access_token text COLLATE pg_catalog."default",
    refresh_token text COLLATE pg_catalog."default",
    token_type character varying(30) COLLATE pg_catalog."default",
    expires_date timestamp without time zone NOT NULL,
    is_success boolean NOT NULL,
    error character varying(30) COLLATE pg_catalog."default",
    error_description text COLLATE pg_catalog."default",
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by character varying(50) COLLATE pg_catalog."default" NOT NULL,
    last_modified_by character varying(50) COLLATE pg_catalog."default",
    record_status character varying(50) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_kkb_authorization_token PRIMARY KEY (id)
)




