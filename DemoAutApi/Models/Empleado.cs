namespace DemoAutApi.Models;

public partial class Empleado
{
    public string Cedula { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string Direccion { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public int Salario { get; set; }

    public int DepartamentoNumero { get; set; }

    public string? SupervidorCedula { get; set; }
}
