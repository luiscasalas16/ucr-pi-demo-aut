namespace DemoAutWeb.Models;

public class Empleado
{
    public string Cedula { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int Salario { get; set; }
}
