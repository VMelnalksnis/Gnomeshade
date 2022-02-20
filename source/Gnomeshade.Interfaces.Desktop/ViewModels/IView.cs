// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using JetBrains.Annotations;

using static JetBrains.Annotations.ImplicitUseKindFlags;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

/// <summary>Marker interface for matching views with view models.</summary>
/// <typeparam name="TViewModel">The view model for this view.</typeparam>
public interface IView<[UsedImplicitly(InstantiatedNoFixedConstructorSignature)] TViewModel>
{
}
