CREATE TABLE transaction_schedules
(
	id                  uuid        DEFAULT (uuid_generate_v4()) NOT NULL,
	created_at          timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	created_by_user_id  uuid                                     NOT NULL,
	deleted_at          timestamptz,
	deleted_by_user_id  uuid,
	owner_id            uuid                                     NOT NULL,
	modified_at         timestamptz DEFAULT CURRENT_TIMESTAMP    NOT NULL,
	modified_by_user_id uuid                                     NOT NULL,
	name                text                                     NOT NULL,
	normalized_name     text                                     NOT NULL,

	starting_at         timestamptz                              NOT NULL,
	period              interval                                 NOT NULL,
	count               int                                      NOT NULL,

	CONSTRAINT "transaction_schedules_id" PRIMARY KEY (id),
	CONSTRAINT "transaction_schedules_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "transaction_schedules_deleted_by_user_id_fkey" FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "transaction_schedules_deleted_check" CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
															(deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL)),
	CONSTRAINT "transaction_schedules_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "transaction_schedules_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "transaction_schedules_normalized_name_unique" UNIQUE (normalized_name, owner_id)
);

ALTER TABLE transactions
	ADD COLUMN planned bool DEFAULT false NOT NULL;

ALTER TABLE transactions
	ADD COLUMN schedule_id uuid NULL REFERENCES transaction_schedules;
