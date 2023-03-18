UPDATE transfers
SET external_reference = bank_reference,
	bank_reference     = NULL
WHERE bank_reference IS NOT NULL
  AND external_reference IS NULL;
