public class Asiento
{
    public int Numero { get; }
    public bool Ocupado { get; private set; }
    public Asistente? Ocupante { get; private set; }

    public Asiento(int numero)
    {
        Numero = numero;
    }

    public void Ocupar(Asistente asistente)
    {
        Ocupado = true;
        Ocupante = asistente;
    }
}
