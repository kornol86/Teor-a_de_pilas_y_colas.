
public class Registrador
{
    private readonly string _nombreRegistrador;
    private readonly Queue<Asistente> _filaDeEspera;
    private readonly Auditorio _auditorio;

    public Registrador(string nombreRegistrador, IEnumerable<Asistente> asistentes, Auditorio auditorio)
    {
        _nombreRegistrador = nombreRegistrador;
        _filaDeEspera = new Queue<Asistente>(asistentes);
        _auditorio = auditorio;
    }

    /// <summary>Procesa su fila registrando asistente por asistente.</summary>
    public void ProcesarFila()
    {
        while (_filaDeEspera.Count > 0)
        {
            var asistente = _filaDeEspera.Dequeue();

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
