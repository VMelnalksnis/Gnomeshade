ALTER TABLE public.accounts
	DROP CONSTRAINT accounts_normalized_name_counterparty_id;

ALTER TABLE public.accounts
	ADD CONSTRAINT accounts_normalized_name_counterparty_id
		UNIQUE (counterparty_id, normalized_name) NOT DEFERRABLE;

ALTER TABLE public.accounts
	DROP CONSTRAINT IF EXISTS counterparties_normalized_name;

ALTER TABLE public.counterparties
	DROP CONSTRAINT IF EXISTS counterparties_normalized_name;
