-- Check 1: Count consistency between file and file_line
SELECT 'CHECK_1: File vs FileLine Count Consistency' as check_name;
SELECT f.file_name,
  f.processed_line_count as file_processed,
  f.successful_line_count as file_success,
  f.failed_line_count as file_failed,
  COUNT(fl.id) as actual_lines,
  COUNT(fl.id) FILTER (WHERE fl.status = 'Success') as actual_success,
  COUNT(fl.id) FILTER (WHERE fl.status = 'Failed') as actual_failed,
  COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') as detail_rows,
  COUNT(fl.id) FILTER (WHERE fl.line_type = 'H') as header_rows,
  COUNT(fl.id) FILTER (WHERE fl.line_type = 'F') as footer_rows
FROM ingestion.file f
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
GROUP BY f.id ORDER BY f.file_name;

-- Check 2: Orphan file_lines (no parent file)
SELECT 'CHECK_2: Orphan Lines' as check_name;
SELECT COUNT(*) as orphan_lines FROM ingestion.file_line fl
WHERE NOT EXISTS (SELECT 1 FROM ingestion.file f WHERE f.id = fl.file_id);

-- Check 3: Duplicate detection status distribution per file
SELECT 'CHECK_3: Duplicate Status Distribution' as check_name;
SELECT f.file_name, fl.duplicate_status, COUNT(*) as cnt
FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
WHERE fl.line_type = 'D'
GROUP BY f.file_name, fl.duplicate_status ORDER BY f.file_name, fl.duplicate_status;

-- Check 4: Reconciliation status distribution per file
SELECT 'CHECK_4: Reconciliation Status Distribution' as check_name;
SELECT f.file_name, fl.reconciliation_status, COUNT(*) as cnt
FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
WHERE fl.line_type = 'D'
GROUP BY f.file_name, fl.reconciliation_status ORDER BY f.file_name, fl.reconciliation_status;

-- Check 5: Lines with NULL or empty parsed_content (detail lines)
SELECT 'CHECK_5: NULL/Empty Parsed Content' as check_name;
SELECT f.file_name, COUNT(*) as null_parsed_lines
FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
WHERE fl.line_type = 'D' AND (fl.parsed_content IS NULL OR fl.parsed_content::text = '{}')
GROUP BY f.file_name;

-- Check 6: Duplicate line_numbers within same file
SELECT 'CHECK_6: Duplicate Line Numbers' as check_name;
SELECT f.file_name, fl.line_number, COUNT(*) as dup_count
FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
GROUP BY f.file_name, fl.line_number HAVING COUNT(*) > 1;

-- Check 7: expected_line_count vs actual detail lines
SELECT 'CHECK_7: Expected vs Actual Detail Count' as check_name;
SELECT f.file_name, f.expected_line_count,
  COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') as actual_detail_count,
  CASE WHEN f.expected_line_count <> COUNT(fl.id) FILTER (WHERE fl.line_type = 'D') THEN 'MISMATCH' ELSE 'OK' END as check_result
FROM ingestion.file f
LEFT JOIN ingestion.file_line fl ON fl.file_id = f.id
GROUP BY f.id ORDER BY f.file_name;

-- Check 8: processed = success + failed
SELECT 'CHECK_8: Processed = Success + Failed' as check_name;
SELECT f.file_name, f.processed_line_count, f.successful_line_count, f.failed_line_count,
  CASE WHEN f.processed_line_count <> (f.successful_line_count + f.failed_line_count) THEN 'MISMATCH' ELSE 'OK' END as sum_check
FROM ingestion.file f ORDER BY f.file_name;

-- Check 9: File record_status check
SELECT 'CHECK_9: Record Status' as check_name;
SELECT f.file_name, f.record_status, f.status FROM ingestion.file f ORDER BY f.file_name;

-- Check 10: Audit fields (created_by, last_modified_by NULL check)
SELECT 'CHECK_10: NULL Audit Fields' as check_name;
SELECT f.file_name,
  CASE WHEN f.created_by IS NULL THEN 'NULL_CREATED_BY' ELSE 'OK' END as created_by_check,
  CASE WHEN f.last_modified_by IS NULL THEN 'NULL_MODIFIED_BY' ELSE 'OK' END as modified_by_check,
  CASE WHEN f.create_date IS NULL THEN 'NULL_CREATE_DATE' ELSE 'OK' END as create_date_check,
  CASE WHEN f.update_date IS NULL THEN 'NULL_UPDATE_DATE' ELSE 'OK' END as update_date_check
FROM ingestion.file f ORDER BY f.file_name;

-- Check 11: File line audit fields
SELECT 'CHECK_11: Line Level NULL Audit Fields' as check_name;
SELECT f.file_name,
  COUNT(*) FILTER (WHERE fl.created_by IS NULL) as null_created_by,
  COUNT(*) FILTER (WHERE fl.last_modified_by IS NULL) as null_modified_by,
  COUNT(*) FILTER (WHERE fl.create_date IS NULL) as null_create_date,
  COUNT(*) FILTER (WHERE fl.update_date IS NULL) as null_update_date
FROM ingestion.file_line fl JOIN ingestion.file f ON f.id = fl.file_id
GROUP BY f.file_name ORDER BY f.file_name;

-- Check 12: Sample parsed_content from a detail line
SELECT 'CHECK_12: Sample Parsed Content (first 3 detail lines of first file)' as check_name;
SELECT fl.line_number, fl.line_type, fl.status, LEFT(fl.parsed_content::text, 200) as parsed_content_preview
FROM ingestion.file_line fl
JOIN ingestion.file f ON f.id = fl.file_id
WHERE f.file_name = 'CARD_TRANSACTIONS_20250710_1.txt' AND fl.line_type = 'D'
ORDER BY fl.line_number LIMIT 3;

-- Check 13: Reconciliation tables status
SELECT 'CHECK_13: Reconciliation Tables' as check_name;
SELECT 'evaluation' as tbl, COUNT(*) as cnt FROM reconciliation.evaluation
UNION ALL SELECT 'operation', COUNT(*) FROM reconciliation.operation
UNION ALL SELECT 'operation_execution', COUNT(*) FROM reconciliation.operation_execution
UNION ALL SELECT 'review', COUNT(*) FROM reconciliation.review
UNION ALL SELECT 'alert', COUNT(*) FROM reconciliation.alert;

