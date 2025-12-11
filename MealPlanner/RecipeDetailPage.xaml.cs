using PantryBlissClone.Models;   
using Microsoft.Maui.Controls;

namespace PantryBlissClone;

public partial class RecipeDetailPage : ContentPage
{
	private Recipe _recipe;

	public RecipeDetailPage(Recipe recipe)
	{
		InitializeComponent();
		_recipe = recipe;
		BindingContext = _recipe;
		LoadIngredients();
	}

	private void LoadIngredients()
	{
		IngredientsStackLayout.Children.Clear();
		foreach (var ing in _recipe.Ingredients)
		{
			IngredientsStackLayout.Children.Add(new Label
			{
				Text = $"{ing.Name} - {ing.Measure}",
				FontSize = 16
			});
		}
	}
}
