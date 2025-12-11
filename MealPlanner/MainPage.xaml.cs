namespace RecipeDiscoveryApp
{
    public partial class HomePage : ContentPage
    {
        public ObservableCollection<Recipe> Recipes { get; set; }

        public HomePage()
        {
            InitializeComponent();
            Recipes = new ObservableCollection<Recipe>();
            BindingContext = this;
        }

        private async void OnSearchButtonPressed(object sender, System.EventArgs e)
        {
            string query = SearchBar.Text;
            if (!string.IsNullOrWhiteSpace(query))
            {
                await SearchRecipes(query);
            }
        }

        private async Task SearchRecipes(string query)
        {
            var client = new HttpClient();
            var response = await client.GetStringAsync($"https://www.themealdb.com/api/json/v1/1/search.php?s={query}");
            var result = JsonConvert.DeserializeObject<RecipeResponse>(response);

            Recipes.Clear();

            if (result.Meals != null)
            {
                foreach (var recipe in result.Meals)
                {
                    Recipes.Add(recipe);
                }
            }
        }
    }

    public class Recipe
    {
        public string StrMeal { get; set; }
        public string StrMealThumb { get; set; }
    }

    public class RecipeResponse
    {
        public List<Recipe> Meals { get; set; }
    }
}
