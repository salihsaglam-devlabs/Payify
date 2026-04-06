DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'core') THEN
CREATE SCHEMA core;
END IF;
END $EF$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
          AND table_name = 'agreement_document'
    ) THEN
CREATE TABLE core.agreement_document (
                                         id uuid NOT NULL,
                                         "name" varchar(50) NOT NULL,
                                         last_version varchar(10) NOT NULL,
                                         language_code varchar(10) NOT NULL,
                                         create_date timestamp NOT NULL,
                                         update_date timestamp NULL,
                                         created_by varchar(50) NOT NULL,
                                         last_modified_by varchar(50) NULL,
                                         record_status varchar(50) NOT NULL,
                                         product_type varchar(50) DEFAULT ''::character varying NOT NULL,
                                         CONSTRAINT pk_agreement_document PRIMARY KEY (id)
);
END IF;
END $$;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
          AND table_name = 'agreement_document_version'
    ) THEN
CREATE TABLE core.agreement_document_version (
                                                 id uuid NOT NULL,
                                                 agreement_document_id uuid NOT NULL,
                                                 "content" text NULL,
                                                 title varchar(150) NOT NULL,
                                                 language_code varchar(10) NOT NULL,
                                                 version varchar(10) NOT NULL,
                                                 is_latest bool NOT NULL,
                                                 is_force_update bool NOT NULL,
                                                 create_date timestamp NOT NULL,
                                                 update_date timestamp NULL,
                                                 created_by varchar(50) NOT NULL,
                                                 last_modified_by varchar(50) NULL,
                                                 record_status varchar(50) NOT NULL,
                                                 is_optional bool DEFAULT false NOT NULL,
                                                 CONSTRAINT pk_agreement_document_version PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'agreement_document_version'
          AND constraint_name = 'fk_agreement_document_version_agreement_document_agreement_doc'
    ) THEN
ALTER TABLE core.agreement_document_version
    ADD CONSTRAINT fk_agreement_document_version_agreement_document_agreement_doc
        FOREIGN KEY (agreement_document_id)
            REFERENCES core.agreement_document(id)
            ON DELETE CASCADE;
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'device_info'
    ) THEN
CREATE TABLE core.device_info (
                                  id uuid NOT NULL,
                                  device_id varchar(255) NULL,
                                  device_type varchar(50) NULL,
                                  device_name varchar(255) NULL,
                                  registration_token varchar(1000) NOT NULL,
                                  manufacturer varchar(20) NULL,
                                  model varchar(255) NULL,
                                  operating_system varchar(50) NULL,
                                  operating_system_version varchar(255) NULL,
                                  screen_resolution varchar(50) NULL,
                                  app_version varchar(40) NULL,
                                  app_build_number varchar(255) NULL,
                                  camera varchar(1000) NULL,
                                  create_date timestamp NOT NULL,
                                  update_date timestamp NULL,
                                  created_by varchar(50) NOT NULL,
                                  last_modified_by varchar(50) NULL,
                                  record_status varchar(50) NOT NULL,
                                  CONSTRAINT pk_device_info PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'login_whitelist'
    ) THEN
CREATE TABLE core.login_whitelist (
                                      id uuid NOT NULL,
                                      phone_code varchar(10) NOT NULL,
                                      phone_number varchar(50) NOT NULL,
                                      first_name varchar(50) NOT NULL,
                                      last_name varchar(50) NOT NULL,
                                      email varchar(50) NULL,
                                      create_date timestamp NOT NULL,
                                      update_date timestamp NULL,
                                      created_by varchar(50) NOT NULL,
                                      last_modified_by varchar(50) NULL,
                                      record_status varchar(50) NOT NULL,
                                      CONSTRAINT pk_login_whitelist PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'permission'
    ) THEN
CREATE TABLE core."permission" (
                                   id uuid NOT NULL,
                                   claim_type varchar(200) NULL,
                                   claim_value varchar(250) NOT NULL,
                                   "module" varchar(200) NULL,
                                   operation_type varchar(50) NOT NULL,
                                   normalized_claim_value varchar(250) NOT NULL,
                                   description varchar(450) NULL,
                                   display_name varchar(200) NULL,
                                   display bool NOT NULL,
                                   create_date timestamp NOT NULL,
                                   update_date timestamp NULL,
                                   created_by varchar(50) NOT NULL,
                                   last_modified_by varchar(50) NULL,
                                   record_status varchar(50) NOT NULL,
                                   CONSTRAINT pk_permission PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'role'
    ) THEN
