using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LookIT.Migrations
{
    /// <inheritdoc />
    public partial class AddEditTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateModified",
                table: "Comments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateModified",
                table: "Comments");
        }
    }
}
