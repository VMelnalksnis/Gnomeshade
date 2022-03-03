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
	private readonly ITagClient _tagClient;

	private DataGridItemCollectionView<TagRow> _tagDataGridView;
	private TagRow? _selectedTag;
	private TagCreationViewModel _tag;

	private TagViewModel(ITagClient tagClient, IEnumerable<TagRow> tagRows, TagCreationViewModel tagCreationViewModel)
	{
		_tagClient = tagClient;
		_tag = tagCreationViewModel;
		Tag.TagCreated += OnTagCreated;

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
			Tag.TagCreated -= OnTagCreated;
			SetAndNotify(ref _tag, value);
			Tag.TagCreated += OnTagCreated;
		}
	}

	/// <summary>Asynchronously creates a new instance of the <see cref="TagViewModel"/> class.</summary>
	/// <param name="tagClient">API client for getting tag data.</param>
	/// <returns>A new instance of the <see cref="TagViewModel"/> class.</returns>
	public static async Task<TagViewModel> CreateAsync(ITagClient tagClient)
	{
		var tags = await tagClient.GetTagsAsync();
		var tagRows = tags.Select(tag => new TagRow(tag.Id, tag.Name, tag.Description));
		var tagCreationViewModel = await TagCreationViewModel.CreateAsync(tagClient);
		return new(tagClient, tagRows, tagCreationViewModel);
	}

	private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is nameof(SelectedTag))
		{
			Tag = Task.Run(() => TagCreationViewModel.CreateAsync(_tagClient, SelectedTag?.Id)).Result;
		}
	}

	private void OnTagCreated(object? sender, TagCreatedEventArgs e)
	{
		var tags = _tagClient.GetTagsAsync().Result;
		var tagRows = tags.Select(tag => new TagRow(tag.Id, tag.Name, tag.Description));

		var sortDescriptions = DataGridView.SortDescriptions;

		TagDataGridView = new(tagRows);
		DataGridView.SortDescriptions.AddRange(sortDescriptions);
		Tag = Task.Run(() => TagCreationViewModel.CreateAsync(_tagClient)).Result;
	}
}