CREATE TABLE core."role" (
                             id uuid NOT NULL,
                             create_date timestamp NOT NULL,
                             update_date timestamp NOT NULL,
                             last_modified_by varchar(100) NULL,
                             created_by varchar(50) NOT NULL,
                             record_status varchar(50) NOT NULL,
                             role_scope varchar(50) NOT NULL,
                             "name" varchar(256) NOT NULL,
                             normalized_name varchar(256) NULL,
                             concurrency_stamp varchar(450) NULL,
                             can_see_sensitive_data bool DEFAULT false NOT NULL,
                             CONSTRAINT pk_role PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'login_activity'
    ) THEN
CREATE TABLE core.login_activity (
                                     id uuid NOT NULL,
                                     user_id uuid NOT NULL,
                                     ip text NULL,
                                     "date" timestamp NOT NULL,
                                     port text NULL,
                                     login_result varchar(50) NOT NULL,
                                     create_date timestamp NOT NULL,
                                     update_date timestamp NULL,
                                     created_by varchar(50) NOT NULL,
                                     last_modified_by varchar(50) NULL,
                                     record_status varchar(50) NOT NULL,
                                     channel varchar(300) NULL,
                                     CONSTRAINT pk_login_activity PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'screen'
    ) THEN
CREATE TABLE core.screen (
                             id uuid NOT NULL,
                             "name" varchar(100) NOT NULL,
                             "module" varchar(100) NOT NULL,
                             operation_type varchar(50) NOT NULL,
                             create_date timestamp NOT NULL,
                             update_date timestamp NULL,
                             created_by varchar(50) NOT NULL,
                             last_modified_by varchar(50) NULL,
                             record_status varchar(50) NOT NULL,
                             icon text NULL,
                             link text NULL,
                             module_icon text NULL,
                             module_link text NULL,
                             module_priority int4 DEFAULT 0 NOT NULL,
                             priority int4 DEFAULT 0 NOT NULL,
                             CONSTRAINT pk_screen PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'security_question'
    ) THEN
CREATE TABLE core.security_question (
                                        id uuid NOT NULL,
                                        question varchar(100) NOT NULL,
                                        language_code varchar(10) NOT NULL,
                                        create_date timestamp NOT NULL,
                                        update_date timestamp NULL,
                                        created_by varchar(50) NOT NULL,
                                        last_modified_by varchar(50) NULL,
                                        record_status varchar(50) NOT NULL,
                                        CONSTRAINT pk_security_question PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'user'
    ) THEN
CREATE TABLE core."user" (
                             id uuid NOT NULL,
                             first_name varchar(50) NOT NULL,
                             last_name varchar(50) NOT NULL,
                             identity_number varchar(50) NULL,
                             birth_date timestamp NOT NULL,
                             user_type varchar(50) NOT NULL,
                             user_status varchar(50) NOT NULL,
                             phone_code varchar(10) NOT NULL,
                             create_date timestamp NOT NULL,
                             update_date timestamp NOT NULL,
                             last_modified_by varchar(100) NULL,
                             created_by varchar(50) NOT NULL,
                             password_modified_date timestamp NOT NULL,
                             record_status varchar(50) NOT NULL,
                             login_last_activity_id uuid NULL,
                             user_name varchar(256) NULL,
                             normalized_user_name varchar(256) NULL,
                             email varchar(256) NULL,
                             normalized_email varchar(256) NULL,
                             email_confirmed bool NOT NULL,
                             password_hash text NULL,
                             security_stamp text NULL,
                             concurrency_stamp text NULL,
                             phone_number varchar(50) NOT NULL,
                             phone_number_confirmed bool NOT NULL,
                             two_factor_enabled bool NOT NULL,
                             lockout_end timestamptz NULL,
                             lockout_enabled bool NOT NULL,
                             access_failed_count int4 NOT NULL,
                             CONSTRAINT pk_user PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'screen_claim'
    ) THEN
CREATE TABLE core.screen_claim (
                                   id uuid NOT NULL,
                                   screen_id uuid NOT NULL,
                                   claim_value text NULL,
                                   create_date timestamp NOT NULL,
                                   update_date timestamp NULL,
                                   created_by varchar(50) NOT NULL,
                                   last_modified_by varchar(50) NULL,
                                   record_status varchar(50) NOT NULL,
                                   CONSTRAINT pk_screen_claim PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'user_address'
    ) THEN
