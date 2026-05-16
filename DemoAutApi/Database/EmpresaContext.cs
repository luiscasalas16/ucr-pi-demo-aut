namespace DemoAutApi.Database;

internal partial class EmpresaContext : DbContext
{
    private readonly string _connectionString;

    public EmpresaContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EmpresaContext")!;
    }

    public EmpresaContext(DbContextOptions<EmpresaContext> options, IConfiguration configuration)
        : base(options)
    {
        _connectionString = configuration.GetConnectionString("EmpresaContext")!;
    }

    public virtual DbSet<Empleado> Empleados { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.UseSqlServer(_connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Cedula);

            entity.ToTable("Empleado", "rh");

            entity.HasIndex(e => e.Correo, "UN_Empleado_Correo").IsUnique();

            entity.Property(e => e.Cedula).HasMaxLength(10).IsUnicode(false);
            entity.Property(e => e.Apellidos).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Correo).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Direccion).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.FechaNacimiento).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.SupervidorCedula).HasMaxLength(10).IsUnicode(false);
        });
    }
}
