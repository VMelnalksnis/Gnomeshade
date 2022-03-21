// Copyright 2021 Valters Melnalksnis
// Licensed under the GNU Affero General Public License v3.0 or later.
// See LICENSE.txt file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Collections;

using Gnomeshade.Interfaces.WebApi.Client;

namespace Gnomeshade.Interfaces.Avalonia.Core.Tags;

/// <summary>An overview of all tags.</summary>
public sealed class TagViewModel : ViewModelBase
{
	private readonly IGnomeshadeClient _gnomeshadeClient;

	private DataGridItemCollectionView<TagRow> _tagDataGridView;
	private TagRow? _selectedTag;
	private TagCreationViewModel _tag;

	private TagViewModel(IGnomeshadeClient gnomeshadeClient, IEnumerable<TagRow> tagRows, TagCreationViewModel tagCreationViewModel)
	{
		_gnomeshadeClient = gnomeshadeClient;
		_tag = tagCreationViewModel;
		Tag.Upserted += OnTagUpserted;

		_tagDataGridView = new(tagRows);
		PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Gets a grid view of all tags.</summary>
	public DataGridCollectionView DataGridView => TagDataGridView;

	/// <summary>Gets a typed collection of tags in <see cref="DataGridView"/>.</summary>
	public DataGridItemCollectionView<TagRow> TagDataGridView
	{
		get => _tagDataGridView;
		private set =>
			SetAndNotifyWithGuard(ref _tagDataGridView, value, nameof(TagDataGridView), nameof(DataGridView));
	}

	/// <summary>Gets or sets the currently selected tag from <see cref="TagDataGridView"/>.</summary>
	public TagRow? SelectedTag
	{
		get => _selectedTag;
		set => SetAndNotify(ref _selectedTag, value);
	}

	/// <summary>Gets the current tag creation view model.</summary>
	public TagCreationViewModel Tag
	{
		get => _tag;
		private set
		{
			Tag.Upserted -= OnTagUpserted;
			SetAndNotify(ref _tag, value);
			Tag.Upserted += OnTagUpserted;
		}
	}

	/// <summary>Asynchronously creates a new instance of the <see cref="TagViewModel"/> class.</summary>
	/// <param name="gnomeshadeClient">API client for getting tag data.</param>
	/// <returns>A new instance of the <see cref="TagViewModel"/> class.</returns>
	public static async Task<TagViewModel> CreateAsync(IGnomeshadeClient gnomeshadeClient)
	{
		var tags = await gnomeshadeClient.GetTagsAsync();
		var tagRows = tags.Select(tag => new TagRow(tag.Id, tag.Name, tag.Description));
		var tagCreationViewModel = await TagCreationViewModel.CreateAsync(gnomeshadeClient);
		return new(gnomeshadeClient, tagRows, tagCreationViewModel);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedTag))
		{
			Tag = Task.Run(() => TagCreationViewModel.CreateAsync(_gnomeshadeClient, SelectedTag?.Id)).Result;
		}
	}

	private void OnTagUpserted(object? sender, UpsertedEventArgs e)
	{
		var tags = _gnomeshadeClient.GetTagsAsync().Result;
		var tagRows = tags.Select(tag => new TagRow(tag.Id, tag.Name, tag.Description));

		var sortDescriptions = DataGridView.SortDescriptions;

		TagDataGridView = new(tagRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		Tag = Task.Run(() => TagCreationViewModel.CreateAsync(_gnomeshadeClient)).Result;
	}
}
