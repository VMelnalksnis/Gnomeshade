// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;

using Gnomeshade.Avalonia.Core.Commands;
using Gnomeshade.Avalonia.Core.Products;
using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Accounts;
using Gnomeshade.WebApi.Models.Products;
using Gnomeshade.WebApi.Models.Projects;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions.Purchases;

/// <summary>Create or update a purchase.</summary>
public sealed partial class PlannedPurchaseUpsertionViewModel : UpsertionViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
	private readonly Guid _transactionId;

	private PlannedPurchase? _purchase;

	/// <summary>Gets or sets the amount paid to purchase an <see cref="Amount"/> of <see cref="Product"/>.</summary>
	[Notify]
	private decimal? _price;

	/// <summary>Gets or sets the id of the currency of <see cref="Price"/>.</summary>
	[Notify]
	private Currency? _currency;

	/// <summary>Gets or sets the id of the purchased product.</summary>
	[Notify]
	private Product? _product;

	/// <summary>Gets or sets the amount of <see cref="Product"/> that was purchased.</summary>
	[Notify]
	private decimal? _amount;

	/// <summary>Gets or sets the project that this purchase is a part of.</summary>
	[Notify]
	private Project? _project;

	/// <summary>Gets a collection of all currencies.</summary>
	[Notify(Setter.Private)]
	private List<Currency> _currencies = [];

	/// <summary>Gets a collection of all products.</summary>
	[Notify(Setter.Private)]
	private List<Product> _products = [];

	/// <summary>Gets a collection of all projects.</summary>
	[Notify(Setter.Private)]
	private List<Project> _projects = [];

	/// <summary>Gets the name of the unit of the <see cref="Product"/>.</summary>
	[Notify(Setter.Private)]
	private string? _unitName;

	/// <summary>Gets or sets the order of the purchase within a transaction.</summary>
	[Notify]
	private uint? _order;

	/// <summary>Initializes a new instance of the <see cref="PlannedPurchaseUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">Gnomeshade API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="transactionId">The id of the transaction to which to add the purchase to.</param>
	/// <param name="id">The id of the purchase to edit.</param>
	public PlannedPurchaseUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid transactionId,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		_transactionId = transactionId;
		Id = id;

		CreateProduct = activityService.Create<Window>(ShowNewProductDialog, "Waiting for product creation");
		PropertyChanged += OnPropertyChanged;
	}

	/// <inheritdoc cref="AutoCompleteSelectors.Currency"/>
	public AutoCompleteSelector<object> CurrencySelector => AutoCompleteSelectors.Currency;

	/// <inheritdoc cref="AutoCompleteSelectors.Product"/>
	public AutoCompleteSelector<object> ProductSelector => AutoCompleteSelectors.Product;

	/// <inheritdoc cref="AutoCompleteSelectors.Project"/>
	public AutoCompleteSelector<object> ProjectSelector => AutoCompleteSelectors.Project;

	/// <inheritdoc />
	public override bool CanSave =>
		Price is not null &&
		Currency is not null &&
		Product is not null &&
		Amount is not null;

	/// <summary>Gets a command for showing a modal dialog for creating a new product.</summary>
	public CommandBase CreateProduct { get; }

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		var (currencies, products, projects) = await (
			GnomeshadeClient.GetCurrenciesAsync(),
			GnomeshadeClient.GetProductsAsync(),
			GnomeshadeClient.GetProjectsAsync())
			.WhenAll();

		Currencies = currencies;
		Products = products;
		Projects = projects;

		if (Id is not { } purchaseId)
		{
			return;
		}

		_purchase = await GnomeshadeClient.GetPlannedPurchase(purchaseId);

		Price = _purchase.Price;
		Currency = Currencies.Single(currency => currency.Id == _purchase.CurrencyId);
		Amount = _purchase.Amount;
		Product = Products.Single(product => product.Id == _purchase.ProductId);
		Order = _purchase.Order;
		Project = _purchase.ProjectIds switch
		{
			[] => null,
			[var projectId] => Projects.Single(project => project.Id == projectId),
			_ => throw new NotSupportedException("Purchases that are a part of multiple projects are not supported"),
		};
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var purchaseCreation = new PlannedPurchaseCreation
		{
			TransactionId = _transactionId,
			Price = Price,
			CurrencyId = Currency!.Id,
			Amount = Amount,
			ProductId = Product!.Id,
			Order = Order,
		};

		if (Id is { } existingId)
		{
			await GnomeshadeClient.PutPlannedPurchase(existingId, purchaseCreation);
		}
		else
		{
			Id = await GnomeshadeClient.CreatePlannedPurchase(purchaseCreation);
		}

		if (_purchase?.ProjectIds is [var projectId])
		{
			if (Project is null || Project.Id != projectId)
			{
				await GnomeshadeClient.RemovePurchaseFromProjectAsync(projectId, Id.Value);
			}

			if (Project is not null && Project.Id != projectId)
			{
				await GnomeshadeClient.AddPurchaseToProjectAsync(Project.Id, Id.Value);
			}
		}
		else if (Project is not null)
		{
			await GnomeshadeClient.AddPurchaseToProjectAsync(Project.Id, Id.Value);
		}

		return Id.Value;
	}

	private async Task ShowNewProductDialog(Window window)
	{
		var viewModel = new ProductUpsertionViewModel(ActivityService, GnomeshadeClient, _dateTimeZoneProvider, null);
		await viewModel.RefreshAsync();

		var result = await _dialogService.ShowDialogValue<ProductUpsertionViewModel, Guid>(window, viewModel, dialog =>
		{
			dialog.Title = "Create product";
			viewModel.Upserted += (_, args) => dialog.Close(args.Id);
		});

		await RefreshAsync();
		if (result is not { } createdId)
		{
			return;
		}

		Product = Products.SingleOrDefault(product => product.Id == createdId);
	}

	private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(Product))
		{
			UnitName = Product?.UnitId is null
				? null
				: (await GnomeshadeClient.GetUnitAsync(Product.UnitId.Value)).Name;
		}
	}
}