CREATE TABLE core.user_address (
                                   id uuid NOT NULL,
                                   user_id uuid NOT NULL,
                                   country_id int4 NOT NULL,
                                   city_id int4 NOT NULL,
                                   district_id int4 NOT NULL,
                                   neighbourhood varchar(450) NOT NULL,
                                   street varchar(450) NOT NULL,
                                   full_address varchar(600) NOT NULL,
                                   create_date timestamp NOT NULL,
                                   update_date timestamp NULL,
                                   created_by varchar(50) NOT NULL,
                                   last_modified_by varchar(100) NULL,
                                   record_status varchar(50) NOT NULL,
                                   CONSTRAINT pk_user_address PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'role_claim'
    ) THEN
CREATE TABLE core.role_claim (
                                 id int4 GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                                 create_date timestamp NOT NULL,
                                 update_date timestamp NOT NULL,
                                 last_modified_by varchar(100) NULL,
                                 created_by varchar(50) NOT NULL,
                                 record_status varchar(50) NOT NULL,
                                 role_id uuid NOT NULL,
                                 claim_type text NULL,
                                 claim_value text NULL
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables WHERE table_schema = 'core' AND table_name = 'role_screen'
    ) THEN
CREATE TABLE core.role_screen (
                                  id uuid NOT NULL,
                                  role_id uuid NOT NULL,
                                  screen_id uuid NOT NULL,
                                  create_date timestamp NOT NULL,
                                  update_date timestamp NULL,
                                  created_by varchar(50) NOT NULL,
                                  last_modified_by varchar(50) NULL,
                                  record_status varchar(50) NOT NULL,
                                  CONSTRAINT pk_role_screen PRIMARY KEY (id)
);
END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_agreement_document'
    ) THEN
CREATE TABLE core.user_agreement_document (
                                              id uuid NOT NULL,
                                              user_id uuid NOT NULL,
                                              agreement_document_version_id uuid NOT NULL,
                                              create_date timestamp NOT NULL,
                                              update_date timestamp NULL,
                                              created_by varchar(50) NOT NULL,
                                              last_modified_by varchar(50) NULL,
                                              record_status varchar(50) NOT NULL,
                                              approval_channel varchar(50) NULL,
                                              CONSTRAINT pk_user_agreement_document PRIMARY KEY (id)
);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_claim'
    ) THEN
CREATE TABLE core.user_claim (
                                 id int4 GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                                 description varchar(450) NULL,
                                 display_name varchar(450) NULL,
                                 create_date timestamp NOT NULL,
                                 update_date timestamp NOT NULL,
                                 last_modified_by varchar(100) NULL,
                                 created_by varchar(50) NOT NULL,
                                 record_status varchar(50) NOT NULL,
                                 user_id uuid NOT NULL,
                                 claim_type text NULL,
                                 claim_value text NULL
);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_device_info'
    ) THEN
CREATE TABLE core.user_device_info (
                                       id uuid NOT NULL,
                                       user_id uuid NOT NULL,
                                       is_main_device bool NOT NULL,
                                       device_info_id uuid NOT NULL,
                                       create_date timestamp NOT NULL,
                                       update_date timestamp NULL,
                                       created_by varchar(50) NOT NULL,
                                       last_modified_by varchar(50) NULL,
                                       record_status varchar(50) NOT NULL,
                                       CONSTRAINT pk_user_device_info PRIMARY KEY (id)
);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_login'
    ) THEN
CREATE TABLE core.user_login (
                                 login_provider text NOT NULL,
                                 provider_key text NOT NULL,
                                 provider_display_name text NULL,
                                 user_id uuid NOT NULL,
                                 CONSTRAINT pk_user_login PRIMARY KEY (login_provider, provider_key)
);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_login_last_activity'
    ) THEN
CREATE TABLE core.user_login_last_activity (
                                               id uuid NOT NULL,
                                               last_succeeded_login timestamp NULL,
                                               last_locked_login timestamp NULL,
                                               last_failed_login timestamp NULL,
                                               login_result varchar(50) NOT NULL,
                                               user_id uuid NOT NULL,
                                               create_date timestamp NOT NULL,
                                               update_date timestamp NULL,
                                               created_by varchar(50) NOT NULL,
                                               last_modified_by varchar(50) NULL,
                                               record_status varchar(50) NOT NULL,
                                               failed_login_count int4 DEFAULT 0 NOT NULL,
                                               channel varchar(300) NULL,
                                               CONSTRAINT pk_user_login_last_activity PRIMARY KEY (id)
);
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = 'core' AND table_name = 'user_password_history'
    ) THEN
