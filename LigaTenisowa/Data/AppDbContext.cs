using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
    { }

    public DbSet<Sezon> Sezony { get; set; }
    public DbSet<Gracz> Gracze { get; set; }
    public DbSet<Mecz> Mecze { get; set; }
    public DbSet<Set> Sety { get; set; }
    public DbSet<StatystykiMeczu> StatystykiMeczow { get; set; }
    public DbSet<Uzytkownik> Uzytkownicy { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Mecz -> Gracz1
        mb.Entity<Mecz>()
            .HasOne(m => m.Gracz1)
            .WithMany(g => g.MeczeJakoGracz1)
            .HasForeignKey(m => m.Gracz1Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Mecz -> Gracz2
        mb.Entity<Mecz>()
            .HasOne(m => m.Gracz2)
            .WithMany(g => g.MeczeJakoGracz2)
            .HasForeignKey(m => m.Gracz2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Mecz -> Zwyciezca
        mb.Entity<Mecz>()
            .HasOne(m => m.Zwyciezca)
            .WithMany(g => g.WygraneMecze)
            .HasForeignKey(m => m.ZwyciezcaId)
            .OnDelete(DeleteBehavior.Restrict);

        // StatystykiMeczu -> Mecz (1:1)
        mb.Entity<StatystykiMeczu>()
            .HasOne(s => s.Mecz)
            .WithOne(m => m.Statystyki)
            .HasForeignKey<StatystykiMeczu>(s => s.MeczId);

        // Uzytkownik -> Gracz (1:1 opcjonalne)
        mb.Entity<Uzytkownik>()
            .HasOne(u => u.Gracz)
            .WithOne(g => g.Uzytkownik)
            .HasForeignKey<Uzytkownik>(u => u.GraczId);
    }
}
