// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

using Avalonia.Controls;

using JetBrains.Annotations;

using static System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes;

using static JetBrains.Annotations.ImplicitUseKindFlags;

namespace Gnomeshade.Avalonia.Core;

/// <summary>Marker interface for matching views with view models.</summary>
/// <typeparam name="TView">The view type.</typeparam>
/// <typeparam name="TViewModel">The view model for this view.</typeparam>
public interface IView<
	[UsedImplicitly(InstantiatedNoFixedConstructorSignature), DynamicallyAccessedMembers(PublicConstructors)] out TView,
	[UsedImplicitly(InstantiatedNoFixedConstructorSignature), DynamicallyAccessedMembers(PublicConstructors)] out TViewModel>
	where TView : Control
	where TViewModel : ViewModelBase
{
}
