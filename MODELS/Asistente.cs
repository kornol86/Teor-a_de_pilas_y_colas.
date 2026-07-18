public class Asistente
{
    public string Nombre { get; }
    public int Fila { get; }                // 1 o 2: por cuál línea entró
    public long TicketLlegada { get; set; }  // orden real de llegada

    public Asistente(string nombre, int fila)
    {
        Nombre = nombre;
        Fila = fila;
    }

    public override string ToString() => $"{Nombre} (Fila {Fila}, ticket #{TicketLlegada})";
}
