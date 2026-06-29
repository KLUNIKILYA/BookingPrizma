using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Legacy;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure;

/// <summary>
/// Контекст бронирования поверх боевой БД PPS_Prizma.
/// Брони хранятся в реальной таблице ZoneReservation (комнаты — Zones, сотрудники — TLogins),
/// а доп-поля (официант/метка/клиент/услуги) — в собственных таблицах Booking_ResExtra/Booking_ResService.
/// Все легаси-таблицы read-only и исключены из миграций.
/// </summary>
public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    // ===== Собственные таблицы (companion) =====
    public DbSet<BookingResExtra> ResExtras => Set<BookingResExtra>();
    public DbSet<BookingResServiceItem> ResServices => Set<BookingResServiceItem>();

    // ===== Боевые таблицы (read-only, кроме ZoneReservation, куда пишем) =====
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<ZoneReservation> ZoneReservations => Set<ZoneReservation>();
    public DbSet<TLogin> TLogins => Set<TLogin>();
    public DbSet<SingleService> SingleServices => Set<SingleService>();
    public DbSet<SingleServiceGroup> SingleServiceGroups => Set<SingleServiceGroup>();
    public DbSet<CashboxVisitor> CashboxVisitors => Set<CashboxVisitor>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketZone> TicketZones => Set<TicketZone>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ---------- Booking_ResExtra (1:1 с бронью) ----------
        b.Entity<BookingResExtra>(e =>
        {
            e.ToTable("Booking_ResExtra");
            e.HasKey(x => x.ReservationId);
            e.Property(x => x.ReservationId).ValueGeneratedNever(); // = ZoneReservation.ID
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Label).HasConversion<int>();
            e.Property(x => x.TariffName).HasMaxLength(300);
            e.Property(x => x.TariffPrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.CelebrantName).HasMaxLength(200);
            e.Property(x => x.CelebrantBirthDate).HasColumnType("date");
            e.Property(x => x.IsPrepaid).HasDefaultValue(false);
            e.Property(x => x.PrepaidAmount).HasColumnType("numeric(18,2)");
            e.HasMany(x => x.Services)
                .WithOne(s => s.Extra!)
                .HasForeignKey(s => s.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Booking_ResService ----------
        b.Entity<BookingResServiceItem>(e =>
        {
            e.ToTable("Booking_ResService");
            e.HasKey(x => x.Id);
            e.Property(x => x.ServiceName).HasMaxLength(300).IsRequired();
            e.Property(x => x.PriceSnapshot).HasColumnType("numeric(18,2)");
            e.Property(x => x.Quantity).HasDefaultValue(1);
            e.Property(x => x.IsTicket).HasDefaultValue(false);
            e.HasIndex(x => x.ReservationId);
        });

        // ---------- ZoneReservation (боевая, пишем; из миграций исключена) ----------
        b.Entity<ZoneReservation>(e =>
        {
            e.ToTable("ZoneReservation", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd(); // identity
            e.Property(x => x.ZoneId).HasColumnName("ZoneID");
            e.Property(x => x.OrderId).HasColumnName("OrderID");
        });

        // ---------- Zones (read-only) ----------
        b.Entity<Zone>(e =>
        {
            e.ToTable("Zones", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.IdZone);
        });

        // ---------- TLogins (read-only) ----------
        b.Entity<TLogin>(e =>
        {
            e.ToTable("TLogins", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Fid);
            e.Property(x => x.Fid).HasColumnName("FID");
            e.Property(x => x.Flogin).HasColumnName("FLOGIN");
            e.Property(x => x.Fuser).HasColumnName("FUSER");
            e.Property(x => x.Factive).HasColumnName("FACTIVE");
        });

        // ---------- SingleService / SingleServiceGroup (read-only) ----------
        b.Entity<SingleService>(e =>
        {
            e.ToTable("SingleService", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.SingleServiceGroupId).HasColumnName("SingleServiceGroupID");
            e.Property(x => x.Price).HasColumnType("numeric(18,2)");
        });

        b.Entity<SingleServiceGroup>(e =>
        {
            e.ToTable("SingleServiceGroup", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.ParentId).HasColumnName("ParentID");
        });

        // ---------- CashboxVisitor (read-only + создание клиента; есть триггеры) ----------
        b.Entity<CashboxVisitor>(e =>
        {
            e.ToTable("CashboxVisitor", t =>
            {
                t.ExcludeFromMigrations();
                t.HasTrigger("trg_CashboxVisitor"); // 2 триггера в БД → EF не должен использовать OUTPUT
            });
            e.HasKey(x => x.IdVisitor);
            e.Property(x => x.IdVisitor).ValueGeneratedOnAdd();
        });

        // ---------- Ticket (read-only): тарифы на бронь + билеты-услуги ----------
        b.Entity<Ticket>(e =>
        {
            e.ToTable("Ticket", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.IdTicket);
            e.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.OnePrice).HasColumnType("numeric(18,2)");
        });

        // ---------- TicketZone (read-only): связь билет↔зона + длительность тарифа ----------
        b.Entity<TicketZone>(e =>
        {
            e.ToTable("TicketZone", t => t.ExcludeFromMigrations());
            e.HasKey(x => new { x.IdTicket, x.IdZone });
        });
    }
}
