DO $$
BEGIN
 
CREATE TABLE IF NOT EXISTS core.manual_transfer_reference (
	id uuid NOT NULL,
	transaction_id uuid NOT NULL,
	approval_request_id uuid NOT NULL,
	document_type_id uuid NOT NULL,
	document_type varchar(50) NOT NULL,
	document_id uuid NOT NULL,
	create_date timestamp NOT NULL,
	update_date timestamp NULL,
	created_by varchar(50) NOT NULL,
	last_modified_by varchar(50) NULL,
	record_status varchar(50) NOT NULL,
	CONSTRAINT pk_manual_transfer_reference PRIMARY KEY (id)
);
 
END $$;
