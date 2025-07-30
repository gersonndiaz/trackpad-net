using Trackpad.MAUI.Views;

namespace Trackpad.MAUI;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		// Navigate to the DiscoveryPage when the MainPage appears
		Navigation.PushAsync(new DiscoveryPage());
	}
}
