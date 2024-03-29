﻿SELECT loans.id,
	   loans.created_at                CreatedAt,
	   loans.owner_id                  OwnerId,
	   loans.created_by_user_id        CreatedByUserId,
	   loans.modified_at               ModifiedAt,
	   loans.modified_by_user_id       ModifiedByUserId,
	   loans.transaction_id            TransactionId,
	   loans.issuing_counterparty_id   IssuingCounterpartyId,
	   loans.receiving_counterparty_id ReceivingCounterpartyId,
	   loans.amount AS                 Amount,
	   loans.currency_id               CurrencyId,
	   loans.deleted_at                DeletedAt,
	   loans.deleted_by_user_id        DeletedByUserId
FROM loans
