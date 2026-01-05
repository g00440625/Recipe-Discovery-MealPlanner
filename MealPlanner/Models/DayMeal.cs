using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MealPlanner.Models
{
	public class DayMeal : INotifyPropertyChanged
	{
		public string Day { get; set; } = string.Empty;

		private Recipe? _breakfast;
		public Recipe? Breakfast
		{
			get => _breakfast;
			set { if (_breakfast != value) { _breakfast = value; OnPropertyChanged(); } }
		}

		private Recipe? _lunch;
		public Recipe? Lunch
		{
			get => _lunch;
			set { if (_lunch != value) { _lunch = value; OnPropertyChanged(); } }
		}

		private Recipe? _dinner;
		public Recipe? Dinner
		{
			get => _dinner;
			set { if (_dinner != value) { _dinner = value; OnPropertyChanged(); } }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string? name = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}

	public class WeekPlan
	{
		public List<DayMeal> Days { get; set; } = new();
	}
}

