using System;

using Tracking.Finance.Web.Data.Models;

namespace Tracking.Finance.Web.Models.Units
{
	/// <summary>
	/// General information about a <see cref="Unit"/>.
	/// </summary>
	public record UnitIndexModel(int Id, string Name, short Exponent, decimal Mantissa, int? ParentUnitId, string? ParentUnitName)
	{
		/// <summary>
		/// Gets the id of the <see cref="Unit"/>.
		/// </summary>
		public int Id { get; init; } = Id;

		/// <summary>
		/// Gets the name of the <see cref="Unit"/>.
		/// </summary>
		public string Name { get; init; } = Name;

		/// <summary>
		/// Gets the exponent of the <see cref="Unit"/>.
		/// </summary>
		public short Exponent { get; init; } = Exponent;

		/// <summary>
		/// Gets the Mantissa of the <see cref="Unit"/>.
		/// </summary>
		public decimal Mantissa { get; init; } = Mantissa;

		/// <summary>
		/// Gets the id of the parent <see cref="Unit"/>.
		/// </summary>
		public int? ParentUnitId { get; init; } = ParentUnitId;

		/// <summary>
		/// Gets the name of the parent <see cref="Unit"/>.
		/// </summary>
		public string? ParentUnitName { get; init; } = ParentUnitName;

		/// <summary>
		/// Gets a formatted scaling factor with parent <see cref="Unit"/>.
		/// </summary>
		public string? ScaledParentName =>
			ParentUnitName is null
				? null
				: $"{Mantissa * (decimal)Math.Pow(10, Exponent)} {ParentUnitName}";
	}
}
