using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SteamDigiSellerBot.Database.Migrations
{
    public partial class SellersAddCommentsAndPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Sellers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Permissions_SteamPointsAutoDelivery",
                table: "Sellers",
                type: "boolean",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Sellers");

            migrationBuilder.DropColumn(
                name: "Permissions_SteamPointsAutoDelivery",
                table: "Sellers");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    BonusBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    IsBlocked = table.Column<bool>(type: "boolean", nullable: false),
                    LastSteamRegion = table.Column<string>(type: "text", nullable: true),
                    Passsword = table.Column<string>(type: "text", nullable: true),
                    RubbleBalance = table.Column<decimal>(type: "numeric", nullable: false),
                    SteamGamesWishList = table.Column<int[]>(type: "integer[]", nullable: true),
                    SteamId = table.Column<string>(type: "text", nullable: true),
                    SynchronizationDateSteamApi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
