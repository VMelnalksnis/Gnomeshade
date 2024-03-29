﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Data.Common;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;

namespace Gnomeshade.Data;

/// <summary>User related actions spanning multiple entities.</summary>
public sealed class UserUnitOfWork
{
	private readonly DbConnection _dbConnection;
	private readonly OwnerRepository _ownerRepository;
	private readonly OwnershipRepository _ownershipRepository;
	private readonly UserRepository _userRepository;
	private readonly CounterpartyRepository _counterpartyRepository;

	/// <summary>Initializes a new instance of the <see cref="UserUnitOfWork"/> class.</summary>
	/// <param name="dbConnection">The database connection for executing queries.</param>
	/// <param name="ownerRepository">The repository for managing owners.</param>
	/// <param name="ownershipRepository">The repository for managing ownerships.</param>
	/// <param name="userRepository">The repository for managing users.</param>
	/// <param name="counterpartyRepository">The repository for managing counterparties.</param>
	public UserUnitOfWork(
		DbConnection dbConnection,
		OwnerRepository ownerRepository,
		OwnershipRepository ownershipRepository,
		UserRepository userRepository,
		CounterpartyRepository counterpartyRepository)
	{
		_dbConnection = dbConnection;
		_ownerRepository = ownerRepository;
		_ownershipRepository = ownershipRepository;
		_userRepository = userRepository;
		_counterpartyRepository = counterpartyRepository;
	}

	/// <summary>Creates a new application user for the specified identity user.</summary>
	/// <param name="applicationUser">The identity user for which to create a new application user.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task CreateUserAsync(ApplicationUser applicationUser)
	{
		var userId = applicationUser.Id;
		var fullName = applicationUser.FullName;
		var user = new UserEntity { Id = userId, ModifiedByUserId = userId };

		await using var dbTransaction = await _dbConnection.OpenAndBeginTransaction();

		_ = await _userRepository.AddWithIdAsync(user, dbTransaction);
		await _ownerRepository.AddDefaultAsync(userId, dbTransaction);
		await _ownershipRepository.AddDefaultAsync(userId, dbTransaction);
		var counterparty = new CounterpartyEntity
		{
			Id = userId,
			OwnerId = userId,
			CreatedByUserId = userId,
			ModifiedByUserId = userId,
			Name = fullName,
		};
		var counterpartyId = await _counterpartyRepository.AddAsync(counterparty, dbTransaction);
		user.CounterpartyId = counterpartyId;
		_ = await _userRepository.UpdateAsync(user, dbTransaction);

		await dbTransaction.CommitAsync();
	}
}
