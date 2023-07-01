// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Avalonia.Core.Commands;

/// <summary>A synchronous command that always can execute.</summary>
public sealed class Command : CommandBase
{
	private readonly ILogger<Command> _logger;
	private readonly IActivityService _activityService;
	private readonly Action _execute;
	private readonly Func<bool> _canExecute;
	private readonly string _activity;

	internal Command(
		ILogger<Command> logger,
		IActivityService activityService,
		Action execute,
		Func<bool> canExecute,
		string activity)
	{
		_logger = logger;
		_activityService = activityService;
		_execute = execute;
		_canExecute = canExecute;
		_activity = activity;
	}

	internal Command(ILogger<Command> logger, IActivityService activityService, Action execute, string activity)
		: this(logger, activityService, execute, () => true, activity)
	{
	}

	/// <inheritdoc />
	public override bool CanExecute(object? parameter) => _canExecute();

	/// <inheritdoc />
	public override void Execute(object? parameter)
	{
		try
		{
			using var activity = _activityService.BeginActivity(_activity);
			_execute();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unexpected error in command");
			_activityService.ShowErrorNotification(exception.Message);
		}
	}
}
