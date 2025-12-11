using PantryBlissClone.Models;   
using PantryBlissClone.Services; 
using Microsoft.Maui.Controls;

namespace PantryBlissClone;

public partial class MainPage : ContentPage
{
	private TheMealDbService _service = new TheMealDbService();
	public MainPage()
	{
		InitializeComponent();
	}

	// Search recipes by name
	private async void OnSearchClicked(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(SearchEntry.Text))
			return;

		try
		{
			var recipes = await _service.SearchByNameAsync(SearchEntry.Text);
			RecipeCollection.ItemsSource = recipes;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}

	// Show a random recipe
	private async void OnRandomClicked(object sender, EventArgs e)
	{
		try
		{
			var recipes = await _service.RandomRecipeAsync();
			RecipeCollection.ItemsSource = recipes;
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}

	// Navigate to Recipe Detail Page
	private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
	{
		var selectedRecipe = e.CurrentSelection.FirstOrDefault() as Recipe;
		if (selectedRecipe == null)
			return;

		await Navigation.PushAsync(new RecipeDetailPage(selectedRecipe));
		RecipeCollection.SelectedItem = null; // deselect item
	}
}
