/*
================================================================================
POSTGRESQL YETKI SCRIPT GENERATOR
MULTI ROLE + CATI ROLE + ROLE_SCREEN + USER_ROLE
================================================================================

Fiziksel tablo ilişkisi:
================================================================================

                            +----------------------+
                            |      core.role       |
                            |----------------------|
                            | id (uuid)            |
                            | name                 |
                            | normalized_name      |
                            | role_scope           |
                            | ...                  |
                            +----------+-----------+
                                       |
                    +------------------+------------------+
                    |                                     |
                    |                                     |
                    v                                     v
         +----------------------+             +----------------------+
         |   core.role_claim    |             |    core.user_role    |
         |----------------------|             |----------------------|
         | id (numeric/identity)|             | user_id (uuid)       |
         | role_id (uuid)       |------------>| role_id (uuid)       |
         | claim_type           |             +----------+-----------+
         | claim_value          |                        |
         | ...                  |                        |
         +----------------------+                        |
                                                        v
                                              +----------------------+
                                              |      core.user       |
                                              |----------------------|
                                              | id (uuid)            |
                                              | ...                  |
                                              +----------------------+
                                              
Yetki nesnelerinin ilişkisi:
================================================================================

+----------------------+        +------------------------+
|     core.screen      |        |   core.screen_claim    |
|----------------------|        |------------------------|
| id (uuid)            |<-------| screen_id (uuid)       |
| name                 |        | claim_value            |
| module               |        | id (uuid)              |
| operation_type       |        | ...                    |
| link                 |        +------------------------+
| ...                  |
+----------+-----------+
           |
           |
           | kavramsal eşleşme
           |
           v
+----------------------+
|   core.permission    |
|----------------------|
| id (uuid)            |
| claim_value          |
| claim_type           |
| module               |
| operation_type       |
| normalized_claim...  |
| display_name         |
| ...                  |
+----------------------+

Tam yetki akışı: 
================================================================================

                POLICY / CLAIM
     örn: Reconciliation.Review.Approve
                          |
          +---------------+---------------+
          |                               |
          v                               v
+----------------------+      +----------------------+
|   core.screen_claim  |      |   core.permission    |
|----------------------|      |----------------------|
| claim_value          |      | claim_value          |
| screen_id ---------- +----->| module               |
+----------------------+      | operation_type       |
                              +----------------------+

                    screen_id
                       |
                       v
               +------------------+
               |   core.screen    |
               |------------------|
               | name             |
               | module           |
               | operation_type   |
               | link             |
               +------------------+

claim role'e verilir:
Reconciliation.Review.Approve
            |
            v
   +----------------------+
   |   core.role_claim    |
   |----------------------|
   | role_id              |
   | claim_value          |
   +----------+-----------+
              |
              v
   +----------------------+
   |      core.role       |
   +----------+-----------+
              |
              v
   +----------------------+
   |    core.user_role    |
   |----------------------|
   | user_id              |
   | role_id              |
   +----------+-----------+
              |
              v
   +----------------------+
   |      core.user       |
   +----------------------+

Çoklu rol + çatı rol mantığı: 
================================================================================

ALT ROLLER
==========

+------------------------+
|   Role: FileIngestion  |
+------------------------+
| FileIngestion.Ingest   |
| FileIngestion....      |
+------------------------+

+----------------------------+
|  Role: Reconciliation      |
+----------------------------+
| Reconciliation.Evaluate    |
| Reconciliation.Execute     |
| Reconciliation.Review....  |
+----------------------------+


CATI ROL
========

+----------------------------------------------+
| Role: FileIngestionAndReconciliation         |
+----------------------------------------------+
| FileIngestion.Ingest                         |
| FileIngestion.IngestByRoute                  |
| Reconciliation.Evaluate                      |
| Reconciliation.Execute                       |
| Reconciliation.Review.Approve                |
| Reconciliation.Review.Reject                 |
| Reconciliation.Review.GetPending             |
| Reconciliation.Alert.Get                     |
+----------------------------------------------+

KULLANICI ATAMASI
=================

+-------------------+        +--------------------------------------+
|     core.user     |------->| core.user_role                       |
+-------------------+        |--------------------------------------|
                             | user_id                              |
                             | role_id = FileIngestionAndRecon...   |
                             +--------------------------------------+
                             
En sade özet:
================================================================================
POLICY
  |
  +--> SCREEN
  +--> SCREEN_CLAIM
  +--> PERMISSION
  +--> ROLE_CLAIM
                |
                v
               ROLE
                |
                v
             USER_ROLE
                |
                v
               USER
               
================================================================================================================================================================  

================================================================================
POSTGRESQL YETKI SCRIPT GENERATOR
CONTROLLER BAZLI ROLE + ROLE_SCREEN + USER_ROLE
================================================================================

Mantik:
- Her controller icin tek role
- Her controller icin sadece 5 operasyon:
  Read, ReadAll, Create, Update, Delete
- Policy formati:
  <Controller>.<Operation>
  Ornek:
  FileIngestion.Read
  Reconciliation.Update

Uretilen hedef scriptte su tablolar icin UPSERT mantigi vardir:
- core."role"
- core.screen
- core.screen_claim
- core.permission
- core.role_claim
- core.role_screen
- core.user_role
================================================================================
*/

