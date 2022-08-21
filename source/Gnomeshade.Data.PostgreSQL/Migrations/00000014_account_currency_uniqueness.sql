ALTER TABLE public.accounts_in_currency
	ADD CONSTRAINT accounts_in_currency_unique_account_id_currency_id
		UNIQUE (account_id, currency_id);
