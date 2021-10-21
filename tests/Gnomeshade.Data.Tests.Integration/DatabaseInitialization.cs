// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

using FluentAssertions;

using Gnomeshade.Data.Entities;
using Gnomeshade.TestingHelpers.Data;

using Microsoft.Extensions.Configuration;

using Npgsql;
using Npgsql.Logging;

using NUnit.Framework;

namespace Gnomeshade.Data.Tests.Integration
{
	[SetUpFixture]
	public class DatabaseInitialization
	{
		private static readonly IConfiguration _configuration =
			new ConfigurationBuilder()
				.AddUserSecrets<DatabaseInitialization>(true, true)
				.AddEnvironmentVariables()
				.Build();

		private static readonly PostgresInitializer _initializer = new(_configuration);

		public static UserEntity TestUser { get; private set; } = null!;

		public static async Task<NpgsqlConnection> CreateConnectionAsync()
		{
			return await _initializer.CreateConnectionAsync();
		}

		[OneTimeSetUp]
		public static async Task SetupDatabaseAsync()
		{
			AssertionOptions.AssertEquivalencyUsing(options => options.ComparingByMembers<AccountEntity>());
			NpgsqlLogManager.Provider = new ConsoleLoggingProvider(NpgsqlLogLevel.Debug);
			NpgsqlLogManager.IsParameterLoggingEnabled = true;

			TestUser = await _initializer.SetupDatabaseAsync();
		}

		[OneTimeTearDown]
		public static async Task DropDatabaseAsync()
		{
			await _initializer.DropDatabaseAsync();
		}
	}
}
