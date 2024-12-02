// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Gnomeshade.WebApi.Models;

[UnconditionalSuppressMessage(
	"Trimming",
	"IL2075",
	Justification = "Configuration can be safely trimmed if it is not referenced")]
internal sealed class RequiredIfNotNullAttribute : RequiredAttribute
{
	private readonly string _propertyName;

	internal RequiredIfNotNullAttribute(string propertyName)
	{
		_propertyName = propertyName;
	}

	/// <inheritdoc />
	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		var instanceType = validationContext.ObjectInstance.GetType();
		var propertyInfo = instanceType.GetProperty(_propertyName);
		if (propertyInfo is null)
		{
			throw new MissingMemberException(instanceType.FullName, _propertyName);
		}

		var propertyValue = propertyInfo.GetValue(validationContext.ObjectInstance);
		return propertyValue is not null
			? base.IsValid(value, validationContext)
			: null;
	}
}
