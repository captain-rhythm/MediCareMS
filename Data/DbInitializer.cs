using Microsoft.EntityFrameworkCore;
using MediCareMS.Models.Entities.Auth;
using MediCareMS.Models.Entities.Doctor;
using MediCareMS.Models.Entities.Patient;
using MediCareMS.Models.Entities.Appointment;
using MediCareMS.Models.Entities.Billing;
using MediCareMS.Models.Entities.System;
using MediCareMS.Models.Enums;

namespace MediCareMS.Data;

public static class DbInitializer
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Super Admin", Description = "System owner with all permissions", CreatedAt = createdAt },
            new Role { Id = 2, Name = "Hospital Admin", Description = "Manages hospital operations", CreatedAt = createdAt },
            new Role { Id = 3, Name = "Doctor", Description = "Doctor access portal", CreatedAt = createdAt },
            new Role { Id = 4, Name = "Receptionist", Description = "Appointment and patient desk", CreatedAt = createdAt },
            new Role { Id = 5, Name = "Nurse", Description = "Nursing operations", CreatedAt = createdAt },
            new Role { Id = 6, Name = "Patient", Description = "Patient self-portal", CreatedAt = createdAt });

        // Default Admin User (password: Admin@12345)
        modelBuilder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = 1,
                UserName = "admin",
                Email = "admin@medicare.local",
                PasswordHash = "ChangeThisHash",
                Status = AccountStatus.Active,
                IsEmailConfirmed = true,
                CreatedAt = createdAt
            });

        modelBuilder.Entity<UserRole>().HasData(new UserRole { UserId = 1, RoleId = 1 });

        // Permissions
        var permissionId = 1;
        var modules = new[] { "Dashboard", "Users", "Roles", "Doctors", "Patients", "Appointments",
            "Prescriptions", "LabTests", "Billing", "Payments", "Departments", "Reports", "Settings", "Notices" };
        var actions = new[] { "View", "Create", "Edit", "Delete", "Approve", "Manage" };

        var permissions = modules.SelectMany(module => actions
            .Select(action => new Permission
            {
                Id = permissionId++,
                Module = module,
                ModuleName = module,
                Action = action,
                Code = $"{module}.{action}",
                CanCreate = action is "Create" or "Manage",
                CanRead = action is "View" or "Manage",
                CanUpdate = action is "Edit" or "Approve" or "Manage",
                CanDelete = action is "Delete" or "Manage",
                CreatedAt = createdAt
            })).ToArray();
        modelBuilder.Entity<Permission>().HasData(permissions);

        var adminPermissions = permissions.Select(p => new RolePermission { RoleId = 1, PermissionId = p.Id });
        var doctorPermissions = permissions
            .Where(p => p.Code is "Dashboard.View" or "Appointments.View" or "Patients.View"
                or "Prescriptions.Create" or "Prescriptions.View" or "LabTests.Create" or "LabTests.View")
            .Select(p => new RolePermission { RoleId = 3, PermissionId = p.Id });
        var receptionistPermissions = permissions
            .Where(p => p.ModuleName is "Dashboard" or "Appointments" or "Patients" or "Billing" && p.Action is not "Delete")
            .Select(p => new RolePermission { RoleId = 4, PermissionId = p.Id });

        modelBuilder.Entity<RolePermission>().HasData(
            adminPermissions.Concat(doctorPermissions).Concat(receptionistPermissions));

        // Departments
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = 1, Name = "General Medicine", Description = "General health consultations", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 2, Name = "Cardiology", Description = "Heart and cardiovascular care", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 3, Name = "Orthopedics", Description = "Bone and joint care", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 4, Name = "Pediatrics", Description = "Child healthcare", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 5, Name = "Gynecology", Description = "Women's health", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 6, Name = "Dermatology", Description = "Skin care", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 7, Name = "Neurology", Description = "Brain and nervous system", IsActive = true, CreatedAt = createdAt },
            new Department { Id = 8, Name = "ENT", Description = "Ear, Nose, Throat", IsActive = true, CreatedAt = createdAt });

        // Specializations
        modelBuilder.Entity<Specialization>().HasData(
            new Specialization { Id = 1, Name = "General Physician", CreatedAt = createdAt },
            new Specialization { Id = 2, Name = "Cardiologist", CreatedAt = createdAt },
            new Specialization { Id = 3, Name = "Orthopedic Surgeon", CreatedAt = createdAt },
            new Specialization { Id = 4, Name = "Pediatrician", CreatedAt = createdAt },
            new Specialization { Id = 5, Name = "Gynecologist", CreatedAt = createdAt },
            new Specialization { Id = 6, Name = "Dermatologist", CreatedAt = createdAt },
            new Specialization { Id = 7, Name = "Neurologist", CreatedAt = createdAt },
            new Specialization { Id = 8, Name = "ENT Specialist", CreatedAt = createdAt });

        // Doctors
        modelBuilder.Entity<DoctorProfile>().HasData(
            new DoctorProfile { Id = 1, DoctorNo = "DOC-001", FullName = "Dr. Ahmed Hasan", DepartmentId = 1, SpecializationId = 1, Qualification = "MBBS, FCPS", ExperienceYears = 12, MobileNumber = "01700000001", ConsultationFee = 800, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 2, DoctorNo = "DOC-002", FullName = "Dr. Farida Begum", DepartmentId = 2, SpecializationId = 2, Qualification = "MBBS, MD (Cardiology)", ExperienceYears = 15, MobileNumber = "01700000002", ConsultationFee = 1200, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 3, DoctorNo = "DOC-003", FullName = "Dr. Rahim Chowdhury", DepartmentId = 4, SpecializationId = 4, Qualification = "MBBS, DCH", ExperienceYears = 8, MobileNumber = "01700000003", ConsultationFee = 700, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 4, DoctorNo = "DOC-004", FullName = "Dr. Samira Khan", DepartmentId = 3, SpecializationId = 3, Qualification = "MBBS, MS (Ortho)", ExperienceYears = 10, MobileNumber = "01700000004", ConsultationFee = 1000, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 5, DoctorNo = "DOC-005", FullName = "Dr. Ayesha Siddiqa", DepartmentId = 5, SpecializationId = 5, Qualification = "MBBS, FCPS (Obs & Gynae)", ExperienceYears = 20, MobileNumber = "01700000005", ConsultationFee = 1500, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 6, DoctorNo = "DOC-006", FullName = "Dr. Kamal Hossain", DepartmentId = 6, SpecializationId = 6, Qualification = "MBBS, DDV", ExperienceYears = 5, MobileNumber = "01700000006", ConsultationFee = 600, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 7, DoctorNo = "DOC-007", FullName = "Dr. Nabila Rahman", DepartmentId = 7, SpecializationId = 7, Qualification = "MBBS, MD (Neurology)", ExperienceYears = 14, MobileNumber = "01700000007", ConsultationFee = 1200, Status = DoctorStatus.Available, CreatedAt = createdAt },
            new DoctorProfile { Id = 8, DoctorNo = "DOC-008", FullName = "Dr. Tariqul Islam", DepartmentId = 8, SpecializationId = 8, Qualification = "MBBS, DLO", ExperienceYears = 9, MobileNumber = "01700000008", ConsultationFee = 800, Status = DoctorStatus.Available, CreatedAt = createdAt });

        // Doctor Schedules
        modelBuilder.Entity<DoctorSchedule>().HasData(
            new DoctorSchedule { Id = 1, DoctorId = 1, DayOfWeek = DayOfWeekEnum.Sunday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(13, 0), SlotDurationMinutes = 20, MaxPatients = 12, IsActive = true, CreatedAt = createdAt },
            new DoctorSchedule { Id = 2, DoctorId = 1, DayOfWeek = DayOfWeekEnum.Tuesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(13, 0), SlotDurationMinutes = 20, MaxPatients = 12, IsActive = true, CreatedAt = createdAt },
            new DoctorSchedule { Id = 3, DoctorId = 2, DayOfWeek = DayOfWeekEnum.Monday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(14, 0), SlotDurationMinutes = 30, MaxPatients = 8, IsActive = true, CreatedAt = createdAt },
            new DoctorSchedule { Id = 4, DoctorId = 3, DayOfWeek = DayOfWeekEnum.Wednesday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(12, 0), SlotDurationMinutes = 15, MaxPatients = 12, IsActive = true, CreatedAt = createdAt });

        // Sample Patients
        modelBuilder.Entity<Patient>().HasData(
            new Patient { Id = 1, PatientNo = "PAT-2026-0001", FullName = "Karim Uddin", DateOfBirth = new DateTime(1985, 3, 15), Gender = Gender.Male, BloodGroup = BloodGroup.B_Positive, MobileNumber = "01800000001", Nationality = "Bangladeshi", CreatedAt = createdAt },
            new Patient { Id = 2, PatientNo = "PAT-2026-0002", FullName = "Sultana Akter", DateOfBirth = new DateTime(1992, 7, 22), Gender = Gender.Female, BloodGroup = BloodGroup.A_Positive, MobileNumber = "01800000002", Nationality = "Bangladeshi", CreatedAt = createdAt });

        // Sample Appointments
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, AppointmentNo = "APT-2026-0001", PatientId = 1, DoctorId = 1, AppointmentDate = new DateOnly(2026, 5, 10), AppointmentTime = new TimeOnly(9, 0), TokenNumber = 1, Status = AppointmentStatus.Confirmed, ChiefComplaint = "Fever and headache", CreatedAt = createdAt },
            new Appointment { Id = 2, AppointmentNo = "APT-2026-0002", PatientId = 2, DoctorId = 2, AppointmentDate = new DateOnly(2026, 5, 12), AppointmentTime = new TimeOnly(10, 0), TokenNumber = 1, Status = AppointmentStatus.Pending, ChiefComplaint = "Chest pain", CreatedAt = createdAt });

        // Sample Invoices
        modelBuilder.Entity<Invoice>().HasData(
            new Invoice { Id = 1, InvoiceNo = "INV-2026-0001", AppointmentId = 1, PatientId = 1, DoctorId = 1, ConsultationFee = 800, TestFee = 0, OtherCharges = 0, Discount = 0, TotalAmount = 800, PaidAmount = 800, Status = PaymentStatus.Paid, DueDate = new DateOnly(2026, 5, 10), CreatedAt = createdAt });

        // Hospital Profile
        modelBuilder.Entity<HospitalProfile>().HasData(
            new HospitalProfile { Id = 1, Name = "MediCare Hospital", Address = "Dhaka, Bangladesh", Phone = "01000000000", Email = "info@medicare.local", CreatedAt = createdAt });

        // Notice
        modelBuilder.Entity<Notice>().HasData(
            new Notice { Id = 1, Title = "Welcome to MediCare Hospital Management System", Body = "All modules are now active and ready for use.", AudienceRole = "All", PublishAt = createdAt, CreatedAt = createdAt });
    }
}
