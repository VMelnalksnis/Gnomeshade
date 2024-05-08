// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;

using Microsoft.Extensions.Logging;

namespace Gnomeshade.Desktop.Authentication;

/// <inheritdoc />
public sealed class GnomeshadeProtocolHandler : IGnomeshadeProtocolHandler
{
	internal const string Name = "gnomeshade";

	private readonly ILogger<GnomeshadeProtocolHandler> _logger;

	/// <summary>Initializes a new instance of the <see cref="GnomeshadeProtocolHandler"/> class.</summary>
	/// <param name="logger">Logger for logging in the specified category.</param>
	public GnomeshadeProtocolHandler(ILogger<GnomeshadeProtocolHandler> logger)
	{
		_logger = logger;
	}

	/// <inheritdoc />
	public async Task<string> GetRequestContent(CancellationToken cancellationToken = default)
	{
		_logger.LogDebug("Starting named pipe server");
		await using var pipeServer = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
		await pipeServer.WaitForConnectionAsync(cancellationToken);

		_logger.LogInformation("Connection with client established, reading content");
		var streamReader = new StreamReader(pipeServer);
		var value = await streamReader.ReadToEndAsync(cancellationToken);

		_logger.LogDebug("Received content from named pipe");
		var uri = new Uri(value, UriKind.Absolute);
		return uri.Query;
	}
}
