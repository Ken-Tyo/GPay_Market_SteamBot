using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SteamDigiSellerBot.Database.Migrations
{
    public partial class AddSellers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sellers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RentDays = table.Column<int>(type: "integer", nullable: true),
                    ItemsLimit = table.Column<int>(type: "integer", nullable: true),
                    Blocked = table.Column<bool>(type: "boolean", nullable: false),
                    Permissions_DigisellerItems = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_KFGItems = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_FuryPayItems = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_ItemsHierarchy = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_OneTimeBots = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_OrderSessionCreation = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_ItemsMultiregion = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_DirectBotsDeposit = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_BotsLimitsParsing = table.Column<bool>(type: "boolean", nullable: true),
                    Permissions_DigisellerItemsGeneration = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sellers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sellers_Users_Id",
                        column: x => x.Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sellers");
        }
    }
}
