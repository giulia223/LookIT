using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LookIT.Migrations
{
    /// <inheritdoc />
    public partial class AddSentimentAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentimentAnalyzedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SentimentConfidence",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentimentAnalyzedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SentimentConfidence",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "Posts");
        }
    }
}
