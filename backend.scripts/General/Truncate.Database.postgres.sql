TRUNCATE TABLE accounting.core.bank_account;
TRUNCATE TABLE accounting.core.customer;
TRUNCATE TABLE accounting.core.external_currency;
TRUNCATE TABLE accounting.core.payment;

TRUNCATE TABLE approval.core.maker_checker;
TRUNCATE TABLE approval.core.request;
ALTER SEQUENCE approval.core.request_reference_id_seq RESTART WITH 1;

TRUNCATE TABLE billing.core.authorization_token;
TRUNCATE TABLE billing.core.commission;
TRUNCATE TABLE billing.core.field;
TRUNCATE TABLE billing.core.institution_mapping;
TRUNCATE TABLE billing.core.saved_bill;
TRUNCATE TABLE billing.core.sector_mapping;
TRUNCATE TABLE billing.core.synchronization_log;
TRUNCATE TABLE billing.core.timeout_transaction;
TRUNCATE TABLE billing.reconciliation.institution_detail;
DELETE FROM billing.core.transaction;
TRUNCATE TABLE billing.reconciliation.institution_summary;
DELETE FROM billing.core.institution;
DELETE FROM billing.core.sector;
TRUNCATE TABLE billing.reconciliation.summary;

TRUNCATE TABLE btrans.core.document_track;
TRUNCATE TABLE btrans.core.balance_information;
TRUNCATE TABLE btrans.core.domestic_transfer;
TRUNCATE TABLE btrans.core.pos_information;

TRUNCATE TABLE campaign_management.core.authorization_token;
TRUNCATE TABLE campaign_management.core.i_wallet_card;
TRUNCATE TABLE campaign_management.core.i_wallet_cash_back_transaction;
TRUNCATE TABLE campaign_management.core.i_wallet_charge_transaction;
TRUNCATE TABLE campaign_management.core.i_wallet_qr_code;
DELETE FROM campaign_management.core.i_wallet_charge;

TRUNCATE TABLE content.core.data_containers;

TRUNCATE TABLE customer_management.core.customer_addresses;
TRUNCATE TABLE customer_management.core.customer_email;
TRUNCATE TABLE customer_management.core.customer_phone;
TRUNCATE TABLE customer_management.core.customer_product;
DELETE FROM customer_management.core.customer;

TRUNCATE TABLE digital_kyc.core.integration_address;
TRUNCATE TABLE digital_kyc.core.integration;

TRUNCATE TABLE document.core.document;

TRUNCATE TABLE emoney.core.account_activity;
TRUNCATE TABLE emoney.core.account_iban;
TRUNCATE TABLE emoney.core.account_kyc_change;
TRUNCATE TABLE emoney.core.account_user;
DELETE FROM emoney.core.account;
TRUNCATE TABLE emoney.core.provision;
TRUNCATE TABLE emoney.core.return_transaction_request;
TRUNCATE TABLE emoney.core.saved_account;
DELETE FROM emoney.core.transaction;
TRUNCATE TABLE emoney.core.transfer_order;
DELETE FROM emoney.core.wallet;
TRUNCATE TABLE emoney.core.withdraw_request;
TRUNCATE TABLE emoney.limit.account_current_level;
TRUNCATE TABLE emoney.limit.account_custom_tier;

TRUNCATE TABLE epin.core.reconciliation_detail;
DELETE FROM epin.core.order;
DELETE FROM epin.core.order_history;
TRUNCATE TABLE epin.core.product;
DELETE FROM epin.core.reconciliation_summary;
DELETE FROM epin.core.brand;
DELETE FROM epin.core.publisher;

TRUNCATE TABLE fraud.core.integration_log;
TRUNCATE TABLE fraud.core.search_log;
TRUNCATE TABLE fraud.core.triggered_rule;
DELETE FROM fraud.core.transaction_monitoring;

TRUNCATE TABLE identity.core.user_address;
TRUNCATE TABLE identity.core.user_agreement_document;
TRUNCATE TABLE identity.core.user_claim;
TRUNCATE TABLE identity.core.user_device_info;
TRUNCATE TABLE identity.core.user_login;
TRUNCATE TABLE identity.core.user_login_last_activity;
TRUNCATE TABLE identity.core.user_password_history;
TRUNCATE TABLE identity.core.user_picture;
TRUNCATE TABLE identity.core.user_role;
TRUNCATE TABLE identity.core.user_security_answer;
TRUNCATE TABLE identity.core.user_session;
TRUNCATE TABLE identity.core.user_token;
DELETE FROM identity.core.device_info;
DELETE FROM identity.core.user;
ALTER SEQUENCE identity.core.user_claim_id_seq RESTART WITH 1;

