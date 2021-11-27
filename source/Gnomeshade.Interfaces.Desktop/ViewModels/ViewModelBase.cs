﻿// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using Avalonia.Controls;

using Gnomeshade.Interfaces.Desktop.ViewModels.Binding;

using JetBrains.Annotations;

namespace Gnomeshade.Interfaces.Desktop.ViewModels;

public abstract class ViewModelBase : PropertyChangedBase
{
}

public abstract class ViewModelBase<[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)] TView>
	: ViewModelBase
	where TView : Control
{
}
