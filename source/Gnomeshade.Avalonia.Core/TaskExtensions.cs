// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Threading.Tasks;

namespace Gnomeshade.Avalonia.Core;

internal static class TaskExtensions
{
	internal static async Task<(T1 Result1, T2 Result2)> WhenAll<T1, T2>(
		this (Task<T1> Task1, Task<T2> Task2) tasks)
	{
		var (task1, task2) = tasks;
		await Task.WhenAll(task1, task2);
		return (task1.Result, task2.Result);
	}

	internal static async Task<(T1 Result1, T2 Result2, T3 Result3)> WhenAll<T1, T2, T3>(
		this (Task<T1> Task1, Task<T2> Task2, Task<T3> Task3) tasks)
	{
		var (task1, task2, task3) = tasks;
		await Task.WhenAll(task1, task2, task3);
		return (task1.Result, task2.Result, task3.Result);
	}

	internal static async Task<(T1 Result1, T2 Result2, T3 Result3, T4 Result4)> WhenAll<T1, T2, T3, T4>(
		this (Task<T1> Task1, Task<T2> Task2, Task<T3> Task3, Task<T4> Task4) tasks)
	{
		var (task1, task2, task3, task4) = tasks;
		await Task.WhenAll(task1, task2, task3, task4);
		return (task1.Result, task2.Result, task3.Result, task4.Result);
	}
}
