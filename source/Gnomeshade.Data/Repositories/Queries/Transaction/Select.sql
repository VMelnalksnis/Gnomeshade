SELECT t.id,
       t.owner_id              OwnerId,
       t.created_at            CreatedAt,
       t.created_by_user_id    CreatedByUserId,
       t.modified_at           ModifiedAt,
       t.modified_by_user_id   ModifiedByUserId,
       t.booked_at             BookedAt,
       t.valued_at             ValuedAt,
       t.description,
       t.import_hash           ImportHash,
       t.imported_at           ImportedAt,
       t.reconciled_at         ReconciledAt,
       t.reconciled_by_user_id ReconciledByUserId
FROM public.transactions t
         INNER JOIN owners ON owners.id = t.owner_id
         INNER JOIN ownerships ON owners.id = ownerships.owner_id
         INNER JOIN access ON access.id = ownerships.access_id
