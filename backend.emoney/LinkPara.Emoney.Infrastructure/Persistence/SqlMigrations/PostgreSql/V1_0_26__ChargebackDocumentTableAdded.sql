DO $$
BEGIN
CREATE TABLE IF NOT EXISTS core.chargeback_document (   
    id uuid NOT NULL,
    chargeback_id uuid NOT NULL,
    transaction_id uuid NOT NULL,
    document_id uuid NOT NULL,
    document_type_id uuid NOT NULL,
    description varchar(500),
    file_name varchar(200)  NOT NULL,
    create_date timestamp without time zone NOT NULL,
    update_date timestamp without time zone,
    created_by varchar(50) NOT NULL,
    last_modified_by varchar(50) ,
    record_status varchar(50)NOT NULL
);
END $$;
