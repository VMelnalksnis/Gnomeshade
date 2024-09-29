// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

using Gnomeshade.WebApi.Client;
using Gnomeshade.WebApi.Models.Transactions;

using NodaTime;

using PropertyChanged.SourceGenerator;

namespace Gnomeshade.Avalonia.Core.Transactions;

/// <summary>Creates or updates <see cref="TransactionSchedule"/>.</summary>
public sealed partial class TransactionScheduleUpsertionViewModel : UpsertionViewModel
{
	private readonly IDialogService _dialogService;
	private readonly IDateTimeZoneProvider _dateTimeZoneProvider;

	/// <inheritdoc cref="TransactionSchedule.Name"/>
	[Notify]
	private string? _name;

	/// <inheritdoc cref="TransactionSchedule.StartingAt"/>
	[Notify]
	private LocalDateTime? _startingAt;

	/// <inheritdoc cref="TransactionSchedule.Period"/>
	[Notify]
	private Period? _period;

	/// <inheritdoc cref="TransactionSchedule.Count"/>
	[Notify]
	private int? _count;

	/// <summary>Gets or sets the transaction to use as a template for all planned transactions for the schedule.</summary>
	[Notify]
	private PlannedTransactionUpsertionViewModel? _templateTransaction;

	/// <summary>Initializes a new instance of the <see cref="TransactionScheduleUpsertionViewModel"/> class.</summary>
	/// <param name="activityService">Service for indicating the activity of the application to the user.</param>
	/// <param name="gnomeshadeClient">A strongly typed API client.</param>
	/// <param name="dialogService">Service for creating dialog windows.</param>
	/// <param name="dateTimeZoneProvider">Time zone provider for localizing instants to local time.</param>
	/// <param name="id">The id of the transaction schedule to edit.</param>
	public TransactionScheduleUpsertionViewModel(
		IActivityService activityService,
		IGnomeshadeClient gnomeshadeClient,
		IDialogService dialogService,
		IDateTimeZoneProvider dateTimeZoneProvider,
		Guid? id)
		: base(activityService, gnomeshadeClient)
	{
		_dialogService = dialogService;
		_dateTimeZoneProvider = dateTimeZoneProvider;
		Id = id;
	}

	/// <inheritdoc />
	public override bool CanSave =>
		!string.IsNullOrWhiteSpace(Name) &&
		StartingAt is not null &&
		Period is not null &&
		Count is not null;

	/// <inheritdoc />
	protected override async Task Refresh()
	{
		if (Id is not { } id)
		{
			TemplateTransaction = null;
			return;
		}

		var schedule = await GnomeshadeClient.GetTransactionSchedule(id);
		var transactions = await GnomeshadeClient.GetPlannedTransactions();
		var timeZone = _dateTimeZoneProvider.GetSystemDefault();

		Name = schedule.Name;
		StartingAt = schedule.StartingAt.InZone(timeZone).LocalDateTime;
		Period = schedule.Period;
		Count = schedule.Count;
		TemplateTransaction = transactions.FirstOrDefault() is { } transaction
			? new PlannedTransactionUpsertionViewModel(ActivityService, GnomeshadeClient, _dialogService, _dateTimeZoneProvider, transaction.Id)
			: null;

		if (TemplateTransaction is not null)
		{
			await TemplateTransaction.RefreshAsync();
		}
	}

	/// <inheritdoc />
	protected override async Task<Guid> SaveValidatedAsync()
	{
		var timeZone = _dateTimeZoneProvider.GetSystemDefault();
		var schedule = new TransactionScheduleCreation
		{
			Name = Name!,
			StartingAt = StartingAt!.Value.InZoneStrictly(timeZone).ToInstant(),
			Period = Period!,
			Count = Count!.Value,
		};

		if (Id is { } existingId)
		{
			await GnomeshadeClient.PutTransactionSchedule(existingId, schedule);
		}
		else
		{
			Id = await GnomeshadeClient.CreateTransactionSchedule(schedule);
		}

		return Id.Value;
	}
}
