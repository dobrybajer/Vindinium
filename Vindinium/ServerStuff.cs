using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace vindinium
{
    public class ServerStuff
    {
        private readonly string _key;
        private readonly bool _trainingMode;
        private readonly uint _turns;
        private readonly string _map;

        private string _playUrl;
        public string ViewUrl { get; private set; }

        public Hero MyHero { get; private set; }
        public List<Hero> Heroes { get; private set; }

        public int CurrentTurn { get; private set; }
        public int MaxTurns { get; private set; }
        public bool Finished { get; private set; }
        public bool Errored { get; private set; }
        public string ErrorText { get; private set; }
        private readonly string _serverUrl;

        public Tile[][] Board { get; private set; }

        //if training mode is false, turns and map are ignored8
        public ServerStuff(string key, bool trainingMode, uint turns, string serverUrl, string map)
        {
            _key = key;
            _trainingMode = trainingMode;
            _serverUrl = serverUrl;

            //the reaons im doing the if statement here is so that i dont have to do it later
            if (!trainingMode) return;
            _turns = turns;
            _map = map;
        }

        //initializes a new game, its syncronised
        public void CreateGame()
        {
            Errored = false;

            string uri;
            
            if (_trainingMode)
            {
                uri = _serverUrl + "/api/training";
            }
            else
            {
                uri = _serverUrl + "/api/arena";
            }

            var myParameters = "key=" + _key;
            if (_trainingMode) myParameters += "&turns=" + _turns;
            if (_map != null) myParameters += "&map=" + _map;
            myParameters += "&timeout_move=" + 100000;

            //make the request
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                try
                {
                    var result = client.UploadString(uri, myParameters);
                    Deserialize(result);
                }
                catch (WebException exception)
                {
                    Errored = true;
                    using (var reader = new StreamReader(exception.Response?.GetResponseStream() ?? Stream.Null))
                    {
                        ErrorText = reader.ReadToEnd();
                    }
                }
            }
        }

        private void Deserialize(string json)
        {
            // convert string to stream
            var byteArray = Encoding.UTF8.GetBytes(json);
            //byte[] byteArray = Encoding.ASCII.GetBytes(json);
            var stream = new MemoryStream(byteArray);

            var ser = new DataContractJsonSerializer(typeof(GameResponse));
            var gameResponse = (GameResponse)ser.ReadObject(stream);

            _playUrl = gameResponse.playUrl;
            ViewUrl = gameResponse.viewUrl;

            MyHero = gameResponse.hero;
            Heroes = gameResponse.game.heroes;

            CurrentTurn = gameResponse.game.turn;
            MaxTurns = gameResponse.game.maxTurns;
            Finished = gameResponse.game.finished;

            CreateBoard(gameResponse.game.board.size, gameResponse.game.board.tiles);
        }

        public void MoveHero(string direction)
        {
            var myParameters = "key=" + _key + "&dir=" + direction;
            
            //make the request
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                try
                {
                    var result = client.UploadString(_playUrl, myParameters);
                    Deserialize(result);
                }
                catch(WebException exception)
                {
                    Errored = true;
                    using(var reader = new StreamReader(exception.Response?.GetResponseStream() ?? Stream.Null))
                    {
                        ErrorText = reader.ReadToEnd();
                    }
                }
            }
        }

        private void CreateBoard(int size, string data)
        {
            //check to see if the board list is already created, if it is, we just overwrite its values
            if (Board == null || Board.Length != size)
            {
                Board = new Tile[size][];

                //need to initialize the lists within the list
                for (var i = 0; i < size; i++)
                {
                    Board[i] = new Tile[size];
                }
            }

            //convert the string to the List<List<Tile>>
            var x = 0;
            var y = 0;
            var charData = data.ToCharArray();

            for(var i = 0;i < charData.Length;i += 2)
            {
                switch (charData[i])
                {
                    case '#':
                        Board[x][y] = Tile.IMPASSABLE_WOOD;
                        break;
                    case ' ':
                        Board[x][y] = Tile.FREE;
                        break;
                    case '@':
                        switch (charData[i + 1])
                        {
                            case '1':
                                Board[x][y] = Tile.HERO_1;
                                break;
                            case '2':
                                Board[x][y] = Tile.HERO_2;
                                break;
                            case '3':
                                Board[x][y] = Tile.HERO_3;
                                break;
                            case '4':
                                Board[x][y] = Tile.HERO_4;
                                break;

                        }
                        break;
                    case '[':
                        Board[x][y] = Tile.TAVERN;
                        break;
                    case '$':
                        switch (charData[i + 1])
                        {
                            case '-':
                                Board[x][y] = Tile.GOLD_MINE_NEUTRAL;
                                break;
                            case '1':
                                Board[x][y] = Tile.GOLD_MINE_1;
                                break;
                            case '2':
                                Board[x][y] = Tile.GOLD_MINE_2;
                                break;
                            case '3':
                                Board[x][y] = Tile.GOLD_MINE_3;
                                break;
                            case '4':
                                Board[x][y] = Tile.GOLD_MINE_4;
                                break;
                        }
                        break;
                }

                //time to increment x and y
                x++;
                if (x == size)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }
}