alter table "GameSessions" drop column "CopyCount";

задача 68

CREATE TABLE "Languages" (
	"Code" varchar(8) primary key,
	"Description" varchar(16) not null
);

insert into "Languages"("Code", "Description") values ('ru-RU', 'Русский');
insert into "Languages"("Code", "Description") values ('en-US', 'English');

CREATE TABLE "ItemInfoTemplates" (
	"Id" serial primary key
);

CREATE TABLE "ItemInfoTemplateValues" (
	"ItemInfoTemplateId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" text not null,
	primary key("ItemInfoTemplateId", "LanguageCode")
);

ALTER TABLE "ItemInfoTemplateValues" add constraint "ItemInfoTemplateValues_ItemInfoTemplateId_fkey" 
foreign key ("ItemInfoTemplateId") references "ItemInfoTemplates"("Id") on delete cascade;

ALTER TABLE "ItemInfoTemplateValues" add constraint "ItemInfoTemplateValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

CREATE TABLE "ItemAdditionalInfoTemplates" (
	"Id" serial primary key
);

CREATE TABLE "ItemAdditionalInfoTemplateValues" (
	"ItemAdditionalInfoTemplateId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" text not null,
	primary key("ItemAdditionalInfoTemplateId", "LanguageCode")
);

ALTER TABLE "ItemAdditionalInfoTemplateValues" add constraint "ItemAdditionalInfoTemplateValues_ItemAdditionalInfoTmplId_fkey" 
foreign key ("ItemAdditionalInfoTemplateId") references "ItemAdditionalInfoTemplates"("Id") on delete cascade;

ALTER TABLE "ItemAdditionalInfoTemplateValues" add constraint "ItemAdditionalInfoTemplateValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

задача 44
ALTER TABLE "GameSessions" add "PriorityPrice" numeric null;
ALTER TABLE "GameSessions" ADD "SteamProfileGifteeAccountID" varchar(50) null;
alter table "Bots" add column "IsON" bool not null default true;
alter table "Bots" drop column "Active";
ALTER TABLE "Bots" add column "TempLimitDeadline" timestamp with time zone not null default now();
ALTER TABLE "Bots" add "SendGameAttemptsCount" numeric not null DEFAULT 0;
ALTER TABLE "GameSessions" add "GameExistsRepeatSendCount" numeric not null DEFAULT 0;
alter table "Items" add "LastSendedRegionId" int null references "SteamCountryCodes"("Id");
ALTER TABLE "GameSessions" add "QueuePosition" numeric not null default 0;
ALTER TABLE "GameSessions" add "QueueWaitingMinutes" numeric not null default 0;
ALTER TABLE "GameSessions" add column "Stage" int4 not null default(1);
alter table "GameSessions" alter column "AddedDateTime" type timestamp with time zone;
alter table "GameSessions" alter column "AddedDateTime" set default now();
alter table "GameSessions" alter column "AddedDateTime" set not null;
alter table "GameSessions" alter column "MaxSellPercent" drop not null;



задача 33
/*
CREATE TABLE "ItemPricePriority" (
	"Id" serial primary key,
	"Name" varchar(200) not null,
	"Code" varchar(2) not null
)
*/

alter table "GamePrices" add "IsManualSet" bool default false;
alter table "GamePrices" add "IsPriority" bool default false;
alter table "GamePrices" add "FailUsingCount" int default 0;


CREATE TABLE "BotSendGameAttempts" (
	"Id" serial primary key,
	"Date" timestamp with time zone not null default now(),
	"BotId" int not null references "Bots"("Id")
);


ALTER TABLE "GameSessions" add "UserId" int null references "Users"("Id");
update "GameSessions" set "UserId" = userid!!!;
alter table "GameSessions" alter column "UserId" set not null;


задача 31
ALTER TABLE "Users" ADD "DigisellerID" varchar(50) null;
ALTER TABLE "Users" ADD "DigisellerApiKey" varchar(100) null;
ALTER TABLE "Users" add column "DigisellerToken" text null;
ALTER TABLE "Users" add column "DigisellerTokenExp" timestamp with time zone null;

