using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using PokerBot.Models;

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

            try
            {
                return
                    await connection.QuerySingleAsync<PokerPlayer>("SELECT * FROM player WHERE discordId = @id",
                        new {id});
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }

        }
        
        public async Task<PokerPlayer> GetPlayerAsync(string playerName)
        {
            connection.Open();

            try
            {
                return
                    await connection.QuerySingleAsync<PokerPlayer>("SELECT * FROM player WHERE username = @playerName",
                        new {playerName});
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task AddPlayerAsync(PokerPlayer player)
        {
            connection.Open();

            try
            {
                await connection.ExecuteAsync("INSERT INTO player (discordId, username) VALUES(@id, @name)", player);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task UpdatePlayersAsync(IEnumerable<PokerPlayer> players)
        {
            connection.Open();

            await connection.ExecuteAsync(
                "UPDATE player SET money = @money, wins = @wins, losses = @losses WHERE discordId = @id", players);

            await connection.CloseAsync();
        }
    }
}
