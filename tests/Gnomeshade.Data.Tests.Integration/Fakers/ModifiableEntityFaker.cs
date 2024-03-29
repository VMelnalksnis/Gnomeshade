﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq.Expressions;

using Bogus;

using Gnomeshade.Data.Entities.Abstractions;

namespace Gnomeshade.Data.Tests.Integration.Fakers;

public abstract class ModifiableEntityFaker<TEntity> : Faker<TEntity>
	where TEntity : Entity, IOwnableEntity, IModifiableEntity
{
	protected ModifiableEntityFaker(Guid userId)
	{
		RuleFor(entity => entity.Id, Guid.NewGuid);
		RuleFor(entity => entity.OwnerId, userId);
		RuleFor(entity => entity.CreatedByUserId, userId);
		RuleFor(entity => entity.ModifiedByUserId, userId);
	}

	/// <inheritdoc />
	public sealed override Faker<TEntity> RuleFor<TProperty>(
		Expression<Func<TEntity, TProperty>> property,
		Func<TProperty> valueFunction)
	{
		return base.RuleFor(property, valueFunction);
	}

	/// <inheritdoc />
	public sealed override Faker<TEntity> RuleFor<TProperty>(
		Expression<Func<TEntity, TProperty>> property,
		TProperty value)
	{
		return base.RuleFor(property, value);
	}
}