ALTER TABLE "Bots"  add "LoginResult" int null;

задача 30
CREATE TABLE "GameSessionStatusLogs" (
	"Id" serial primary key,
	"InsertDate" timestamp with time zone not null default now(),
	"Value" json null
);

ALTER TABLE "GameSessionStatusLogs" add "StatusId" int not null references "GameSessionStatus"("StatusId");
ALTER TABLE "GameSessionStatusLogs" ADD "GameSessionId" int not null references "GameSessions"("Id");

ALTER TABLE "GameSessionStatus" add "Description" text null;


задача 29
alter table "GameSessions" add column "SteamProfileAvatarUrl" text null;
ALTER TABLE "GameSessions" add column "AutoSendInvitationTime" timestamp with time zone null;


задача 28
create TABLE "SteamCountryCodes" (
	"Id" serial primary key,
	"Name" varchar(200) not null,
	"Code" varchar(2) not null
)

alter table "Items" add "SteamCountryCodeId" int null references "SteamCountryCodes"("Id");
update "Items" set "SteamCountryCodeId" = 28;

alter table "GameSessions" add "SteamCountryCodeId" int null references "SteamCountryCodes"("Id");
update "GameSessions" set "SteamCountryCodeId" = 28;


задача 26
CREATE TABLE "GameSessionStatus" (
	"Id" serial primary key,
	"StatusId" int not null,
	"Name" text not null,
	"Color" text not null,
);

ALTER TABLE "GameSessionStatus" ADD CONSTRAINT "StatusId_unique" UNIQUE("StatusId");

alter  table "GameSessions" drop "ActivationCountry";
alter  table "GameSessions" drop "GameId";
alter table "GameSessions" drop column "Status";

ALTER TABLE "GameSessions" ADD "ItemId" int not null references "Items"("Id");
ALTER TABLE "GameSessions" ADD "StatusId" int not null references "GameSessionStatus"("StatusId");
ALTER TABLE "GameSessions" add column "DaysExpiration" int null;
ALTER TABLE "GameSessions" add column "MaxSellPercent" int not null default 0;
ALTER TABLE "GameSessions" add column "CopyCount" int not null default 0;
ALTER TABLE "GameSessions" add column "SteamContactType" int4 not null default(100);
ALTER TABLE "GameSessions" add column "SteamContactValue" varchar(500);
ALTER TABLE "GameSessions" add column "SteamProfileName" varchar(200) null;
alter table "GameSessions" add column "ActivationEndDate" timestamp with time zone null;


CREATE TABLE "Users" (
	"Id" serial primary key
);
ALTER TABLE "Users" ADD "AspNetUserId" text null references "AspNetUsers"("Id");
insert into "Users"("AspNetUserId") values ('b87ae120-fb37-49f8-8b86-de87fea7e1f8');



задача 27
CREATE TABLE "BotRegionSettings" (
	"Id" serial primary key,
	"GiftSendSteamCurrencyId" int null references "Currencies"("SteamId"),
	"PreviousPurchasesSteamCurrencyId" int null references "Currencies"("SteamId"),
	"PreviousPurchasesJPY" numeric null,
	"PreviousPurchasesCNY" numeric null
);
	
alter table "Bots" add column "BotRegionSettingId" int null references "BotRegionSettings"("Id");
alter table "Bots" add column "IsProblemRegion" bool not null default false;
alter table "BotRegionSettings" add column "CreateDate" timestamp without time zone not null;
alter table "Bots" add column "HasProblemPurchase" bool not null default false;

CREATE TABLE "BotTransactions" (
	"Id" serial primary key,
	"BotId" int not null references "Bots"("Id"),
	"SteamCurrencyId" int null references "Currencies"("SteamId"),
	"Value" numeric not null,
	"Type" int4 not null,
	"Date" time not null
);





-------------------------старое


ALTER TABLE "AspNetUsers" ADD "DigisellerID" varchar(50) null
ALTER TABLE "AspNetUsers" ADD "DigisellerApiKey" varchar(100) null

