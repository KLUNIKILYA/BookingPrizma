using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Legacy;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<BookingResExtra> ResExtras => Set<BookingResExtra>();
    public DbSet<BookingResServiceItem> ResServices => Set<BookingResServiceItem>();
    public DbSet<BookingZoneType> ZoneTypes => Set<BookingZoneType>();
    public DbSet<BookingZoneAssignment> ZoneAssignments => Set<BookingZoneAssignment>();
    public DbSet<BookingLabelDef> Labels => Set<BookingLabelDef>();

    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<ZoneReservation> ZoneReservations => Set<ZoneReservation>();
    public DbSet<TLogin> TLogins => Set<TLogin>();
    public DbSet<SingleService> SingleServices => Set<SingleService>();
    public DbSet<SingleServiceGroup> SingleServiceGroups => Set<SingleServiceGroup>();
    public DbSet<CashboxVisitor> CashboxVisitors => Set<CashboxVisitor>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketZone> TicketZones => Set<TicketZone>();
    public DbSet<TicketFolder> TicketFolders => Set<TicketFolder>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<BookingResExtra>(e =>
        {
            e.ToTable("Booking_ResExtra");
            e.HasKey(x => x.ReservationId);
            e.Property(x => x.ReservationId).ValueGeneratedNever();
            e.Property(x => x.Title).HasMaxLength(300);
            e.Property(x => x.Label).HasConversion<int>();
            e.Property(x => x.TariffName).HasMaxLength(300);
            e.Property(x => x.TariffPrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.CelebrantName).HasMaxLength(200);
            e.Property(x => x.CelebrantBirthDate).HasColumnType("date");
            e.Property(x => x.IsPrepaid).HasDefaultValue(false);
            e.Property(x => x.PrepaidAmount).HasColumnType("numeric(18,2)");
            e.Property(x => x.IsCancelled).HasDefaultValue(false);
            e.HasMany(x => x.Services)
                .WithOne(s => s.Extra!)
                .HasForeignKey(s => s.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        b.Entity<BookingZoneType>(e =>
        {
            e.ToTable("Booking_ZoneType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Active).HasDefaultValue(true);
            e.HasMany(x => x.Zones)
                .WithOne(z => z.Type!)
                .HasForeignKey(z => z.ZoneTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BookingZoneAssignment>(e =>
        {
            e.ToTable("Booking_ZoneAssignment");
            e.HasKey(x => x.ZoneId);
            e.Property(x => x.ZoneId).ValueGeneratedNever();
            e.HasIndex(x => x.ZoneTypeId);
        });

        b.Entity<BookingLabelDef>(e =>
        {
            e.ToTable("Booking_Label");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Color).HasMaxLength(9).IsRequired();
            e.Property(x => x.IsActive).HasDefaultValue(true);
        });

        b.Entity<ZoneReservation>(e =>
        {
            e.ToTable("ZoneReservation", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            e.Property(x => x.ZoneId).HasColumnName("ZoneID");
            e.Property(x => x.OrderId).HasColumnName("OrderID");
        });

        b.Entity<Zone>(e =>
        {
            e.ToTable("Zones", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.IdZone);
        });

        b.Entity<TLogin>(e =>
        {
            e.ToTable("TLogins", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Fid);
            e.Property(x => x.Fid).HasColumnName("FID");
            e.Property(x => x.Flogin).HasColumnName("FLOGIN");
            e.Property(x => x.Fuser).HasColumnName("FUSER");
            e.Property(x => x.Factive).HasColumnName("FACTIVE");
        });

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
                t.HasTrigger("trg_CashboxVisitor");
            });
            e.HasKey(x => x.IdVisitor);
            e.Property(x => x.IdVisitor).ValueGeneratedOnAdd();
        });

        b.Entity<Ticket>(e =>
        {
            e.ToTable("Ticket", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.IdTicket);
            e.Property(x => x.TicketFolderId).HasColumnName("TicketFolderID");
            e.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.OnePrice).HasColumnType("numeric(18,2)");
        });

        b.Entity<TicketFolder>(e =>
        {
            e.ToTable("TicketFolder", t => t.ExcludeFromMigrations());
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("ID");
            e.Property(x => x.ParentId).HasColumnName("ParentID");
        });

        b.Entity<TicketZone>(e =>
        {
            e.ToTable("TicketZone", t => t.ExcludeFromMigrations());
            e.HasKey(x => new { x.IdTicket, x.IdZone });
        });
    }
}
