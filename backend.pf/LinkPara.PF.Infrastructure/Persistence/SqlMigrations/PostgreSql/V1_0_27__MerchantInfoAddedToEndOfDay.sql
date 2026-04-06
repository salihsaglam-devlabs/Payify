ALTER TABLE physical.end_of_day ADD COLUMN IF NOT EXISTS merchant_name character varying(150);
ALTER TABLE physical.end_of_day ADD COLUMN IF NOT EXISTS merchant_number character varying(15);
