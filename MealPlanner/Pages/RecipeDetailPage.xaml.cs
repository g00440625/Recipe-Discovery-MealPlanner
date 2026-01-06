using MealPlanner.Models;

namespace MealPlanner.Pages;

public partial class RecipeDetailPage : ContentPage
{
	private Recipe _recipe;
	private bool _isFavorite;

	public RecipeDetailPage(Recipe recipe)
	{
		InitializeComponent();
		_recipe = recipe;
		LoadRecipeDetails();
	}

	private async void LoadRecipeDetails()
	{
		// If recipe doesn't have full details, fetch from cache/API
		if (string.IsNullOrEmpty(_recipe.Instructions))
		{
			var fullRecipe = await App.CacheService.GetRecipe(_recipe.Id);
			if (fullRecipe != null)
			{
				_recipe = fullRecipe;
			}
		}

		// Display recipe
		RecipeImage.Source = _recipe.ImageUrl;
		RecipeNameLabel.Text = _recipe.Name;
		CategoryLabel.Text = _recipe.Category;
		InstructionsLabel.Text = _recipe.Instructions;
		IngredientsListView.ItemsSource = _recipe.Ingredients;

		// Check if already favorited
		await UpdateFavoriteButtonAsync();
	}

	private async Task UpdateFavoriteButtonAsync()
	{
		var favorites = await App.StorageService.LoadFavorites();
		_isFavorite = favorites.Any(f => f.Id == _recipe.Id);

		if (_isFavorite)
		{
			FavoriteButton.Text = "? Remove from Favorites";
			FavoriteButtonBorder.BackgroundColor = Colors.Gray;
		}
		else
		{
			FavoriteButton.Text = "? Add to Favorites";
			FavoriteButtonBorder.BackgroundColor = Color.FromArgb("#E91E63");
		}
	}

	private async void OnFavoriteClicked(object sender, EventArgs e)
	{
		try
		{
			if (_isFavorite)
			{
				// Remove from favorites
				await App.StorageService.RemoveFromFavorites(_recipe.Id);
				await DisplayAlert("Removed", $"{_recipe.Name} removed from favorites", "OK");
			}
			else
			{
				// Add to favorites
				await App.StorageService.AddToFavorites(_recipe);
				await DisplayAlert("Added", $"{_recipe.Name} added to favorites!", "OK");
			}

			// Update button
			await UpdateFavoriteButtonAsync();
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to update favorites: {ex.Message}", "OK");
		}
	}

	private async void OnAddToMealPlanClicked(object sender, EventArgs e)
	{
		// Navigate to meal planner with this recipe
		await Navigation.PushAsync(new MealPlannerPage(_recipe));
	}
}