ALTER TABLE "Bots" ADD "AvatarUrl" varchar(300) null

ALTER TABLE "Bots" ADD "Region" char(2) null
ALTER TABLE "Bots" ADD "SteamId" varchar(30) null
ALTER TABLE "Bots" ADD "State" integer null



CREATE TABLE "VacGames" (
	"Id" serial primary key,
	"AppId" text not null,
	"SubId" text not null,
	"Name" text not null
)

alter table "Bots" ADD "VacGames" JSON null 


19.12
ALTER TABLE "Currencies" ADD "SteamId" INT NULL;
ALTER TABLE "Currencies" ADD "Name" varchar(100) NULL;
ALTER TABLE "Currencies" ADD "Position" INT NULL;
ALTER TABLE "Currencies" ADD "CountryCode" char(2) NULL;
DELETE FROM "Currencies";
DELETE FROM "CurrencyData";

20.12
ALTER TABLE "Currencies" ADD CONSTRAINT "CurrSteamId_unique" UNIQUE("SteamId");
ALTER TABLE "Games" ADD "SteamCurrencyId" int NULL REFERENCES "Currencies" ("SteamId");
Update "Games" set "SteamCurrencyId" = 5;

21.12
CREATE TABLE "GamePrices" (
	"Id" serial primary key,
	"GameId" int not null references "Games"("Id") ,
	"CurrentSteamPrice" numeric not null,
	"OriginalSteamPrice" numeric not null,
	"SteamCurrencyId" int not null references "Currencies"("SteamId"),
	"LastUpdate" timestamp without time zone null
);


24.12
ALTER TABLE "Games" ADD "IsPriceParseError" boolean default false;

ALTER TABLE "GamePrices" drop constraint "GamePrices_SteamCurrencyId_fkey";
ALTER TABLE "GamePrices" add constraint "GamePrices_SteamCurrencyId_fkey" 
foreign key ("SteamCurrencyId") references "Currencies"("SteamId") on delete cascade;

ALTER TABLE "GamePrices" drop constraint "GamePrices_GameId_fkey";
ALTER TABLE "GamePrices" add constraint "GamePrices_GameId_fkey" 
foreign key ("GameId") references "Games"("Id") on delete cascade;


alter table "Games" drop column "CurrentSteamPrice";
alter table "Games" drop column "OriginalSteamPrice";

25.12
ALTER TABLE "Games" ADD "IsBundle" boolean default false

26.12
ALTER TABLE "Items" ADD "IsFixedPrice" boolean default false;
ALTER TABLE "Items" ADD "IsAutoActivation" boolean default false;
ALTER TABLE "Items" ADD "MinActualThreshold" int null;
ALTER TABLE "Items" ADD "FixedDigiSellerPrice" numeric null;

28.12
ALTER TABLE "Bots" ADD "TotalPurchaseSumUSD" numeric not null default 0;
ALTER TABLE "Bots" ADD "GameSendLimitAddParam" int not null default 0;
ALTER TABLE "Bots" ADD "SteamCurrencyId" int null references "Currencies"("SteamId");
ALTER TABLE "Bots" ADD "MaxSendedGiftsUpdateDate" timestamp without time zone not null default now();

14.04
alter table "Bots" ADD "SendGameAttemptsArray" JSON null;

17.04
ALTER TABLE "Items" ADD "CurrentDigiSellerPriceNeedAttention" boolean default false;

05.05 
ALTER TABLE "GameSessions" ADD BotSwitchList JSON null;

28.06
ALTER TABLE "Bots" add "SendGameAttemptsCountDaily" numeric not null DEFAULT 0;
alter table "Bots" ADD "SendGameAttemptsArrayDaily" JSON null;

20.07
alter table "GameSessions" add "BlockOrder" boolean not null default false;

05.08
alter table "Bots" add "LastTimeUpdated" timestamp without time zone null;
alter table "Bots" add "LastTimeBalanceUpdated" timestamp without time zone null;

05.09.2024 Задача 68
CREATE TABLE "TagTypeReplacements" (
	"Id" serial PRIMARY KEY,
	"IsDlc" BOOL DEFAULT false
);

