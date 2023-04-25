// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Data.Entities;
using Gnomeshade.Data.Identity;
using Gnomeshade.Data.Repositories;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Gnomeshade.WebApi.Areas.Admin.Pages.Users;

/// <summary>User overview and management page.</summary>
public sealed class Index : PageModel
{
	private readonly UserRepository _userRepository;
	private readonly CounterpartyRepository _counterpartyRepository;
	private readonly UserManager<ApplicationUser> _userManager;

	/// <summary>Initializes a new instance of the <see cref="Index"/> class.</summary>
	/// <param name="userRepository">The repository for performing CRUD operations on <see cref="UserEntity"/>.</param>
	/// <param name="counterpartyRepository">The repository for performing CRUD operations on <see cref="CounterpartyEntity"/>.</param>
	/// <param name="userManager">Application user manager.</param>
	public Index(
		UserRepository userRepository,
		CounterpartyRepository counterpartyRepository,
		UserManager<ApplicationUser> userManager)
	{
		_userRepository = userRepository;
		_counterpartyRepository = counterpartyRepository;
		_userManager = userManager;
	}

	/// <summary>Gets a collection of all users.</summary>
	public IEnumerable<UserData> Users { get; private set; } = Array.Empty<UserData>();

	/// <summary>Handles requests made by the user.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	public async Task OnGet(CancellationToken cancellationToken)
	{
		var userEntities = await _userRepository.Get(cancellationToken);
		var counterparties = (await _counterpartyRepository.GetAllAsync(true, cancellationToken))
			.ToList();
		var identityUsers = await _userManager.Users.ToListAsync(cancellationToken: cancellationToken);

		var users = new List<UserData>();
		foreach (var userEntity in userEntities)
		{
			var counterparty = counterparties.Single(entity => entity.Id == userEntity.CounterpartyId);
			var identityUser = identityUsers.SingleOrDefault(user => user.Id == userEntity.Id.ToString());

			var lockoutEnabled = identityUser is null
				? (bool?)null
				: await _userManager.IsLockedOutAsync(identityUser);
			users.Add(new(userEntity.Id, counterparty.Name, lockoutEnabled, identityUser?.LockoutEnd));
		}

		Users = users;
	}

	/// <summary>Locks out the specified user.</summary>
	/// <param name="id">The id of the user to lock out.</param>
	public async Task<PageResult> OnPostDisable(Guid id) => await SetLockoutEndDateAsync(id, DateTimeOffset.MaxValue);

	/// <summary>Disables lockout for the specified user.</summary>
	/// <param name="id">The id of the user to disable lockout for.</param>
	public async Task<PageResult> OnPostEnable(Guid id) => await SetLockoutEndDateAsync(id, null);

	private async Task<PageResult> SetLockoutEndDateAsync(Guid id, DateTimeOffset? lockoutEnd)
	{
		var identityUser = await _userManager.FindByIdAsync(id.ToString("D", CultureInfo.InvariantCulture));
		if (identityUser is null)
		{
			throw new InvalidOperationException();
		}

		var result = await _userManager.SetLockoutEndDateAsync(identityUser, lockoutEnd);
		if (!result.Succeeded)
		{
			throw new InvalidOperationException();
		}

		await OnGet(default);
		return Page();
	}

	/// <summary>General user information and lockout status.</summary>
	/// <param name="Id">The id of the user.</param>
	/// <param name="Name">The name of the user.</param>
	/// <param name="IsLockedOut">A value indicating whether the user is locked out.</param>
	/// <param name="LockoutEnd">The point in time until which the user is locked out. Only set if <see cref="IsLockedOut"/> is <c>true</c>.</param>
	public sealed record UserData(Guid Id, string Name, bool? IsLockedOut, DateTimeOffset? LockoutEnd);
}
