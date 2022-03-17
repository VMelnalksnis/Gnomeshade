ALTER TABLE public.transactions
    RENAME COLUMN "date" TO booked_at;

ALTER TABLE public.transactions
    ALTER COLUMN booked_at DROP NOT NULL;

ALTER TABLE public.transactions
    ADD COLUMN valued_at timestamptz NULL;

ALTER TABLE public.transactions
    ADD CONSTRAINT transactions_one_of_dates_required CHECK (num_nonnulls(booked_at, transactions.valued_at) > 0);

ALTER TABLE public.transactions
    RENAME COLUMN validated_at TO reconciled_at;

ALTER TABLE public.transactions
    RENAME COLUMN validated_by_user_id TO reconciled_by_user_id;