comment on table "TagTypeReplacements" is 'Замены тэгов типа продукта - %type%';
comment on column "TagTypeReplacements"."Id" is 'Идентификатор';
comment on column "TagTypeReplacements"."IsDlc" is 'Признак DLC';

CREATE TABLE "TagTypeReplacementValues" (
	"TagTypeReplacementId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" VARCHAR(512) not null,
	PRIMARY KEY("TagTypeReplacementId", "LanguageCode")
);

comment on table "TagTypeReplacementValues" is 'Значения для замены тэгов продукта %type%';
comment on column "TagTypeReplacementValues"."TagTypeReplacementId" is 'ИД замены тэга продукта';
comment on column "TagTypeReplacementValues"."LanguageCode" is 'Код языка';
comment on column "TagTypeReplacementValues"."Value" is 'Значение для замены тэга';

ALTER TABLE "TagTypeReplacementValues" add constraint "TagTypeReplacementValues_TagTypeReplacementId_fkey" 
foreign key ("TagTypeReplacementId") references "TagTypeReplacements"("Id") on delete cascade;

ALTER TABLE "TagTypeReplacementValues" add constraint "TagTypeReplacementValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

CREATE TABLE "MarketPlaces" (
	"Id" INT PRIMARY KEY,
	"Name" VARCHAR(16)
);

comment on table "MarketPlaces" is 'Площадки';
comment on column "MarketPlaces"."Id" is 'Идентификатор';
comment on column "MarketPlaces"."Name" is 'Наименование';

INSERT INTO "MarketPlaces"("Id", "Name") VALUES(1, 'WMCentre');
INSERT INTO "MarketPlaces"("Id", "Name") VALUES(2, 'GGSel');
INSERT INTO "MarketPlaces"("Id", "Name") VALUES(3, 'Plati');

CREATE TABLE "TagPromoReplacements" (
	"Id" serial PRIMARY KEY,
	"MarketPlaceId" INT
);

ALTER TABLE "TagPromoReplacements" add constraint "TagPromoReplacements_MarketPlaceId_fkey" 
foreign key ("MarketPlaceId") references "MarketPlaces"("Id") on delete cascade;

comment on table "TagPromoReplacements" is 'Замены тэгов типа продукта - %promo%';
comment on column "TagPromoReplacements"."Id" is 'Идентификатор';
comment on column "TagPromoReplacements"."MarketPlaceId" is 'ИД площадки';

CREATE TABLE "TagPromoReplacementValues" (
	"TagPromoReplacementId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" VARCHAR(512) not null,
	PRIMARY KEY("TagPromoReplacementId", "LanguageCode")
);

comment on table "TagPromoReplacementValues" is 'Значения для замены тэгов промо-акции %promo%';
comment on column "TagPromoReplacementValues"."TagPromoReplacementId" is 'ИД замены тэга промо-акции';
comment on column "TagPromoReplacementValues"."LanguageCode" is 'Код языка';
comment on column "TagPromoReplacementValues"."Value" is 'Значение для замены тэга';

ALTER TABLE "TagPromoReplacementValues" add constraint "TagPromoReplacementValues_TagPromoReplacementId_fkey" 
foreign key ("TagPromoReplacementId") references "TagPromoReplacements"("Id") on delete cascade;

ALTER TABLE "TagPromoReplacementValues" add constraint "TagPromoReplacementValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

insert into "TagTypeReplacements"  ("Id", "IsDlc") values (1, false)
insert into "TagTypeReplacementValues" ("TagTypeReplacementId", "LanguageCode", "Value") values (1, 'ru-RU', 'игра / программа')
insert into "TagTypeReplacementValues" ("TagTypeReplacementId", "LanguageCode", "Value") values (1, 'en-US', 'game / software')
insert into "TagTypeReplacements" ("Id", "IsDlc") values (2, true)
insert into "TagTypeReplacementValues" ("TagTypeReplacementId", "LanguageCode", "Value") values (2, 'ru-RU', 'дополнение')
insert into "TagTypeReplacementValues" ("TagTypeReplacementId", "LanguageCode", "Value") values (2, 'en-US', 'DLC')


