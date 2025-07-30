using System.Net.Sockets; // Permite usar TCP para enviar gestos al servidor.
using Trackpad.MAUI.Models; // Importa el modelo ServerInfo.

namespace Trackpad.MAUI.Views; // Espacio de nombres de la vista.

/// <summary>
/// Página TrackpadPage que permite enviar gestos táctiles a un servidor remoto mediante TCP.
/// Implementa el manejo de gestos como zoom (pinch), scroll (pan de dos dedos) y cambio de escritorio (pan de cuatro dedos).
/// Los gestos se detectan y envían al servidor solo si superan ciertos umbrales para evitar el envío excesivo.
/// </summary>
/// <remarks>
/// - <c>_tcpClient</c>: Cliente TCP para la comunicación con el servidor.
/// - <c>_writer</c>: Escribe los mensajes de gestos al servidor.
/// - <c>_blocked</c>: Bloquea temporalmente el envío de gestos para evitar spam.
/// - <c>_initialScale</c>: Escala inicial para el gesto de zoom.
/// - <c>ZoomThreshold</c>: Umbral mínimo para detectar el gesto de zoom.
/// - <c>ScrollThreshold</c>: Umbral mínimo para detectar el gesto de scroll.
/// - <c>SwipeThreshold</c>: Umbral mínimo para detectar el gesto de cambio de escritorio.
/// 
/// Métodos principales:
/// - <c>ConnectAsync</c>: Realiza la conexión TCP asíncrona al servidor.
/// - <c>SendGesture</c>: Envía el gesto detectado al servidor y actualiza la UI.
/// - <c>OnPinchUpdated</c>: Maneja el gesto de zoom.
/// - <c>OnPanUpdated</c>: Maneja el gesto de scroll.
/// - <c>OnDeskPanUpdated</c>: Maneja el gesto de cambio de escritorio.
/// </remarks>
public partial class TrackpadPage : ContentPage // Página de trackpad en la app.
{
	private TcpClient _tcpClient; // Cliente TCP para enviar gestos al servidor.
	private StreamWriter _writer; // Escribe mensajes al servidor por TCP.
	private bool _blocked; // Bloquea el envío de gestos para evitar spam.

	// Variables para el gesto de zoom (pinch)
	private double _initialScale = 1; // Escala inicial al empezar el gesto.
	private const double ZoomThreshold = 0.2; // Umbral mínimo para detectar zoom.

	// Variables para el gesto de scroll (pan de dos dedos)
	private const double ScrollThreshold = 20; // Umbral mínimo para detectar scroll.

	// Variables para el gesto de cambio de escritorio (pan de cuatro dedos)
	private const double SwipeThreshold = 50; // Umbral mínimo para detectar swipe.

	// Constructor: recibe el servidor y conecta
	public TrackpadPage(ServerInfo server)
	{
		InitializeComponent(); // Inicializa los componentes visuales.
		ConnectAsync(server); // Intenta conectar al servidor.
	}

	// Conexión TCP asíncrona al servidor
	private async void ConnectAsync(ServerInfo server)
	{
		try
		{
			_tcpClient = new TcpClient(); // Crea el cliente TCP.
			await _tcpClient.ConnectAsync(server.IP, server.Port); // Conecta al servidor.
			_writer = new StreamWriter(_tcpClient.GetStream()) { AutoFlush = true }; // Prepara el escritor.
			StatusLabel.Text = "Conectado ✅"; // Actualiza el estado en la UI.
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Error conexión: {ex.Message}"; // Muestra error en la UI.
		}
	}

	// Envía el gesto al servidor por TCP
	private void SendGesture(string gesture)
	{
		if (_blocked || _writer == null) return; // Si está bloqueado o no hay conexión, no hace nada.
		_writer.WriteLine(gesture); // Envía el gesto como texto.
		GestureLabel.Text = gesture; // Muestra el gesto en la UI.
		_blocked = true; // Bloquea el envío por 300ms.
		Dispatcher.StartTimer(TimeSpan.FromMilliseconds(300), () => { _blocked = false; return false; }); // Desbloquea después.
	}

	// Manejador de gesto de zoom (pinch)
	private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
	{
		if (_blocked) return; // Si está bloqueado, no hace nada.

		if (e.Status == GestureStatus.Started)
		{
			_initialScale = e.Scale; // Guarda la escala inicial.
		}
		else if (e.Status == GestureStatus.Running)
		{
			var delta = e.Scale - _initialScale; // Calcula el cambio de escala.
			if (Math.Abs(delta) > ZoomThreshold) // Si supera el umbral...
			{
				SendGesture(delta > 0 ? "🔍 Zoom+" : "🔎 Zoom-"); // Envía gesto de zoom in/out.
				_initialScale = e.Scale; // Actualiza la escala inicial.
			}
		}
	}

	// Manejador de gesto de scroll (pan de dos dedos)
	private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
	{
		if (_blocked || e.StatusType != GestureStatus.Running) return; // Solo si está en movimiento y no bloqueado.

		if (Math.Abs(e.TotalX) > ScrollThreshold) // Si el movimiento horizontal supera el umbral...
		{
			SendGesture(e.TotalX > 0 ? "➡️ Scroll H" : "⬅️ Scroll H"); // Envía gesto de scroll horizontal.
		}
		else if (Math.Abs(e.TotalY) > ScrollThreshold) // Si el movimiento vertical supera el umbral...
		{
			SendGesture(e.TotalY > 0 ? "⬇️ Scroll V" : "⬆️ Scroll V"); // Envía gesto de scroll vertical.
		}
	}

	// Manejador de gesto de cambio de escritorio (pan de cuatro dedos)
	private void OnDeskPanUpdated(object sender, PanUpdatedEventArgs e)
	{
		if (_blocked || e.StatusType != GestureStatus.Running) return; // Solo si está en movimiento y no bloqueado.

		if (Math.Abs(e.TotalX) > SwipeThreshold) // Si el movimiento horizontal supera el umbral...
		{
			SendGesture(e.TotalX > 0
				? "➡️ Cambio escritorio"
				: "⬅️ Cambio escritorio"); // Envía gesto de cambio de escritorio.
		}
	}
	
	// Handler para tap simple
    private void OnSingleTap(object sender, TappedEventArgs e)
    {
        if (_blocked) return;
        SendGesture("🤏 Tap");
    }

    // Handler para doble tap
    private void OnDoubleTap(object sender, TappedEventArgs e)
    {
        if (_blocked) return;
        SendGesture("🤏🤏 Double Tap");
    }

    // Handler para swipe hacia arriba
    private void OnSwipeUp(object sender, SwipedEventArgs e)
    {
        if (_blocked) return;
        SendGesture("⬆️ Swipe Up");
    }

    // Handler para swipe hacia abajo
    private void OnSwipeDown(object sender, SwipedEventArgs e)
    {
        if (_blocked) return;
        SendGesture("⬇️ Swipe Down");
    }
}