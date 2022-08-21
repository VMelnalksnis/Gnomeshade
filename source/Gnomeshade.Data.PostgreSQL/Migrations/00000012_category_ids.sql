ALTER TABLE "public"."products"
	ADD COLUMN "category_id" uuid NULL;

ALTER TABLE "public"."products"
	ADD CONSTRAINT products_category_id_fkey
		FOREIGN KEY (category_id) REFERENCES categories (id) NOT DEFERRABLE;

ALTER TABLE "public"."categories"
	ADD COLUMN "category_id" uuid NULL;

ALTER TABLE "public"."categories"
	ADD CONSTRAINT categories_category_id_fkey
		FOREIGN KEY (category_id) REFERENCES categories (id) NOT DEFERRABLE;
