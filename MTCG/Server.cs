using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Webserver;
using Newtonsoft.Json;
namespace MTCG
{
    public class Server
    {
        #region ClassesForDeserialization
        class UserDes
        {
            public string Username;
            public string Password;
        }
        #endregion
        private IDatabase db = new PostgreSQLDB();
        public void RegisterRoutes()
        {
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
                if (user!=null&&Login(user))
                {
                    HTTPServer.SendSuccess(sw, HttpStatusCode.OK, $"Authorization: Basic {user.Username}-mtcgToken");
                    return;
                }
                HTTPServer.SendError(sw, HttpStatusCode.NotFound,"User not found");
            });
            server.RegisterRoute("POST", "/packages", (ac, sw) =>
            {
                if (CheckAuthorization(ac)==null)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");
                    return;
                }
                //ToDO: Create Packages
            });
            server.RegisterRoute("POST", "/transactions/packages", (ac, sw) =>
            {
                var res = JsonConvert.DeserializeObject<UserDes>(ac.Payload);
                User us = db.ReadPlayer(res.Username);
                GetPackage(us);
                //ToDO: Acquire Packages
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
                if (user != null)
                { 
                    //ToDo: ReadCards with DB and check for correct user
                    //user.Deck.Contains()
                }
            });
            server.RegisterRoute("GET", "/users/", (ac, sw) =>
            {
                string usernameURI = ac.Path.Substring(ac.Path.LastIndexOf('/'));
                string username;
                if ((username = CheckAuthorization(ac)) == null||username!=usernameURI||username!="admin")//username=username aus token und usernameUri ist der aus dem Path. Wenn ein Admin einen Acc sehen will darf er das.
                {
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");//ToDo: Test get user mit admin.
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
                    HTTPServer.SendError(sw, HttpStatusCode.Forbidden, "Invalid token");//ToDo: Test put user mit admin.
                    return;
                }
                if (ac.Payload == string.Empty)
                {
                    HTTPServer.SendError(sw, HttpStatusCode.BadRequest,"No data for creating user");
                    return;
                }
                User user = db.ReadPlayer(username);
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
                //ToDo: GetAllUsersAndOrderBY select elo, wongames from user order by elo, wongames;
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
                if ((user=db.ReadPlayer(username))==null)
                    return;
                JoinMatchmaking(user);
            });
            server.RegisterRoute("GET", "/tradings", (ac, sw) =>
            {

            });
            server.RegisterRoute("POST", "/tradings", (ac, sw) =>
            {

            }); 
            server.RegisterRoute("DELETE", "/tradings", (ac, sw) =>
            {

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
        bool Login(User user)
        {
            return db.ReadPlayer(user.Username) == null ? false : true;
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

        void JoinMatchmaking(User user)
        {

        }

        Scoreboard Scoreboard()
        {
            throw new NotImplementedException();
        }

        bool ChangeUsername(string username)
        {
            throw new NotImplementedException();
        }

        bool ChangePassword(string password)
        {
            throw new NotImplementedException();
        }

        int GetELO(User user) => user.ELO;

        List<Card> GetPackage(User user)
        {
            throw new NotImplementedException();
        }


    }
}
