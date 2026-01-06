using System.Text.Json;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class StorageService
{
	// ===== SIMPLE FILE PATHS =====
	private string GetFilePath(string filename)
	{
		return Path.Combine(FileSystem.AppDataDirectory, filename);
	}

	// ===== FAVORITES =====
	public async Task SaveFavorites(List<Recipe> favorites)
	{
		var json = JsonSerializer.Serialize(favorites);
		await File.WriteAllTextAsync(GetFilePath("favorites.json"), json);
	}

	public async Task<List<Recipe>> LoadFavorites()
	{
		var path = GetFilePath("favorites.json");

		if (!File.Exists(path))
			return new List<Recipe>(); // Return empty list if file doesn't exist

		var json = await File.ReadAllTextAsync(path);
		return JsonSerializer.Deserialize<List<Recipe>>(json) ?? new List<Recipe>();
	}

	public async Task AddToFavorites(Recipe recipe)
	{
		var favorites = await LoadFavorites();
		if (!favorites.Any(f => f.Id == recipe.Id))
		{
			favorites.Add(recipe);
			await SaveFavorites(favorites);
		}
	}

	public async Task RemoveFromFavorites(string recipeId)
	{
		var favorites = await LoadFavorites();
		favorites.RemoveAll(f => f.Id == recipeId);
		await SaveFavorites(favorites);
	}

	// ===== MEAL PLAN =====
	public async Task SaveMealPlan(Dictionary<DateTime, List<Recipe>> plan)
	{
		var json = JsonSerializer.Serialize(plan);
		await File.WriteAllTextAsync(GetFilePath("mealplan.json"), json);
	}

	public async Task<Dictionary<DateTime, List<Recipe>>> LoadMealPlan()
	{
		var path = GetFilePath("mealplan.json");

		if (!File.Exists(path))
			return new Dictionary<DateTime, List<Recipe>>();

		var json = await File.ReadAllTextAsync(path);
		return JsonSerializer.Deserialize<Dictionary<DateTime, List<Recipe>>>(json)
			?? new Dictionary<DateTime, List<Recipe>>();
	}

	// ===== SHOPPING LIST =====
	public async Task SaveShoppingList(List<Ingredient> items)
	{
		var json = JsonSerializer.Serialize(items);
		await File.WriteAllTextAsync(GetFilePath("shopping.json"), json);
	}

	public async Task<List<Ingredient>> LoadShoppingList()
	{
		var path = GetFilePath("shopping.json");

		if (!File.Exists(path))
			return new List<Ingredient>();

		var json = await File.ReadAllTextAsync(path);
		return JsonSerializer.Deserialize<List<Ingredient>>(json)
			?? new List<Ingredient>();
	}
}