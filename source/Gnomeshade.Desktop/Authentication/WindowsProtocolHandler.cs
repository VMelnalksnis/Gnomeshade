// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

using Gnomeshade.Avalonia.Core.Authentication;

namespace Gnomeshade.Desktop.Authentication;

/// <inheritdoc />
public sealed class WindowsProtocolHandler : IGnomeshadeProtocolHandler
{
	internal const string Name = "gnomeshade";

	/// <inheritdoc />
	public async Task<string> GetRequestContent(CancellationToken cancellationToken = default)
	{
		await using var pipeServer = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
		await pipeServer.WaitForConnectionAsync(cancellationToken);

		var streamReader = new StreamReader(pipeServer);
		var value = await streamReader.ReadToEndAsync();

		var uri = new Uri(value, UriKind.Absolute);
		return uri.Query;
	}
}
