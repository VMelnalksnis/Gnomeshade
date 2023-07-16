// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Avalonia.Core.Commands;

/// <inheritdoc />
public sealed class CommandFactory : ICommandFactory
{
	private readonly ILoggerFactory _loggerFactory;
	private readonly IActivityService _activityService;

	/// <summary>Initializes a new instance of the <see cref="CommandFactory"/> class.</summary>
	/// <param name="loggerFactory">Logger factory for creating loggers for each command.</param>
	/// <param name="activityService">Activity service for commands.</param>
	public CommandFactory(ILoggerFactory loggerFactory, IActivityService activityService)
	{
		_loggerFactory = loggerFactory;
		_activityService = activityService;
	}

	/// <inheritdoc />
	public CommandBase Create(Action execute, Func<bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<Command>();
		return new Command(logger, _activityService, execute, canExecute, activity);
	}

	/// <inheritdoc />
	public CommandBase Create(Func<Task> execute, Func<bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<AsyncCommand>();
		return new AsyncCommand(logger, _activityService, execute, canExecute, activity);
	}

	/// <inheritdoc />
	public CommandBase Create<T>(Func<T, Task> execute, string activity) => Create(execute, static _ => true, activity);

	/// <inheritdoc />
	public CommandBase Create<T>(Func<T, Task> execute, Func<T, bool> canExecute, string activity)
	{
		var logger = _loggerFactory.CreateLogger<AsyncCommand<T>>();
		return new AsyncCommand<T>(logger, _activityService, execute, canExecute, activity);
	}
}
