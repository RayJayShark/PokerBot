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
    class SqlService
    {
        private readonly MySqlConnection connection;

        public SqlService(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }

        public async Task<PokerPlayer> GetPlayerAsync(ulong id)
        {
            connection.Open();

            return await connection.QuerySingleAsync<PokerPlayer>("SELECT * FROM player WHERE discordId = @id", id);
        }
    }
}
