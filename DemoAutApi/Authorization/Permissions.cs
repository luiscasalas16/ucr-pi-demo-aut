namespace DemoAutApi.Authorization;

// Centraliza los nombres de los permisos funcionales utilizados por el API.
// Esto evita dispersar strings literales en los endpoints y reduce errores de escritura
// al declarar los permisos requeridos para cada operación.
internal static class Permissions
{
    // Permisos asociados a las operaciones sobre empleados.
    internal static class Empleados
    {
        // Permite consultar información de empleados.
        public const string Read = "Empleado.Read";

        // Permite ejecutar operaciones de escritura relacionadas con empleados.
        public const string Write = "Empleado.Write";
    }
}
