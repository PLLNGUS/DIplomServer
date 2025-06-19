using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DIplomServer.Migrations
{
    /// <inheritdoc />
    public partial class migrat31222 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BorderStyle",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BorderStyle",
                table: "Users");
        }
    }
}