CREATE TABLE core.user_password_history (
                                            id uuid NOT NULL,
                                            user_id uuid NOT NULL,
                                            password_hash text NULL,
                                            create_date timestamp NOT NULL,
                                            update_date timestamp NULL,
                                            created_by varchar(50) NOT NULL,
                                            last_modified_by varchar(50) NULL,
                                            record_status varchar(50) NOT NULL,
                                            CONSTRAINT pk_user_password_history PRIMARY KEY (id)
);
END IF;
END $$;

    
DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
        AND table_name = 'user_picture'
    ) THEN
CREATE TABLE core.user_picture (
                                   id uuid NOT NULL,
                                   bytes bytea NULL,
                                   content_type text NULL,
                                   user_id uuid NOT NULL,
                                   create_date timestamp NOT NULL,
                                   update_date timestamp NULL,
                                   created_by varchar(50) NOT NULL,
                                   last_modified_by varchar(50) NULL,
                                   record_status varchar(50) NOT NULL,
                                   CONSTRAINT pk_user_picture PRIMARY KEY (id)
);
END IF;
END
$$;


DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
        AND table_name = 'user_role'
    ) THEN
CREATE TABLE core.user_role (
                                user_id uuid NOT NULL,
                                role_id uuid NOT NULL,
                                CONSTRAINT pk_user_role PRIMARY KEY (user_id, role_id)
);
END IF;
END
$$;


DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
        AND table_name = 'user_security_answer'
    ) THEN
CREATE TABLE core.user_security_answer (
                                           id uuid NOT NULL,
                                           user_id uuid NOT NULL,
                                           security_question_id uuid NOT NULL,
                                           answer_hash varchar(400) NOT NULL,
                                           create_date timestamp NOT NULL,
                                           update_date timestamp NULL,
                                           created_by varchar(50) NOT NULL,
                                           last_modified_by varchar(50) NULL,
                                           record_status varchar(50) NOT NULL,
                                           CONSTRAINT pk_user_security_answer PRIMARY KEY (id)
);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
        AND table_name = 'user_session'
    ) THEN
CREATE TABLE core.user_session (
                                   id uuid NOT NULL,
                                   user_id uuid NOT NULL,
                                   refresh_token text NULL,
                                   refresh_token_expiration timestamp NOT NULL,
                                   create_date timestamp NOT NULL,
                                   update_date timestamp NULL,
                                   created_by varchar(50) NOT NULL,
                                   last_modified_by varchar(50) NULL,
                                   record_status varchar(50) NOT NULL,
                                   CONSTRAINT pk_user_session PRIMARY KEY (id)
);
END IF;
END
$$;


DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'core'
        AND table_name = 'user_token'
    ) THEN
CREATE TABLE core.user_token (
                                 user_id uuid NOT NULL,
                                 login_provider text NOT NULL,
                                 name text NOT NULL,
                                 value text NULL,
                                 CONSTRAINT pk_user_token PRIMARY KEY (user_id, login_provider, name)
);
END IF;
END
$$;

DO
$$
BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.tables
            WHERE table_schema = 'public'
              AND table_name = 'max_id'
        ) THEN
CREATE TABLE public.max_id (id int4 NULL);
END IF;
END
$$;

CREATE INDEX IF NOT EXISTS ix_user_agreement_document_agreement_document_version_id
    ON core.user_agreement_document (agreement_document_version_id);

CREATE INDEX IF NOT EXISTS ix_user_agreement_document_user_id
    ON core.user_agreement_document (user_id);

CREATE INDEX IF NOT EXISTS ix_user_claim_user_id
    ON core.user_claim (user_id);

CREATE INDEX IF NOT EXISTS ix_user_device_info_device_info_id
    ON core.user_device_info (device_info_id);

CREATE INDEX IF NOT EXISTS ix_user_login_user_id
    ON core.user_login (user_id);

CREATE UNIQUE INDEX IF NOT EXISTS ix_user_login_last_activity_user_id
    ON core.user_login_last_activity (user_id);

CREATE INDEX IF NOT EXISTS ix_user_password_history_user_id
    ON core.user_password_history (user_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_agreement_document_agreement_document_version_agreemen'
    ) THEN
