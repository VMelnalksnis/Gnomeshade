// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core;

[SuppressMessage(
	"StyleCop.CSharp.ReadabilityRules",
	"SA1414:Tuple types in signatures should have element names",
	Justification = "Names don't provide any value")]
internal static class TaskExtensions
{
	internal static async Task<(T1, T2)> WhenAll<T1, T2>(
		this (Task<T1>, Task<T2>) tasks)
	{
		var (t1, t2) = tasks;
		await Task.WhenAll(t1, t2);
		return (t1.Result, t2.Result);
	}

	internal static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(
		this (Task<T1>, Task<T2>, Task<T3>) tasks)
	{
		var (t1, t2, t3) = tasks;
		await Task.WhenAll(t1, t2, t3);
		return (t1.Result, t2.Result, t3.Result);
	}

	internal static async Task<(T1, T2, T3, T4)> WhenAll<T1, T2, T3, T4>(
		this (Task<T1>, Task<T2>, Task<T3>, Task<T4>) tasks)
	{
		var (t1, t2, t3, t4) = tasks;
		await Task.WhenAll(t1, t2, t3, t4);
		return (t1.Result, t2.Result, t3.Result, t4.Result);
	}

	internal static async Task<(T1, T2, T3, T4, T5)> WhenAll<T1, T2, T3, T4, T5>(
		this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>) tasks)
	{
		var (t1, t2, t3, t4, t5) = tasks;
		await Task.WhenAll(t1, t2, t3, t4, t5);
		return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result);
	}

	internal static async Task<(T1, T2, T3, T4, T5, T6)> WhenAll<T1, T2, T3, T4, T5, T6>(
		this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>) tasks)
	{
		var (t1, t2, t3, t4, t5, t6) = tasks;
		await Task.WhenAll(t1, t2, t3, t4, t5, t6);
		return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result);
	}

	internal static async Task<(T1, T2, T3, T4, T5, T6, T7)> WhenAll<T1, T2, T3, T4, T5, T6, T7>(
		this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>) tasks)
	{
		var (t1, t2, t3, t4, t5, t6, t7) = tasks;
		await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7);
		return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result);
	}

	internal static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAll<T1, T2, T3, T4, T5, T6, T7, T8>(
		this (Task<T1>, Task<T2>, Task<T3>, Task<T4>, Task<T5>, Task<T6>, Task<T7>, Task<T8>) tasks)
	{
		var (t1, t2, t3, t4, t5, t6, t7, t8) = tasks;
		await Task.WhenAll(t1, t2, t3, t4, t5, t6, t7, t8);
		return (t1.Result, t2.Result, t3.Result, t4.Result, t5.Result, t6.Result, t7.Result, t8.Result);
	}
}
