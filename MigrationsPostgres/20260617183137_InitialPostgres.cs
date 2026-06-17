using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediCareMS.MigrationsPostgres
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL") return;

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HospitalProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Website = table.Column<string>(type: "text", nullable: true),
                    LogoPath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    AudienceRole = table.Column<string>(type: "text", nullable: true),
                    PublishAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientNo = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    BloodGroup = table.Column<int>(type: "integer", nullable: true),
                    MobileNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Nationality = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "text", nullable: true),
                    KnownAllergies = table.Column<string>(type: "text", nullable: true),
                    ChronicConditions = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Module = table.Column<string>(type: "text", nullable: false),
                    ModuleName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    CanCreate = table.Column<bool>(type: "boolean", nullable: false),
                    CanRead = table.Column<bool>(type: "boolean", nullable: false),
                    CanUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsEmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ActivationToken = table.Column<string>(type: "text", nullable: true),
                    ActivationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResetToken = table.Column<string>(type: "text", nullable: true),
                    ResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DiagnosedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsOngoing = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientDocuments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorNo = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    DepartmentId = table.Column<int>(type: "integer", nullable: false),
                    SpecializationId = table.Column<int>(type: "integer", nullable: false),
                    Qualification = table.Column<string>(type: "text", nullable: true),
                    ExperienceYears = table.Column<int>(type: "integer", nullable: true),
                    BmdcRegNo = table.Column<string>(type: "text", nullable: true),
                    MobileNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ChamberAddress = table.Column<string>(type: "text", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    ProfileImagePath = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Doctors_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RegisteredFullName = table.Column<string>(type: "text", nullable: true),
                    RegisteredPhone = table.Column<string>(type: "text", nullable: true),
                    RequestedRole = table.Column<string>(type: "text", nullable: true),
                    InvitedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invitations_Users_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentNo = table.Column<string>(type: "text", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AppointmentTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TokenNumber = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    MaxPatients = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSchedules_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceNo = table.Column<string>(type: "text", nullable: false),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    ConsultationFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TestFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    FollowUpInstructions = table.Column<string>(type: "text", nullable: true),
                    FollowUpDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvoiceId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    TransactionReference = table.Column<string>(type: "text", nullable: true),
                    SslTransactionId = table.Column<string>(type: "text", nullable: true),
                    SslSessionKey = table.Column<string>(type: "text", nullable: true),
                    SslValidationId = table.Column<string>(type: "text", nullable: true),
                    SslCardType = table.Column<string>(type: "text", nullable: true),
                    SslCardIssuer = table.Column<string>(type: "text", nullable: true),
                    SslStatus = table.Column<int>(type: "integer", nullable: false),
                    SslBankTransactionId = table.Column<string>(type: "text", nullable: true),
                    SslGwVersion = table.Column<string>(type: "text", nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabTestRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    TestName = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReportFilePath = table.Column<string>(type: "text", nullable: true),
                    ReportUploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTestRequests_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionMedicines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrescriptionId = table.Column<int>(type: "integer", nullable: false),
                    MedicineName = table.Column<string>(type: "text", nullable: false),
                    Dosage = table.Column<string>(type: "text", nullable: true),
                    Frequency = table.Column<string>(type: "text", nullable: true),
                    Duration = table.Column<string>(type: "text", nullable: true),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionMedicines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionMedicines_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "General health consultations", true, false, "General Medicine", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Heart and cardiovascular care", true, false, "Cardiology", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bone and joint care", true, false, "Orthopedics", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Child healthcare", true, false, "Pediatrics", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Women's health", true, false, "Gynecology", null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Skin care", true, false, "Dermatology", null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Brain and nervous system", true, false, "Neurology", null },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ear, Nose, Throat", true, false, "ENT", null }
                });

            migrationBuilder.InsertData(
                table: "HospitalProfiles",
                columns: new[] { "Id", "Address", "CreatedAt", "Email", "LogoPath", "Name", "Phone", "UpdatedAt", "Website" },
                values: new object[] { 1, "Dhaka, Bangladesh", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "info@medicare.local", null, "MediCare Hospital", "01000000000", null, null });

            migrationBuilder.InsertData(
                table: "Notices",
                columns: new[] { "Id", "AudienceRole", "Body", "CreatedAt", "CreatedBy", "IsDeleted", "PublishAt", "Title" },
                values: new object[] { 1, "All", "All modules are now active and ready for use.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, false, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome to MediCare Hospital Management System" });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "Address", "BloodGroup", "ChronicConditions", "CreatedAt", "CreatedBy", "DateOfBirth", "Email", "EmergencyContactName", "EmergencyContactPhone", "EmergencyContactRelation", "FullName", "Gender", "IsDeleted", "KnownAllergies", "MobileNumber", "Nationality", "PatientNo", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { 1, null, 3, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(1985, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, null, "Karim Uddin", 0, false, null, "01800000001", "Bangladeshi", "PAT-2026-0001", null, null, null },
                    { 2, null, 1, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(1992, 7, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, null, "Sultana Akter", 1, false, null, "01800000002", "Bangladeshi", "PAT-2026-0002", null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "CanCreate", "CanDelete", "CanRead", "CanUpdate", "Code", "CreatedAt", "IsDeleted", "Module", "ModuleName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "View", false, false, true, false, "Dashboard.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 2, "Create", true, false, false, false, "Dashboard.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 3, "Edit", false, false, false, true, "Dashboard.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 4, "Delete", false, true, false, false, "Dashboard.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 5, "Approve", false, false, false, true, "Dashboard.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 6, "Manage", true, true, true, true, "Dashboard.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dashboard", "Dashboard", null },
                    { 7, "View", false, false, true, false, "Users.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 8, "Create", true, false, false, false, "Users.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 9, "Edit", false, false, false, true, "Users.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 10, "Delete", false, true, false, false, "Users.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 11, "Approve", false, false, false, true, "Users.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 12, "Manage", true, true, true, true, "Users.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Users", "Users", null },
                    { 13, "View", false, false, true, false, "Roles.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 14, "Create", true, false, false, false, "Roles.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 15, "Edit", false, false, false, true, "Roles.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 16, "Delete", false, true, false, false, "Roles.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 17, "Approve", false, false, false, true, "Roles.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 18, "Manage", true, true, true, true, "Roles.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Roles", "Roles", null },
                    { 19, "View", false, false, true, false, "Doctors.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 20, "Create", true, false, false, false, "Doctors.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 21, "Edit", false, false, false, true, "Doctors.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 22, "Delete", false, true, false, false, "Doctors.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 23, "Approve", false, false, false, true, "Doctors.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 24, "Manage", true, true, true, true, "Doctors.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Doctors", "Doctors", null },
                    { 25, "View", false, false, true, false, "Patients.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 26, "Create", true, false, false, false, "Patients.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 27, "Edit", false, false, false, true, "Patients.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 28, "Delete", false, true, false, false, "Patients.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 29, "Approve", false, false, false, true, "Patients.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 30, "Manage", true, true, true, true, "Patients.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Patients", "Patients", null },
                    { 31, "View", false, false, true, false, "Appointments.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 32, "Create", true, false, false, false, "Appointments.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 33, "Edit", false, false, false, true, "Appointments.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 34, "Delete", false, true, false, false, "Appointments.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 35, "Approve", false, false, false, true, "Appointments.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 36, "Manage", true, true, true, true, "Appointments.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Appointments", "Appointments", null },
                    { 37, "View", false, false, true, false, "Prescriptions.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 38, "Create", true, false, false, false, "Prescriptions.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 39, "Edit", false, false, false, true, "Prescriptions.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 40, "Delete", false, true, false, false, "Prescriptions.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 41, "Approve", false, false, false, true, "Prescriptions.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 42, "Manage", true, true, true, true, "Prescriptions.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Prescriptions", "Prescriptions", null },
                    { 43, "View", false, false, true, false, "LabTests.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 44, "Create", true, false, false, false, "LabTests.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 45, "Edit", false, false, false, true, "LabTests.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 46, "Delete", false, true, false, false, "LabTests.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 47, "Approve", false, false, false, true, "LabTests.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 48, "Manage", true, true, true, true, "LabTests.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "LabTests", "LabTests", null },
                    { 49, "View", false, false, true, false, "Billing.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 50, "Create", true, false, false, false, "Billing.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 51, "Edit", false, false, false, true, "Billing.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 52, "Delete", false, true, false, false, "Billing.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 53, "Approve", false, false, false, true, "Billing.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 54, "Manage", true, true, true, true, "Billing.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Billing", "Billing", null },
                    { 55, "View", false, false, true, false, "Payments.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 56, "Create", true, false, false, false, "Payments.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 57, "Edit", false, false, false, true, "Payments.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 58, "Delete", false, true, false, false, "Payments.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 59, "Approve", false, false, false, true, "Payments.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 60, "Manage", true, true, true, true, "Payments.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Payments", "Payments", null },
                    { 61, "View", false, false, true, false, "Departments.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 62, "Create", true, false, false, false, "Departments.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 63, "Edit", false, false, false, true, "Departments.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 64, "Delete", false, true, false, false, "Departments.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 65, "Approve", false, false, false, true, "Departments.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 66, "Manage", true, true, true, true, "Departments.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Departments", "Departments", null },
                    { 67, "View", false, false, true, false, "Reports.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 68, "Create", true, false, false, false, "Reports.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 69, "Edit", false, false, false, true, "Reports.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 70, "Delete", false, true, false, false, "Reports.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 71, "Approve", false, false, false, true, "Reports.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 72, "Manage", true, true, true, true, "Reports.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Reports", "Reports", null },
                    { 73, "View", false, false, true, false, "Settings.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 74, "Create", true, false, false, false, "Settings.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 75, "Edit", false, false, false, true, "Settings.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 76, "Delete", false, true, false, false, "Settings.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 77, "Approve", false, false, false, true, "Settings.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 78, "Manage", true, true, true, true, "Settings.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Settings", "Settings", null },
                    { 79, "View", false, false, true, false, "Notices.View", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null },
                    { 80, "Create", true, false, false, false, "Notices.Create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null },
                    { 81, "Edit", false, false, false, true, "Notices.Edit", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null },
                    { 82, "Delete", false, true, false, false, "Notices.Delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null },
                    { 83, "Approve", false, false, false, true, "Notices.Approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null },
                    { 84, "Manage", true, true, true, true, "Notices.Manage", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Notices", "Notices", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System owner with all permissions", false, "Super Admin", null, null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Manages hospital operations", false, "Hospital Admin", null, null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Doctor access portal", false, "Doctor", null, null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Appointment and patient desk", false, "Receptionist", null, null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Nursing operations", false, "Nurse", null, null },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Patient self-portal", false, "Patient", null, null }
                });

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "General Physician" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Cardiologist" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Orthopedic Surgeon" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Pediatrician" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Gynecologist" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dermatologist" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Neurologist" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "ENT Specialist" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ActivationToken", "ActivationTokenExpiry", "CreatedAt", "CreatedBy", "Email", "IsDeleted", "IsEmailConfirmed", "LastLoginAt", "PasswordHash", "PhoneNumber", "ResetToken", "ResetTokenExpiry", "Status", "UpdatedAt", "UpdatedBy", "UserName" },
                values: new object[] { 1, null, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@medicare.local", false, true, null, "ChangeThisHash", null, null, null, 1, null, null, "admin" });

            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Bio", "BmdcRegNo", "ChamberAddress", "ConsultationFee", "CreatedAt", "CreatedBy", "DepartmentId", "DoctorNo", "Email", "ExperienceYears", "FullName", "IsDeleted", "MobileNumber", "ProfileImagePath", "Qualification", "SpecializationId", "Status", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { 1, null, null, null, 800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "DOC-001", null, 12, "Dr. Ahmed Hasan", false, "01700000001", null, "MBBS, FCPS", 1, 0, null, null, null },
                    { 2, null, null, null, 1200m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, "DOC-002", null, 15, "Dr. Farida Begum", false, "01700000002", null, "MBBS, MD (Cardiology)", 2, 0, null, null, null },
                    { 3, null, null, null, 700m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 4, "DOC-003", null, 8, "Dr. Rahim Chowdhury", false, "01700000003", null, "MBBS, DCH", 4, 0, null, null, null },
                    { 4, null, null, null, 1000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "DOC-004", null, 10, "Dr. Samira Khan", false, "01700000004", null, "MBBS, MS (Ortho)", 3, 0, null, null, null },
                    { 5, null, null, null, 1500m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, "DOC-005", null, 20, "Dr. Ayesha Siddiqa", false, "01700000005", null, "MBBS, FCPS (Obs & Gynae)", 5, 0, null, null, null },
                    { 6, null, null, null, 600m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, "DOC-006", null, 5, "Dr. Kamal Hossain", false, "01700000006", null, "MBBS, DDV", 6, 0, null, null, null },
                    { 7, null, null, null, 1200m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, "DOC-007", null, 14, "Dr. Nabila Rahman", false, "01700000007", null, "MBBS, MD (Neurology)", 7, 0, null, null, null },
                    { 8, null, null, null, 800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, "DOC-008", null, 9, "Dr. Tariqul Islam", false, "01700000008", null, "MBBS, DLO", 8, 0, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 4, 1 },
                    { 5, 1 },
                    { 6, 1 },
                    { 7, 1 },
                    { 8, 1 },
                    { 9, 1 },
                    { 10, 1 },
                    { 11, 1 },
                    { 12, 1 },
                    { 13, 1 },
                    { 14, 1 },
                    { 15, 1 },
                    { 16, 1 },
                    { 17, 1 },
                    { 18, 1 },
                    { 19, 1 },
                    { 20, 1 },
                    { 21, 1 },
                    { 22, 1 },
                    { 23, 1 },
                    { 24, 1 },
                    { 25, 1 },
                    { 26, 1 },
                    { 27, 1 },
                    { 28, 1 },
                    { 29, 1 },
                    { 30, 1 },
                    { 31, 1 },
                    { 32, 1 },
                    { 33, 1 },
                    { 34, 1 },
                    { 35, 1 },
                    { 36, 1 },
                    { 37, 1 },
                    { 38, 1 },
                    { 39, 1 },
                    { 40, 1 },
                    { 41, 1 },
                    { 42, 1 },
                    { 43, 1 },
                    { 44, 1 },
                    { 45, 1 },
                    { 46, 1 },
                    { 47, 1 },
                    { 48, 1 },
                    { 49, 1 },
                    { 50, 1 },
                    { 51, 1 },
                    { 52, 1 },
                    { 53, 1 },
                    { 54, 1 },
                    { 55, 1 },
                    { 56, 1 },
                    { 57, 1 },
                    { 58, 1 },
                    { 59, 1 },
                    { 60, 1 },
                    { 61, 1 },
                    { 62, 1 },
                    { 63, 1 },
                    { 64, 1 },
                    { 65, 1 },
                    { 66, 1 },
                    { 67, 1 },
                    { 68, 1 },
                    { 69, 1 },
                    { 70, 1 },
                    { 71, 1 },
                    { 72, 1 },
                    { 73, 1 },
                    { 74, 1 },
                    { 75, 1 },
                    { 76, 1 },
                    { 77, 1 },
                    { 78, 1 },
                    { 79, 1 },
                    { 80, 1 },
                    { 81, 1 },
                    { 82, 1 },
                    { 83, 1 },
                    { 84, 1 },
                    { 1, 3 },
                    { 25, 3 },
                    { 31, 3 },
                    { 37, 3 },
                    { 38, 3 },
                    { 43, 3 },
                    { 44, 3 },
                    { 1, 4 },
                    { 2, 4 },
                    { 3, 4 },
                    { 5, 4 },
                    { 6, 4 },
                    { 25, 4 },
                    { 26, 4 },
                    { 27, 4 },
                    { 29, 4 },
                    { 30, 4 },
                    { 31, 4 },
                    { 32, 4 },
                    { 33, 4 },
                    { 35, 4 },
                    { 36, 4 },
                    { 49, 4 },
                    { 50, 4 },
                    { 51, 4 },
                    { 53, 4 },
                    { 54, 4 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 1 });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AppointmentDate", "AppointmentNo", "AppointmentTime", "CancellationReason", "ChiefComplaint", "CreatedAt", "CreatedBy", "DoctorId", "IsDeleted", "Notes", "PatientId", "Status", "TokenNumber", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateOnly(2026, 5, 10), "APT-2026-0001", new TimeOnly(9, 0, 0), null, "Fever and headache", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, false, null, 1, 1, 1, null, null },
                    { 2, new DateOnly(2026, 5, 12), "APT-2026-0002", new TimeOnly(10, 0, 0), null, "Chest pain", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 2, false, null, 2, 0, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "DoctorSchedules",
                columns: new[] { "Id", "CreatedAt", "DayOfWeek", "DoctorId", "EndTime", "IsActive", "MaxPatients", "SlotDurationMinutes", "StartTime", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1, new TimeOnly(13, 0, 0), true, 12, 20, new TimeOnly(9, 0, 0), null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, 1, new TimeOnly(13, 0, 0), true, 12, 20, new TimeOnly(9, 0, 0), null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 2, new TimeOnly(14, 0, 0), true, 8, 30, new TimeOnly(10, 0, 0), null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3, new TimeOnly(12, 0, 0), true, 12, 15, new TimeOnly(9, 0, 0), null }
                });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "Id", "AppointmentId", "ConsultationFee", "CreatedAt", "CreatedBy", "Discount", "DoctorId", "DueDate", "InvoiceNo", "IsDeleted", "Notes", "OtherCharges", "PaidAmount", "PatientId", "Status", "TestFee", "TotalAmount", "UpdatedAt" },
                values: new object[] { 1, 1, 800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0m, 1, new DateOnly(2026, 5, 10), "INV-2026-0001", false, null, 0m, 800m, 1, 1, 0m, 800m, null });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_AppointmentNo",
                table: "Appointments",
                column: "AppointmentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_DepartmentId",
                table: "Doctors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_DoctorNo",
                table: "Doctors",
                column: "DoctorNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecializationId",
                table: "Doctors",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSchedules_DoctorId",
                table: "DoctorSchedules",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Invitations_InvitedByUserId",
                table: "Invitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_AppointmentId",
                table: "Invoices",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DoctorId",
                table: "Invoices",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNo",
                table: "Invoices",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PatientId",
                table: "Invoices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_LabTestRequests_PrescriptionId",
                table: "LabTestRequests",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PatientId",
                table: "MedicalHistories",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_UserId",
                table: "PasswordResetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_PatientId",
                table: "PatientDocuments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientNo",
                table: "Patients",
                column: "PatientNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionMedicines_PrescriptionId",
                table: "PrescriptionMedicines",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_DoctorId",
                table: "Prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL") return;

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DoctorSchedules");

            migrationBuilder.DropTable(
                name: "HospitalProfiles");

            migrationBuilder.DropTable(
                name: "Invitations");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "LabTestRequests");

            migrationBuilder.DropTable(
                name: "MedicalHistories");

            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "PasswordResetTokens");

            migrationBuilder.DropTable(
                name: "PatientDocuments");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PrescriptionMedicines");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Specializations");
        }
    }
}
