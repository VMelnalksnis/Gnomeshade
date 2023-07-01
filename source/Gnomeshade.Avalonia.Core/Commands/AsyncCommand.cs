// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

// Same type, just generic
#pragma warning disable SA1402

namespace Gnomeshade.Avalonia.Core.Commands;

/// <summary>An async command.</summary>
public sealed class AsyncCommand : CommandBase
{
	private readonly ILogger<AsyncCommand> _logger;
	private readonly IActivityService _activityService;
	private readonly Func<Task> _execute;
	private readonly Func<bool> _canExecute;
	private readonly string _activity;

	internal AsyncCommand(
		ILogger<AsyncCommand> logger,
		IActivityService activityService,
		Func<Task> execute,
		Func<bool> canExecute,
		string activity)
	{
		_logger = logger;
		_activityService = activityService;
		_execute = execute;
		_canExecute = canExecute;
		_activity = activity;
	}

	/// <inheritdoc />
	public override bool CanExecute(object? parameter) => _canExecute();

	/// <inheritdoc />
	public override async void Execute(object? parameter)
	{
		try
		{
			using var activity = _activityService.BeginActivity(_activity);
			await _execute();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unexpected error in command");
			_activityService.ShowErrorNotification(exception.Message);
		}
	}
}

/// <summary>An async command which expects the parameter to be of type <typeparamref name="T"/>.</summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public sealed class AsyncCommand<T> : CommandBase
{
	private readonly ILogger<AsyncCommand<T>> _logger;
	private readonly IActivityService _activityService;
	private readonly Func<T, Task> _execute;
	private readonly string _activity;

	internal AsyncCommand(
		ILogger<AsyncCommand<T>> logger,
		IActivityService activityService,
		Func<T, Task> execute,
		string activity)
	{
		_logger = logger;
		_activityService = activityService;
		_execute = execute;
		_activity = activity;
	}

	/// <inheritdoc />
	public override bool CanExecute(object? parameter) => true;

	/// <inheritdoc />
	public override async void Execute(object? parameter)
	{
		try
		{
			var expected = GetParameter(parameter);

			using var activity = _activityService.BeginActivity(_activity);
			await _execute(expected);
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Unexpected error in command");
			_activityService.ShowErrorNotification(exception.Message);
		}
	}

	private static T GetParameter(object? parameter)
	{
		if (parameter is T expected)
		{
			return expected;
		}

		throw new ArgumentException(
			$"Expected parameter to be of type {typeof(T).Name}, but was {parameter?.GetType().Name}",
			nameof(parameter));
	}
}
