// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System;
using System.Text;

namespace Tracking.Finance
{
	/// <summary>
	/// The value of a SHA-2 hash with a digest size of 512 bits (64 bytes).
	/// </summary>
	public readonly struct Sha512Value : IEquatable<Sha512Value>
	{
		private const int _hashByteCount = 64;
		private readonly byte[] _bytes;

		/// <summary>
		/// Initializes a new instance of the <see cref="Sha512Value"/> struct.
		/// </summary>
		/// <param name="bytes">The bytes of the hash value.</param>
		/// <exception cref="ArgumentException"><paramref name="bytes"/> length is not 64 bytes.</exception>
		public Sha512Value(byte[] bytes)
		{
			if (bytes.Length != _hashByteCount)
			{
				throw new ArgumentException($"SHA512 hash value must be {_hashByteCount} bytes long", nameof(bytes));
			}

			_bytes = bytes;
		}

		/// <summary>
		/// Implicitly converts a hash value to its binary value.
		/// </summary>
		/// <param name="value">The hash value to convert.</param>
		/// <returns>The binary value of the hash value.</returns>
		public static implicit operator byte[](Sha512Value value) => value._bytes;

		/// <summary>
		/// Indicates whether the two values are equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if values are equal; otherwise <see langword="false"/>.</returns>
		public static bool operator ==(Sha512Value left, Sha512Value right) => left.Equals(right);

		/// <summary>
		/// Indicates whether the two values are not equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true"/> if values are not equal; otherwise <see langword="false"/>.</returns>
		public static bool operator !=(Sha512Value left, Sha512Value right) => !left.Equals(right);

		/// <inheritdoc />
		public bool Equals(Sha512Value other) => SpansAreEqual(_bytes, other._bytes);

		/// <inheritdoc />
		public override bool Equals(object? obj) => obj is Sha512Value other && Equals(other);

		/// <inheritdoc />
		public override int GetHashCode()
		{
			var hash = 17;
			for (var index = _bytes.Length - 1; index >= 0; index--)
			{
				hash ^= 31 + _bytes[index].GetHashCode();
			}

			return hash;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var stringBuilder = new StringBuilder(_bytes.Length * 2);
			for (var index = _bytes.Length - 1; index >= 0; index--)
			{
				stringBuilder.Append(_bytes[index].ToString("x2"));
			}

			return stringBuilder.ToString();
		}

		private static bool SpansAreEqual(ReadOnlySpan<byte> s1, ReadOnlySpan<byte> s2) => s1.SequenceEqual(s2);
	}
}
