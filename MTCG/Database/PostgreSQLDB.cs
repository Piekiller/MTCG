using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;

namespace MTCG
{
    public class PostgreSQLDB:IDatabase
    {
        public NpgsqlConnection OpenConnection()
        {
            NpgsqlConnection conn =new NpgsqlConnection("Host=localhost;Username=Mtcg;Password=mtcg;Database=MTCG");
            conn.Open();
            return conn;
        }

        public User ReadPlayer(string username)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select id, pwhash, coins, elo, wongames, image, bio from \"user\" where username=@username", conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;

                Guid guid = reader.GetFieldValue<Guid>(0);
                byte[] pw = reader.GetFieldValue<byte[]>(1);
                int coins = reader.GetInt32(2);
                int elo = reader.GetInt32(3);
                int won = reader.GetInt32(4);
                string image = reader.GetString(5);
                string bio = reader.GetString(6);
                return new User(guid,username,pwhash,coins,elo,won,image,bio);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool UpdatePlayer(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("update user set Name=@name, Password=@password, Image=@image, Bio=@bio, Elo=@elo, WonGames=@wonGames, Coins=@coins where id=@id", conn);
                cmd.Parameters.AddWithValue("@name", user.Username);
                cmd.Parameters.AddWithValue("@password", user.Username);
                cmd.Parameters.AddWithValue("@image", user.Image);
                cmd.Parameters.AddWithValue("@bio", user.Bio);
                cmd.Parameters.AddWithValue("@elo", user.ELO);
                cmd.Parameters.AddWithValue("@wonGames", user.WonGames);
                cmd.Parameters.AddWithValue("@coins", user.Coins);
                cmd.Parameters.AddWithValue("@id", user.ID);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CreatePlayer(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"user\" values (@name, @password, @image, @bio, @elo, @wonGames, @coins, @id)", conn);
                cmd.Parameters.AddWithValue("@name", user.Username);
                cmd.Parameters.AddWithValue("@password", user.PWHash);
                cmd.Parameters.AddWithValue("@image", user.Image);
                cmd.Parameters.AddWithValue("@bio", user.Bio);
                cmd.Parameters.AddWithValue("@elo", user.ELO);
                cmd.Parameters.AddWithValue("@wonGames", user.WonGames);
                cmd.Parameters.AddWithValue("@coins", user.Coins);
                cmd.Parameters.AddWithValue("@id", user.ID);

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(   e.Message);
                return false;
            }
        }

        public bool CreateCard(Card card)
        {
            throw new NotImplementedException();
        }

        public Card ReadCard(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CreatePackage(Package pack)
        {
            throw new NotImplementedException();
        }

        public Package ReadPackage(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool CreateDeck(Card[] deck)
        {
            throw new NotImplementedException();
        }

        public bool UpdateDeck(Card[] deck)
        {
            throw new NotImplementedException();
        }

        public Card[] ReadDeck(Guid player)
        {
            throw new NotImplementedException();
        }

        public bool CreateTrade(Trade trade)
        {
            throw new NotImplementedException();
        }

        public Trade[] ReadTrades()
        {
            throw new NotImplementedException();
        }

        public bool UpdateTrade(Trade trade)
        {
            throw new NotImplementedException();
        }
    }
}
