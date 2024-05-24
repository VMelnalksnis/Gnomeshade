using System;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Commands;
using Gnomeshade.WebApi.Client;

using NodaTime;

namespace Gnomeshade.Avalonia.Core.Transactions.Transfers;

public sealed partial class PlannedTransferViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	public PlannedTransferViewModel(IActivityService activityService, IGnomeshadeClient gnomeshadeClient)
		: base(activityService)
	{
		_gnomeshadeClient = gnomeshadeClient;
	}
}
