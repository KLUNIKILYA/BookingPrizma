using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Legacy;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure;

/// <summary>
/// Контекст системы бронирования. Владеет собственными таблицами Booking_*,
/// а легаси-таблицы (SingleService, SingleServiceGroup, CashboxVisitor) подключает
/// в режиме read-only и исключает из миграций (ExcludeFromMigrations), чтобы
/// миграции никогда не трогали существующую схему и таблицы аренды.
/// </summary>
public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }

    // ===== Собственные таблицы =====
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingServiceItem> BookingServiceItems => Set<BookingServiceItem>();
    public DbSet<BookingResource> Resources => Set<BookingResource>();
    public DbSet<BookingServiceSetting> ServiceSettings => Set<BookingServiceSetting>();

    // ===== Read-only легаси =====
    public DbSet<SingleService> SingleServices => Set<SingleService>();
    public DbSet<SingleServiceGroup> SingleServiceGroups => Set<SingleServiceGroup>();
    public DbSet<CashboxVisitor> CashboxVisitors => Set<CashboxVisitor>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // ---------- Booking_Resource ----------
        b.Entity<BookingResource>(e =>
        {
            e.ToTable("Booking_Resource");
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Color).HasMaxLength(16);
            e.HasIndex(x => x.VisitorId);
        });

        // ---------- Booking_ServiceSetting ----------
        b.Entity<BookingServiceSetting>(e =>
        {
            e.ToTable("Booking_ServiceSetting");
            e.HasKey(x => x.Id);
            e.Property(x => x.ColorOverride).HasMaxLength(16);
            e.HasIndex(x => x.ServiceId).IsUnique();
        });

        // ---------- Booking_Booking ----------
        b.Entity<Booking>(e =>
        {
            e.ToTable("Booking_Booking");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(300).IsRequired();
            e.Property(x => x.Note).HasMaxLength(2000);
            e.Property(x => x.Label).HasConversion<int>();
            e.HasIndex(x => new { x.ResourceId, x.TimeFrom });
            e.HasIndex(x => x.TimeFrom);

            e.HasOne(x => x.Resource)
                .WithMany(r => r.Bookings)
                .HasForeignKey(x => x.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(x => x.Services)
                .WithOne(s => s.Booking!)
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Booking_BookingService ----------
        b.Entity<BookingServiceItem>(e =>
        {
            e.ToTable("Booking_BookingService");
            e.HasKey(x => x.Id);
            e.Property(x => x.ServiceName).HasMaxLength(300).IsRequired();
            e.Property(x => x.PriceSnapshot).HasColumnType("numeric(18,2)");
            e.HasIndex(x => x.BookingId);
        });

        // ---------- Read-only легаси (исключены из миграций) ----------
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

        b.Entity<CashboxVisitor>(e =>
        {
            e.ToTable("CashboxVisitor", t =>
            {
                t.ExcludeFromMigrations();
                // У таблицы есть триггеры в БД. Без этого EF Core 8 вставляет с OUTPUT-клаузой,
                // что SQL Server запрещает для таблиц с триггерами (ошибка 334).
                t.HasTrigger("trg_CashboxVisitor");
            });
            e.HasKey(x => x.IdVisitor);
            e.Property(x => x.IdVisitor).ValueGeneratedOnAdd(); // IdVisitor — IDENTITY
        });
    }
}
