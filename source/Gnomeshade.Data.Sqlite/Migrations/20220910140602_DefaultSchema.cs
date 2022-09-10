// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gnomeshade.Data.Sqlite.Migrations
{
	/// <inheritdoc />
	public partial class DefaultSchema : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				name: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetUserTokens",
				newName: "AspNetUserTokens",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetUsers",
				newName: "AspNetUsers",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetUserRoles",
				newName: "AspNetUserRoles",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetUserLogins",
				newName: "AspNetUserLogins",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetUserClaims",
				newName: "AspNetUserClaims",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetRoles",
				newName: "AspNetRoles",
				newSchema: "identity");

			migrationBuilder.RenameTable(
				name: "AspNetRoleClaims",
				newName: "AspNetRoleClaims",
				newSchema: "identity");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameTable(
				name: "AspNetUserTokens",
				schema: "identity",
				newName: "AspNetUserTokens");

			migrationBuilder.RenameTable(
				name: "AspNetUsers",
				schema: "identity",
				newName: "AspNetUsers");

			migrationBuilder.RenameTable(
				name: "AspNetUserRoles",
				schema: "identity",
				newName: "AspNetUserRoles");

			migrationBuilder.RenameTable(
				name: "AspNetUserLogins",
				schema: "identity",
				newName: "AspNetUserLogins");

			migrationBuilder.RenameTable(
				name: "AspNetUserClaims",
				schema: "identity",
				newName: "AspNetUserClaims");

			migrationBuilder.RenameTable(
				name: "AspNetRoles",
				schema: "identity",
				newName: "AspNetRoles");

			migrationBuilder.RenameTable(
				name: "AspNetRoleClaims",
				schema: "identity",
				newName: "AspNetRoleClaims");
		}
	}
}
