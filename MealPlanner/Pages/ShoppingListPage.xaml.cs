using System.Collections.Generic;
using System.Linq;
using MealPlanner.Models;

namespace MealPlanner.Pages
{
	public partial class ShoppingListPage : ContentPage
	{
		public ShoppingListPage()
		{
			InitializeComponent();
		}

		// New constructor accepting generated shopping items
		public ShoppingListPage(IEnumerable<ShoppingList> items) : this()
		{
			ItemsCollectionView.ItemsSource = items;
		}

		// Save shopping list to storage and navigate back
		private async void OnDoneClicked(object sender, EventArgs e)
		{
			try
			{
				// Get current items from the CollectionView
				var items = (ItemsCollectionView.ItemsSource as IEnumerable<ShoppingList>)?.ToList()
							?? new List<ShoppingList>();

				// Convert to Ingredient objects used by StorageService
				var ingredients = items
					.Where(i => !string.IsNullOrWhiteSpace(i.Name))
					.Select(i => new Ingredient { Name = i.Name.Trim(), Measure = i.Measure?.Trim() ?? string.Empty })
					.ToList();

				// Save via the shared StorageService (App.StorageService should be initialized)
				if (App.StorageService != null)
				{
					await App.StorageService.SaveShoppingList(ingredients);
					await DisplayAlert("Saved", "Shopping list saved.", "OK");
				}
				else
				{
					await DisplayAlert("Warning", "Storage service not available. Cannot save shopping list.", "OK");
				}

				// Close page
				if (Navigation.ModalStack.Count > 0)
					await Navigation.PopModalAsync();
				else
					await Navigation.PopAsync();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to save shopping list: {ex.Message}", "OK");
			}
		}
	}
}