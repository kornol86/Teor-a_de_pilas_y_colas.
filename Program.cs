
/// <summary>Punto de entrada: arma la simulación de doble línea de ingreso.</summary>
public class Program
{
    private static long _contadorTickets = 0;

    // Genera un ticket de llegada único y atómico (thread-safe)
    private static long SiguienteTicket() => Interlocked.Increment(ref _contadorTickets);

    public static void Main()
    {
        const int capacidadAuditorio = 100;
        const int totalAsistentes = 100; // igual a la capacidad, para probar el límite

        var auditorio = new Auditorio(capacidadAuditorio);

        // Generamos los asistentes de cada fila y les damos su ticket
        // de llegada en el momento en que "se forman" en la fila.
        var fila1 = new List<Asistente>();
        var fila2 = new List<Asistente>();

        for (int i = 1; i <= totalAsistentes; i++)
        {
            int filaAsignada = (i % 2 == 0) ? 2 : 1; // alterna entre filas
            var asistente = new Asistente($"Asistente{i:D3}", filaAsignada)
            {
                TicketLlegada = SiguienteTicket()
            };

            if (filaAsignada == 1) fila1.Add(asistente);
            else fila2.Add(asistente);
        }

        var registrador1 = new Registrador("Registrador A - Fila 1", fila1, auditorio);
        var registrador2 = new Registrador("Registrador B - Fila 2", fila2, auditorio);

        // Cada registrador corre en su propio hilo -> ingreso doble real
        var hilo1 = new Thread(registrador1.ProcesarFila) { Name = "HiloFila1" };
        var hilo2 = new Thread(registrador2.ProcesarFila) { Name = "HiloFila2" };

        Console.WriteLine("=== Iniciando registro con doble línea de ingreso ===\n");
        hilo1.Start();
        hilo2.Start();

        hilo1.Join();
        hilo2.Join();

        Console.WriteLine("\n=== Registro finalizado ===");
        Console.WriteLine($"Asientos ocupados: {capacidadAuditorio - auditorio.AsientosDisponibles} / {capacidadAuditorio}");
        Console.WriteLine($"Asientos libres restantes: {auditorio.AsientosDisponibles}");

        // Verificación de integridad: no debe haber asientos duplicados
        var historial = auditorio.ObtenerHistorialOrdenadoPorLlegada();
        var numerosAsignados = historial.Where(r => r.Exito).Select(r => r.Asiento!.Numero).ToList();
        bool sinDuplicados = numerosAsignados.Distinct().Count() == numerosAsignados.Count;
        Console.WriteLine($"\nVerificación: {(sinDuplicados ? "OK, sin asientos duplicados" : "ERROR: hay asientos duplicados")}");

        // ===== REPORTERÍA: visualizar y consultar la estructura completa =====

        // 1) Reporte visual de los 100 asientos (libres y ocupados)
        auditorio.ImprimirReporteConsola();

        // 2) Consultas puntuales de ejemplo sobre la estructura
        var asiento50 = auditorio.ConsultarAsiento(50);
        Console.WriteLine($"\nConsulta puntual -> Asiento #50: " +
            $"{(asiento50!.Ocupado ? $"ocupado por {asiento50.Ocupante!.Nombre}" : "libre")}");

        var libres = auditorio.ConsultarAsientosLibres();
        Console.WriteLine($"Consulta -> Asientos libres restantes: {libres.Count} " +
            $"(ej: {string.Join(", ", libres.Take(5).Select(a => a.Numero))}{(libres.Count > 5 ? "..." : "")})");

        var ocupados = auditorio.ConsultarAsientosOcupados();
        Console.WriteLine($"Consulta -> Asientos ocupados: {ocupados.Count}");

        var primerAsistente = fila1.First();
        var asientoDePersona = auditorio.ConsultarAsientoPorAsistente(primerAsistente.Nombre);
        Console.WriteLine($"Consulta -> Asiento de '{primerAsistente.Nombre}': " +
            $"{(asientoDePersona != null ? $"#{asientoDePersona.Numero}" : "no encontrado")}");

//evita que la consola se cierre inmediatamente después de mostrar los resultados
            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
    }
}