ALTER TABLE core.user_agreement_document
    ADD CONSTRAINT fk_user_agreement_document_agreement_document_version_agreemen
        FOREIGN KEY (agreement_document_version_id)
            REFERENCES core.agreement_document_version(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_agreement_document_users_user_id'
    ) THEN
ALTER TABLE core.user_agreement_document
    ADD CONSTRAINT fk_user_agreement_document_users_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_claim_user_user_id'
    ) THEN
ALTER TABLE core.user_claim
    ADD CONSTRAINT fk_user_claim_user_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_device_info_device_info_device_info_id'
    ) THEN
ALTER TABLE core.user_device_info
    ADD CONSTRAINT fk_user_device_info_device_info_device_info_id
        FOREIGN KEY (device_info_id)
            REFERENCES core.device_info(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_login_user_user_id'
    ) THEN
ALTER TABLE core.user_login
    ADD CONSTRAINT fk_user_login_user_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_login_last_activity_users_user_id'
    ) THEN
ALTER TABLE core.user_login_last_activity
    ADD CONSTRAINT fk_user_login_last_activity_users_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_user_password_history_users_user_id'
    ) THEN
ALTER TABLE core.user_password_history
    ADD CONSTRAINT fk_user_password_history_users_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;


DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_picture_user_id'
    ) THEN
CREATE INDEX ix_user_picture_user_id ON core.user_picture USING btree (user_id);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_role_role_id'
    ) THEN
CREATE INDEX ix_user_role_role_id ON core.user_role USING btree (role_id);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_security_answer_security_question_id'
    ) THEN
CREATE INDEX ix_user_security_answer_security_question_id ON core.user_security_answer USING btree (security_question_id);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_security_answer_user_id'
    ) THEN
CREATE INDEX ix_user_security_answer_user_id ON core.user_security_answer USING btree (user_id);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_session_refresh_token_expiration'
    ) THEN
CREATE INDEX ix_user_session_refresh_token_expiration ON core.user_session USING btree (refresh_token_expiration);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'core'
        AND indexname = 'ix_user_session_user_id'
    ) THEN
CREATE INDEX ix_user_session_user_id ON core.user_session USING btree (user_id);
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_picture_user_user_id'
    ) THEN
ALTER TABLE core.user_picture
    ADD CONSTRAINT fk_user_picture_user_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_role_user_user_id'
    ) THEN
ALTER TABLE core.user_role
    ADD CONSTRAINT fk_user_role_user_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_role_role_role_id'
    ) THEN
ALTER TABLE core.user_role
    ADD CONSTRAINT fk_user_role_role_role_id FOREIGN KEY (role_id) REFERENCES core."role"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_security_answer_users_user_id'
    ) THEN
ALTER TABLE core.user_security_answer
    ADD CONSTRAINT fk_user_security_answer_users_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_security_answer_security_question_security_question_id'
    ) THEN
ALTER TABLE core.user_security_answer
    ADD CONSTRAINT fk_user_security_answer_security_question_security_question_id FOREIGN KEY (security_question_id) REFERENCES core.security_question(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_session_users_user_id'
    ) THEN
ALTER TABLE core.user_session
    ADD CONSTRAINT fk_user_session_users_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;

DO
$$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
        AND constraint_name = 'fk_user_token_user_user_id'
    ) THEN
ALTER TABLE core.user_token
    ADD CONSTRAINT fk_user_token_user_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END
$$;


DROP INDEX IF EXISTS core.ix_agreement_document_record_status;

CREATE INDEX IF NOT EXISTS ix_agreement_document_record_status
    ON core.agreement_document (record_status);

DROP INDEX IF EXISTS core.ix_login_whitelist_phone_number;

CREATE UNIQUE INDEX IF NOT EXISTS ix_login_whitelist_phone_number
    ON core.login_whitelist (phone_number);

DROP INDEX IF EXISTS core.ix_agreement_document_version_agreement_document_id;

CREATE INDEX IF NOT EXISTS ix_agreement_document_version_agreement_document_id
    ON core.agreement_document_version (agreement_document_id);


DROP INDEX IF EXISTS core.ix_device_info_device_id;

CREATE INDEX IF NOT EXISTS ix_device_info_device_id
    ON core.device_info (device_id);

DROP INDEX IF EXISTS core.ix_permission_claim_value;

CREATE UNIQUE INDEX IF NOT EXISTS ix_permission_claim_value
    ON core."permission" (claim_value);


