using DemoAutWeb.Models;

namespace DemoAutWeb.Utils
{
    public static class Faker
    {
        public static Empleado[] GenerateEmpleadosFake(int count)
        {
            var faker = new Bogus.Faker();

            return
            [
                .. Enumerable
                    .Range(0, count)
                    .Select(_ => new Empleado
                    {
                        Cedula = faker.Random.String2(10, "0123456789"),
                        Nombre = faker.Person.FirstName,
                        Apellidos = faker.Person.LastName,
                        Correo = faker.Internet.Email(),
                        Salario = faker.Random.Number(0, 5000),
                    }),
            ];
        }
    }
}
