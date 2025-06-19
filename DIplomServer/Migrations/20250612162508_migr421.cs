using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DIplomServer.Migrations
{
    /// <inheritdoc />
    public partial class migr421 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRewardClaimed",
                table: "Quests",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRewardClaimed",
                table: "Quests");
        }
    }
}
