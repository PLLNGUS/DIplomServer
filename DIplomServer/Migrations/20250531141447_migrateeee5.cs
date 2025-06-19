using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DIplomServer.Migrations
{
    /// <inheritdoc />
    public partial class migrateeee5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "Habits",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "Habits");
        }
    }
}
