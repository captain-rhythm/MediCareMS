using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediCareMS.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationUsedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "Invitations",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "Invitations");
        }
    }
}
