using MealPlanner.Services;

namespace MealPlanner;

public partial class App : Application
{
	public static ApiService ApiService { get; private set; }
	public static StorageService StorageService { get; private set; }
	public static CacheService CacheService { get; private set; }

	public App()
	{
		InitializeComponent();

		// Initialize services once
		ApiService = new ApiService();
		StorageService = new StorageService();
		CacheService = new CacheService(ApiService, StorageService);

		// Use NavigationPage for back button support
		MainPage = new NavigationPage(new MainPage())
		{
			BarBackgroundColor = Color.FromArgb("#4CAF50"),
			BarTextColor = Colors.White
		};
	}
}