WITH global_config AS (
    SELECT
        '019c5665-c7d5-7ea8-a5fc-d386d416e92a'::text AS created_by_key,
        'BackOffice'::text AS claim_type,
        'Active'::text AS record_status,
        'CARD'::text AS screen_module,
        'Card'::text AS permission_module,
        'list-outline'::text AS icon,
        'clipboard-outline'::text AS module_icon,
        NULL::text AS module_link,
        999::int AS module_priority,
        999::int AS priority
),
roles AS (
    SELECT *
    FROM (
        VALUES
            ('FileIngestion', 'Internal', 'FileIngestion', true),
            ('Reconciliation', 'Internal', 'Reconciliation', true)
    ) AS t(role_key, role_scope, role_name, can_see_sensitive_data)
),
users AS (
    SELECT *
    FROM (
        VALUES
            ('019c56f7-845e-76a2-adea-e4bba13c2b55'::text, 'Salih Sağlam', 'salihsaqlam@icloud.com'),
            ('019c5665-c7d5-7ea8-a5fc-d386d416e92a'::text, 'Salih Saglam', 'salih.saglam@linktera.com')
    ) AS t(user_id, full_name, email)
),
user_role_assignments AS (
    SELECT *
    FROM (
        VALUES
            ('019c56f7-845e-76a2-adea-e4bba13c2b55'::text, 'FileIngestion'::text),
            ('019c56f7-845e-76a2-adea-e4bba13c2b55'::text, 'Reconciliation'::text),
            ('019c5665-c7d5-7ea8-a5fc-d386d416e92a'::text, 'FileIngestion'::text),
            ('019c5665-c7d5-7ea8-a5fc-d386d416e92a'::text, 'Reconciliation'::text)
    ) AS t(user_id, role_key)
),
policies AS (
    SELECT *
    FROM (
        VALUES
            -- policy_seed, operation_types, screen_name, display_name, link, role_key

            ('FileIngestion', 'Read,ReadAll,Create,Update,Delete', NULL, 'FileIngestion', '/FileIngestion', 'FileIngestion'),
            ('Reconciliation', 'Read,ReadAll,Create,Update,Delete', NULL, 'Reconciliation', '/Reconciliation', 'Reconciliation')
    ) AS t(policy_seed, operation_types, screen_name, display_name, link, role_key)
),
allowed_operation_types AS (
    SELECT *
    FROM (
        VALUES
            ('Read'::text),
            ('ReadAll'::text),
            ('Create'::text),
            ('Update'::text),
            ('Delete'::text)
    ) AS t(operation_type)
),
policy_operation_map AS (
    SELECT
        p.policy_seed,
        trim(x.operation_type) AS operation_type,
        p.screen_name,
        p.display_name,
        p.link,
        p.role_key
    FROM policies p
    CROSS JOIN LATERAL unnest(string_to_array(p.operation_types, ',')) AS x(operation_type)
),
validated_policy_operations AS (
    SELECT pom.*
    FROM policy_operation_map pom
    JOIN allowed_operation_types a
      ON a.operation_type = pom.operation_type
),
normalized_global_config AS (
    SELECT
        CASE
            WHEN created_by_key ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                THEN lower(created_by_key)
            ELSE
                lower(
                    substr(md5(trim(created_by_key)), 1, 8) || '-' ||
                    substr(md5(trim(created_by_key)), 9, 4) || '-' ||
                    substr(md5(trim(created_by_key)), 13, 4) || '-' ||
                    substr(md5(trim(created_by_key)), 17, 4) || '-' ||
                    substr(md5(trim(created_by_key)), 21, 12)
                )
        END AS created_by,
        claim_type,
        record_status,
        screen_module,
        permission_module,
        icon,
        module_icon,
        module_link,
        module_priority,
        priority
    FROM global_config
),
normalized_roles AS (
    SELECT
        r.role_key,
        CASE
            WHEN r.role_key ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                THEN lower(r.role_key)
            ELSE
                lower(
                    substr(md5(trim(r.role_key)), 1, 8) || '-' ||
                    substr(md5(trim(r.role_key)), 9, 4) || '-' ||
                    substr(md5(trim(r.role_key)), 13, 4) || '-' ||
                    substr(md5(trim(r.role_key)), 17, 4) || '-' ||
                    substr(md5(trim(r.role_key)), 21, 12)
                )
        END AS role_id,
        r.role_scope,
        r.role_name,
        upper(r.role_name) AS normalized_role_name,
        lower(
            substr(md5('role_concurrency|' || trim(r.role_name)), 1, 8) || '-' ||
            substr(md5('role_concurrency|' || trim(r.role_name)), 9, 4) || '-' ||
            substr(md5('role_concurrency|' || trim(r.role_name)), 13, 4) || '-' ||
            substr(md5('role_concurrency|' || trim(r.role_name)), 17, 4) || '-' ||
            substr(md5('role_concurrency|' || trim(r.role_name)), 21, 12)
        ) AS concurrency_stamp,
        r.can_see_sensitive_data
    FROM roles r
),
policy_claim_definitions AS (
    SELECT DISTINCT
        vpo.policy_seed,
        vpo.operation_type,
        (vpo.policy_seed || '.' || vpo.operation_type) AS claim_value,
        COALESCE(
            vpo.screen_name,
            upper(
                regexp_replace(
                    split_part(vpo.policy_seed, '.', 1),
                    '([a-z0-9])([A-Z])',
                    '\1-\2',
                    'g'
                )
            )
        ) AS screen_name,
        COALESCE(vpo.display_name, split_part(vpo.policy_seed, '.', 1)) AS display_name,
        COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)) AS link,
        upper(vpo.policy_seed || '.' || vpo.operation_type) AS normalized_claim_value,

        lower(
            substr(
                md5(
                    'screen|' ||
                    trim(COALESCE(
                        vpo.screen_name,
                        upper(
                            regexp_replace(
                                split_part(vpo.policy_seed, '.', 1),
                                '([a-z0-9])([A-Z])',
                                '\1-\2',
                                'g'
                            )
                        )
                    )) || '|' ||
                    'CARD' || '|' ||
                    trim(vpo.operation_type) || '|' ||
                    trim(COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)))
                ),
                1, 8
            ) || '-' ||
            substr(
                md5(
                    'screen|' ||
                    trim(COALESCE(
                        vpo.screen_name,
                        upper(
                            regexp_replace(
                                split_part(vpo.policy_seed, '.', 1),
                                '([a-z0-9])([A-Z])',
                                '\1-\2',
                                'g'
                            )
                        )
                    )) || '|' ||
                    'CARD' || '|' ||
                    trim(vpo.operation_type) || '|' ||
                    trim(COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)))
                ),
                9, 4
            ) || '-' ||
            substr(
                md5(
                    'screen|' ||
                    trim(COALESCE(
                        vpo.screen_name,
                        upper(
                            regexp_replace(
                                split_part(vpo.policy_seed, '.', 1),
                                '([a-z0-9])([A-Z])',
                                '\1-\2',
                                'g'
                            )
                        )
                    )) || '|' ||
                    'CARD' || '|' ||
                    trim(vpo.operation_type) || '|' ||
                    trim(COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)))
                ),
                13, 4
            ) || '-' ||
            substr(
                md5(
                    'screen|' ||
                    trim(COALESCE(
                        vpo.screen_name,
                        upper(
                            regexp_replace(
                                split_part(vpo.policy_seed, '.', 1),
                                '([a-z0-9])([A-Z])',
                                '\1-\2',
                                'g'
                            )
                        )
                    )) || '|' ||
                    'CARD' || '|' ||
                    trim(vpo.operation_type) || '|' ||
                    trim(COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)))
                ),
                17, 4
            ) || '-' ||
            substr(
                md5(
                    'screen|' ||
                    trim(COALESCE(
                        vpo.screen_name,
                        upper(
                            regexp_replace(
                                split_part(vpo.policy_seed, '.', 1),
                                '([a-z0-9])([A-Z])',
                                '\1-\2',
                                'g'
                            )
                        )
                    )) || '|' ||
                    'CARD' || '|' ||
                    trim(vpo.operation_type) || '|' ||
                    trim(COALESCE(vpo.link, '/' || split_part(vpo.policy_seed, '.', 1)))
                ),
                21, 12
            )
        ) AS screen_id,

        lower(
            substr(md5('screen_claim|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 1, 8) || '-' ||
            substr(md5('screen_claim|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 9, 4) || '-' ||
            substr(md5('screen_claim|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 13, 4) || '-' ||
            substr(md5('screen_claim|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 17, 4) || '-' ||
            substr(md5('screen_claim|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 21, 12)
        ) AS screen_claim_id,

        lower(
            substr(md5('permission|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 1, 8) || '-' ||
            substr(md5('permission|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 9, 4) || '-' ||
            substr(md5('permission|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 13, 4) || '-' ||
            substr(md5('permission|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 17, 4) || '-' ||
            substr(md5('permission|' || trim(vpo.policy_seed) || '|' || trim(vpo.operation_type)), 21, 12)
        ) AS permission_id
    FROM validated_policy_operations vpo
),
policy_role_assignments AS (
    SELECT DISTINCT
        vpo.policy_seed,
        vpo.operation_type,
        (vpo.policy_seed || '.' || vpo.operation_type) AS claim_value,
        vpo.role_key
    FROM validated_policy_operations vpo
),
role_blocks AS (
    SELECT
        '000_ROLE_' || nr.role_key AS order_key,
        'ROLE' AS block_type,
        format($SQL$
UPDATE core."role"
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%2$s',
    record_status = '%3$s',
    role_scope = '%4$s',
    "name" = '%5$s',
    normalized_name = '%6$s',
    concurrency_stamp = '%7$s',
    can_see_sensitive_data = %8$s
WHERE id = '%9$s'::uuid
   OR normalized_name = '%6$s'
   OR "name" = '%5$s';

INSERT INTO core."role"(
    id, create_date, update_date, last_modified_by, created_by,
    record_status, role_scope, "name", normalized_name,
    concurrency_stamp, can_see_sensitive_data
)
SELECT
    '%9$s', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '%2$s',
    '%3$s', '%4$s', '%5$s', '%6$s', '%7$s', %8$s
WHERE NOT EXISTS (
    SELECT 1
    FROM core."role" r
    WHERE r.id = '%9$s'::uuid
       OR r.normalized_name = '%6$s'
       OR r."name" = '%5$s'
);
$SQL$,
            nr.role_key,
            gc.created_by,
            gc.record_status,
            nr.role_scope,
            nr.role_name,
            nr.normalized_role_name,
            nr.concurrency_stamp,
            CASE WHEN nr.can_see_sensitive_data THEN 'true' ELSE 'false' END,
            nr.role_id
        ) AS script_block
    FROM normalized_roles nr
    CROSS JOIN normalized_global_config gc
),
screen_blocks AS (
    SELECT
        '100_SCREEN_' || pcd.claim_value AS order_key,
        'SCREEN' AS block_type,
        format($SQL$
UPDATE core.screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%6$s',
    record_status = '%7$s',
    name = '%3$s',
    module = '%4$s',
    operation_type = '%2$s',
    icon = '%8$s',
    link = '%9$s',
    module_icon = '%10$s',
    module_link = %11$s,
    module_priority = %12$s,
    priority = %13$s
WHERE id = '%1$s'::uuid
   OR (
        name = '%3$s'
    AND module = '%4$s'
    AND operation_type = '%2$s'
    AND link = '%9$s'
   );

INSERT INTO core.screen(
    id, name, module, operation_type, create_date, update_date,
    created_by, last_modified_by, record_status,
    icon, link, module_icon, module_link, module_priority, priority
)
SELECT
    '%1$s', '%3$s', '%4$s', '%2$s',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '%6$s', null, '%7$s',
    '%8$s', '%9$s', '%10$s', %11$s, %12$s, %13$s
WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen s
    WHERE s.id = '%1$s'::uuid
       OR (
            s.name = '%3$s'
        AND s.module = '%4$s'
        AND s.operation_type = '%2$s'
        AND s.link = '%9$s'
       )
);
$SQL$,
            pcd.screen_id,
            pcd.operation_type,
            pcd.screen_name,
            gc.screen_module,
            pcd.link,
            gc.created_by,
            gc.record_status,
            gc.icon,
            pcd.link,
            gc.module_icon,
            CASE WHEN gc.module_link IS NULL THEN 'null' ELSE quote_literal(gc.module_link) END,
            gc.module_priority,
            gc.priority
        ) AS script_block
    FROM policy_claim_definitions pcd
    CROSS JOIN normalized_global_config gc
),
screen_claim_blocks AS (
    SELECT
        '110_SCREENCLAIM_' || pcd.claim_value AS order_key,
        'SCREEN_CLAIM' AS block_type,
        format($SQL$
UPDATE core.screen_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%4$s',
    record_status = '%5$s',
    screen_id = '%2$s'
WHERE id = '%3$s'::uuid
   OR claim_value = '%1$s';

INSERT INTO core.screen_claim(
    id, screen_id, claim_value, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '%3$s', '%2$s', '%1$s',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '%4$s', null, '%5$s'
WHERE NOT EXISTS (
    SELECT 1
    FROM core.screen_claim sc
    WHERE sc.id = '%3$s'::uuid
       OR sc.claim_value = '%1$s'
);
$SQL$,
            pcd.claim_value,
            pcd.screen_id,
            pcd.screen_claim_id,
            gc.created_by,
            gc.record_status
        ) AS script_block
    FROM policy_claim_definitions pcd
    CROSS JOIN normalized_global_config gc
),
permission_blocks AS (
    SELECT
        '120_PERMISSION_' || pcd.claim_value AS order_key,
        'PERMISSION' AS block_type,
        format($SQL$
UPDATE core.permission
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%4$s',
    record_status = '%5$s',
    claim_type = '%6$s',
    claim_value = '%1$s',
    module = '%7$s',
    operation_type = '%2$s',
    normalized_claim_value = '%8$s',
    description = '%9$s',
    display_name = '%10$s',
    display = true
WHERE id = '%3$s'::uuid
   OR claim_value = '%1$s';

INSERT INTO core.permission(
    id, claim_type, claim_value, module, operation_type,
    normalized_claim_value, description, display_name,
    display, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '%3$s', '%6$s', '%1$s', '%7$s', '%2$s',
    '%8$s', '%9$s', '%10$s',
    true, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '%4$s', null, '%5$s'
WHERE NOT EXISTS (
    SELECT 1
    FROM core.permission p
    WHERE p.id = '%3$s'::uuid
       OR p.claim_value = '%1$s'
);
$SQL$,
            pcd.claim_value,
            pcd.operation_type,
            pcd.permission_id,
            gc.created_by,
            gc.record_status,
            gc.claim_type,
            gc.permission_module,
            pcd.normalized_claim_value,
            pcd.display_name || ' - ' || pcd.operation_type,
            pcd.display_name
        ) AS script_block
    FROM policy_claim_definitions pcd
    CROSS JOIN normalized_global_config gc
),
role_claim_blocks AS (
    SELECT
        '150_ROLECLAIM_' || pra.role_key || '_' || pra.claim_value AS order_key,
        'ROLE_CLAIM' AS block_type,
        format($SQL$
UPDATE core.role_claim
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%3$s',
    record_status = '%4$s',
    role_id = '%5$s',
    claim_type = 'RoleScope',
    claim_value = '%2$s'
WHERE role_id = '%5$s'::uuid
  AND claim_type = 'RoleScope'
  AND claim_value = '%2$s';

INSERT INTO core.role_claim(
    create_date, update_date, last_modified_by, created_by,
    record_status, role_id, claim_type, claim_value
)
SELECT
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, null, '%3$s',
    '%4$s', '%5$s', 'RoleScope', '%2$s'
WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_claim rc
    WHERE rc.role_id = '%5$s'::uuid
      AND rc.claim_type = 'RoleScope'
      AND rc.claim_value = '%2$s'
);
$SQL$,
            pra.role_key,
            pra.claim_value,
            gc.created_by,
            gc.record_status,
            nr.role_id
        ) AS script_block
    FROM policy_role_assignments pra
    JOIN normalized_roles nr
      ON nr.role_key = pra.role_key
    CROSS JOIN normalized_global_config gc
),
role_screen_blocks AS (
    SELECT
        '175_ROLESCREEN_' || pra.role_key || '_' || pra.claim_value AS order_key,
        'ROLE_SCREEN' AS block_type,
        format($SQL$
UPDATE core.role_screen
SET
    update_date = CURRENT_TIMESTAMP,
    last_modified_by = null,
    created_by = '%4$s',
    record_status = '%5$s',
    role_id = '%6$s',
    screen_id = '%7$s'
WHERE role_id = '%6$s'::uuid
  AND screen_id = '%7$s'::uuid;

INSERT INTO core.role_screen(
    id, role_id, screen_id, create_date, update_date,
    created_by, last_modified_by, record_status
)
SELECT
    '%8$s', '%6$s', '%7$s',
    CURRENT_TIMESTAMP, CURRENT_TIMESTAMP,
    '%4$s', null, '%5$s'
WHERE NOT EXISTS (
    SELECT 1
    FROM core.role_screen rs
    WHERE rs.role_id = '%6$s'::uuid
      AND rs.screen_id = '%7$s'::uuid
);
$SQL$,
            pra.role_key,
            pra.claim_value,
            pra.operation_type,
            gc.created_by,
            gc.record_status,
            nr.role_id,
            pcd.screen_id,
            lower(
                substr(md5('role_screen|' || pra.role_key || '|' || pra.claim_value), 1, 8) || '-' ||
                substr(md5('role_screen|' || pra.role_key || '|' || pra.claim_value), 9, 4) || '-' ||
                substr(md5('role_screen|' || pra.role_key || '|' || pra.claim_value), 13, 4) || '-' ||
                substr(md5('role_screen|' || pra.role_key || '|' || pra.claim_value), 17, 4) || '-' ||
                substr(md5('role_screen|' || pra.role_key || '|' || pra.claim_value), 21, 12)
            )
        ) AS script_block
    FROM policy_role_assignments pra
    JOIN policy_claim_definitions pcd
      ON pcd.claim_value = pra.claim_value
    JOIN normalized_roles nr
      ON nr.role_key = pra.role_key
    CROSS JOIN normalized_global_config gc
),
user_role_blocks AS (
    SELECT
        '200_USERROLE_' || ura.user_id || '_' || ura.role_key AS order_key,
        'USER_ROLE' AS block_type,
        format($SQL$
UPDATE core.user_role
SET role_id = '%5$s'
WHERE user_id = '%1$s'::uuid
  AND role_id = '%5$s'::uuid;

INSERT INTO core.user_role(user_id, role_id)
SELECT '%1$s', '%5$s'
WHERE NOT EXISTS (
    SELECT 1
    FROM core.user_role ur
    WHERE ur.user_id = '%1$s'::uuid
      AND ur.role_id = '%5$s'::uuid
);
$SQL$,
            u.user_id,
            u.full_name,
            u.email,
            ura.role_key,
            nr.role_id
        ) AS script_block
    FROM user_role_assignments ura
    JOIN users u
      ON u.user_id = ura.user_id
    JOIN normalized_roles nr
      ON nr.role_key = ura.role_key
),
all_blocks AS (
    SELECT order_key, block_type, script_block FROM role_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM screen_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM screen_claim_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM permission_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM role_claim_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM role_screen_blocks
    UNION ALL
    SELECT order_key, block_type, script_block FROM user_role_blocks
)
SELECT string_agg(script_block, E'\n\n' ORDER BY order_key) AS generated_sql
FROM all_blocks;