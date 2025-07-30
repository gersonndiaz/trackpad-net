using System.Net.Sockets;
using Trackpad.MAUI.Models;

namespace Trackpad.MAUI.Views;

public partial class TrackpadPage : ContentPage
{

	private TcpClient _tcpClient;
    private StreamWriter _writer;
    private bool _blocked;

    // Zoom (pinch)
    private double _initialScale = 1;
    private const double ZoomThreshold = 0.2;

    // Scroll (2-finger pan)
    private const double ScrollThreshold = 20;

    // Desktop change (4-finger swipe/pan)
    private const double SwipeThreshold = 50;

    public TrackpadPage(ServerInfo server)
    {
        InitializeComponent();
        ConnectAsync(server);
    }

    private async void ConnectAsync(ServerInfo server)
    {
        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(server.IP, server.Port);
            _writer = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true };
            StatusLabel.Text = "Conectado ✅";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error conexión: {ex.Message}";
        }
    }

    private void SendGesture(string gesture)
    {
        if (_blocked || _writer == null) return;
        _writer.WriteLine(gesture);
        GestureLabel.Text = gesture;
        _blocked = true;
        Device.StartTimer(TimeSpan.FromMilliseconds(300), () => { _blocked = false; return false; });
    }

    // Zoom handler
    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (_blocked) return;

        if (e.Status == GestureStatus.Started)
        {
            _initialScale = e.Scale;
        }
        else if (e.Status == GestureStatus.Running)
        {
            var delta = e.Scale - _initialScale;
            if (Math.Abs(delta) > ZoomThreshold)
            {
                SendGesture(delta > 0 ? "🔍 Zoom+" : "🔎 Zoom-");
                _initialScale = e.Scale;
            }
        }
    }

    // Scroll handler (2 fingers)
    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_blocked || e.StatusType != GestureStatus.Running) return;

        if (Math.Abs(e.TotalX) > ScrollThreshold)
        {
            SendGesture(e.TotalX > 0 ? "➡️ Scroll H" : "⬅️ Scroll H");
        }
        else if (Math.Abs(e.TotalY) > ScrollThreshold)
        {
            SendGesture(e.TotalY > 0 ? "⬇️ Scroll V" : "⬆️ Scroll V");
        }
    }

    // Desktop change handler (4 fingers)
    private void OnDeskPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_blocked || e.StatusType != GestureStatus.Running) return;

        if (Math.Abs(e.TotalX) > SwipeThreshold)
        {
            SendGesture(e.TotalX > 0
                ? "➡️ Cambio escritorio"
                : "⬅️ Cambio escritorio");
        }
    }
}