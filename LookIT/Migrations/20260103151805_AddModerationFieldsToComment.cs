using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LookIT.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationFieldsToComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlagCategory",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlagged",
                table: "Comments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModeratedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlagCategory",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "IsFlagged",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ModeratedAt",
                table: "Comments");
        }
    }
}
