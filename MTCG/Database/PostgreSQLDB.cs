using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using NpgsqlTypes;
using System.Linq;
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
        public PostgreSQLDB()
        {
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Element>("Element");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<Cardtype>("Cardtype");
        }
        public User ReadPlayer(string username)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"Id\", \"Password\", \"Coins\", \"Elo\", \"WonGames\", \"Image\", \"Bio\" from \"user\" where \"Name\"=@username", conn);
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
                List<Card> deck, stack;
                deck = ReadDeck(guid);
                stack = ReadStack(guid);

                return new User(guid, username, pw, coins, elo, won, image, bio, deck, stack);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public User ReadPlayer(Guid id)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"username\", \"Password\", \"Coins\", \"Elo\", \"WonGames\", \"Image\", \"Bio\" from \"user\" where \"Id\"=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;

                string username = reader.GetString(0);
                byte[] pw = reader.GetFieldValue<byte[]>(1);
                int coins = reader.GetInt32(2);
                int elo = reader.GetInt32(3);
                int won = reader.GetInt32(4);
                string image = reader.GetString(5);
                string bio = reader.GetString(6);
                List<Card> deck, stack;
                deck = ReadDeck(id);
                stack = ReadStack(id);

                return new User(id, username, pw, coins, elo, won, image, bio, deck, stack);
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
                using var cmd = new NpgsqlCommand("update user set Name=@name, Password=@password, Image=@image, Bio=@bio, Elo=@elo, WonGames=@wonGames, Coins=@coins where \"Id\"=@id", conn);
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
                CreateDeck(user);
                CreateStack(user);
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
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"Card\" values (@id, @name, @damage, @isLocked, @element, @cardtype)", conn);
                cmd.Parameters.AddWithValue("@id", card.id) ;
                cmd.Parameters.AddWithValue("@name", card.name);
                cmd.Parameters.AddWithValue("@damage", card.damage);
                cmd.Parameters.AddWithValue("@isLocked", card.isLocked);
                cmd.Parameters.AddWithValue("@element", card.element);
                cmd.Parameters.AddWithValue("@cardtype", card.type);


                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public Card ReadCard(Guid id)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"Name\", \"Damage\", \"isLocked\", \"Element\", \"Cardtype\" from \"Card\" where \"id\"=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                string name = reader.GetString(0);
                int damage = reader.GetInt32(1);
                bool isLocked = reader.GetBoolean(2);
                Element element = reader.GetFieldValue<Element>(3);
                Cardtype type = reader.GetFieldValue<Cardtype>(4);


                return new Card(id,element,name,damage,isLocked);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool CreatePackage(Package pack)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"Package\" values (@id,@cards)", conn);
                cmd.Parameters.AddWithValue("@id", pack.id);
                cmd.Parameters.AddWithValue("@cards",pack.cards.Select((v)=>v.id).ToList());

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<Package> ReadPackages()
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"ID\", \"Cards\" from \"Package\"", conn);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                List<Package> packages = new List<Package>();
                while (reader.Read())
                {
                    Guid id = reader.GetFieldValue<Guid>(0);
                    Guid[] cards = reader.GetFieldValue<Guid[]>(1);

                    List<Card> res = new List<Card>();
                    foreach (var item in cards)
                    {
                        Card tmp;
                        if ((tmp = ReadCard(item)) != null)
                        {
                            res.Add(tmp);
                        }
                    }
                    packages.Add(new Package(id, res));
                }
                return packages;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool CreateDeck(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"Deck\" values (@id,@cards)", conn);
                cmd.Parameters.AddWithValue("@id", user.ID);
                cmd.Parameters.AddWithValue("@cards", user.Deck.Select((v) => v.id).ToList());

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("CreateDeck");
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool UpdateDeck(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("update \"Deck\" set \"Card\"=@cards where @id=\"Player\"", conn);
                cmd.Parameters.AddWithValue("@id", user.ID);
                cmd.Parameters.AddWithValue("@cards", user.Deck.Select((v) => v.id).ToList());

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<Card> ReadDeck(Guid id)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"Card\" from \"Deck\" where \"Player\"=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                Guid[] cards = reader.GetFieldValue<Guid[]>(0);

                List<Card> res = new List<Card>();
                foreach (var item in cards)
                {
                    Card tmp;
                    if ((tmp = ReadCard(item)) != null)
                    {
                        res.Add(tmp);
                    }
                }
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public bool CreateTrade(Trade trade)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"Trade\" values (@id,@card,@cardtype,@element,@mindamage,@player)", conn);
                cmd.Parameters.AddWithValue("@id", trade.id);
                cmd.Parameters.AddWithValue("@card", trade.card.id);
                cmd.Parameters.AddWithValue("@cardtype", trade.cardtype);
                cmd.Parameters.AddWithValue("@element", trade.element);
                cmd.Parameters.AddWithValue("@mindamage", trade.minDamage);
                cmd.Parameters.AddWithValue("@player", trade.user);

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<Trade> ReadTrades()
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("SELECT \"Id\", card, cardtype, element, minDamage, user FROM \"Trade\"", conn);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                List<Trade> trades = new List<Trade>();
                while (reader.Read())
                {
                    Guid guid = reader.GetFieldValue<Guid>(0);
                    Guid carduuid = reader.GetFieldValue<Guid>(1);
                    Cardtype type = reader.GetFieldValue<Cardtype>(2);
                    Element element = reader.GetFieldValue<Element>(3);
                    int minDamage = reader.GetInt32(4);
                    Guid userid = reader.GetGuid(5);
                    User user = ReadPlayer(userid);
                    Card card = ReadCard(carduuid);
                    if (element == default)
                        trades.Add(new Trade(user,guid, card, type, minDamage));
                    else
                        trades.Add(new Trade(user,guid, card, type, element));
                }
                return trades;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
        }

        /*public bool UpdateTrade(Trade trade)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("update Trade set card=@card, cardtype=@cardtype, element=@element, mindamage=@mindamage where \"Id\"=@id", conn);
                cmd.Parameters.AddWithValue("@id", trade.id);
                cmd.Parameters.AddWithValue("@card", trade.card.id);
                cmd.Parameters.AddWithValue("@cardtype", trade.cardtype);
                cmd.Parameters.AddWithValue("@element", trade.element);
                cmd.Parameters.AddWithValue("@mindamage", trade.minDamage);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }*/
        public bool DeleteTrade(Guid id)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("delete from \"Trade\" where @id=\"ID\"", conn);
                cmd.Parameters.AddWithValue("@id", id);

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
    
        public bool DeletePackage(Package pack)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("delete from \"Package\" where @id=\"ID\"", conn);
                cmd.Parameters.AddWithValue("@id", pack.id);

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool CreateStack(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("insert into \"Stack\" values (@id,@cards)", conn);
                cmd.Parameters.AddWithValue("@id", user.ID);
                cmd.Parameters.AddWithValue("@cards", user.Stack.Select((v) => v.id).ToList());

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool UpdateStack(User user)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("update \"Stack\" set \"Card\"=@cards where @id=\"Player\"", conn);
                cmd.Parameters.AddWithValue("@id", user.ID);
                cmd.Parameters.AddWithValue("@cards", user.Stack.Select((v) => v.id).ToList());

                Console.WriteLine(cmd.CommandText);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<Card> ReadStack(Guid id)
        {
            try
            {
                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"Card\" from \"Stack\" where \"Player\"=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                Guid[] cards = reader.GetFieldValue<Guid[]>(0);

                List<Card> res = new List<Card>();
                foreach (var item in cards)
                {
                    Card tmp;
                    if ((tmp = ReadCard(item)) != null)
                    {
                        res.Add(tmp);
                    }
                }
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public List<User> ReadScoreboard()
        {
            try
            {

                using NpgsqlConnection conn = OpenConnection();
                using var cmd = new NpgsqlCommand("select \"Id\", \"Password\", \"Coins\", \"Elo\", \"WonGames\", \"Image\", \"Bio\", \"Name\" from \"user\" order by \"Elo\", \"WonGames\"", conn);
                cmd.Prepare();

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (!reader.Read())
                    return null;
                List<User> sortedList = new List<User>();
                while (reader.Read())
                {
                    Guid guid = reader.GetFieldValue<Guid>(0);
                    byte[] pw = reader.GetFieldValue<byte[]>(1);
                    int coins = reader.GetInt32(2);
                    int elo = reader.GetInt32(3);
                    int won = reader.GetInt32(4);
                    string image = reader.GetString(5);
                    string bio = reader.GetString(6);
                    string username = reader.GetString(7);
                    sortedList.Add(new User(guid, username, pw, coins, elo, won, image, bio,null,null));
                }
                return sortedList;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
