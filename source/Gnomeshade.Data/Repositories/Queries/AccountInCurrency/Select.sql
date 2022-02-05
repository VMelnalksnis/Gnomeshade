SELECT a.id,
       a.owner_id          OwnerId,
       a.created_at        CreatedAt,
       created_by_user_id  CreatedByUserId,
       modified_at         ModifiedAt,
       modified_by_user_id ModifiedByUserId,
       account_id          AccountId,
       currency_id         CurrencyId,
       disabled_at         DisabledAt,
       disabled_by_user_id DisabledByUserId
FROM accounts_in_currency a
         INNER JOIN owners ON owners.id = a.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
