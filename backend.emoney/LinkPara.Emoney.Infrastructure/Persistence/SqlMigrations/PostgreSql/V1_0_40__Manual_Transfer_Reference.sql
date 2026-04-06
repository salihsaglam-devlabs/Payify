 
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'manual_transfer_reference' AND indexname = 'ix_manual_transfer_reference_transaction_id'
  ) THEN
    CREATE INDEX ix_manual_transfer_reference_transaction_id 
    ON core.manual_transfer_reference USING btree (transaction_id);
  END IF;
END $$;
 
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT indexname FROM pg_indexes
      WHERE schemaname = 'core' AND tablename = 'manual_transfer_reference' AND indexname = 'ix_manual_transfer_reference_transaction_id_document_type'
  ) THEN
    CREATE INDEX ix_manual_transfer_reference_transaction_id_document_type 
    ON core.manual_transfer_reference USING btree (transaction_id, document_type);
  END IF;
END $$;