
DO $$
BEGIN
  IF NOT EXISTS (
      SELECT column_name
               FROM information_schema.columns
               WHERE table_schema='core' AND table_name='request' AND column_name='module_name'
    ) THEN
    ALTER TABLE core.request ADD module_name character varying(100) NOT NULL DEFAULT '';
END IF;
END $$;

update core.request set module_name = 'BusinessParameter' where resource = 'Parameters';
update core.request set module_name = 'Approval' where resource = 'Cases';
update core.request set module_name = 'Identity' where resource = 'Questions';
update core.request set module_name = 'PF' where resource = 'Merchants';
update core.request set module_name = 'CustomerManagement' where resource = 'CustomerAddress';
update core.request set module_name = 'BusinessParameter' where resource = 'ParameterTemplate';
update core.request set module_name = 'BusinessParameter' where resource = 'ParameterGroups';
update core.request set module_name = 'Emoney' where resource = 'EmoneyAccounts';
update core.request set module_name = 'Identity' where resource = 'Users';
update core.request set module_name = 'PF' where resource = 'MerchantTransactions';
update core.request set module_name = 'Billing' where resource = 'Institution';
update core.request set module_name = 'Identity' where resource = 'Roles';
update core.request set module_name = 'Emoney' where resource = 'Limits';
update core.request set module_name = 'PF' where resource = 'AcquireBanks';