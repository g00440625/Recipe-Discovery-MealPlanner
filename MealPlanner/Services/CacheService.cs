using System.Collections.Concurrent;
using System.Text.Json;
using MealPlanner.Models;

namespace MealPlanner.Services;

public class CacheService
{
	private readonly ApiService _apiService;
	private readonly StorageService _storage;

	private readonly ConcurrentDictionary<string, Recipe> _memoryCache = new();
	private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

	private readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNameCaseInsensitive = true,
		WriteIndented = false
	};

	public CacheService(ApiService apiService, StorageService storage)
	{
		_apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
		_storage = storage ?? throw new ArgumentNullException(nameof(storage));
	}

	private static string GetCacheFilePath(string recipeId)
	{
		var safeId = string.IsNullOrWhiteSpace(recipeId)
			? "unknown"
			: string.Concat(recipeId.Split(Path.GetInvalidFileNameChars()));
		return Path.Combine(FileSystem.AppDataDirectory, $"cache_{safeId}.json");
	}

	// Main method - get recipe from cache or API
	public async Task<Recipe?> GetRecipe(string recipeId)
	{
		if (string.IsNullOrWhiteSpace(recipeId))
			return null;

		// 1. Try memory cache
		if (_memoryCache.TryGetValue(recipeId, out var cached))
		{
			Console.WriteLine("✅ Found in memory cache");
			return cached;
		}

		var sem = _locks.GetOrAdd(recipeId, _ => new SemaphoreSlim(1, 1));
		await sem.WaitAsync();
		try
		{
			if (_memoryCache.TryGetValue(recipeId, out cached))
				return cached;

			// 2. Try file cache
			var fileCached = await LoadFromFileCache(recipeId);
			if (fileCached != null)
			{
				Console.WriteLine("💾 Found in file cache");
				_memoryCache[recipeId] = fileCached;
				return fileCached;
			}

			// 3. Fetch from API
			Console.WriteLine("🌐 Fetching from API...");
			var recipe = await _apiService.GetRecipeById(recipeId);
			if (recipe != null)
			{
				_memoryCache[recipeId] = recipe;
				await SaveToFileCache(recipe);
				Console.WriteLine("✨ Saved to cache");
			}

			return recipe;
		}
		finally
		{
			sem.Release();
			_locks.TryRemove(recipeId, out _);
		}
	}

	// ADD THESE NEW METHODS FOR COMPATIBILITY:

	// Search recipes and cache them
	public async Task<List<Recipe>> SearchRecipesAsync(string searchTerm)
	{
		var recipes = await _apiService.SearchRecipes(searchTerm);

		// Cache each recipe
		foreach (var recipe in recipes)
		{
			if (!string.IsNullOrWhiteSpace(recipe.Id))
			{
				_memoryCache[recipe.Id] = recipe;
				await SaveToFileCache(recipe);
			}
		}

		return recipes;
	}

	// Get random recipe and cache it
	public async Task<Recipe?> GetRandomRecipeAsync()
	{
		var recipe = await _apiService.GetRandomRecipe();

		if (recipe != null && !string.IsNullOrWhiteSpace(recipe.Id))
		{
			_memoryCache[recipe.Id] = recipe;
			await SaveToFileCache(recipe);
		}

		return recipe;
	}

	// Get recipe by ID (alias for GetRecipe)
	public async Task<Recipe?> GetRecipeAsync(string recipeId)
	{
		return await GetRecipe(recipeId);
	}

	private async Task SaveToFileCache(Recipe recipe)
	{
		if (recipe == null || string.IsNullOrWhiteSpace(recipe.Id))
			return;

		try
		{
			var filePath = GetCacheFilePath(recipe.Id);
			Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? FileSystem.AppDataDirectory);
			var json = JsonSerializer.Serialize(recipe, _jsonOptions);
			await File.WriteAllTextAsync(filePath, json);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Couldn't save to cache: {ex.Message}");
		}
	}

	private async Task<Recipe?> LoadFromFileCache(string recipeId)
	{
		var filePath = GetCacheFilePath(recipeId);
		if (!File.Exists(filePath))
			return null;

		try
		{
			var fileAge = DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath);
			if (fileAge.TotalDays > 30)
			{
				try { File.Delete(filePath); } catch { }
				return null;
			}

			var json = await File.ReadAllTextAsync(filePath);
			return JsonSerializer.Deserialize<Recipe>(json, _jsonOptions);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to read cache: {ex.Message}");
			return null;
		}
	}

	public void ClearCache()
	{
		_memoryCache.Clear();

		var cacheFiles = Directory.GetFiles(FileSystem.AppDataDirectory, "cache_*.json");
		foreach (var file in cacheFiles)
		{
			try { File.Delete(file); } catch { }
		}

		Console.WriteLine("🧹 All caches cleared");
	}

	public async Task PreloadRecipes(List<string> recipeIds)
	{
		if (recipeIds == null || recipeIds.Count == 0)
			return;

		Console.WriteLine($"Preloading {recipeIds.Count} recipes...");

		foreach (var id in recipeIds)
		{
			if (File.Exists(GetCacheFilePath(id)))
				continue;

			await GetRecipe(id);
			await Task.Delay(100);
		}
	}
}