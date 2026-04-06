DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'security_picture'
    ) THEN
CREATE TABLE core.security_picture (
    id uuid NOT NULL,
    name varchar(100) NOT NULL,
    bytes bytea NOT NULL,
    content_type varchar(50) NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_security_picture PRIMARY KEY (id)
);
END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_security_picture'
    ) THEN
CREATE TABLE core.user_security_picture (
    id uuid NOT NULL,
    user_id uuid NOT NULL,
    security_picture_id uuid NOT NULL,
    create_date timestamp NOT NULL,
    update_date timestamp NULL,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) NULL,
    record_status varchar(50) NOT NULL,
    CONSTRAINT pk_user_security_picture PRIMARY KEY (id)
);
END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'core' AND indexname = 'ix_user_security_picture_user_id'
    ) THEN
CREATE INDEX ix_user_security_picture_user_id ON core.user_security_picture USING btree (user_id);
END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes
        WHERE schemaname = 'core' AND indexname = 'ix_user_security_picture_security_picture_id'
    ) THEN
CREATE INDEX ix_user_security_picture_security_picture_id ON core.user_security_picture USING btree (security_picture_id);
END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'user_security_picture'
          AND constraint_name = 'fk_user_security_picture_security_picture_security_picture_id'
    ) THEN
ALTER TABLE core.user_security_picture
    ADD CONSTRAINT fk_user_security_picture_security_picture_security_picture_id
        FOREIGN KEY (security_picture_id)
            REFERENCES core.security_picture(id)
            ON DELETE CASCADE;
END IF;
END $$;
