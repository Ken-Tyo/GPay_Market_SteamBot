#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["SteamDigiSellerBot/SteamDigiSellerBot.csproj", "SteamDigiSellerBot/"]
COPY ["SteamDigiSellerBot.Database/SteamDigiSellerBot.Database.csproj", "SteamDigiSellerBot.Database/"]
COPY ["SteamDigiSellerBot.Network/SteamDigiSellerBot.Network.csproj", "SteamDigiSellerBot.Network/"]
COPY ["SteamDigiSellerBot.Utilities/SteamDigiSellerBot.Utilities.csproj", "SteamDigiSellerBot.Utilities/"]
RUN dotnet restore "SteamDigiSellerBot/SteamDigiSellerBot.csproj"

COPY . .
WORKDIR "/src/SteamDigiSellerBot"

RUN curl -sL https://deb.nodesource.com/setup_14.x | bash -
RUN apt-get install -y nodejs
RUN npm i
RUN npm run build

RUN dotnet build "SteamDigiSellerBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SteamDigiSellerBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SteamDigiSellerBot.dll"]