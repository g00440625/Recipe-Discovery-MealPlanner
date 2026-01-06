using System.Collections.ObjectModel;
using MealPlanner.Models;

namespace MealPlanner.Pages;

public partial class RecipeBrowserPage : ContentPage
{
	private ObservableCollection<Recipe> _recipes;
	private bool _isSearchTabActive = true;

	public RecipeBrowserPage()
	{
		InitializeComponent();
		_recipes = new ObservableCollection<Recipe>();
		RecipeCollectionView.ItemsSource = _recipes;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// If on favorites tab, reload favorites
		if (!_isSearchTabActive)
		{
			await LoadFavoritesAsync();
		}
	}

	// === TAB SWITCHING ===

	private void OnSearchTabClicked(object sender, EventArgs e)
	{
		if (_isSearchTabActive) return;

		_isSearchTabActive = true;

		// Update tab appearance
		SearchTabBorder.BackgroundColor = Color.FromArgb("#4CAF50");
		FavoritesTabBorder.BackgroundColor = Colors.White;

		// Show/hide sections
		SearchSection.IsVisible = true;

		// Clear recipes and reset message
		_recipes.Clear();
		ResultLabel.Text = "Enter a search term to find recipes";
		EmptyIcon.Text = "??";
		EmptyTitle.Text = "Search for recipes";
		EmptyMessage.Text = "Use the search bar above";
	}

	private async void OnFavoritesTabClicked(object sender, EventArgs e)
	{
		if (!_isSearchTabActive) return;

		_isSearchTabActive = false;

		// Update tab appearance
		SearchTabBorder.BackgroundColor = Colors.White;
		FavoritesTabBorder.BackgroundColor = Color.FromArgb("#E91E63");

		// Hide search section
		SearchSection.IsVisible = false;

		// Load favorites
		await LoadFavoritesAsync();
	}

	// === SEARCH FUNCTIONALITY ===

	private async void OnSearchPressed(object sender, EventArgs e)
	{
		var searchTerm = SearchBar.Text?.Trim();

		if (string.IsNullOrWhiteSpace(searchTerm))
		{
			await DisplayAlert("Search", "Please enter a search term", "OK");
			return;
		}

		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;
		ResultLabel.Text = "Searching...";

		try
		{
			var recipes = await App.CacheService.SearchRecipesAsync(searchTerm);

			_recipes.Clear();
			foreach (var recipe in recipes)
			{
				_recipes.Add(recipe);
			}

			ResultLabel.Text = $"Found {recipes.Count} recipes";

			if (recipes.Count == 0)
			{
				EmptyIcon.Text = "??";
				EmptyTitle.Text = "No recipes found";
				EmptyMessage.Text = $"No results for '{searchTerm}'";
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to search recipes: {ex.Message}", "OK");
			ResultLabel.Text = "Search failed";
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
		}
	}

	private async void OnRandomClicked(object sender, EventArgs e)
	{
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;
		ResultLabel.Text = "Getting random recipe...";

		try
		{
			var recipe = await App.ApiService.GetRandomRecipe();

			if (recipe != null)
			{
				await Navigation.PushAsync(new RecipeDetailPage(recipe));
			}
			else
			{
				await DisplayAlert("Error", "Could not fetch random recipe", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
			ResultLabel.Text = "Enter a search term to find recipes";
		}
	}

	// === FAVORITES FUNCTIONALITY ===

	private async Task LoadFavoritesAsync()
	{
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;
		ResultLabel.Text = "Loading favorites...";

		try
		{
			var favorites = await App.StorageService.LoadFavorites();

			_recipes.Clear();
			foreach (var recipe in favorites)
			{
				_recipes.Add(recipe);
			}

			ResultLabel.Text = $"{favorites.Count} favorite recipe{(favorites.Count == 1 ? "" : "s")}";

			if (favorites.Count == 0)
			{
				EmptyIcon.Text = "?";
				EmptyTitle.Text = "No favorites yet";
				EmptyMessage.Text = "Add recipes to favorites from the detail page";
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to load favorites: {ex.Message}", "OK");
			ResultLabel.Text = "Failed to load favorites";
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
		}
	}

	// === NAVIGATION ===

	private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is Recipe selectedRecipe)
		{
			RecipeCollectionView.SelectedItem = null;
			await Navigation.PushAsync(new RecipeDetailPage(selectedRecipe));
		}
	}
}