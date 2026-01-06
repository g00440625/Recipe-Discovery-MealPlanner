using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class ApiService
{
	private readonly HttpClient _httpClient;
	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true
	};
	private const string BaseUrl = "https://www.themealdb.com/api/json/v1/1/"; 

	private class ApiResponse
	{
		public List<ApiMeal>? meals { get; set; }
	}

	private class ApiMeal
	{
		public string? idMeal { get; set; }
		public string? strMeal { get; set; }
		public string? strCategory { get; set; }
		public string? strInstructions { get; set; }
		public string? strMealThumb { get; set; }
	}

	public ApiService() : this(new HttpClient { BaseAddress = new Uri(BaseUrl) }) { }

	public ApiService(HttpClient httpClient)
	{
		_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
		if (_httpClient.BaseAddress == null)
			_httpClient.BaseAddress = new Uri(BaseUrl);
	}

	public async Task<Recipe?> GetRecipeById(string recipeId)
	{
		if (string.IsNullOrWhiteSpace(recipeId))
			return null;

		try
		{
			var response = await _httpClient.GetFromJsonAsync<ApiResponse>($"lookup.php?i={Uri.EscapeDataString(recipeId)}", _jsonOptions);
			var apiMeal = response?.meals?.FirstOrDefault();
			if (apiMeal == null)
				return null;

			return ConvertToRecipe(apiMeal);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"API GetRecipeById error: {ex.Message}");
			return null;
		}
	}

	public async Task<List<Recipe>> SearchRecipes(string searchTerm)
	{
		if (string.IsNullOrWhiteSpace(searchTerm))
			return new List<Recipe>();

		try
		{
			var response = await _httpClient.GetFromJsonAsync<ApiResponse>($"search.php?s={Uri.EscapeDataString(searchTerm)}", _jsonOptions);
			if (response?.meals == null)
				return new List<Recipe>();

			return response.meals.Select(ConvertToRecipe).ToList();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"API SearchRecipes error: {ex.Message}");
			return new List<Recipe>();
		}
	}

	public async Task<Recipe?> GetRandomRecipe()
	{
		try
		{
			var response = await _httpClient.GetFromJsonAsync<ApiResponse>("random.php", _jsonOptions);
			var apiMeal = response?.meals?.FirstOrDefault();
			if (apiMeal == null)
				return null;

			return ConvertToRecipe(apiMeal);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"API GetRandomRecipe error: {ex.Message}");
			return null;
		}
	}

	private Recipe ConvertToRecipe(ApiMeal meal)
	{
		var recipe = new Recipe
		{
			Id = meal.idMeal,
			Name = meal.strMeal,
			Category = meal.strCategory,
			Instructions = meal.strInstructions,
			ImageUrl = meal.strMealThumb,
			Ingredients = new List<Ingredient>()
		};

		var mealType = typeof(ApiMeal);
		for (int i = 1; i <= 20; i++)
		{
			var ingProp = mealType.GetProperty($"strIngredient{i}", BindingFlags.Public | BindingFlags.Instance);
			var measProp = mealType.GetProperty($"strMeasure{i}", BindingFlags.Public | BindingFlags.Instance);

			if (ingProp == null)
				continue;

			var ingVal = (ingProp.GetValue(meal) as string)?.Trim();
			var measVal = measProp != null ? (measProp.GetValue(meal) as string)?.Trim() : null;

			if (!string.IsNullOrWhiteSpace(ingVal) && !string.Equals(ingVal, "null", StringComparison.OrdinalIgnoreCase))
			{
				recipe.Ingredients.Add(new Ingredient
				{
					Name = ingVal,
					Measure = measVal ?? string.Empty
				});
			}
		}
		return recipe;
	}
}