using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using PokerBot.Classes;

namespace PokerBot.Services
{
    public class SqlService
    {
        private readonly MySqlConnection connection;

        public SqlService(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }

        public async Task<PokerPlayer> GetPlayerAsync(ulong id)
        {
            connection.Open();

            var player = await connection.QuerySingleAsync<PokerPlayer>("SELECT * FROM player WHERE discordId = @id", id);

            await connection.CloseAsync();

            return player;
        }

        public async Task AddPlayerAsync(PokerPlayer player)
        {
            connection.Open();

            await connection.ExecuteAsync("INSERT INTO player (discordId, username) VALUES(@id, @name)", player);

            await connection.CloseAsync();
        }

        public async Task UpdatePlayersAsync(IEnumerable<PokerPlayer> players)
        {
            connection.Open();

            await connection.ExecuteAsync(
                "UPDATE player SET money = @money, wins = @wins, losses = @losses WHERE discordId = id", players);

            await connection.CloseAsync();
        }
    }
}
