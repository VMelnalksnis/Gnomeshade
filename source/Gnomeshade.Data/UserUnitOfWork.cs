// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Models;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data
{
	public sealed class UserUnitOfWork : IDisposable
	{
		private readonly IDbConnection _dbConnection;
		private readonly OwnerRepository _ownerRepository;
		private readonly OwnershipRepository _ownershipRepository;
		private readonly UserRepository _userRepository;
		private readonly CounterpartyRepository _counterpartyRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserUnitOfWork"/> class.
		/// </summary>
		/// <param name="dbConnection">The database connection for executing queries.</param>
		public UserUnitOfWork(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;

			_ownerRepository = new(dbConnection);
			_ownershipRepository = new(dbConnection);
			_userRepository = new(dbConnection);
			_counterpartyRepository = new(dbConnection);
		}

		/// <summary>
		/// Creates a new application user for the specified identity user.
		/// </summary>
		/// <param name="applicationUser">The identity user for which to create a new application user.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateUserAsync(ApplicationUser applicationUser)
		{
			var userId = Guid.Parse(applicationUser.Id); // todo convert to parse exact
			var fullName = applicationUser.FullName;
			var user = new User { Id = userId, ModifiedByUserId = userId };

			using var dbTransaction = _dbConnection.OpenAndBeginTransaction();

			try
			{
				_ = await _userRepository.AddWithIdAsync(user, dbTransaction);
				_ = await _ownerRepository.AddAsync(userId, dbTransaction);
				_ = await _ownershipRepository.AddDefaultAsync(userId, dbTransaction);
				var counterparty = new Counterparty
				{
					OwnerId = userId,
					CreatedByUserId = userId,
					ModifiedByUserId = userId,
					Name = fullName,
					NormalizedName = fullName.ToUpperInvariant(),
				};
				var counterpartyId = await _counterpartyRepository.AddAsync(counterparty, dbTransaction);
				_ = await _userRepository.AddCounterparty(userId, counterpartyId, dbTransaction);

				dbTransaction.Commit();
			}
			catch (Exception)
			{
				dbTransaction.Rollback();
				throw;
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_dbConnection.Dispose();
			_ownerRepository.Dispose();
			_userRepository.Dispose();
			_counterpartyRepository.Dispose();
		}
	}
}
