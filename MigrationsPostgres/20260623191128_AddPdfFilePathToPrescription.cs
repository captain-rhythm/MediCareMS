using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCareMS.MigrationsPostgres
{
    /// <inheritdoc />
    public partial class AddPdfFilePathToPrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Prescriptions");
        }
    }
}
