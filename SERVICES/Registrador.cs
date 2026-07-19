/// <summary>
/// Representa a una de las dos personas que registran asistentes
/// (una fila de ingreso). Procesa su cola y pide asiento al Auditorio.
/// </summary>
public class Registrador
{
    private readonly string _nombreRegistrador;
    private readonly Queue<Asistente> _filaDeEspera;
    private readonly Auditorio _auditorio;
    private readonly object _candadoFila = new object();

    public string Nombre => _nombreRegistrador;

    public Registrador(string nombreRegistrador, IEnumerable<Asistente> asistentes, Auditorio auditorio)
    {
        _nombreRegistrador = nombreRegistrador;
        _filaDeEspera = new Queue<Asistente>(asistentes);
        _auditorio = auditorio;
    }

    /// <summary>Reportería: cuántas personas quedan por atender en esta fila.</summary>
    public int PersonasEnEspera
    {
        get { lock (_candadoFila) { return _filaDeEspera.Count; } }
    }

    /// <summary>Reportería: visualizar quiénes siguen esperando, sin sacarlos de la fila.</summary>
    public IReadOnlyList<Asistente> ConsultarFilaDeEspera()
    {
        lock (_candadoFila)
        {
            return _filaDeEspera.ToList(); // copia; no altera la cola original
        }
    }

    /// <summary>Procesa su fila registrando asistente por asistente.</summary>
    public void ProcesarFila()
    {
        while (true)
        {
            Asistente asistente;
            lock (_candadoFila)
            {
                if (_filaDeEspera.Count == 0) break;
                asistente = _filaDeEspera.Dequeue();
            }

            // Simula el tiempo que tarda un registrador en atender
            // a cada persona (verificar datos, imprimir gafete, etc.)
            Thread.Sleep(Random.Shared.Next(5, 25));

            var resultado = _auditorio.AsignarAsiento(asistente);

            if (resultado.Exito)
            {
                Console.WriteLine(
                    $"[{_nombreRegistrador}] {asistente.Nombre,-12} -> Asiento #{resultado.Asiento!.Numero:D3} " +
                    $"(ticket llegada #{asistente.TicketLlegada})");
            }
            else
            {
                Console.WriteLine($"[{_nombreRegistrador}] {asistente.Nombre,-12} -> SIN CUPO, auditorio lleno");
            }
        }
    }
}