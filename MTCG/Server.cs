using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Webserver;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace MTCG
{
    public class Server
    {
        ConcurrentQueue<Package> packages = new ConcurrentQueue<Package>();
        private volatile User fp;//Firstplayer joining matchmaking
        private SemaphoreSlim battleLimit = new SemaphoreSlim(2, 2);//Limits the users joining matchmaking
        private SemaphoreSlim lockSerial = new SemaphoreSlim(1, 1);//Limits on one user going through
        private SemaphoreSlim lockResult = new SemaphoreSlim(0, 1);//Locks the first player for waiting on the result
        private List<string> log;//The returned log

        #region ClassesForDeserialization
        public class UserDes//classes for deserialization of jsons, should be put in another file
        {
            public string Username;
            public string Password;
        }
        class CardDes
        {
            public Guid id;
            public string name;
            public double damage;
        }
        class TradeDes
        {
            public Guid id;
            public Guid CardToTrade;
            public Cardtype Type;
            public int MinimumDamage;
            public Element element;
        }
        #endregion
        public Server(IDatabase db)
        {
            this.db = db;
        }
        IDatabase db;
        public void RegisterRoutes()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() } };//Enables Conversion of enums
            HTTPServer server = new HTTPServer(10001);
            server.RegisterRoute("POST", "/users", (ac,sw) =>
            {
                var res = JsonConvert.DeserializeObject<UserDes>(ac.Payload);//deserialize the basic user(name,pw)
                User user = new User(Guid.NewGuid(), res.Username, res.Password);
                if (Register(user))
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.Created, "User successfully registered");
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.Conflict, "User already exists");
            });
            server.RegisterRoute("POST", "/sessions", (ac, sw) =>
            {
                var res = JsonConvert.DeserializeObject<UserDes>(ac.Payload);
                User user = db.ReadPlayer(res.Username);
                if (user!=null&&Login(res))
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, $"Authorization: Basic {user.Username}-mtcgToken");
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound,"User not found or Password false");
            });
            server.RegisterRoute("POST", "/packages", (ac, sw) =>
            {
                string user;
                if ((user=CheckAuthorization(ac))==null||user!="admin")//check if correct user or admin
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                List<CardDes> cards = JsonConvert.DeserializeObject<List<CardDes>>(ac.Payload);
                List<Card> res = new List<Card>();
                foreach (var item in cards)//go through every card parse it and then save it to the db.
                {
                    Element element;
                    if (item.name.ToLower().Contains("water"))
                        element = Element.Water;
                    else if (item.name.ToLower().Contains("fire")||item.name=="Dragon")
                        element = Element.Fire;
                    else
                        element = Element.Normal;
                    Card card = new Card(item.id, element, item.name, (int)item.damage);
                    if (!db.CreateCard(card))
                    {
                        HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Error sending Cards to Database");
                        return;
                    }
                    res.Add(card);
                }
                Package package = new Package(Guid.NewGuid(), res);//Create package and save it in the queue and the database. (in the queue because the order of the packages would be weird and the curl script wont work)
                packages.Enqueue(package);
                if (db.CreatePackage(package))
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.Created, "Package sucessfully created");
                    return;
                }

                HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Error sending Package to Database");
            });
            server.RegisterRoute("POST", "/transactions/packages", (ac, sw) =>
            {
                string user;
                if ((user = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }

                User us = db.ReadPlayer(user);
                if (packages.Count == 0)//If there are no packages in Queue
                {
                    List<Package> package = new List<Package>();
                    if ((package = db.ReadPackages()) == null||package.Count==0)//get Packages from database and if there are no packages
                    {
                        HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "There are no packages");
                        return;
                    }
                    package.ForEach(v => packages.Enqueue(v));//Saves every package from the db into the queue
                }

                if(us.Coins<5)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Not enough coins");
                    return;
                }
                packages.TryDequeue(out Package bought);
                us.Coins -= 5;
                us.Stack.AddRange(bought.cards);
                if (!db.UpdatePlayer(us))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not save coins update of Player in database");
                    us.Coins += 5;
                    return;
                }
                if (!db.UpdateStack(us))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not save Stack in database");
                    us.Coins += 5;
                    return;
                }
                if(!db.DeletePackage(bought))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not delete package in database");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Succesfully aquired package");
            });
            server.RegisterRoute("GET", "/cards", (ac, sw) =>
            {
                string username;
                if ((username=CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                if (user != null)
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user.Stack, Formatting.Indented));
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound, "User not found");
            });
            server.RegisterRoute("GET", "/deck", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                if (user != null)
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user.Deck, Formatting.Indented));
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound, "User not found");
            });
            server.RegisterRoute("PUT", "/deck", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                List<Guid> cardids = JsonConvert.DeserializeObject<List<Guid>>(ac.Payload);//Deserialize cards
                if(cardids.Count!=4)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Too many or too few cards");
                    return;
                }
                List<Card> cards = new List<Card>();
                cardids.ForEach(v =>//read every card
                {
                    Card c;
                    if ((c = db.ReadCard(v)) != null)
                        cards.Add(c);
                });
                if (cards.Count != 4)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Too many or too few cards, Problem in reading Card");
                    return;
                }
                if (user != null)
                {
                    bool isinListOrNotLocked = true;
                    
                    cards.ForEach(v =>
                    {//Check if the card is not in the stack or is locked(trading)
                        if (!user.Stack.Contains(v)||v.isLocked)
                            isinListOrNotLocked = false;
                    }) ;
                    if (!isinListOrNotLocked)
                    {
                        HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Not every card is in stack or locked");
                    }
                    else
                    {
                        user.Deck = cards;
                        db.UpdateDeck(user);
                        HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Deck saved");
                    }
                }
                HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "User not found");
            });
            server.RegisterRoute("GET", "/users/", (ac, sw) =>
            {
                string usernameURI = ac.Path.Substring(ac.Path.LastIndexOf('/')+1);
                string username;
                if ((username = CheckAuthorization(ac)) == null||(username!=usernameURI&&username!="admin"))//username=username from token and usernameUri from path. If a admin wants to see a profile they can
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user, Formatting.Indented));
            });
            server.RegisterRoute("PUT", "/users/", (ac, sw) =>
            {
                string usernameURI = ac.Path.Substring(ac.Path.LastIndexOf('/')+1);
                string username;
                if ((username = CheckAuthorization(ac)) == null || username != usernameURI)//username=username from token and usernameUri from path.
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                if (string.IsNullOrWhiteSpace(ac.Payload))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest,"No data for creating user");
                    return;
                }
                User user = db.ReadPlayer(username);
                if (user == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "User not found");
                    return;
                }
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(ac.Payload);
                foreach (var item in data)//see what Key is present and change it
                {
                    switch (item.Key)
                    {
                        case "Name":
                            user.Username = item.Value;
                            break;
                        case "Password":
                            user.SetPassword(item.Value);
                            break;
                        case "Bio":
                            user.Bio = item.Value;
                            break;
                        case "Image":
                            user.Image = item.Value;
                            break;
                    }
                }
                if (db.UpdatePlayer(user))
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "User updated");
                else
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "User could not be updated");
            });
            server.RegisterRoute("GET", "/stats", (ac, sw) =>
            {
                string username;
                
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                if (user != null)
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(new { ELO=user.ELO,WonGames=user.WonGames}, Formatting.Indented));//Create anonymous type so it is more readable in json, could be made into a class
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound,"User not found");
            });
            server.RegisterRoute("GET", "/score", (ac, sw) =>
            {
                List<User> scoreboard=db.ReadScoreboard();
                if (scoreboard != null)
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(scoreboard.Select(u=> new { Username=u.Username,ELO = u.ELO,WonGames = u.WonGames}).ToList(), Formatting.Indented));
                else
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not load scoreboard");
            });
            server.RegisterRoute("POST", "/battle/normal", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user;
                if ((user = db.ReadPlayer(username)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "User not found");
                    return;
                }
                List<string> log=JoinMatchmaking(user,false);
                if (!db.UpdatePlayer(user))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Player could not be updated");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(log, Formatting.Indented));
            });
            server.RegisterRoute("POST", "/battle/random", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user;
                if ((user = db.ReadPlayer(username)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "User not found");
                    return;
                }
                List<string> log = JoinMatchmaking(user,true);
                if (!db.UpdatePlayer(user))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Player could not be updated");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(log, Formatting.Indented));
            });
            server.RegisterRoute("GET", "/tradings", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                List<Trade> trades;
                if ((trades = db.ReadTrades()) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Trades not found");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(trades.Select(t=>new { ID = t.id, Card = t.card,Type=t.cardtype,Element=t.element,minDamage=t.minDamage }), Formatting.Indented));
            });
            server.RegisterRoute("POST", "/tradings", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user;
                if ((user = db.ReadPlayer(username)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "User not found");
                    return;
                }
                
                TradeDes deserialized = JsonConvert.DeserializeObject<TradeDes>(ac.Payload);
                Card card;
                if ((card = db.ReadCard(deserialized.CardToTrade)) == null||card.isLocked)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Card not found or locked");
                    return;
                }
                if(user.Deck.Contains(card))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Card contained in deck");
                    return;
                }
                Trade trade;
                if (deserialized.element == default)
                    trade = new Trade(user, deserialized.id, card, deserialized.Type, deserialized.MinimumDamage);
                else
                    trade = new Trade(user, deserialized.id, card, deserialized.Type, deserialized.element);
                if (!db.CreateTrade(trade))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Trade could not be created in the database");
                    return;
                }
                card.isLocked = true;
                if (!db.UpdateCard(card))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Card could not be updated");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Trade successfull and Card is locked");
            });
            server.RegisterRoute("POST", "/tradings/", (ac, sw) =>
            {
                Guid id = Guid.Parse(ac.Path.Substring(ac.Path.LastIndexOf('/')+1));
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                Trade trade = db.ReadTrades().Where(v => v.id == id).FirstOrDefault();
                if (trade == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Trade not found");
                    return;
                }
                User user = db.ReadPlayer(username);
                if (user == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "User not found");
                    return;
                }
                if (trade.user.ID == user.ID)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Cannot trade with yourself");
                    return;
                }
                Guid otherid = JsonConvert.DeserializeObject<Guid>(ac.Payload);
                Card other;
                if ((other = db.ReadCard(otherid))==null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "No other card");
                    return;
                }
                if (trade.TryTrade(other))
                {
                    user.Stack.Remove(other);
                    trade.user.Stack.Remove(trade.card);

                    user.Stack.Add(trade.card);
                    trade.user.Stack.Add(other);
                    bool b1 = db.UpdateStack(user);
                    bool b2 = db.UpdateStack(trade.user);
                    if (!b1||!b2)
                    {
                        HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "You got scammed");
                        return;
                    }
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Trading successful");
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Trading was cancelled");
            });
            server.RegisterRoute("DELETE", "/tradings/", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                List<Trade> trades;
                if ((trades = db.ReadTrades()) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Trade not found");
                    return;
                }
                Guid id = Guid.Parse(ac.Path.Substring(ac.Path.LastIndexOf('/') + 1));
                Trade t = trades.Where(v => v.id == id).FirstOrDefault();
                Card c;
                if ((c = db.ReadCard(t.card.id))==null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Card not found");
                    return;
                }
                c.isLocked = false;
                if (!db.UpdateCard(c))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Card could not be updated");
                    return;
                }
                if (!db.DeleteTrade(id))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Trade could not be deleted");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Trade deleted");
            });
            server.Start();
        }
        public string CheckAuthorization(RequestContext ac)
        {
            string token;
            if (!ac.Header.TryGetValue("Authorization", out token))//there is no token Autorization
                return null;
            string username = token.Split("-")[0].Substring(6);//gets the name out of the token. split the token on '-' get the frist part and then do a substring from the 6th char to the end
            if (db.ReadPlayer(username) != null)//Check if user exists
                return username;
            return null;
        }
        public bool Login(UserDes user)
        {
            User res;
            return (res=db.ReadPlayer(user.Username)) != null&&!res.CheckPassword(user.Password) ? false : true;//Check person exists and check password
        }
        public bool Register(User user)
        {
            if (db.ReadPlayer(user.Username) == null)//Create player if no player
            {
                if (db.CreatePlayer(user))
                {
                    return true;
                }
            }
            return false;//If readPlayer or createplayer doesn't work
        }
        
        
        List<string> JoinMatchmaking(User user,bool isRandom)
        {
            battleLimit.Wait();//2 users can join
            lockSerial.Wait();//1 user can progress

            if(fp is null)//check if firstplayer
            {
                fp = user;
                lockSerial.Release();//The second user is free to start the battle
                lockResult.Wait();//Locks the 1. player until the second player runs the Battle Method

                lockSerial.Release();//Releases the semaphor for other users to join.
                battleLimit.Release(2);//Release for other users having a battle

                return log;
            }
            if (isRandom)
                log = Battle.StartRandomBattle(fp, user);
            else
                log = Battle.StartBattle(fp, user);
            lockResult.Release();//1. user can finish and return the log;
            return log;
        }
    }
}
