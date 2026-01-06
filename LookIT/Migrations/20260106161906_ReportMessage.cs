using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LookIT.Migrations
{
    /// <inheritdoc />
    public partial class ReportMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isReported",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isReported",
                table: "Messages");
        }
    }
}
