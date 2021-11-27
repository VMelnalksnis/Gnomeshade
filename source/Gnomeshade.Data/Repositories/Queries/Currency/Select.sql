SELECT id,
       created_at      CreatedAt,
       name,
       normalized_name NormalizedName,
       numeric_code    NumericCode,
       alphabetic_code AlphabeticCode,
       minor_unit      MinorUnit,
       official,
       crypto,
       historical,
       active_from     ActiveFrom,
       active_until    ActiveUntil
FROM currencies
