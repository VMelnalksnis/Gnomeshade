// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core.Authentication;

/// <summary>Handles requests made in the gnomeshade protocol.</summary>
public interface IGnomeshadeProtocolHandler
{
	/// <summary>Gets the content of a gnomeshade protocol request.</summary>
	/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
	/// <returns>The request content.</returns>
	Task<string> GetRequestContent(CancellationToken cancellationToken = default);
}
