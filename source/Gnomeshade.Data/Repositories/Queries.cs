﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Gnomeshade.Data.Repositories;

internal static class Queries
{
	private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();
	private static readonly Type _namespaceType = typeof(Queries);

	private static string Read(string resourceName)
	{
		using var resourceStream = _assembly.GetManifestResourceStream(_namespaceType, resourceName);
		if (resourceStream is null)
		{
			var message = $"Could not find resource {resourceName} within namespace {_namespaceType.Namespace}";
			throw new MissingManifestResourceException(message);
		}

		using var reader = new StreamReader(resourceStream);
		return reader.ReadToEnd();
	}

	internal static class Account
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Account)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Account)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Account)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Account)}.Update.sql");
	}

	internal static class AccountInCurrency
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(AccountInCurrency)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(AccountInCurrency)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(AccountInCurrency)}.Select.sql");
	}

	internal static class Counterparty
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Counterparty)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Counterparty)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Counterparty)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Counterparty)}.Update.sql");

		internal static string Merge { get; } = Read($"Queries.{nameof(Counterparty)}.Merge.sql");
	}

	internal static class Currency
	{
		internal static string Select { get; } = Read($"Queries.{nameof(Currency)}.Select.sql");
	}

	internal static class Ownership
	{
		internal static string Insert { get; } = Read($"Queries.{nameof(Ownership)}.Insert.sql");
	}

	internal static class Link
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Link)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Link)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Link)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Link)}.Update.sql");
	}

	internal static class Product
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Product)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Product)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Product)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Product)}.Update.sql");
	}

	internal static class Category
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Category)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Category)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Category)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Category)}.Update.sql");
	}

	internal static class Transaction
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Transaction)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Transaction)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Transaction)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Transaction)}.Update.sql");
	}

	internal static class Transfer
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Transfer)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Transfer)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Transfer)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Transfer)}.Update.sql");
	}

	internal static class Purchase
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Purchase)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Purchase)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Purchase)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Purchase)}.Update.sql");
	}

	internal static class Unit
	{
		internal static string Delete { get; } = Read($"Queries.{nameof(Unit)}.Delete.sql");

		internal static string Insert { get; } = Read($"Queries.{nameof(Unit)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(Unit)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(Unit)}.Update.sql");
	}

	internal static class User
	{
		internal static string Insert { get; } = Read($"Queries.{nameof(User)}.Insert.sql");

		internal static string Select { get; } = Read($"Queries.{nameof(User)}.Select.sql");

		internal static string Update { get; } = Read($"Queries.{nameof(User)}.Update.sql");
	}
}
