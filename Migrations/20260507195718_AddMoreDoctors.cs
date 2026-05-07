using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediCareMS.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreDoctors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Doctors",
                columns: new[] { "Id", "Bio", "BmdcRegNo", "ChamberAddress", "ConsultationFee", "CreatedAt", "CreatedBy", "DepartmentId", "DoctorNo", "Email", "ExperienceYears", "FullName", "IsDeleted", "MobileNumber", "ProfileImagePath", "Qualification", "SpecializationId", "Status", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { 4, null, null, null, 1000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 3, "DOC-004", null, 10, "Dr. Samira Khan", false, "01700000004", null, "MBBS, MS (Ortho)", 3, 0, null, null, null },
                    { 5, null, null, null, 1500m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 5, "DOC-005", null, 20, "Dr. Ayesha Siddiqa", false, "01700000005", null, "MBBS, FCPS (Obs & Gynae)", 5, 0, null, null, null },
                    { 6, null, null, null, 600m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 6, "DOC-006", null, 5, "Dr. Kamal Hossain", false, "01700000006", null, "MBBS, DDV", 6, 0, null, null, null },
                    { 7, null, null, null, 1200m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 7, "DOC-007", null, 14, "Dr. Nabila Rahman", false, "01700000007", null, "MBBS, MD (Neurology)", 7, 0, null, null, null },
                    { 8, null, null, null, 800m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 8, "DOC-008", null, 9, "Dr. Tariqul Islam", false, "01700000008", null, "MBBS, DLO", 8, 0, null, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
