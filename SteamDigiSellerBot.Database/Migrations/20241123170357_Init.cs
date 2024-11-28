using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SteamDigiSellerBot.Database.Entities;
using SteamKit2.Internal;

namespace SteamDigiSellerBot.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DigisellerID = table.Column<string>(type: "text", nullable: true),
                    DigisellerIDC = table.Column<string>(type: "text", nullable: true),
                    DigisellerApiKey = table.Column<string>(type: "text", nullable: true),
                    DigisellerApiKeyC = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BotRegionSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GiftSendSteamCurrencyId = table.Column<int>(type: "integer", nullable: true),
                    PreviousPurchasesJPY = table.Column<decimal>(type: "numeric", nullable: true),
                    PreviousPurchasesCNY = table.Column<decimal>(type: "numeric", nullable: true),
                    PreviousPurchasesSteamCurrencyId = table.Column<int>(type: "integer", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotRegionSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastUpdateDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddedDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDiscount = table.Column<bool>(type: "boolean", nullable: false),
                    IsBundle = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountEndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AppId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SubId = table.Column<string>(type: "text", nullable: true),
                    IsDlc = table.Column<bool>(type: "boolean", nullable: false),
                    SteamCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    IsPriceParseError = table.Column<bool>(type: "boolean", nullable: false),
                    GameInfo = table.Column<StoreItem>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    SteamPercent = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemAdditionalInfoTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAdditionalInfoTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemInfoTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInfoTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "MarketPlaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketPlaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamCountryCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamCountryCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamProxies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Host = table.Column<string>(type: "text", nullable: true),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PasswordC = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamProxies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TagTypeReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IsDlc = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypeReplacements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VacGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppId = table.Column<string>(type: "text", nullable: true),
                    SubId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AspNetUserId = table.Column<string>(type: "text", nullable: true),
                    DigisellerToken = table.Column<string>(type: "text", nullable: true),
                    DigisellerTokenExp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DigisellerID = table.Column<string>(type: "text", nullable: true),
                    DigisellerIDC = table.Column<string>(type: "text", nullable: true),
                    DigisellerApiKey = table.Column<string>(type: "text", nullable: true),
                    DigisellerApiKeyC = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_AspNetUsers_AspNetUserId",
                        column: x => x.AspNetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastTimeUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastTimeBalanceUpdated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActivationCountry = table.Column<int>(type: "integer", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: true),
                    TempLimitDeadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SendGameAttemptsCount = table.Column<int>(type: "integer", nullable: false),
                    SendGameAttemptsCountDaily = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    PersonName = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: true),
                    PasswordC = table.Column<string>(type: "text", nullable: true),
                    MaFileStr = table.Column<string>(type: "text", nullable: true),
                    MaFileStrC = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    ProxyStr = table.Column<string>(type: "text", nullable: true),
                    ProxyStrC = table.Column<string>(type: "text", nullable: true),
                    SteamCookiesStr = table.Column<string>(type: "text", nullable: true),
                    SteamCookiesStrC = table.Column<string>(type: "text", nullable: true),
                    Region = table.Column<string>(type: "text", nullable: true),
                    SteamId = table.Column<string>(type: "text", nullable: true),
                    TotalPurchaseSumUSD = table.Column<decimal>(type: "numeric", nullable: false),
                    SendedGiftsSum = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxSendedGiftsSum = table.Column<decimal>(type: "numeric", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    GameSendLimitAddParam = table.Column<int>(type: "integer", nullable: false),
                    SteamCurrencyId = table.Column<int>(type: "integer", nullable: true),
                    MaxSendedGiftsUpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsProblemRegion = table.Column<bool>(type: "boolean", nullable: false),
                    HasProblemPurchase = table.Column<bool>(type: "boolean", nullable: false),
                    BotRegionSettingId = table.Column<int>(type: "integer", nullable: true),
                    IsON = table.Column<bool>(type: "boolean", nullable: false),
                    VacGames = table.Column<IEnumerable<VacGame>>(type: "json", nullable: true),
                    LoginResult = table.Column<int>(type: "integer", nullable: true),
                    SendGameAttemptsArray = table.Column<List<DateTimeOffset>>(type: "json", nullable: true),
                    SendGameAttemptsArrayDaily = table.Column<List<DateTimeOffset>>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bots_BotRegionSettings_BotRegionSettingId",
                        column: x => x.BotRegionSettingId,
                        principalTable: "BotRegionSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    SteamId = table.Column<int>(type: "integer", nullable: false),
                    SteamSymbol = table.Column<string>(type: "text", nullable: true),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: true),
                    CountryCode = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrencyDataId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Currencies_CurrencyData_CurrencyDataId",
                        column: x => x.CurrencyDataId,
                        principalTable: "CurrencyData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GamePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    CurrentSteamPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OriginalSteamPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    SteamCurrencyId = table.Column<int>(type: "integer", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsManualSet = table.Column<bool>(type: "boolean", nullable: false),
                    IsPriority = table.Column<bool>(type: "boolean", nullable: false),
                    FailUsingCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GamePrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GamePrices_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemAdditionalInfoTemplateValues",
                columns: table => new
                {
                    ItemAdditionalInfoTemplateId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAdditionalInfoTemplateValues", x => new { x.ItemAdditionalInfoTemplateId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_ItemAdditionalInfoTemplateValues_ItemAdditionalInfoTemplate~",
                        column: x => x.ItemAdditionalInfoTemplateId,
                        principalTable: "ItemAdditionalInfoTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemAdditionalInfoTemplateValues_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemInfoTemplateValues",
                columns: table => new
                {
                    ItemInfoTemplateId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemInfoTemplateValues", x => new { x.ItemInfoTemplateId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_ItemInfoTemplateValues_ItemInfoTemplates_ItemInfoTemplateId",
                        column: x => x.ItemInfoTemplateId,
                        principalTable: "ItemInfoTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemInfoTemplateValues_Languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "Languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagPromoReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MarketPlaceId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagPromoReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagPromoReplacements_MarketPlaces_MarketPlaceId",
                        column: x => x.MarketPlaceId,
                        principalTable: "MarketPlaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CurrentDigiSellerPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentDigiSellerPriceNeedAttention = table.Column<bool>(type: "boolean", nullable: false),
                    FixedDigiSellerPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    DigiSellerIds = table.Column<List<string>>(type: "text[]", nullable: true),
                    SteamPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    AddPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsFixedPrice = table.Column<bool>(type: "boolean", nullable: false),
                    IsAutoActivation = table.Column<bool>(type: "boolean", nullable: false),
                    MinActualThreshold = table.Column<int>(type: "integer", nullable: true),
                    SteamCountryCodeId = table.Column<int>(type: "integer", nullable: true),
                    LastSendedRegionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Games_Id",
                        column: x => x.Id,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Items_SteamCountryCodes_LastSendedRegionId",
                        column: x => x.LastSendedRegionId,
                        principalTable: "SteamCountryCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_SteamCountryCodes_SteamCountryCodeId",
                        column: x => x.SteamCountryCodeId,
                        principalTable: "SteamCountryCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TagTypeReplacementValues",
                columns: table => new
                {
                    TagTypeReplacementId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTypeReplacementValues", x => new { x.TagTypeReplacementId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_TagTypeReplacementValues_TagTypeReplacements_TagTypeReplace~",
                        column: x => x.TagTypeReplacementId,
                        principalTable: "TagTypeReplacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BotSendGameAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BotId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotSendGameAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BotSendGameAttempts_Bots_BotId",
                        column: x => x.BotId,
                        principalTable: "Bots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TagPromoReplacementValues",
                columns: table => new
                {
                    TagPromoReplacementId = table.Column<int>(type: "integer", nullable: false),
                    LanguageCode = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagPromoReplacementValues", x => new { x.TagPromoReplacementId, x.LanguageCode });
                    table.ForeignKey(
                        name: "FK_TagPromoReplacementValues_TagPromoReplacements_TagPromoRepl~",
                        column: x => x.TagPromoReplacementId,
                        principalTable: "TagPromoReplacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddedDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BotId = table.Column<int>(type: "integer", nullable: true),
                    ItemId = table.Column<int>(type: "integer", nullable: true),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    SteamProfileName = table.Column<string>(type: "text", nullable: true),
                    SteamProfileUrl = table.Column<string>(type: "text", nullable: true),
                    SteamProfileAvatarUrl = table.Column<string>(type: "text", nullable: true),
                    SteamProfileGifteeAccountID = table.Column<string>(type: "text", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    DigiSellerDealId = table.Column<string>(type: "text", nullable: true),
                    IsSteamMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    UniqueCode = table.Column<string>(type: "text", nullable: true),
                    DaysExpiration = table.Column<int>(type: "integer", nullable: true),
                    MaxSellPercent = table.Column<int>(type: "integer", nullable: true),
                    SteamContactType = table.Column<int>(type: "integer", nullable: false),
                    SteamContactValue = table.Column<string>(type: "text", nullable: true),
                    ActivationEndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AutoSendInvitationTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SteamCountryCodeId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PriorityPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    DigiSellerDealPriceUsd = table.Column<decimal>(type: "numeric", nullable: true),
                    GameExistsRepeatSendCount = table.Column<int>(type: "integer", nullable: false),
                    QueuePosition = table.Column<int>(type: "integer", nullable: false),
                    QueueWaitingMinutes = table.Column<int>(type: "integer", nullable: false),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    GameSessionItemId = table.Column<int>(type: "integer", nullable: true),
                    BotSwitchList = table.Column<List<int>>(type: "json", nullable: true),
                    BlockOrder = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Bots_BotId",
                        column: x => x.BotId,
                        principalTable: "Bots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_GameSessionItems_GameSessionItemId",
                        column: x => x.GameSessionItemId,
                        principalTable: "GameSessionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_SteamCountryCodes_SteamCountryCodeId",
                        column: x => x.SteamCountryCodeId,
                        principalTable: "SteamCountryCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameSessionStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameSessionId = table.Column<int>(type: "integer", nullable: false),
                    InsertDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<GameSessionStatusLog.ValueJson>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessionStatusLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessionStatusLogs_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bots_BotRegionSettingId",
                table: "Bots",
                column: "BotRegionSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_BotSendGameAttempts_BotId",
                table: "BotSendGameAttempts",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CurrencyDataId",
                table: "Currencies",
                column: "CurrencyDataId");

            migrationBuilder.CreateIndex(
                name: "gameprices_un",
                table: "GamePrices",
                columns: new[] { "GameId", "SteamCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "gamesessions_un",
                table: "GameSessions",
                column: "UniqueCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_BotId",
                table: "GameSessions",
                column: "BotId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_GameSessionItemId",
                table: "GameSessions",
                column: "GameSessionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_ItemId",
                table: "GameSessions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_SteamCountryCodeId",
                table: "GameSessions",
                column: "SteamCountryCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_UserId",
                table: "GameSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessionStatusLogs_GameSessionId",
                table: "GameSessionStatusLogs",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAdditionalInfoTemplateValues_LanguageCode",
                table: "ItemAdditionalInfoTemplateValues",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_ItemInfoTemplateValues_LanguageCode",
                table: "ItemInfoTemplateValues",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_Items_LastSendedRegionId",
                table: "Items",
                column: "LastSendedRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_SteamCountryCodeId",
                table: "Items",
                column: "SteamCountryCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TagPromoReplacements_MarketPlaceId",
                table: "TagPromoReplacements",
                column: "MarketPlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AspNetUserId",
                table: "Users",
                column: "AspNetUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BotSendGameAttempts");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "GamePrices");

            migrationBuilder.DropTable(
                name: "GameSessionStatus");

            migrationBuilder.DropTable(
                name: "GameSessionStatusLogs");

            migrationBuilder.DropTable(
                name: "ItemAdditionalInfoTemplateValues");

            migrationBuilder.DropTable(
                name: "ItemInfoTemplateValues");

            migrationBuilder.DropTable(
                name: "SteamProxies");

            migrationBuilder.DropTable(
                name: "TagPromoReplacementValues");

            migrationBuilder.DropTable(
                name: "TagTypeReplacementValues");

            migrationBuilder.DropTable(
                name: "VacGames");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CurrencyData");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "ItemAdditionalInfoTemplates");

            migrationBuilder.DropTable(
                name: "ItemInfoTemplates");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "TagPromoReplacements");

            migrationBuilder.DropTable(
                name: "TagTypeReplacements");

            migrationBuilder.DropTable(
                name: "Bots");

            migrationBuilder.DropTable(
                name: "GameSessionItems");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MarketPlaces");

            migrationBuilder.DropTable(
                name: "BotRegionSettings");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "SteamCountryCodes");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
