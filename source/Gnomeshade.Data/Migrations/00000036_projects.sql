CREATE TABLE projects
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

	parent_project_id   uuid                                     NULL,

	CONSTRAINT "projects_id" PRIMARY KEY (id),
	CONSTRAINT "projects_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "projects_deleted_by_user_id_fkey" FOREIGN KEY (deleted_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "projects_deleted_check" CHECK ((deleted_at IS NULL AND deleted_by_user_id IS NULL) OR
											   (deleted_at IS NOT NULL AND deleted_by_user_id IS NOT NULL)),
	CONSTRAINT "projects_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
	CONSTRAINT "projects_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

	CONSTRAINT "projects_normalized_name_unique" UNIQUE (normalized_name, owner_id),

	CONSTRAINT "projects_parent_project_id_fkey" FOREIGN KEY (parent_project_id) REFERENCES projects (id) NOT DEFERRABLE
);

CREATE TABLE project_purchases
(
	created_at         timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
	created_by_user_id uuid                                  NOT NULL,

	project_id         uuid                                  NOT NULL,
	purchase_id        uuid                                  NOT NULL,

	CONSTRAINT "project_purchases_pkey" PRIMARY KEY (project_id, purchase_id),

	CONSTRAINT "project_purchases_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
	CONSTRAINT "project_purchases_project_id_fkey" FOREIGN KEY (project_id) REFERENCES projects (id) NOT DEFERRABLE,
	CONSTRAINT "project_purchases_purchase_id_fkey" FOREIGN KEY (purchase_id) REFERENCES purchases (id) NOT DEFERRABLE
);