27.10.2024 96-шифровать-хешировать-пароли-в-бд-ботов-steam-аккаунтов-при-добавлении-в-разделе
ALTER TABLE "Bots" ADD "PasswordC" text NULL;
ALTER TABLE "Bots" ADD "ProxyStrC" text NULL;
ALTER TABLE "Bots" ADD "MaFileStrC" text NULL;
ALTER TABLE "Bots" ADD "SteamCookiesStrC" text NULL;
ALTER TABLE "SteamProxies" ADD "PasswordC" text NULL;
ALTER TABLE "AspNetUsers" ADD "DigisellerIDC" text NULL;
ALTER TABLE "AspNetUsers" ADD "DigisellerApiKeyC" text NULL;
ALTER TABLE "Users" ADD "DigisellerIDC" text NULL;
ALTER TABLE "Users" ADD "DigisellerApiKeyC" text NULL;

19.11.2024
ALTER TABLE public."Bots" ADD COLUMN "IsReserve" BOOL DEFAULT false

19.11 
ALTER TABLE "GameSessions" add "DigiSellerDealPriceUsd" numeric null;

21.11
alter table "Games" ADD "GameInfo" JSON null;

19.11.2024 Задача 142 - Добавление тэга %infoApps%
CREATE TABLE "TagInfoAppsReplacements" (
	"Id" serial PRIMARY KEY
);

comment on table "TagInfoAppsReplacements" is 'Замены тэгов состава изделия - %infoApps%';
comment on column "TagInfoAppsReplacements"."Id" is 'Идентификатор';

CREATE TABLE "TagInfoAppsReplacementValues" (
	"TagInfoAppsReplacementId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" VARCHAR(512) not null,
	PRIMARY KEY("TagInfoAppsReplacementId", "LanguageCode")
);

comment on table "TagInfoAppsReplacementValues" is 'Значения для замены тэгов состава изделия - %infoApps%';
comment on column "TagInfoAppsReplacementValues"."TagInfoAppsReplacementId" is 'ИД замены тэга состава изделия';
comment on column "TagInfoAppsReplacementValues"."LanguageCode" is 'Код языка';
comment on column "TagInfoAppsReplacementValues"."Value" is 'Значение для замены тэга';

ALTER TABLE "TagInfoAppsReplacementValues" add constraint "TagInfoAppsReplacementValues_TagInfoAppsReplacementId_fkey" 
foreign key ("TagInfoAppsReplacementId") references "TagInfoAppsReplacements"("Id") on delete cascade;

ALTER TABLE "TagInfoAppsReplacementValues" add constraint "TagInfoAppsReplacementValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

20.11.2024 Задача 141 - Добавление тэга %infoDLC%
CREATE TABLE "TagInfoDlcReplacements" (
	"Id" serial PRIMARY KEY
);

comment on table "TagInfoDlcReplacements" is 'Замены тэгов состава изделия - %infoDLC%';
comment on column "TagInfoDlcReplacements"."Id" is 'Идентификатор';

CREATE TABLE "TagInfoDlcReplacementValues" (
	"TagInfoDlcReplacementId" serial,
	"LanguageCode" varchar(8) not null,
	"Value" VARCHAR(512) not null,
	PRIMARY KEY("TagInfoDlcReplacementId", "LanguageCode")
);

comment on table "TagInfoDlcReplacementValues" is 'Значения для замены тэгов с требованиями основной игры к DLC - %infoDLC%';
comment on column "TagInfoDlcReplacementValues"."TagInfoDlcReplacementId" is 'ИД замены тэга с требованиями основной игры к DLC';
comment on column "TagInfoDlcReplacementValues"."LanguageCode" is 'Код языка';
comment on column "TagInfoDlcReplacementValues"."Value" is 'Значение для замены тэга';

ALTER TABLE "TagInfoDlcReplacementValues" add constraint "TagInfoDlcReplacementValues_TagInfoDlcReplacementId_fkey" 
foreign key ("TagInfoDlcReplacementId") references "TagInfoDlcReplacements"("Id") on delete cascade;

