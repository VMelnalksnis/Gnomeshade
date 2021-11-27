// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Gnomeshade.Core;

public static class ObjectExtensions
{
	/// <summary>
	/// Computes a <see cref="SHA512"/> hash value for the specified <paramref name="@object"/> of type <typeparamref name="T"/>.
	/// </summary>
	/// <param name="object">The object for which to compute the hash value for.</param>
	/// <typeparam name="T">The type of the object for which to compute the hash value for.</typeparam>
	/// <returns>The computed <see cref="SHA512"/> hash value.</returns>
	public static Sha512Value GetHash<T>(this T @object)
		where T : class
	{
		using var stream = new MemoryStream();
		var serializer = new XmlSerializer(typeof(T));
		serializer.Serialize(stream, @object);
		stream.Position = 0;

		using var sha512 = SHA512.Create();
		var hashBytes = sha512.ComputeHash(stream);
		return new(hashBytes);
	}

	/// <summary>
	/// Asynchronously computes a <see cref="SHA512"/> hash value for the specified <paramref name="@object"/> of type <typeparamref name="T"/>.
	/// </summary>
	/// <param name="object">The object for which to compute the hash value for.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <typeparam name="T">The type of the object for which to compute the hash value for.</typeparam>
	/// <returns>A task that represents the asynchronous hash value computation operation and wraps the computed <see cref="SHA512"/> hash value.</returns>
	public static async Task<Sha512Value> GetHashAsync<T>(this T @object, CancellationToken cancellationToken = default)
		where T : class
	{
		await using var stream = new MemoryStream();
		var serializer = new XmlSerializer(typeof(T));
		serializer.Serialize(stream, @object);
		stream.Position = 0;

		using var sha512 = SHA512.Create();
		var hashBytes = await sha512.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
		return new(hashBytes);
	}
}
