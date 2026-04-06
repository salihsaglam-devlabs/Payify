INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{A143D269-4753-4746-9FE8-722512A6B779}'::uuid, '/v1/MerchantPools', NULL, 'BaseUrl', 'MerchantPools', 'Post', 'PF', 'Üye İşyeri Başvuru', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{5DC195B8-7262-44B0-B261-B0E8A2DEBCCC}'::uuid, '/v1/MerchantPools', NULL, 'BaseUrl', 'MerchantPools', 'Put', 'PF', 'Üye İşyeri Başvuru Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{85CF44C8-39E5-45DE-B8F2-2D57C3256456}'::uuid, '/v1/MerchantIntegrators', NULL, 'BaseUrl', 'MerchantIntegrators', 'Post', 'PF', 'Üye İşyeri Entegrator Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ace8a7c2-71c3-4d91-aa68-5f4cbebf84f6'::uuid, '/v1/Merchants/approve', NULL, 'BaseUrl', 'Merchants', 'Put', 'PF', 'Üye İşyeri Onay Süreci', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('e0f7199c-86f6-437c-ad99-0544f98d5bbe'::uuid, '/v1/Vpos', NULL, 'BaseUrl', 'Vpos', 'Post', 'PF', 'Pos Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('d6cad408-99ae-4936-be98-a05948c04349'::uuid, '/v1/AcquireBanks', NULL, 'BaseUrl', 'AcquireBanks', 'Put', 'PF', 'Banka Güncelleme', '2023-05-03 01:33:03.518', '2023-10-24 21:43:49.035', '', 'c46da32f-0b9e-43ae-a724-984691d73819', 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('f080018c-1245-4a49-a9ec-b1858a728944'::uuid, '/v1/PostingBalances', 'Patch', 'ActionName', 'PostingBalances', 'Patch', 'PF', 'Posting Balance Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('3da50323-8045-4e79-b33f-19102bd90164'::uuid, '/v1/BankHealthChecks', NULL, 'BaseUrl', 'BankHealthChecks', 'Put', 'PF', 'Health Check İzni', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('e864dfdc-f0ec-4b30-af8c-b16c941e38d4'::uuid, '/v1/MerchantTransactions/manual-return', NULL, 'BaseUrl', 'MerchantTransactions', 'Post', 'PF', 'Manual İade', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ffcf37d4-a8c2-45ac-af67-d5e54ee392de'::uuid, '/v1/DataEncryptionKey', NULL, 'BaseUrl', 'DataEncryptionKey', 'Post', 'PF', 'DataEncryptionKey Yenileme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('438e1566-76df-40c8-b566-34ed4d2f3ba0'::uuid, '/v1/PostingBalances/retry-payment', NULL, 'BaseUrl', 'PostingBalances', 'Put', 'PF', 'Ödeme Yenileme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{CCC41474-49E1-439A-9395-E73FDDC1A28E}'::uuid, '/v1/BankLimits', 'Delete', 'ActionName', 'BankLimits', 'Delete', 'PF', 'Banka Limiti Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('9e5e8a4c-70bc-496c-a210-408eea31e79e'::uuid, '/v1/MerchantIntegrators', 'Delete', 'ActionName', 'MerchantIntegrators', 'Delete', 'PF', 'Üye İş Yeri Entegratörü Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('13db96fa-def3-44e9-8083-dd4930060750'::uuid, '/v1/MerchantIntegrators', NULL, 'BaseUrl', 'MerchantIntegrators', 'Post', 'PF', 'Üye İş Yeri Entegratörü Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('2f64dc07-6725-46fc-abb0-a07a5d5878c3'::uuid, '/v1/MerchantLimits', NULL, 'BaseUrl', 'MerchantLimits', 'Put', 'PF', 'Üye İş Yeri Limit Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('0426ac21-9bdc-42ea-b3c0-a6472ae39659'::uuid, '/v1/MerchantLimits', NULL, 'BaseUrl', 'MerchantLimits', 'Post', 'PF', 'Üye İş Yeri Limit Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('661b4f9d-144f-43d1-bb80-4aeee5b03237'::uuid, '/v1/MerchantUsers', NULL, 'BaseUrl', 'MerchantUsers', 'Put', 'PF', 'Üye İş Yeri Kullanıcısı Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('9b2aa085-5428-4fc1-a35c-04df63282157'::uuid, '/v1/DueProfile', NULL, 'BaseUrl', 'DueProfile', 'Post', 'PF', 'Aidat Profili Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ff0c2154-8207-42c2-a2da-3824c5e58742'::uuid, '/v1/DueProfile', NULL, 'BaseUrl', 'DueProfile', 'Put', 'PF', 'Aidat Profili Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('36ee5bc1-c5a5-4d1e-8309-e49a5369bb75'::uuid, '/v1/MerchantBusinessPartner', NULL, 'BaseUrl', 'MerchantBusinessPartner', 'Post', 'PF', 'Üye İş Yeri İş Partneri Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('04881cf7-ff80-4b72-b9fa-0473d2aa929a'::uuid, '/v1/MerchantBusinessPartner', NULL, 'BaseUrl', 'MerchantBusinessPartner', 'Put', 'PF', 'Üye İş Yeri İş Partneri Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('3725e947-fc5b-478a-b0bf-4e1b7c80996b'::uuid, '/v1/Vpos', 'Delete', 'ActionName', 'Vpos', 'Delete', 'PF', 'Pos Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('7f0ceb0f-631f-4cc9-8b93-656b24e53306'::uuid, '/v1/MerchantDue', NULL, 'BaseUrl', 'MerchantDue', 'Post', 'PF', 'Üye İş Yeri Aidat Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('cc3abac6-dab1-495c-9c02-c400edd6132f'::uuid, '/v1/Merchants/update', 'Patch', 'ActionName', 'Merchants', 'Patch', 'PF', 'Üye İşyeri Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('4eb143a7-ff3e-45b0-9855-3ee7d5c5420b'::uuid, '/v1/MerchantBlockages', NULL, 'BaseUrl', 'MerchantBlockages', 'Put', 'PF', 'Üye İş Yeri Bloke Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('d3000681-8234-4284-a86f-aed8505112cd'::uuid, '/v1/DueProfile', 'Delete', 'ActionName', 'DueProfile', 'Delete', 'PF', 'Aidat Profili Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ed24b5c9-6ed1-46e6-9187-b3f3bb1c85c3'::uuid, '/v1/MerchantBlockages/payment-date', NULL, 'BaseUrl', 'MerchantBlockages', 'Put', 'PF', 'Üye İş Yeri Ödeme Tarihi Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('f52a9572-c7c8-4343-92b6-571f7cec5bf6'::uuid, '/v1/MerchantReturnPools', NULL, 'BaseUrl', 'MerchantReturnPools', 'Post', 'PF', 'Üye İş Yeri İade Havuzu Aksiyonu', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('a9f9ad0c-02e1-409c-ac67-d3f94aacb9b1'::uuid, '/v1/MerchantDue', 'DeleteMerchantDue', 'ActionName', 'MerchantDue', 'Delete', 'PF', 'Üye İş Yeri Aidat Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('5fc21b89-79c2-4946-904d-ede0ad5f0fec'::uuid, '/v1/Vpos', NULL, 'BaseUrl', 'Vpos', 'Put', 'PF', 'Pos Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('026ebd7c-ce7c-4e6a-b63b-3f1266676b97'::uuid, '/v1/PricingProfiles', NULL, 'BaseUrl', 'PricingProfiles', 'Put', 'PF', 'Üye İş Yeri Maliyet Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('66d74708-f275-4205-b706-fb4638f77e0f'::uuid, '/v1/PricingProfiles', NULL, 'BaseUrl', 'PricingProfiles', 'Post', 'PF', 'Üye İş Yeri Maliyet Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('27d08ea4-3103-4023-b742-859a653224e4'::uuid, '/v1/CostProfiles', NULL, 'BaseUrl', 'CostProfiles', 'Post', 'PF', 'Pos Ücretlendirmesi Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('294a170d-58cf-440b-a616-49d2f61d7b94'::uuid, '/v1/CostProfiles', 'Patch', 'ActionName', 'CostProfiles', 'Patch', 'PF', 'Pos Ücretlendirmesi Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('30724ea3-8b8c-4fd2-a4f6-0935f9f90c68'::uuid, '/v1/CardBins', NULL, 'BaseUrl', 'CardBins', 'Post', 'PF', 'Kart BIN Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('58ea5a02-03b4-4dc8-a0ee-7b531f0af4d9'::uuid, '/v1/CardBins', 'Delete', 'ActionName', 'CardBins', 'Delete', 'PF', 'Kart BIN Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('34304497-e06d-4962-9a73-da297980296b'::uuid, '/v1/CardBins', 'Patch', 'ActionName', 'CardBins', 'Patch', 'PF', 'Kart BIN Güncelleme Patch', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{BC825C52-791C-4393-BE73-949996311CD0}'::uuid, '/v1/BankLimits', NULL, 'BaseUrl', 'BankLimits', 'Post', 'PF', 'Banka Limiti Ekleme', '2024-03-13 15:53:09.697', '2024-07-31 11:49:39.264', 'c46da32f-0b9e-43ae-a724-984691d73819', 'aea61854-4a35-4657-be08-22ff12eb6d78', 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('f283275f-ca97-4408-a3e9-61fd316a9ade'::uuid, '/v1/MerchantBlockages', NULL, 'BaseUrl', 'MerchantBlockages', 'Post', 'PF', 'Üye İş Yeri Bloke Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('708c53da-783a-45e4-9ca3-291e801f2398'::uuid, '/v1/PricingProfiles', 'Delete', 'ActionName', 'PricingProfiles', 'Delete', 'PF', 'Üye İş Yeri Maliyet Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('d50bad6e-e883-4d3e-b051-a93afb9ae502'::uuid, '/v1/PricingProfiles', 'Patch', 'ActionName', 'PricingProfiles', 'Patch', 'PF', 'Üye İş Yeri Maliyet Güncelleme Patch', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('81a3d627-0f9b-430c-aa71-44f3fd3db5d8'::uuid, '/v1/CardBins', NULL, 'BaseUrl', 'CardBins', 'Put', 'PF', 'Kart BIN Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('bade5d26-fdfe-4cc9-9110-26401c85397e'::uuid, '/v1/MerchantCategoryCodes', NULL, 'BaseUrl', 'MerchantCategoryCodes', 'Post', 'PF', 'MCC Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ba1f0af7-61f5-405d-a075-4da495227b1b'::uuid, '/v1/MerchantCategoryCodes', NULL, 'BaseUrl', 'MerchantCategoryCodes', 'Put', 'PF', 'MCC Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('f1686563-2471-4244-8322-0d30cab89af2'::uuid, '/v1/Vpos', 'Patch', 'ActionName', 'Vpos', 'Patch', 'PF', 'Pos Güncelleme Patch', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('01afa4ba-ee48-43e3-a094-218f0886e4c0'::uuid, '/v1/MerchantCategoryCodes', 'Delete', 'ActionName', 'MerchantCategoryCodes', 'Delete', 'PF', 'MCC Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('8a10bd67-622f-43a6-8036-b312a7b4f286'::uuid, '/v1/MerchantCategoryCodes', 'Patch', 'ActionName', 'MerchantCategoryCodes', 'Patch', 'PF', 'MCC Güncelleme Patch', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('c713c4cf-51df-4703-8d5b-b12fc824d97a'::uuid, '/v1/AcquireBanks', NULL, 'BaseUrl', 'AcquireBanks', 'Post', 'PF', 'Banka Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('ca7c9c60-95e6-44f4-a892-daa9a5349c79'::uuid, '/v1/AcquireBanks', 'Patch', 'ActionName', 'AcquireBanks', 'Patch', 'PF', 'Banka Güncelleme Patch', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('aed59bc9-be91-49d6-a7f7-b285481376ff'::uuid, '/v1/AcquireBanks', 'Delete', 'ActionName', 'AcquireBanks', 'Delete', 'PF', 'Banka Silme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('{9DA62FFB-BEEC-4A57-841D-E5E8EC5C334C}'::uuid, '/v1/BankLimits', NULL, 'BaseUrl', 'BankLimits', 'Put', 'PF', 'Banka Limiti Güncelleme', '2024-03-13 15:53:09.697', '2024-07-31 11:49:48.632', 'c46da32f-0b9e-43ae-a724-984691d73819', 'aea61854-4a35-4657-be08-22ff12eb6d78', 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('a6a5393a-062c-484f-bd3a-9dcc37b766fe'::uuid, '/v1/Merchants', NULL, 'BaseUrl', 'Merchants', 'Put', 'PF', 'Üye İş Yeri Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('5a64ddc9-d10f-4e84-94a8-9e0c4188bcf7'::uuid, '/v1/Merchants', 'ApiKeyPatch', 'ActionName', 'Merchants', 'Patch', 'PF', 'Üye İş Yeri Api Key Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('a24473ea-5cdf-4dea-8219-e7df88fa348d'::uuid, '/v1/Merchants', 'SaveAnnulment', 'ActionName', 'Merchants', 'Post', 'PF', 'Üye İş Yeri Fesih Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('1a240e8e-2307-4ccb-a0f6-edded5695339'::uuid, '/v1/Merchants', 'DeleteMerchant', 'ActionName', 'Merchants', 'Delete', 'PF', 'Üye İş Yeri Silme ', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('569d7c06-a2c8-4ea6-b920-7887121487ae'::uuid, '/v1/MerchantTransactions', 'Patch', 'ActionName', 'MerchantTransactions', 'Patch', 'PF', 'Üye İş Yeri İşlem Güncelleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;

INSERT INTO core."case"
(id, base_url, action_name, case_type, resource, "action", module_name, display_name, create_date, update_date, created_by, last_modified_by, record_status)
VALUES('f2abdaf8-7ada-4d70-bd5b-47f9c272686a'::uuid, '/v1/MerchantUsers', NULL, 'BaseUrl', 'MerchantUsers', 'Post', 'PF', 'Üye İş Yeri Kullanıcı Ekleme', '2024-03-13 15:53:09.697', NULL, 'c46da32f-0b9e-43ae-a724-984691d73819', NULL, 'Active') ON CONFLICT DO NOTHING;
