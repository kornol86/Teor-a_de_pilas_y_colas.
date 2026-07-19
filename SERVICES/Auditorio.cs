/// <summary>
/// Recurso compartido: administra los asientos del auditorio y
/// garantiza asignación thread-safe entre los dos registradores.
/// </summary>
public class Auditorio
{
    private readonly Queue<Asiento> _asientosLibres;
    private readonly List<Asiento> _todosLosAsientos; // lista maestra: los 100, libres u ocupados
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
        // Se crea la lista maestra una sola vez; la cola de libres referencia
        // los mismos objetos. Esto permite reportar los 100 asientos aunque
        // algunos ya hayan salido de la cola (por estar ocupados).
        _todosLosAsientos = Enumerable.Range(1, capacidad).Select(n => new Asiento(n)).ToList();
        _asientosLibres = new Queue<Asiento>(_todosLosAsientos);
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

    // ================= REPORTERÍA =================
    // Los métodos siguientes permiten VISUALIZAR y CONSULTAR el contenido
    // completo de la estructura de datos (los 100 asientos) en cualquier
    // momento de la ejecución, sin alterar su estado. Todos son de solo
    // lectura y están protegidos con lock porque leen estado compartido
    // que otros hilos pueden estar modificando al mismo tiempo.

    /// <summary>Reporte completo del estado de TODOS los asientos, ordenados por número.</summary>
    public IReadOnlyList<Asiento> ObtenerReporteCompleto()
    {
        lock (_candado)
        {
            return _todosLosAsientos.OrderBy(a => a.Numero).ToList();
        }
    }

    /// <summary>Consulta puntual: el estado de un asiento específico por su número.</summary>
    public Asiento? ConsultarAsiento(int numero)
    {
        lock (_candado)
        {
            return _todosLosAsientos.FirstOrDefault(a => a.Numero == numero);
        }
    }

    /// <summary>Lista solo los asientos actualmente libres, en el orden en que serían entregados.</summary>
    public IReadOnlyList<Asiento> ConsultarAsientosLibres()
    {
        lock (_candado)
        {
            return _asientosLibres.ToList(); // ToList() sobre Queue<T> NO la vacía, solo copia
        }
    }

    /// <summary>Lista solo los asientos ya ocupados, con su ocupante.</summary>
    public IReadOnlyList<Asiento> ConsultarAsientosOcupados()
    {
        lock (_candado)
        {
            return _todosLosAsientos.Where(a => a.Ocupado).OrderBy(a => a.Numero).ToList();
        }
    }

    /// <summary>Busca el asiento asignado a un asistente por nombre.</summary>
    public Asiento? ConsultarAsientoPorAsistente(string nombreAsistente)
    {
        lock (_candado)
        {
            return _todosLosAsientos.FirstOrDefault(
                a => a.Ocupado && a.Ocupante!.Nombre.Equals(nombreAsistente, System.StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Imprime en consola una tabla legible con el estado de los 100 asientos.</summary>
    public void ImprimirReporteConsola()
    {
        var reporte = ObtenerReporteCompleto(); // ya viene ordenado y con su propio lock
        System.Console.WriteLine("\n===== REPORTE DE ASIENTOS =====");
        System.Console.WriteLine($"{"N°",-5} {"Estado",-10} {"Ocupante",-15} {"Fila",-6} {"Ticket",-8}");
        System.Console.WriteLine(new string('-', 50));
        foreach (var asiento in reporte)
        {
            string estado = asiento.Ocupado ? "Ocupado" : "Libre";
            string ocupante = asiento.Ocupado ? asiento.Ocupante!.Nombre : "-";
            string fila = asiento.Ocupado ? asiento.Ocupante!.Fila.ToString() : "-";
            string ticket = asiento.Ocupado ? asiento.Ocupante!.TicketLlegada.ToString() : "-";
            System.Console.WriteLine($"{asiento.Numero,-5} {estado,-10} {ocupante,-15} {fila,-6} {ticket,-8}");
        }
        System.Console.WriteLine(new string('-', 50));
        System.Console.WriteLine($"Total: {reporte.Count} | Ocupados: {reporte.Count(a => a.Ocupado)} | Libres: {reporte.Count(a => !a.Ocupado)}");
    }
}