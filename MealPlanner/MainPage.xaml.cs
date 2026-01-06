namespace MealPlanner;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnBrowseRecipesClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Pages.RecipeBrowserPage());
	}

	private async void OnMealPlannerClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Pages.MealPlannerPage());
	}

	private async void OnShoppingListClicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new Pages.ShoppingListPage());
	}

	private async void OnRandomRecipeClicked(object sender, EventArgs e)
	{
		LoadingIndicator.IsRunning = true;
		LoadingIndicator.IsVisible = true;

		try
		{
			var recipe = await App.ApiService.GetRandomRecipe();

			if (recipe != null)
			{
				await Navigation.PushAsync(new Pages.RecipeDetailPage(recipe));
			}
			else
			{
				await DisplayAlert("Error", "Could not fetch random recipe. Check your internet connection.", "OK");
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to get random recipe: {ex.Message}", "OK");
		}
		finally
		{
			LoadingIndicator.IsRunning = false;
			LoadingIndicator.IsVisible = false;
		}
	}
}