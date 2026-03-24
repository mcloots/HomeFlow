using HomeFlow.BuildingBlocks.Application.Abstractions.Persistence;
using HomeFlow.BuildingBlocks.MultiTenancy.Models;
using HomeFlow.Modules.Households.Domain.Aggregates;
using HomeFlow.Modules.Households.Domain.Ids;
using HomeFlow.Modules.Scheduling.Domain.Aggregates;
using HomeFlow.Modules.Scheduling.Domain.Entities;
using HomeFlow.Modules.Scheduling.Domain.Ids;
using HomeFlow.Modules.Tenancy.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.BuildingBlocks.Infrastructure.Persistence;

public sealed class HomeFlowDbContext : DbContext, IUnitOfWork
{
    public HomeFlowDbContext(DbContextOptions<HomeFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdInvitation> HouseholdInvitations => Set<HouseholdInvitation>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenancy(modelBuilder);
        ConfigureHouseholds(modelBuilder);
        ConfigureHouseholdInvitations(modelBuilder);
        ConfigureScheduling(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureTenancy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(builder =>
        {
            builder.ToTable("tenants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new TenantId(value));

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();
        });
    }

    private static void ConfigureHouseholds(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Household>(builder =>
        {
            builder.ToTable("households");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdId(value));

            builder.Property(x => x.TenantId)
                .HasConversion(
                    id => id.Value,
                    value => new TenantId(value))
                .IsRequired();

            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasMany(x => x.Members)
               .WithOne()
               .HasForeignKey("HouseholdId")
               .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Household.Members))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<HouseholdMember>(builder =>
        {
            builder.ToTable("household_members");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdMemberId(value));

            builder.Property<HouseholdId>("HouseholdId")
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdId(value))
                .IsRequired();

            builder.Property(x => x.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique();
        });
    }

    private static void ConfigureHouseholdInvitations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HouseholdInvitation>(builder =>
        {
            builder.ToTable("household_invitations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdInvitationId(value));

            builder.Property(x => x.HouseholdId)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdId(value))
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(320)
                .IsRequired();

            builder.Property(x => x.Role)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(x => new { x.HouseholdId, x.Email, x.Status });
        });
    }

    private static void ConfigureScheduling(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(builder =>
        {
            builder.ToTable("appointments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new AppointmentId(value));

            builder.Property(x => x.TenantId)
                .HasConversion(
                    id => id.Value,
                    value => new TenantId(value))
                .IsRequired();

            builder.Property(x => x.HouseholdId)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdId(value))
                .IsRequired();

            builder.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.StartsAtUtc)
                .IsRequired();

            builder.Property(x => x.EndsAtUtc)
                .IsRequired();

            builder.Property(x => x.Location)
                .HasMaxLength(300);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasMany(x => x.Participants)
                .WithOne()
                .HasForeignKey("AppointmentId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata.FindNavigation(nameof(Appointment.Participants))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<AppointmentParticipant>(builder =>
        {
            builder.ToTable("appointment_participants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new AppointmentParticipantId(value));

            builder.Property<AppointmentId>("AppointmentId")
                .HasConversion(
                    id => id.Value,
                    value => new AppointmentId(value))
                .IsRequired();

            builder.Property(x => x.HouseholdMemberId)
                .HasConversion(
                    id => id.Value,
                    value => new HouseholdMemberId(value))
                .IsRequired();

            builder.HasIndex("AppointmentId", nameof(AppointmentParticipant.HouseholdMemberId))
                .IsUnique();
        });
    }
    public async new Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
    }
}