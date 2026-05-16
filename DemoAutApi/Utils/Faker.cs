namespace DemoAutApi.Utils
{
    public static class Faker
    {
        public static Empleado GenerateEmpleadoFake()
        {
            var faker = new Bogus.Faker();

            return new Empleado
            {
                Cedula = faker.Random.String2(10, "0123456789"),
                Nombre = faker.Person.FirstName,
                Apellidos = faker.Person.LastName,
                FechaNacimiento = faker.Person.DateOfBirth,
                Direccion = faker.Address.StreetName(),
                Correo = faker.Internet.Email(),
                SupervidorCedula = null,
                Salario = faker.Random.Number(0, 5000),
                DepartamentoNumero = faker.PickRandom(1, 2, 3),
            };
        }
    }
}