ALTER TABLE "TagInfoDlcReplacementValues" add constraint "TagInfoDlcReplacementValues_LanguageCode_fkey" 
foreign key ("LanguageCode") references "Languages"("Code") on delete cascade;

22.11.2024 Задача 68
CREATE TABLE "UpdateItemInfoStat" (
	"JobCode" varchar(16),
	"UpdateDate" date,
	"RequestCount" int not null,
	PRIMARY KEY("JobCode")
);

comment on table "UpdateItemInfoStat" is 'Статистика обновлений описаний товаров за день';
comment on column "UpdateItemInfoStat"."JobCode" is 'Код задачи на обновление';
comment on column "UpdateItemInfoStat"."UpdateDate" is 'Дата обновления';
comment on column "UpdateItemInfoStat"."RequestCount" is 'Количество отправленных запросов';

23.11 - ДУБЛЬ СТРОКИ 125
alter table "Items" add "SteamCountryCodeId" int null references "SteamCountryCodes"("Id");

24.11 Задача #70 - GiftBan
ALTER TABLE "Bots" add "RemainingSumToGift" numeric null;
23.11
alter table "Items" add "SteamCountryCodeId" int null references "SteamCountryCodes"("Id");
INSERT INTO "public"."GameSessionStatus" ("Id", "StatusId", "Name", "Color", "Description") VALUES (49, 23, 'Ошибка (Нет игры)', '#E13F29', NULL);

26.11
ALTER TABLE "GameSessions" ADD "AccountSwitchList" JSON null;
ALTER TABLE "Bots" ADD "IgnoreSendLimits" BOOLEAN  not null default FALSE;

02.12
ALTER TABLE "GameSessions" ADD "Market" int null;

04.12 *fix
ALTER TABLE "GameSessions" ADD "ItemSteamCountryCodeId" int null;

05.12 Задача 163 - Список игр (приложений и комплектов), которые принадлежат боту
CREATE TABLE "BotSteamLicenses"
(
    "Id" integer NOT NULL,
    "AppIdList" integer[] NOT NULL,
    "SubIdList" integer[] NOT NULL,
    CONSTRAINT "PK_BotSteamLicanses" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BotSteamLicenses_Bots" FOREIGN KEY ("Id")
        REFERENCES "Bots" ("Id") MATCH SIMPLE
        ON UPDATE CASCADE
        ON DELETE CASCADE
        NOT VALID
);

05.12.2024 96-шифровать-хешировать-пароли-в-бд-ботов-steam-аккаунтов-при-добавлении-в-разделе
ALTER TABLE "Bots" drop column "PasswordC";
ALTER TABLE "Bots" drop column "ProxyStrC";
ALTER TABLE "Bots" drop column "MaFileStrC";
ALTER TABLE "Bots" drop column "SteamCookiesStrC";
ALTER TABLE "SteamProxies" drop column "PasswordC";
ALTER TABLE "AspNetUsers" drop column "DigisellerIDC";
ALTER TABLE "AspNetUsers" drop column "DigisellerApiKeyC";
ALTER TABLE "Users" drop column "DigisellerIDC";
ALTER TABLE "Users" drop column "DigisellerApiKeyC";

11.12.2024 Задача 140
CREATE VIEW "GamePublishersView" AS
SELECT 
	row_number() OVER (PARTITION BY gp."Id", pub->>'creator_clan_account_id' ORDER BY pub->>'creator_clan_account_id') AS "Id",
	(pub->>'creator_clan_account_id')::BIGINT AS "GamePublisherId", 
	gp."Id" AS "GameId", 
	pub->>'name' AS "Name"
FROM "Games" gp,
	 json_array_elements(gp."GameInfo"->'basic_info'->'publishers') pub
WHERE (pub->>'creator_clan_account_id')::BIGINT IS NOT NULL;

11.12.2024 Забытая колонка?
ALTER TABLE "Items" ADD column "InSetPriceProcess" TIMESTAMP null;
