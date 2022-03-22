ALTER TABLE public.accounts
    DROP CONSTRAINT accounts_normalized_name;

ALTER TABLE public.accounts
    ADD CONSTRAINT accounts_normalized_name_counterparty_id
        UNIQUE (counterparty_id, normalized_name);
