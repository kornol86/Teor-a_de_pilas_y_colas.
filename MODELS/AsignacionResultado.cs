public class AsignacionResultado
{
    public Asistente Asistente { get; }
    public Asiento? Asiento { get; }
    public bool Exito => Asiento != null;

    public AsignacionResultado(Asistente asistente, Asiento? asiento)
    {
        Asistente = asistente;
        Asiento = asiento;
    }
}
