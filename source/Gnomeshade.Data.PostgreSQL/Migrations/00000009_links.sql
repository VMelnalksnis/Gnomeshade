DROP TABLE IF EXISTS "public"."transaction_links";
DROP TABLE IF EXISTS "public"."links";

CREATE TABLE "public"."links"
(
    "id"                  uuid        DEFAULT uuid_generate_v4() NOT NULL,
    "created_at"          timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "owner_id"            uuid                                   NOT NULL,
    "created_by_user_id"  uuid                                   NOT NULL,
    "modified_at"         timestamptz DEFAULT CURRENT_TIMESTAMP  NOT NULL,
    "modified_by_user_id" uuid                                   NOT NULL,

    "uri"                 text                                   NOT NULL,

    CONSTRAINT "links_id" PRIMARY KEY ("id"),
    CONSTRAINT "links_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "links_owner_id_fkey" FOREIGN KEY (owner_id) REFERENCES owners (id) NOT DEFERRABLE,
    CONSTRAINT "links_modified_by_user_id_fkey" FOREIGN KEY (modified_by_user_id) REFERENCES users (id) NOT DEFERRABLE,

    CONSTRAINT "links_uri_unique" UNIQUE (uri) NOT DEFERRABLE
) WITH (OIDS = FALSE);

CREATE TABLE "public"."transaction_links"
(
    "created_at"         timestamptz DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "created_by_user_id" uuid                                  NOT NULL,
    "link_id"            uuid                                  NOT NULL,
    "transaction_id"     uuid                                  NOT NULL,

    CONSTRAINT "transaction_links_id" PRIMARY KEY ("link_id", "transaction_id"),
    CONSTRAINT "transaction_links_created_by_user_id_fkey" FOREIGN KEY (created_by_user_id) REFERENCES users (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_links_link_id_fkey" FOREIGN KEY (link_id) REFERENCES links (id) NOT DEFERRABLE,
    CONSTRAINT "transaction_links_transaction_id_fkey" FOREIGN KEY (transaction_id) REFERENCES transactions (id) NOT DEFERRABLE
) WITH (OIDS = FALSE);
