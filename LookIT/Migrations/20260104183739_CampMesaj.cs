using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LookIT.Migrations
{
    /// <inheritdoc />
    public partial class CampMesaj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSafe",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSafe",
                table: "Messages");
        }
    }
}
