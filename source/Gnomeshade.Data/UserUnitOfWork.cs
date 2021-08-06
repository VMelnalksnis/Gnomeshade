// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

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

		public UserUnitOfWork(IDbConnection dbConnection)
		{
			_dbConnection = dbConnection;
			_ownerRepository = new(dbConnection);
			_ownershipRepository = new(dbConnection);
			_userRepository = new(dbConnection);
			_counterpartyRepository = new(dbConnection);
		}

		public async Task CreateUser(Guid identityId, string fullName)
		{
			if (!_dbConnection.State.HasFlag(ConnectionState.Open))
			{
				_dbConnection.Open();
			}

			var applicationUser = new User { Id = identityId, ModifiedByUserId = identityId };
			using var dbTransaction = _dbConnection.BeginTransaction();

			try
			{
				_ = await _userRepository.AddWithIdAsync(applicationUser, dbTransaction);
				_ = await _ownerRepository.AddAsync(identityId, dbTransaction);
				_ = await _ownershipRepository.AddDefaultAsync(identityId, dbTransaction);
				var counterparty = new Counterparty
				{
					OwnerId = identityId,
					CreatedByUserId = identityId,
					ModifiedByUserId = identityId,
					Name = fullName,
					NormalizedName = fullName.ToUpperInvariant(),
				};
				var counterpartyId = await _counterpartyRepository.AddAsync(counterparty, dbTransaction);
				_ = await _userRepository.AddCounterparty(identityId, counterpartyId, dbTransaction);

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
