INSERT INTO loans2
    (id,
	 created_by_user_id,
     owner_id,
     modified_by_user_id,
     name,
     normalized_name,
     issuing_counterparty_id,
     receiving_counterparty_id,
     principal,
     currency_id)
VALUES
    (@Id,
	 @CreatedByUserId,
     @OwnerId,
     @ModifiedByUserId,
     @Name,
     upper(@Name),
     @IssuingCounterpartyId,
     @ReceivingCounterpartyId,
     @Principal,
     @CurrencyId)
RETURNING id;
