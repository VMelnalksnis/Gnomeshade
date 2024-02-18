// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Loans;
using Gnomeshade.WebApi.Models.Owners;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Loans;

/// <summary>Creates or updates a single loan.</summary>
public sealed partial class LoanUpsertionViewModel : UpsertionViewModel
{
	/// <summary>Gets a collection of all owners.</summary>
	[Notify(Setter.Private)]
	private List<Owner> _owners = [];

	/// <summary>Gets a collection of all counterparties.</summary>
	[Notify(Setter.Private)]
	private List<Counterparty> _counterparties = [];

	/// <summary>Gets a collection of all currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies = [];

	/// <summary>Gets or sets the name of the loan.</summary>
	[Notify]
	private string? _name;

	/// <summary>Gets or sets the owner of the loan.</summary>
	[Notify]
	private Owner? _owner;

	/// <summary>Gets or sets the issuer of the loan.</summary>
	[Notify]
	private Counterparty? _issuer;

	/// <summary>Gets or sets the receiver of the loan.</summary>
	[Notify]
	private Counterparty? _receiver;

	/// <summary>Gets or sets the amount of capital originally borrowed or invested.</summary>
	[Notify]
	private decimal? _principal;

	/// <summary>Gets or sets the currency of <see cref="Principal"/>.</summary>
	[Notify]
	private Currency? _currency;

	/// <summary>Initializes a new instance of the <see cref="LoanUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="id">The id of the loan to edit.</param>
	public LoanUpsertionViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient, Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		Id = id;
	}

	/// <inheritdoc cref="AutoCompleteSelectors.Counterparty"/>
	public AutoCompleteSelector<object> CounterpartySelector => AutoCompleteSelectors.Counterparty;

	/// <inheritdoc cref="AutoCompleteSelectors.Currency"/>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <inheritdoc cref="AutoCompleteSelectors.Owner"/>
	public AutoCompleteSelector<object> OwnerSelector => AutoCompleteSelectors.Owner;

	/// <inheritdoc />
	public override bool CanSave =>
		!string.IsNullOrWhiteSpace(Name) &&
		Issuer is not null &&
		Receiver is not null &&
		Issuer.Id != Receiver.Id &&
		Principal is not null &&
		Currency is not null;

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var id = Id ?? Guid.NewGuid();
		var loan = new LoanCreation
		{
			OwnerId = Owner?.Id,
			Name = Name!,
			IssuingCounterpartyId = Issuer?.Id,
			ReceivingCounterpartyId = Receiver?.Id,
			Principal = Principal,
			CurrencyId = Currency?.Id,
		};

		await GnomeshadeClient.PutLoanAsync(id, loan);
		return id;
	}

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		Owners = await GnomeshadeClient.GetOwnersAsync();
		Counterparties = await GnomeshadeClient.GetCounterpartiesAsync();
		Currencies = await GnomeshadeClient.GetCurrenciesAsync();

		if (Id is not { } id)
		{
			return;
		}

		var loan = await GnomeshadeClient.GetLoanAsync(id);
		Owner = Owners.Single(owner => owner.Id == loan.OwnerId);
		Name = loan.Name;
		Issuer = Counterparties.Single(counterparty => counterparty.Id == loan.IssuingCounterpartyId);
		Receiver = Counterparties.Single(counterparty => counterparty.Id == loan.ReceivingCounterpartyId);
		Principal = loan.Principal;
		Currency = Currencies.Single(currency => currency.Id == loan.CurrencyId);
	}
}