DROP INDEX IF EXISTS core."RoleNameIndex";
DROP INDEX IF EXISTS core.ix_role_name;

CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex"
    ON core."role" (normalized_name);

CREATE UNIQUE INDEX IF NOT EXISTS ix_role_name
    ON core."role" ("name");


CREATE INDEX IF NOT EXISTS "EmailIndex" ON core."user" (normalized_email);
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON core."user" (normalized_user_name);
CREATE INDEX IF NOT EXISTS ix_user_email ON core."user" (email);
CREATE UNIQUE INDEX IF NOT EXISTS ix_user_identity_number ON core."user" (identity_number);
CREATE INDEX IF NOT EXISTS ix_user_phone_number ON core."user" (phone_number);
CREATE UNIQUE INDEX IF NOT EXISTS ix_user_user_name ON core."user" (user_name);
CREATE INDEX IF NOT EXISTS ix_user_user_status ON core."user" (user_status);
CREATE INDEX IF NOT EXISTS ix_user_user_type ON core."user" (user_type);

DROP INDEX IF EXISTS core.ix_screen_claim_screen_id;

CREATE INDEX IF NOT EXISTS ix_screen_claim_screen_id ON core.screen_claim (screen_id);


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE table_schema = 'core' AND table_name = 'screen_claim' AND constraint_name = 'fk_screen_claim_screen_screen_id'
    ) THEN
ALTER TABLE core.screen_claim
    ADD CONSTRAINT fk_screen_claim_screen_screen_id FOREIGN KEY (screen_id) REFERENCES core.screen(id) ON DELETE CASCADE;
END IF;
END $$;


DROP INDEX IF EXISTS core.ix_login_activity_user_id;

CREATE INDEX IF NOT EXISTS ix_login_activity_user_id
    ON core.login_activity (user_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_schema = 'core'
          AND table_name = 'login_activity'
          AND constraint_name = 'fk_login_activity_users_user_id'
    ) THEN
ALTER TABLE core.login_activity
    ADD CONSTRAINT fk_login_activity_users_user_id
        FOREIGN KEY (user_id)
            REFERENCES core."user"(id)
            ON DELETE CASCADE;
END IF;
END $$;


DROP INDEX IF EXISTS core.ix_user_address_user_id;

CREATE INDEX IF NOT EXISTS ix_user_address_user_id ON core.user_address (user_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE table_schema = 'core' AND table_name = 'user_address' AND constraint_name = 'fk_user_address_users_user_id'
    ) THEN
ALTER TABLE core.user_address
    ADD CONSTRAINT fk_user_address_users_user_id FOREIGN KEY (user_id) REFERENCES core."user"(id) ON DELETE CASCADE;
END IF;
END $$;

DROP INDEX IF EXISTS core.ix_role_claim_role_id;

CREATE INDEX IF NOT EXISTS ix_role_claim_role_id ON core.role_claim (role_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE table_schema = 'core' AND table_name = 'role_claim' AND constraint_name = 'fk_role_claim_role_role_id'
    ) THEN
ALTER TABLE core.role_claim
    ADD CONSTRAINT fk_role_claim_role_role_id FOREIGN KEY (role_id) REFERENCES core."role"(id) ON DELETE CASCADE;
END IF;
END $$;


DROP INDEX IF EXISTS core.ix_role_screen_role_id;
DROP INDEX IF EXISTS core.ix_role_screen_screen_id;

CREATE INDEX IF NOT EXISTS ix_role_screen_role_id ON core.role_screen (role_id);
CREATE INDEX IF NOT EXISTS ix_role_screen_screen_id ON core.role_screen (screen_id);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE table_schema = 'core' AND table_name = 'role_screen' AND constraint_name = 'fk_role_screen_roles_role_id'
    ) THEN
ALTER TABLE core.role_screen
    ADD CONSTRAINT fk_role_screen_roles_role_id FOREIGN KEY (role_id) REFERENCES core."role"(id) ON DELETE CASCADE;
END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints
        WHERE table_schema = 'core' AND table_name = 'role_screen' AND constraint_name = 'fk_role_screen_screen_screen_id'
    ) THEN
ALTER TABLE core.role_screen
    ADD CONSTRAINT fk_role_screen_screen_screen_id FOREIGN KEY (screen_id) REFERENCES core.screen(id) ON DELETE CASCADE;
END IF;
END $$;

