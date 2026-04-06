DO $$
BEGIN
    
INSERT INTO core.parameter_group
(id, group_code, explanation, parameter_value_type, create_date, update_date, created_by, last_modified_by, record_status)
select 'e709150b-2916-441d-b6cd-084e8d7f3c97'::uuid, 'EmoneyCommercialPricing', 'E para ticari kullanım', 'String', '2023-09-21 11:41:23.016', NULL, '8c912eb7-e8f0-4aa8-1886-08dafdf49fd4', NULL, 'Active'
where not exists (select * from core.parameter_group where group_code = 'EmoneyCommercialPricing');

INSERT INTO core."parameter"
(id, group_code, parameter_code, parameter_value, create_date, update_date, created_by, last_modified_by, record_status)
select '2b099b9e-363d-4ef0-a1d6-7055b00a670a'::uuid, 'EmoneyCommercialPricing', 'MinAmountLimit', '5', '2023-09-06 00:00:00.000', '2023-09-06 00:00:00.000', 'BATCH', 'BATCH', 'Active'
where not exists (select * from core."parameter" where group_code = 'EmoneyCommercialPricing' and parameter_code = 'MinAmountLimit' );

END $$;