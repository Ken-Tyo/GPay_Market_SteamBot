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