﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DIplomServer.Migrations
{
    /// <inheritdoc />
    public partial class migrat646464 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastStreakDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastStreakDate",
                table: "Users");
        }
    }
}
