using Microsoft.EntityFrameworkCore;
using MediCareMS.Models.Entities.Auth;
using MediCareMS.Models.Entities.Doctor;
using MediCareMS.Models.Entities.Patient;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Entities.Prescription;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Entities.System;
using MediCareMS.Models.Entities.Chat;

namespace MediCareMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Doctor
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<DoctorProfile> Doctors => Set<DoctorProfile>();
    public DbSet<DoctorSchedule> DoctorSchedules => Set<DoctorSchedule>();

    // Patient
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalHistory> MedicalHistories => Set<MedicalHistory>();
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();

    // Appointment
    public DbSet<Appointment> Appointments => Set<Appointment>();

    // Prescription
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionMedicine> PrescriptionMedicines => Set<PrescriptionMedicine>();
    public DbSet<LabTestRequest> LabTestRequests => Set<LabTestRequest>();

    // Billing
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    // System
    public DbSet<HospitalProfile> HospitalProfiles => Set<HospitalProfile>();
    public DbSet<Notice> Notices => Set<Notice>();

    // Invitations
    public DbSet<Invitation> Invitations => Set<Invitation>();

    // Chat / AI
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite keys
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(x => new { x.RoleId, x.PermissionId });

        // Chat relationships
        modelBuilder.Entity<ChatSession>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .Property(m => m.Sender)
            .HasConversion<string>();

        // Unique indexes
        modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Invoice>().HasIndex(x => x.InvoiceNo).IsUnique();
        modelBuilder.Entity<Appointment>().HasIndex(x => x.AppointmentNo).IsUnique();
        modelBuilder.Entity<DoctorProfile>().HasIndex(x => x.DoctorNo).IsUnique();
        modelBuilder.Entity<Patient>().HasIndex(x => x.PatientNo).IsUnique();

        // ── Performance indexes for Patient list queries ──────────────────────
        // Covers: WHERE IsDeleted=0 ORDER BY FullName  (default list)
        modelBuilder.Entity<Patient>()
            .HasIndex(x => new { x.IsDeleted, x.FullName })
            .HasDatabaseName("IX_Patients_IsDeleted_FullName");

        // Covers: WHERE IsDeleted=0 AND BloodGroup=?  (blood-group filter)
        modelBuilder.Entity<Patient>()
            .HasIndex(x => new { x.IsDeleted, x.BloodGroup })
            .HasDatabaseName("IX_Patients_IsDeleted_BloodGroup");

        // Covers: WHERE PatientId=? AND IsDeleted=0  (visit-count sub-query)
        modelBuilder.Entity<MediCareMS.Models.Entities.Appointment.Appointment>()
            .HasIndex(x => new { x.PatientId, x.IsDeleted })
            .HasDatabaseName("IX_Appointments_PatientId_IsDeleted");

        // Decimal precision
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties()
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }

        // Restrict delete behavior
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        DbInitializer.Seed(modelBuilder);
    }
}