TRUNCATE TABLE iks.core.iks_transaction;

TRUNCATE TABLE log_consumer.core.audit_log;
TRUNCATE TABLE log_consumer.core.entity_change_log;

TRUNCATE TABLE money_transfer.core.bank_account_balance;
TRUNCATE TABLE money_transfer.core.bank_transaction;
DELETE FROM money_transfer.core.bank_transaction_summary;
TRUNCATE TABLE money_transfer.core.incoming_transaction;
DELETE FROM money_transfer.core.incoming_transaction_summary;
TRUNCATE TABLE money_transfer.core.returned_transaction;
TRUNCATE TABLE money_transfer.core.timeout_transaction;
TRUNCATE TABLE money_transfer.core.transaction;
TRUNCATE TABLE money_transfer.core.transaction_history;
TRUNCATE TABLE money_transfer.core.transaction_reference_counter;
TRUNCATE TABLE money_transfer.reconciliation.batch_log;
TRUNCATE TABLE money_transfer.reconciliation.detail;
DELETE FROM money_transfer.reconciliation.summary;
ALTER SEQUENCE money_transfer.core.transaction_reference_counter_transaction_reference_int_seq RESTART WITH 1;

TRUNCATE TABLE notification.core.notification_log;
TRUNCATE TABLE notification.core.otp_record;
TRUNCATE TABLE notification.core.user_inbox;

TRUNCATE TABLE pf.bank.transaction;

TRUNCATE TABLE pf.card.token;
DELETE FROM pf.card.store;

TRUNCATE TABLE pf.core.cost_profile_item;
DELETE FROM pf.core.cost_profile;
TRUNCATE TABLE pf.core.pricing_profile_item;
DELETE FROM pf.core.pricing_profile;
TRUNCATE TABLE pf.core.three_d_verification;
TRUNCATE TABLE pf.core.time_out_transaction;

TRUNCATE TABLE pf.limit.merchant_daily_usage;
TRUNCATE TABLE pf.limit.merchant_limit;
TRUNCATE TABLE pf.limit.merchant_monthly_usage;

TRUNCATE TABLE pf.merchant.api_key;
TRUNCATE TABLE pf.merchant.api_log;
TRUNCATE TABLE pf.merchant.api_validation_log;
TRUNCATE TABLE pf.merchant.bank_account;
TRUNCATE TABLE pf.merchant.blockage_detail;
DELETE FROM pf.merchant.blockage;
TRUNCATE TABLE pf.merchant.business_partner;
TRUNCATE TABLE pf.merchant.counter;
TRUNCATE TABLE pf.merchant.document;
TRUNCATE TABLE pf.merchant.email;
TRUNCATE TABLE pf.merchant.history;
TRUNCATE TABLE pf.merchant.merchant_deduction;
TRUNCATE TABLE pf.merchant.merchant_due;
TRUNCATE TABLE pf.merchant.merchant_return_pool;
TRUNCATE TABLE pf.merchant.merchant_statement;
TRUNCATE TABLE pf.merchant.score;
TRUNCATE TABLE pf.posting.transfer_error;
DELETE FROM pf.merchant.transaction;
TRUNCATE TABLE pf.merchant.user;
TRUNCATE TABLE pf.merchant.vpos;
ALTER SEQUENCE pf.merchant.counter_number_counter_seq RESTART WITH 1;

TRUNCATE TABLE pf.posting.balance;
DELETE FROM pf.posting.bank_balance;
TRUNCATE TABLE pf.posting.batch_status;
TRUNCATE TABLE pf.posting.item;
TRUNCATE TABLE pf.posting.transaction;

TRUNCATE TABLE pf.vpos.currency;

DELETE FROM pf.merchant.merchant;
DELETE FROM pf.merchant.mcc;
DELETE FROM pf.merchant.pool;
DELETE FROM pf.merchant.integrator;
DELETE FROM pf.core.customer;
DELETE FROM pf.core.contact_person;
DELETE FROM pf.vpos.vpos;

DELETE FROM pf.link.link;
DELETE FROM pf.link.link_customer;
DELETE FROM pf.link.link_installment;
DELETE FROM pf.link.link_transaction;
DELETE FROM pf.merchant.merchant_content;
DELETE FROM pf.merchant.merchant_content_version;
DELETE FROM pf.merchant.merchant_logo;