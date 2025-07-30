using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Trackpad.MAUI.Models;

namespace Trackpad.MAUI.Views;

public partial class DiscoveryPage : ContentPage
{
	public ObservableCollection<ServerInfo> Servers { get; set; } = new ObservableCollection<ServerInfo>();
	private UdpClient udpClient;

	public DiscoveryPage()
	{
		InitializeComponent();
		BindingContext = this;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		_ = StartListeningAsync();
	}

	private async Task StartListeningAsync()
	{
		udpClient = new UdpClient(4568) { EnableBroadcast = true };
		while (true)
		{
			try
			{
				var result = await udpClient.ReceiveAsync();
				var json = Encoding.UTF8.GetString(result.Buffer);
				var doc = JsonDocument.Parse(json);
				var name = doc.RootElement.GetProperty("name").GetString();
				var ip = doc.RootElement.GetProperty("ip").GetString();
				var port = doc.RootElement.GetProperty("port").GetInt32();

				MainThread.BeginInvokeOnMainThread(() =>
				{
					var existing = Servers.FirstOrDefault(s => s.IP == ip);
					if (existing == null)
						Servers.Add(new ServerInfo(name, ip, port));
					else
						existing.LastSeen = DateTime.Now;
				});
			}
			catch { }
		}
	}
	private async void OnServerTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item is ServerInfo server)
        {
            await Navigation.PushAsync(new TrackpadPage(server));
            ((ListView)sender).SelectedItem = null;
        }
    }
}