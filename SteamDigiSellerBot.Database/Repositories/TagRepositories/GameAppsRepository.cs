using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamDigiSellerBot.Database.Repositories.TagRepositories
{
    public sealed class GameAppsRepository
    {
        private const string GamesWithMultipleSubIdPlainSQL = @"
WITH cte AS
(SELECT DISTINCT
	""Id"",
	COUNT(1) OVER(PARTITION BY ""AppId"") AS GamesByAppIdCnt
FROM
	public.""Games"" g)
SELECT
    g.""AppId"",
    g.""Name""
FROM
	""Games"" g
JOIN cte ON cte.GamesByAppIdCnt > 1 AND cte.""Id"" = g.""Id""
ORDER BY g.""AppId""";

        private const string GetDlcWithParentPlainSQL = @"
SELECT
	g.""AppId"",
	g.""Name"",
	pg.""AppId"",
	pg.""Name""
FROM
	""Games"" g
LEFT JOIN
	""Games"" pg
ON
	(g.""GameInfo""->'related_items'->>'parent_appid') IS NOT NULL
	AND (g.""GameInfo""->'related_items'->>'parent_appid')::INT <> 0
	AND pg.""AppId"" = (g.""GameInfo""->'related_items'->>'parent_appid')::TEXT
	AND pg.""IsBundle"" = false
WHERE
	g.""IsDlc"" = TRUE";

        private string _connectionString;

        public GameAppsRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Dictionary<string, IGrouping<string, GameApp>> GetAppIdWithNames()
        {
            using NpgsqlConnection dbConnection = new NpgsqlConnection(_connectionString);
            return dbConnection
                .Query<GameApp>(GamesWithMultipleSubIdPlainSQL)
                .GroupBy(x => x.AppId)
                .ToDictionary(x => x.Key);
        }

        public IReadOnlyList<GameAppWithParent> GetDlcWithParent()
        {
            using NpgsqlConnection dbConnection = new NpgsqlConnection(_connectionString);
            return dbConnection
                .Query<GameAppWithParent, GameAppWithParent, GameAppWithParent>(
                    GetDlcWithParentPlainSQL,
                    (GameAppWithParent child, GameAppWithParent parent) =>
                    {
                        child.ParentGameApp = parent;
                        return child;
                    },
                    splitOn: "AppId")
                .ToList();
        }
    }
}
