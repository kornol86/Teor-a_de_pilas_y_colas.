/// <summary>
/// administra los asientos del auditorio y
/// garantiza asignación thread-safe entre los dos registradores.
/// </summary>
public class Auditorio
{
    private readonly Queue<Asiento> _asientosLibres;
    private readonly object _candado = new object(); // lock para exclusión mutua
    private readonly List<AsignacionResultado> _historial = new();

    public int Capacidad { get; }
    public int AsientosDisponibles
    {
        get { lock (_candado) { return _asientosLibres.Count; } }
    }

    public Auditorio(int capacidad = 100)
    {
        Capacidad = capacidad;
        _asientosLibres = new Queue<Asiento>(
            Enumerable.Range(1, capacidad).Select(n => new Asiento(n)));
    }

    /// <summary>
    /// Asigna el siguiente asiento disponible al asistente. Es la
    /// única puerta de entrada al recurso compartido, por eso todo
    /// el cuerpo va protegido con lock: garantiza que dos hilos
    /// (los dos registradores) nunca tomen el mismo asiento.
    /// </summary>
    public AsignacionResultado AsignarAsiento(Asistente asistente)
    {
        lock (_candado)
        {
            if (_asientosLibres.Count == 0)
            {
                var fallo = new AsignacionResultado(asistente, null);
                _historial.Add(fallo);
                return fallo;
            }

            var asiento = _asientosLibres.Dequeue();
            asiento.Ocupar(asistente);
            var resultado = new AsignacionResultado(asistente, asiento);
            _historial.Add(resultado);
            return resultado;
        }
    }

    public IReadOnlyList<AsignacionResultado> ObtenerHistorialOrdenadoPorLlegada()
    {
        lock (_candado)
        {
            return _historial.OrderBy(r => r.Asistente.TicketLlegada).ToList();
        }
    }
}
