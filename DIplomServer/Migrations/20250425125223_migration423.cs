using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DIplomServer.Migrations
{
    /// <inheritdoc />
    public partial class migration423 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentProgress",
                table: "Achievements",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageIndex",
                table: "Achievements",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Achievements",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TargetProgress",
                table: "Achievements",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentProgress",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "ImageIndex",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "TargetProgress",
                table: "Achievements");
        }
    }
}
