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
        #region ClassesForDeserialization
        ConcurrentQueue<Package> packages = new ConcurrentQueue<Package>();
        class UserDes
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
        #endregion
        private IDatabase db = new PostgreSQLDB();
        public void RegisterRoutes()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() { Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() } };
            HTTPServer server = new HTTPServer(10001);
            server.RegisterRoute("POST", "/users", (ac,sw) =>
            {
                var res = JsonConvert.DeserializeObject<UserDes>(ac.Payload);
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
                if ((user=CheckAuthorization(ac))==null||user!="admin")
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                List<CardDes> cards = JsonConvert.DeserializeObject<List<CardDes>>(ac.Payload);
                List<Card> res = new List<Card>();
                foreach (var item in cards)
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
                Package package = new Package(Guid.NewGuid(), res);
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
                        HTTPServer.SendError(sw, HttpStatusCode.NoContent, "There are no packages");
                        return;
                    }
                    package.ForEach(v => packages.Enqueue(v));//Speichert alle packages in die concurrent Queue
                }
                packages.TryDequeue(out Package bought);
                if(us.Coins<5)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Not enough coins");
                    return;
                }
                us.Coins -= 5;
                us.Stack.AddRange(bought.cards);
                if (db.UpdateStack(us) == false)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not save Stack in database");
                    us.Coins += 5;
                    return;
                }
                if(db.DeletePackage(bought) == false)
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
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user.Stack));
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
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user.Deck));
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
                List<Guid> cardids = JsonConvert.DeserializeObject<List<Guid>>(ac.Payload);
                if(cardids.Count!=4)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Too many or too few cards");
                    return;
                }
                List<Card> cards = new List<Card>();
                cardids.ForEach(v =>
                {
                    Card c;
                    if ((c = db.ReadCard(v)) != null)
                        cards.Add(c);
                });
                if (user != null)
                {
                    bool isinListOrNotLocked = true;
                    
                    cards.ForEach(v =>
                    {
                        if (!user.Stack.Contains(v)||v.isLocked)
                            isinListOrNotLocked = false;
                    }) ;
                    if (!isinListOrNotLocked)
                    {
                        HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Not every card is in stack");
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
                if ((username = CheckAuthorization(ac)) == null||(username!=usernameURI&&username!="admin"))//username=username aus token und usernameUri ist der aus dem Path. Wenn ein Admin einen Acc sehen will darf er das.
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                User user = db.ReadPlayer(username);
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(user));
            });
            server.RegisterRoute("PUT", "/users/", (ac, sw) =>
            {
                string usernameURI = ac.Path.Substring(ac.Path.LastIndexOf('/'));
                string username;
                if ((username = CheckAuthorization(ac)) == null || username != usernameURI || username != "admin")//username=username aus token und usernameUri ist der aus dem Path. Wenn ein Admin einen Acc sehen will darf er das.
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                if (ac.Payload == string.Empty)
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
                foreach (var item in data)
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
                    //TODO: Maybe fix if structure bad
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(new { ELO=user.ELO,WonGames=user.WonGames}));
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound,"User not found");
            });
            server.RegisterRoute("GET", "/score", (ac, sw) =>
            {
                List<User> scoreboard=db.ReadScoreboard();
                if (scoreboard != null)
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(scoreboard.Select(u=>(u.Username,u.WonGames,u.ELO)).ToList()));
                else
                    HTTPServer.SendError(sw, HttpStatusCode.InternalServerError, "Could not load scoreboard");
            });
            server.RegisterRoute("POST", "/battles", (ac, sw) =>
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
                List<string> log=JoinMatchmaking(user);
                db.UpdatePlayer(user);
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(log));
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
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, JsonConvert.SerializeObject(trades));
            });
            server.RegisterRoute("POST", "/tradings", (ac, sw) =>
            {
                //ToDo: POST Tradings
            }); 
            server.RegisterRoute("DELETE", "/tradings", (ac, sw) =>
            {
                string username;
                if ((username = CheckAuthorization(ac)) == null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                Guid id = JsonConvert.DeserializeObject<Guid>(ac.Payload);
                if (!db.DeleteTrade(id))
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest, "Trade could not be deleted");
                    return;
                }
                HTTPServer.SendSuccess(sw, HttpStatusCode.OK, "Trade del");
            });
            server.Start();
        }
        string CheckAuthorization(RequestContext ac)
        {
            string token;
            if (!ac.Header.TryGetValue("Authorization", out token))//Es gibt keinen Token
                return null;
            string username = token.Split("-")[0].Substring(6);//Filtert den Namen aus dem Token raus. Als erstes den ersten Teil des tokens getrennt mit - und dann den string ab der 6 Stelle um Basic zu entfernen
            if (db.ReadPlayer(username) != null)
                return username;
            return null;
        }
        bool Login(UserDes user)
        {
            User res;
            return (res=db.ReadPlayer(user.Username)) != null&&!res.CheckPassword(user.Password) ? false : true;
        }
        bool Register(User user)
        {
            if (db.ReadPlayer(user.Username) == null)//Wenn es keinen User mit dem Username gibt dann wird einer erstellt
            {
                db.CreatePlayer(user);
                return true;
            }
            return false;//Wenn es einen gibt dann passiert nix
        }
        
        private volatile User fp;
        private SemaphoreSlim battleLimit = new SemaphoreSlim(2, 2);
        private SemaphoreSlim lockSerial = new SemaphoreSlim(1, 1);
        private SemaphoreSlim lockResult = new SemaphoreSlim(0, 1);
        private List<string> log;
        List<string> JoinMatchmaking(User user)
        {
            battleLimit.Wait();//Nur 2 User können hinein
            lockSerial.Wait();//Nur 1 User kann weiter machen, dass keine Probleme enstehen.

            if(fp is null)
            {
                fp = user;
                lockSerial.Release();//Der 2.te User kann jetzt weiter machen
                lockResult.Wait();//Locks the 1. player until the second player runs the Battle Method

                lockSerial.Release();//Releases the semaphor for other users to join.
                battleLimit.Release(2);

                return log;
            }
            log = Battle.Startbattle(fp, user);

            lockResult.Release();//1. user can finish and return the log;
            return log;
        }
    }
}
