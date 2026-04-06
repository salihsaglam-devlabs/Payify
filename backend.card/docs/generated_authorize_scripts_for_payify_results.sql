
UPDATE core."role"
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_scope = 'Internal',
    "name" = 'FileIngestion',
    normalized_name = 'FILEINGESTION',
    concurrency_stamp = '543e1d0e-ed8e-6858-d2ed-badf26fa23af',
    can_see_sensitive_data = true
WHERE id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
   OR normalized_name = 'FILEINGESTION'
   OR "name" = 'FileIngestion';

INSERT INTO core."role"(
    id, create_date, update_date, last_modified_by, created_by,
    record_status, role_scope, "name", normalized_name,
    concurrency_stamp, can_see_sensitive_data
)
SELECT
    'd15b0af7-c22b-d561-aef9-fd02654261c7', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    'Active', 'Internal', 'FileIngestion', 'FILEINGESTION', '543e1d0e-ed8e-6858-d2ed-badf26fa23af', true
    WHERE NOT EXISTS (
    SELECT 1
    FROM core."role" r
    WHERE r.id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
       OR r.normalized_name = 'FILEINGESTION'
       OR r."name" = 'FileIngestion'
);



UPDATE core."role"
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_scope = 'Internal',
    "name" = 'Reconciliation',
    normalized_name = 'RECONCILIATION',
    concurrency_stamp = '500c8e61-8af0-a157-76f3-25674f2910e3',
    can_see_sensitive_data = true
WHERE id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
   OR normalized_name = 'RECONCILIATION'
   OR "name" = 'Reconciliation';

