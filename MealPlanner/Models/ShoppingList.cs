namespace MealPlanner.Models
{
	public class ShoppingList
	{
		public string Name { get; set; } = "";
		public bool IsChecked { get; set; } = false;
		public string Measure { get; internal set; }
	}
}
