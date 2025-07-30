using System.ComponentModel;

namespace Trackpad.MAUI.Models;

/// <summary>
/// Representa información sobre un servidor, incluyendo su nombre, dirección IP, puerto y la última vez que fue visto.
/// Implementa <see cref="INotifyPropertyChanged"/> para soportar notificaciones de cambio de propiedad.
/// </summary>
///
/// <remarks>
/// La propiedad <see cref="DisplayInfo"/> proporciona una cadena formateada que contiene el nombre del servidor, IP, puerto
/// y el número de segundos desde la última vez que fue visto.
/// </remarks>
public class ServerInfo : INotifyPropertyChanged // Implementa la interfaz para notificar cambios en propiedades.
{
    public string Name; // Almacena el nombre del servidor.
    public string IP; // Almacena la dirección IP del servidor.
    public int Port; // Almacena el puerto del servidor.
    public DateTime _lastSeen; // Campo privado para la última vez que se vio el servidor.

    // Propiedad para acceder y modificar la última vez que se vio el servidor.
    public DateTime LastSeen
    {
        get => _lastSeen; // Devuelve el valor actual de _lastSeen.
        set
        {
            if (_lastSeen != value) // Solo actualiza si el valor es diferente.
            {
                _lastSeen = value; // Actualiza el valor.
                OnPropertyChanged(nameof(LastSeen)); // Notifica que la propiedad LastSeen cambió.
                OnPropertyChanged(nameof(DisplayInfo)); // Notifica que DisplayInfo cambió (ya que depende de LastSeen).
            }
        }
    }

    // Propiedad que muestra información formateada del servidor.
    public string DisplayInfo => $"{Name} ({IP}:{Port}): {(DateTime.Now - LastSeen).Seconds}s";
    // Ejemplo: "Servidor1 (192.168.1.1:8080): 5s" (5 segundos desde la última vez visto).

    public event PropertyChangedEventHandler PropertyChanged; // Evento para notificar cambios de propiedad.

    // Método para disparar el evento PropertyChanged.
    void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Constructor que inicializa las propiedades del servidor.
    public ServerInfo(string name, string ip, int port)
    {
        Name = name; // Asigna el nombre.
        IP = ip; // Asigna la IP.
        Port = port; // Asigna el puerto.
        LastSeen = DateTime.Now; // Inicializa la última vez visto con el momento actual.
    }
}