INSERT INTO core."role"(
    id, create_date, update_date, last_modified_by, created_by,
    record_status, role_scope, "name", normalized_name,
    concurrency_stamp, can_see_sensitive_data
)
SELECT
    '5ebcfd4f-7b7c-4394-6615-3455211322e6', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    'Active', 'Internal', 'Reconciliation', 'RECONCILIATION', '500c8e61-8af0-a157-76f3-25674f2910e3', true
    WHERE NOT EXISTS (
    SELECT 1
    FROM core."role" r
    WHERE r.id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
       OR r.normalized_name = 'RECONCILIATION'
       OR r."name" = 'Reconciliation'
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'FILE-INGESTION',
    module = 'CARD',
    operation_type = 'Create',
    icon = 'list-outline',
    link = '/FileIngestion',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'::uuid
   OR (
        name = 'FILE-INGESTION'
    AND module = 'CARD'
    AND operation_type = 'Create'
    AND link = '/FileIngestion'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    '158f329b-aa8b-2c58-2d03-092d9979c2f3', 'FILE-INGESTION', 'CARD', 'Create',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/FileIngestion', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'::uuid
       OR (
            s.name = 'FILE-INGESTION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Create'
        AND s.link = '/FileIngestion'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'FILE-INGESTION',
    module = 'CARD',
    operation_type = 'Delete',
    icon = 'list-outline',
    link = '/FileIngestion',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'd7f0677d-4b27-f07f-8372-120f514c670e'::uuid
   OR (
        name = 'FILE-INGESTION'
    AND module = 'CARD'
    AND operation_type = 'Delete'
    AND link = '/FileIngestion'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'd7f0677d-4b27-f07f-8372-120f514c670e', 'FILE-INGESTION', 'CARD', 'Delete',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/FileIngestion', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'd7f0677d-4b27-f07f-8372-120f514c670e'::uuid
       OR (
            s.name = 'FILE-INGESTION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Delete'
        AND s.link = '/FileIngestion'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'FILE-INGESTION',
    module = 'CARD',
    operation_type = 'Read',
    icon = 'list-outline',
    link = '/FileIngestion',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'::uuid
   OR (
        name = 'FILE-INGESTION'
    AND module = 'CARD'
    AND operation_type = 'Read'
    AND link = '/FileIngestion'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'a32d3dda-813a-ce51-3d65-aed5e6b507ce', 'FILE-INGESTION', 'CARD', 'Read',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/FileIngestion', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'::uuid
       OR (
            s.name = 'FILE-INGESTION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Read'
        AND s.link = '/FileIngestion'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'FILE-INGESTION',
    module = 'CARD',
    operation_type = 'ReadAll',
    icon = 'list-outline',
    link = '/FileIngestion',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'::uuid
   OR (
        name = 'FILE-INGESTION'
    AND module = 'CARD'
    AND operation_type = 'ReadAll'
    AND link = '/FileIngestion'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a', 'FILE-INGESTION', 'CARD', 'ReadAll',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/FileIngestion', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'::uuid
       OR (
            s.name = 'FILE-INGESTION'
        AND s.module = 'CARD'
        AND s.operation_type = 'ReadAll'
        AND s.link = '/FileIngestion'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'FILE-INGESTION',
    module = 'CARD',
    operation_type = 'Update',
    icon = 'list-outline',
    link = '/FileIngestion',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'::uuid
   OR (
        name = 'FILE-INGESTION'
    AND module = 'CARD'
    AND operation_type = 'Update'
    AND link = '/FileIngestion'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    '4250e13b-f83d-6dc1-4613-6cd870fe866b', 'FILE-INGESTION', 'CARD', 'Update',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/FileIngestion', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'::uuid
       OR (
            s.name = 'FILE-INGESTION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Update'
        AND s.link = '/FileIngestion'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'RECONCILIATION',
    module = 'CARD',
    operation_type = 'Create',
    icon = 'list-outline',
    link = '/Reconciliation',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'::uuid
   OR (
        name = 'RECONCILIATION'
    AND module = 'CARD'
    AND operation_type = 'Create'
    AND link = '/Reconciliation'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    '9ce69c55-8c1c-a639-1d79-b46bbc74a05d', 'RECONCILIATION', 'CARD', 'Create',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/Reconciliation', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'::uuid
       OR (
            s.name = 'RECONCILIATION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Create'
        AND s.link = '/Reconciliation'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'RECONCILIATION',
    module = 'CARD',
    operation_type = 'Delete',
    icon = 'list-outline',
    link = '/Reconciliation',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'::uuid
   OR (
        name = 'RECONCILIATION'
    AND module = 'CARD'
    AND operation_type = 'Delete'
    AND link = '/Reconciliation'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'c46585bf-cb8a-bb22-d2c1-03e58615ff01', 'RECONCILIATION', 'CARD', 'Delete',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/Reconciliation', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'::uuid
       OR (
            s.name = 'RECONCILIATION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Delete'
        AND s.link = '/Reconciliation'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'RECONCILIATION',
    module = 'CARD',
    operation_type = 'Read',
    icon = 'list-outline',
    link = '/Reconciliation',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'::uuid
   OR (
        name = 'RECONCILIATION'
    AND module = 'CARD'
    AND operation_type = 'Read'
    AND link = '/Reconciliation'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    '169ee328-4c78-a2d6-d1bd-90dff5bae860', 'RECONCILIATION', 'CARD', 'Read',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/Reconciliation', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'::uuid
       OR (
            s.name = 'RECONCILIATION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Read'
        AND s.link = '/Reconciliation'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'RECONCILIATION',
    module = 'CARD',
    operation_type = 'ReadAll',
    icon = 'list-outline',
    link = '/Reconciliation',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'::uuid
   OR (
        name = 'RECONCILIATION'
    AND module = 'CARD'
    AND operation_type = 'ReadAll'
    AND link = '/Reconciliation'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'e35684fc-c9ad-f76e-1199-8289cc1e25ae', 'RECONCILIATION', 'CARD', 'ReadAll',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/Reconciliation', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'::uuid
       OR (
            s.name = 'RECONCILIATION'
        AND s.module = 'CARD'
        AND s.operation_type = 'ReadAll'
        AND s.link = '/Reconciliation'
       )
);



UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    name = 'RECONCILIATION',
    module = 'CARD',
    operation_type = 'Update',
    icon = 'list-outline',
    link = '/Reconciliation',
    module_icon = 'clipboard-outline',
    module_link = null,
    module_priority = 999,
    priority = 999
WHERE id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'::uuid
   OR (
        name = 'RECONCILIATION'
    AND module = 'CARD'
    AND operation_type = 'Update'
    AND link = '/Reconciliation'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    'b0919d08-381d-9ead-bc68-2eb098b4c468', 'RECONCILIATION', 'CARD', 'Update',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active',
    'list-outline', '/Reconciliation', 'clipboard-outline', null, 999, 999
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'::uuid
       OR (
            s.name = 'RECONCILIATION'
        AND s.module = 'CARD'
        AND s.operation_type = 'Update'
        AND s.link = '/Reconciliation'
       )
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'
WHERE id = '7a044354-62d0-6b21-de99-4e19b5d2c484'::uuid
   OR claim_value = 'FileIngestion:Create';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '7a044354-62d0-6b21-de99-4e19b5d2c484', '158f329b-aa8b-2c58-2d03-092d9979c2f3', 'FileIngestion:Create',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '7a044354-62d0-6b21-de99-4e19b5d2c484'::uuid
       OR sc.claim_value = 'FileIngestion:Create'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'd7f0677d-4b27-f07f-8372-120f514c670e'
WHERE id = '9de4e822-2b3e-f84a-d534-4954d769e76e'::uuid
   OR claim_value = 'FileIngestion:Delete';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '9de4e822-2b3e-f84a-d534-4954d769e76e', 'd7f0677d-4b27-f07f-8372-120f514c670e', 'FileIngestion:Delete',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '9de4e822-2b3e-f84a-d534-4954d769e76e'::uuid
       OR sc.claim_value = 'FileIngestion:Delete'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'
WHERE id = 'ae8f98be-56fa-ef6b-9b00-9f806f55dd69'::uuid
   OR claim_value = 'FileIngestion:Read';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'ae8f98be-56fa-ef6b-9b00-9f806f55dd69', 'a32d3dda-813a-ce51-3d65-aed5e6b507ce', 'FileIngestion:Read',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = 'ae8f98be-56fa-ef6b-9b00-9f806f55dd69'::uuid
       OR sc.claim_value = 'FileIngestion:Read'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'
WHERE id = '73d1b361-36e8-c1e6-cbaf-daacc8b92a68'::uuid
   OR claim_value = 'FileIngestion:ReadAll';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '73d1b361-36e8-c1e6-cbaf-daacc8b92a68', 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a', 'FileIngestion:ReadAll',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '73d1b361-36e8-c1e6-cbaf-daacc8b92a68'::uuid
       OR sc.claim_value = 'FileIngestion:ReadAll'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'
WHERE id = 'd48ff582-505f-8b3d-21fd-02e4a379c7b4'::uuid
   OR claim_value = 'FileIngestion:Update';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'd48ff582-505f-8b3d-21fd-02e4a379c7b4', '4250e13b-f83d-6dc1-4613-6cd870fe866b', 'FileIngestion:Update',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = 'd48ff582-505f-8b3d-21fd-02e4a379c7b4'::uuid
       OR sc.claim_value = 'FileIngestion:Update'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'
WHERE id = '6e7ac17c-f724-4f5f-1de7-b09e2deb649b'::uuid
   OR claim_value = 'Reconciliation:Create';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '6e7ac17c-f724-4f5f-1de7-b09e2deb649b', '9ce69c55-8c1c-a639-1d79-b46bbc74a05d', 'Reconciliation:Create',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '6e7ac17c-f724-4f5f-1de7-b09e2deb649b'::uuid
       OR sc.claim_value = 'Reconciliation:Create'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'
WHERE id = '2e6fcce7-41b0-b22b-7f98-d0dfa56ca041'::uuid
   OR claim_value = 'Reconciliation:Delete';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '2e6fcce7-41b0-b22b-7f98-d0dfa56ca041', 'c46585bf-cb8a-bb22-d2c1-03e58615ff01', 'Reconciliation:Delete',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '2e6fcce7-41b0-b22b-7f98-d0dfa56ca041'::uuid
       OR sc.claim_value = 'Reconciliation:Delete'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'
WHERE id = '8cb8b9db-50f6-f147-e602-4be05d692a19'::uuid
   OR claim_value = 'Reconciliation:Read';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '8cb8b9db-50f6-f147-e602-4be05d692a19', '169ee328-4c78-a2d6-d1bd-90dff5bae860', 'Reconciliation:Read',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '8cb8b9db-50f6-f147-e602-4be05d692a19'::uuid
       OR sc.claim_value = 'Reconciliation:Read'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'
WHERE id = '7f728589-b54b-5e0c-bc02-ee9928f6ef52'::uuid
   OR claim_value = 'Reconciliation:ReadAll';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '7f728589-b54b-5e0c-bc02-ee9928f6ef52', 'e35684fc-c9ad-f76e-1199-8289cc1e25ae', 'Reconciliation:ReadAll',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '7f728589-b54b-5e0c-bc02-ee9928f6ef52'::uuid
       OR sc.claim_value = 'Reconciliation:ReadAll'
);



UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    screen_id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'
WHERE id = '5ba36458-6189-8901-cd1b-5a516a4e25ed'::uuid
   OR claim_value = 'Reconciliation:Update';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '5ba36458-6189-8901-cd1b-5a516a4e25ed', 'b0919d08-381d-9ead-bc68-2eb098b4c468', 'Reconciliation:Update',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '5ba36458-6189-8901-cd1b-5a516a4e25ed'::uuid
       OR sc.claim_value = 'Reconciliation:Update'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'FileIngestion:Create',
    module = 'Card',
    operation_type = 'Create',
    normalized_claim_value = 'FILEINGESTION:CREATE',
    description = 'FileIngestion - Create',
    display_name = 'FileIngestion',
    display = true
WHERE id = 'a037ff79-6ab1-633d-1040-a061482d5392'::uuid
   OR claim_value = 'FileIngestion:Create';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'a037ff79-6ab1-633d-1040-a061482d5392', 'BackOffice', 'FileIngestion:Create', 'Card', 'Create',
    'FILEINGESTION:CREATE', 'FileIngestion - Create', 'FileIngestion',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'a037ff79-6ab1-633d-1040-a061482d5392'::uuid
       OR p.claim_value = 'FileIngestion:Create'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'FileIngestion:Delete',
    module = 'Card',
    operation_type = 'Delete',
    normalized_claim_value = 'FILEINGESTION:DELETE',
    description = 'FileIngestion - Delete',
    display_name = 'FileIngestion',
    display = true
WHERE id = 'e1c18f98-e35f-f8c9-53ae-62ed15d702bb'::uuid
   OR claim_value = 'FileIngestion:Delete';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'e1c18f98-e35f-f8c9-53ae-62ed15d702bb', 'BackOffice', 'FileIngestion:Delete', 'Card', 'Delete',
    'FILEINGESTION:DELETE', 'FileIngestion - Delete', 'FileIngestion',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'e1c18f98-e35f-f8c9-53ae-62ed15d702bb'::uuid
       OR p.claim_value = 'FileIngestion:Delete'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'FileIngestion:Read',
    module = 'Card',
    operation_type = 'Read',
    normalized_claim_value = 'FILEINGESTION:READ',
    description = 'FileIngestion - Read',
    display_name = 'FileIngestion',
    display = true
WHERE id = 'b7df3af6-b10f-3b66-2197-22802e38d9ec'::uuid
   OR claim_value = 'FileIngestion:Read';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'b7df3af6-b10f-3b66-2197-22802e38d9ec', 'BackOffice', 'FileIngestion:Read', 'Card', 'Read',
    'FILEINGESTION:READ', 'FileIngestion - Read', 'FileIngestion',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'b7df3af6-b10f-3b66-2197-22802e38d9ec'::uuid
       OR p.claim_value = 'FileIngestion:Read'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'FileIngestion:ReadAll',
    module = 'Card',
    operation_type = 'ReadAll',
    normalized_claim_value = 'FILEINGESTION:READALL',
    description = 'FileIngestion - ReadAll',
    display_name = 'FileIngestion',
    display = true
WHERE id = 'defa2161-2584-8671-d83d-680e98f6b323'::uuid
   OR claim_value = 'FileIngestion:ReadAll';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'defa2161-2584-8671-d83d-680e98f6b323', 'BackOffice', 'FileIngestion:ReadAll', 'Card', 'ReadAll',
    'FILEINGESTION:READALL', 'FileIngestion - ReadAll', 'FileIngestion',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'defa2161-2584-8671-d83d-680e98f6b323'::uuid
       OR p.claim_value = 'FileIngestion:ReadAll'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'FileIngestion:Update',
    module = 'Card',
    operation_type = 'Update',
    normalized_claim_value = 'FILEINGESTION:UPDATE',
    description = 'FileIngestion - Update',
    display_name = 'FileIngestion',
    display = true
WHERE id = 'db080ca3-5bca-baab-a951-cec3ba20e3c6'::uuid
   OR claim_value = 'FileIngestion:Update';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'db080ca3-5bca-baab-a951-cec3ba20e3c6', 'BackOffice', 'FileIngestion:Update', 'Card', 'Update',
    'FILEINGESTION:UPDATE', 'FileIngestion - Update', 'FileIngestion',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'db080ca3-5bca-baab-a951-cec3ba20e3c6'::uuid
       OR p.claim_value = 'FileIngestion:Update'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'Reconciliation:Create',
    module = 'Card',
    operation_type = 'Create',
    normalized_claim_value = 'RECONCILIATION:CREATE',
    description = 'Reconciliation - Create',
    display_name = 'Reconciliation',
    display = true
WHERE id = '8a34ba43-4c01-8326-6c4e-92e09b2d73c0'::uuid
   OR claim_value = 'Reconciliation:Create';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '8a34ba43-4c01-8326-6c4e-92e09b2d73c0', 'BackOffice', 'Reconciliation:Create', 'Card', 'Create',
    'RECONCILIATION:CREATE', 'Reconciliation - Create', 'Reconciliation',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = '8a34ba43-4c01-8326-6c4e-92e09b2d73c0'::uuid
       OR p.claim_value = 'Reconciliation:Create'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'Reconciliation:Delete',
    module = 'Card',
    operation_type = 'Delete',
    normalized_claim_value = 'RECONCILIATION:DELETE',
    description = 'Reconciliation - Delete',
    display_name = 'Reconciliation',
    display = true
WHERE id = 'fe8a400e-bacd-6b3d-807d-24316e062af4'::uuid
   OR claim_value = 'Reconciliation:Delete';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'fe8a400e-bacd-6b3d-807d-24316e062af4', 'BackOffice', 'Reconciliation:Delete', 'Card', 'Delete',
    'RECONCILIATION:DELETE', 'Reconciliation - Delete', 'Reconciliation',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'fe8a400e-bacd-6b3d-807d-24316e062af4'::uuid
       OR p.claim_value = 'Reconciliation:Delete'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'Reconciliation:Read',
    module = 'Card',
    operation_type = 'Read',
    normalized_claim_value = 'RECONCILIATION:READ',
    description = 'Reconciliation - Read',
    display_name = 'Reconciliation',
    display = true
WHERE id = '71a0b236-662f-ef2b-5fb5-f21d83e0c139'::uuid
   OR claim_value = 'Reconciliation:Read';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '71a0b236-662f-ef2b-5fb5-f21d83e0c139', 'BackOffice', 'Reconciliation:Read', 'Card', 'Read',
    'RECONCILIATION:READ', 'Reconciliation - Read', 'Reconciliation',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = '71a0b236-662f-ef2b-5fb5-f21d83e0c139'::uuid
       OR p.claim_value = 'Reconciliation:Read'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'Reconciliation:ReadAll',
    module = 'Card',
    operation_type = 'ReadAll',
    normalized_claim_value = 'RECONCILIATION:READALL',
    description = 'Reconciliation - ReadAll',
    display_name = 'Reconciliation',
    display = true
WHERE id = '1375dd11-033c-b7a8-cfa1-0a88d6895ff7'::uuid
   OR claim_value = 'Reconciliation:ReadAll';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '1375dd11-033c-b7a8-cfa1-0a88d6895ff7', 'BackOffice', 'Reconciliation:ReadAll', 'Card', 'ReadAll',
    'RECONCILIATION:READALL', 'Reconciliation - ReadAll', 'Reconciliation',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = '1375dd11-033c-b7a8-cfa1-0a88d6895ff7'::uuid
       OR p.claim_value = 'Reconciliation:ReadAll'
);



UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    claim_type = 'BackOffice',
    claim_value = 'Reconciliation:Update',
    module = 'Card',
    operation_type = 'Update',
    normalized_claim_value = 'RECONCILIATION:UPDATE',
    description = 'Reconciliation - Update',
    display_name = 'Reconciliation',
    display = true
WHERE id = 'b0f49ccf-6a7c-287f-0b97-b8e461b4b1ae'::uuid
   OR claim_value = 'Reconciliation:Update';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'b0f49ccf-6a7c-287f-0b97-b8e461b4b1ae', 'BackOffice', 'Reconciliation:Update', 'Card', 'Update',
    'RECONCILIATION:UPDATE', 'Reconciliation - Update', 'Reconciliation',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = 'b0f49ccf-6a7c-287f-0b97-b8e461b4b1ae'::uuid
       OR p.claim_value = 'Reconciliation:Update'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    claim_type = 'RoleScope',
    claim_value = 'FileIngestion:Create'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'FileIngestion:Create';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'RoleScope', 'FileIngestion:Create'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'FileIngestion:Create'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    claim_type = 'RoleScope',
    claim_value = 'FileIngestion:Delete'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'FileIngestion:Delete';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'RoleScope', 'FileIngestion:Delete'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'FileIngestion:Delete'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    claim_type = 'RoleScope',
    claim_value = 'FileIngestion:Read'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'FileIngestion:Read';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'RoleScope', 'FileIngestion:Read'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'FileIngestion:Read'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    claim_type = 'RoleScope',
    claim_value = 'FileIngestion:ReadAll'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'FileIngestion:ReadAll';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'RoleScope', 'FileIngestion:ReadAll'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'FileIngestion:ReadAll'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    claim_type = 'RoleScope',
    claim_value = 'FileIngestion:Update'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'FileIngestion:Update';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'RoleScope', 'FileIngestion:Update'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'FileIngestion:Update'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    claim_type = 'RoleScope',
    claim_value = 'Reconciliation:Create'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'Reconciliation:Create';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'RoleScope', 'Reconciliation:Create'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'Reconciliation:Create'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    claim_type = 'RoleScope',
    claim_value = 'Reconciliation:Delete'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'Reconciliation:Delete';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'RoleScope', 'Reconciliation:Delete'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'Reconciliation:Delete'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    claim_type = 'RoleScope',
    claim_value = 'Reconciliation:Read'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'Reconciliation:Read';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'RoleScope', 'Reconciliation:Read'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'Reconciliation:Read'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    claim_type = 'RoleScope',
    claim_value = 'Reconciliation:ReadAll'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'Reconciliation:ReadAll';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'RoleScope', 'Reconciliation:ReadAll'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'Reconciliation:ReadAll'
);



UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    claim_type = 'RoleScope',
    claim_value = 'Reconciliation:Update'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = 'Reconciliation:Update';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
            CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
            'Active', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'RoleScope', 'Reconciliation:Update'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = 'Reconciliation:Update'
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    screen_id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND screen_id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'b7a0f8ce-f65c-53d2-a8c0-765b76154127', 'd15b0af7-c22b-d561-aef9-fd02654261c7', '158f329b-aa8b-2c58-2d03-092d9979c2f3',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rs.screen_id = '158f329b-aa8b-2c58-2d03-092d9979c2f3'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    screen_id = 'd7f0677d-4b27-f07f-8372-120f514c670e'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND screen_id = 'd7f0677d-4b27-f07f-8372-120f514c670e'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'c3b87ab6-c3cb-5b61-e0b4-1161cfff1c54', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'd7f0677d-4b27-f07f-8372-120f514c670e',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rs.screen_id = 'd7f0677d-4b27-f07f-8372-120f514c670e'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    screen_id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND screen_id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'b6269453-6c6b-0aa4-d0b9-8159aa228fec', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'a32d3dda-813a-ce51-3d65-aed5e6b507ce',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rs.screen_id = 'a32d3dda-813a-ce51-3d65-aed5e6b507ce'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    screen_id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND screen_id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'ff46ba87-60fe-49ae-73ce-37fb7deeeeac', 'd15b0af7-c22b-d561-aef9-fd02654261c7', 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rs.screen_id = 'cdea8a3d-f2f4-e854-64d6-d4f8a467ee5a'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7',
    screen_id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'
WHERE role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
  AND screen_id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'b0387ecf-a250-50ad-1076-fe503958dc3a', 'd15b0af7-c22b-d561-aef9-fd02654261c7', '4250e13b-f83d-6dc1-4613-6cd870fe866b',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
      AND rs.screen_id = '4250e13b-f83d-6dc1-4613-6cd870fe866b'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    screen_id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND screen_id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '4f1bc939-535a-2691-c487-14a63f5b8453', '5ebcfd4f-7b7c-4394-6615-3455211322e6', '9ce69c55-8c1c-a639-1d79-b46bbc74a05d',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rs.screen_id = '9ce69c55-8c1c-a639-1d79-b46bbc74a05d'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    screen_id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND screen_id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '50aebd92-7efb-8df3-93ef-47387a6e1776', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'c46585bf-cb8a-bb22-d2c1-03e58615ff01',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rs.screen_id = 'c46585bf-cb8a-bb22-d2c1-03e58615ff01'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    screen_id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND screen_id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '8efe5d00-b48c-0864-61bc-63af4f822048', '5ebcfd4f-7b7c-4394-6615-3455211322e6', '169ee328-4c78-a2d6-d1bd-90dff5bae860',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rs.screen_id = '169ee328-4c78-a2d6-d1bd-90dff5bae860'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    screen_id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND screen_id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '1ed3e84a-3459-9fb2-1bb0-3f157ac247f5', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'e35684fc-c9ad-f76e-1199-8289cc1e25ae',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rs.screen_id = 'e35684fc-c9ad-f76e-1199-8289cc1e25ae'::uuid
);



UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '019c5665-c7d5-7ea8-a5fc-d386d416e92a',
    record_status = 'Active',
    role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6',
    screen_id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'
WHERE role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
  AND screen_id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    'c0cc8808-45c8-788f-d649-30d93b875d9c', '5ebcfd4f-7b7c-4394-6615-3455211322e6', 'b0919d08-381d-9ead-bc68-2eb098b4c468',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '019c5665-c7d5-7ea8-a5fc-d386d416e92a', null, 'Active'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
      AND rs.screen_id = 'b0919d08-381d-9ead-bc68-2eb098b4c468'::uuid
);



UPDATE core.user_role
SET role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'
WHERE user_id = '019c5665-c7d5-7ea8-a5fc-d386d416e92a'::uuid
  AND role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid;

INSERT INTO core.user_role(user_id, role_id)
SELECT '019c5665-c7d5-7ea8-a5fc-d386d416e92a', 'd15b0af7-c22b-d561-aef9-fd02654261c7'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_role ur
    WHERE ur.user_id = '019c5665-c7d5-7ea8-a5fc-d386d416e92a'::uuid
      AND ur.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
);



UPDATE core.user_role
SET role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'
WHERE user_id = '019c5665-c7d5-7ea8-a5fc-d386d416e92a'::uuid
  AND role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid;

INSERT INTO core.user_role(user_id, role_id)
SELECT '019c5665-c7d5-7ea8-a5fc-d386d416e92a', '5ebcfd4f-7b7c-4394-6615-3455211322e6'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_role ur
    WHERE ur.user_id = '019c5665-c7d5-7ea8-a5fc-d386d416e92a'::uuid
      AND ur.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
);



UPDATE core.user_role
SET role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'
WHERE user_id = '019c56f7-845e-76a2-adea-e4bba13c2b55'::uuid
  AND role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid;

INSERT INTO core.user_role(user_id, role_id)
SELECT '019c56f7-845e-76a2-adea-e4bba13c2b55', 'd15b0af7-c22b-d561-aef9-fd02654261c7'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_role ur
    WHERE ur.user_id = '019c56f7-845e-76a2-adea-e4bba13c2b55'::uuid
      AND ur.role_id = 'd15b0af7-c22b-d561-aef9-fd02654261c7'::uuid
);



UPDATE core.user_role
SET role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'
WHERE user_id = '019c56f7-845e-76a2-adea-e4bba13c2b55'::uuid
  AND role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid;

INSERT INTO core.user_role(user_id, role_id)
SELECT '019c56f7-845e-76a2-adea-e4bba13c2b55', '5ebcfd4f-7b7c-4394-6615-3455211322e6'
    WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_role ur
    WHERE ur.user_id = '019c56f7-845e-76a2-adea-e4bba13c2b55'::uuid
      AND ur.role_id = '5ebcfd4f-7b7c-4394-6615-3455211322e6'::uuid
);