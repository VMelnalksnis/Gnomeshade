ALTER TABLE "categories"
	ADD COLUMN "linked_product_id" uuid NULL REFERENCES products;
