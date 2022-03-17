// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace Gnomeshade.Interfaces.WebApi.Models;

internal sealed class RequiredIfNullAttribute : RequiredAttribute
{
	private readonly string _propertyName;

	internal RequiredIfNullAttribute(string propertyName)
	{
		_propertyName = propertyName;
	}

	/// <inheritdoc />
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is not null)
		{
			return null;
		}

		var instanceType = validationContext.ObjectInstance.GetType();
		var propertyInfo = instanceType.GetProperty(_propertyName);
		if (propertyInfo is null)
		{
			throw new MissingMemberException(instanceType.FullName, _propertyName);
		}

		var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);
		return propertyValue is null
			? new ValidationResult($"One of {_propertyName} and {validationContext.DisplayName} is required.")
			: null;
	}
}
