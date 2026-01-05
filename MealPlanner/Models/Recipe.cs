namespace MealPlanner.Models
{

	public class Ingredient
	{
		public string Name { get; set; } = "";
		public string Measure { get; set; } = "";
	}

	public class Recipe
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public string Category { get; set; } = "";
		public string Area { get; set; } = "";
		public string Instructions { get; set; } = "";
		public string ImageUrl { get; set; } = "";
		public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
	}
}