using System.Collections.ObjectModel;
using System.Linq;
using MealPlanner.Models;

namespace MealPlanner.Pages;

public partial class MealPlannerPage : ContentPage
{
	private ObservableCollection<DayMeal> _weekPlan = new();
	private Recipe? _recipeToAdd;

	public MealPlannerPage()
	{
		InitializeComponent();
		InitializeWeekPlan();
	}

	// Constructor: Open meal planner with recipe to add
	public MealPlannerPage(Recipe recipe) : this()
	{
		_recipeToAdd = recipe;

		// Show the "recipe to add" section
		if (_recipeToAdd != null)
		{
			RecipeToAddSection.IsVisible = true;
			RecipeToAddLabel.Text = _recipeToAdd.Name ?? string.Empty;
		}
	}

	private void InitializeWeekPlan()
	{
		_weekPlan = new ObservableCollection<DayMeal>
		{
			new DayMeal { Day = "Monday" },
			new DayMeal { Day = "Tuesday" },
			new DayMeal { Day = "Wednesday" },
			new DayMeal { Day = "Thursday" },
			new DayMeal { Day = "Friday" },
			new DayMeal { Day = "Saturday" },
			new DayMeal { Day = "Sunday" }
		};

		// Bind to CollectionView (ensure x:Name="DaysCollectionView" exists in XAML)
		DaysCollectionView.ItemsSource = _weekPlan;
	}

	// Helpers used by the Clicked event handlers in your XAML
	private void AddRecipeToDay(string day, string mealType)
	{
		var entry = _weekPlan.FirstOrDefault(d => d.Day == day);
		if (entry == null)
			return;

		// Place the selected Recipe into the meal slot
		switch (mealType)
		{
			case "Breakfast": entry.Breakfast = _recipeToAdd; break;
			case "Lunch": entry.Lunch = _recipeToAdd; break;
			case "Dinner": entry.Dinner = _recipeToAdd; break;
		}

		// hide the add section after placing the recipe
		RecipeToAddSection.IsVisible = false;
	}

	// Safe Clicked handlers for the + buttons (they use CommandParameter="{Binding Day}" in XAML)
	private void OnBreakfastClicked(object sender, EventArgs e)
	{
		if (sender is Button b && b.CommandParameter is string day)
			AddRecipeToDay(day, "Breakfast");
	}

	private void OnLunchClicked(object sender, EventArgs e)
	{
		if (sender is Button b && b.CommandParameter is string day)
			AddRecipeToDay(day, "Lunch");
	}

	private void OnDinnerClicked(object sender, EventArgs e)
	{
		if (sender is Button b && b.CommandParameter is string day)
			AddRecipeToDay(day, "Dinner");
	}

	// Generate shopping list from current week plan and navigate to ShoppingListPage
	private async void OnGenerateShoppingListClicked(object sender, EventArgs e)
	{
		// Collect all recipe references from the week plan
		var recipes = _weekPlan
			.SelectMany(d => new[] { d.Breakfast, d.Lunch, d.Dinner })
			.Where(r => r != null)
			.Cast<Recipe>()
			.ToList();

		// Aggregate ingredients by name (case-insensitive). Measures are joined if multiple present.
		var ingredientGroups = recipes
			.SelectMany(r => r.Ingredients.Select(i => new { i.Name, i.Measure }))
			.Where(x => !string.IsNullOrWhiteSpace(x.Name))
			.GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase)
			.Select(g => new ShoppingList
			{
				Name = g.Key,
				Measure = string.Join(" + ", g.Select(x => x.Measure?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct())
			})
			.ToList();

		// Navigate to shopping list page and pass aggregated items
		await Navigation.PushAsync(new ShoppingListPage(ingredientGroups));
	}
}