SELECT loans2.id,
	   loans2.created_at                CreatedAt,
	   loans2.created_by_user_id        CreatedByUserId,
	   loans2.deleted_at                DeletedAt,
	   loans2.deleted_by_user_id        DeletedByUserId,
	   loans2.owner_id                  OwnerId,
	   loans2.modified_at               ModifiedAt,
	   loans2.modified_by_user_id       ModifiedByUserId,
	   loans2.name,
	   loans2.normalized_name           NormalizedName,

	   loans2.issuing_counterparty_id   IssuingCounterpartyId,
	   loans2.receiving_counterparty_id ReceivingCounterpartyId,
	   loans2.principal,
	   loans2.currency_id               CurrencyId
FROM loans2
