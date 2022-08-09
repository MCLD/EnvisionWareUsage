using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EnvisionwareLoader.Data.Migrations
{
    public partial class v100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "DailySummaries",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    Branch = table.Column<string>(maxLength: 50, nullable: false),
                    Minutes = table.Column<int>(nullable: false),
                    MaxMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySummaries", x => new { x.Date, x.Branch });
                });

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    Key = table.Column<int>(nullable: false),
                    Minutes = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Branch = table.Column<string>(maxLength: 50, nullable: true),
                    Area = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "HourlySummaries",
                columns: table => new
                {
                    Date = table.Column<DateTime>(nullable: false),
                    Branch = table.Column<string>(maxLength: 50, nullable: false),
                    Minutes = table.Column<int>(nullable: false),
                    MaxMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlySummaries", x => new { x.Date, x.Branch });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "DailySummaries");

            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.DropTable(
                name: "HourlySummaries");
        }
    }
}
