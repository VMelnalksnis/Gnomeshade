﻿SELECT a.id,
	   a.created_at            CreatedAt,
	   a.owner_id              OwnerId,
	   a.created_by_user_id    CreatedByUserId,
	   a.modified_at           ModifiedAt,
	   a.modified_by_user_id   ModifiedByUserId,
	   a.name,
	   a.normalized_name       NormalizedName,
	   a.counterparty_id       CounterpartyId,
	   a.preferred_currency_id PreferredCurrencyId,
	   a.bic,
	   a.iban,
	   a.account_number        AccountNumber,
	   pc.id,
	   pc.created_at           CreatedAt,
	   pc.name,
	   pc.normalized_name      NormalizedName,
	   pc.numeric_code         NumericCode,
	   pc.alphabetic_code      AlphabeticCode,
	   pc.minor_unit           MinorUnit,
	   pc.official,
	   pc.crypto,
	   pc.historical,
	   pc.active_from          ActiveFrom,
	   pc.active_until         ActiveUntil,
	   aic.id,
	   aic.created_at          CreatedAt,
	   aic.owner_id            OwnerId,
	   aic.created_by_user_id  CreatedByUserId,
	   aic.modified_at         ModifiedAt,
	   aic.modified_by_user_id ModifiedByUserId,
	   aic.account_id          AccountId,
	   aic.currency_id         CurrencyId,
	   c.alphabetic_code       CurrencyAlphabeticCode
FROM accounts a
		 INNER JOIN owners ON owners.id = a.owner_id
		 INNER JOIN ownerships ON owners.id = ownerships.owner_id
		 INNER JOIN access ON access.id = ownerships.access_id
		 LEFT JOIN currencies pc ON a.preferred_currency_id = pc.id
		 LEFT JOIN accounts_in_currency aic ON a.id = aic.account_id
		 LEFT JOIN currencies c ON aic.currency_id = c.id
WHERE ownerships.user_id = @userId
  AND (access.normalized_name = @access OR access.normalized_name = 'OWNER')
