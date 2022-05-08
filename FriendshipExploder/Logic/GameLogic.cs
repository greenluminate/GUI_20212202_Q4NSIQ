using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FriendshipExploder.Menu;
using FriendshipExploder.Model;

namespace FriendshipExploder.Logic
{
    public class GameLogic : IGameModel, IGameControl
    {
        public Random rnd { get; set; }
        private Queue<string[]> playgrounds; //path-okat tartalmaz, előre generált pálxák? //Mert vagy beletesszük ak iválaszott pályát választott meccs számszor, vagy előre legeneráljuk a random pélykat, csak beletesszük, hogy melyik fix és melyik, mely random. VAgy kuka az egész és mindig más laapján generálunk random.
        //Lehet ide kéne betenn ia köztes képernyőket is pl.: MainMenu, playground, who win image, curren leaderboard image, next playground és így körbe.

        //pályán lévő elemek (játékos, fal, stb.)
        public IElement[,] Elements { get; set; }
        public IElement[,] Powerups { get; set; }

        public List<Player> Players { get; set; }

        //játéktér mérete (cella x, cella y)
        public int[] PlayGroundSize { get; set; }

        //kockák mérete
        private int GameRectSize { get; set; }

        private Point GameSize { get; set; }

        public string Timer { get; set; }

        public object _ElementsListLockObject { get; set; }
        public object _PowerupsListLockObject { get; set; }
        public object _PlayersListLockObject { get; set; }
        public object _TimerLockObject { get; set; }
        public double PlayerHeightRate { get; set; }
        public double PlayerHeightRateHangsIn { get; set; }
        public double PlayerWidthRate { get; set; }
        public bool GamePaused { get; set; }
        public bool RoundOver { get; set; }
        public bool RoundScore { get; set; }

        public GameLogic()
        {
            _ElementsListLockObject = new object();
            _PowerupsListLockObject = new object();
            _PlayersListLockObject = new object();
            _TimerLockObject = new object();

            rnd = new Random();

            Players = new List<Player>();

            playgrounds = new Queue<string[]>();
            PlayGroundSize = new int[2];

            PlayerHeightRate = 0.8;
            PlayerHeightRateHangsIn = 0.2;
            PlayerWidthRate = 0.6;

            RoundOver = false;
            RoundScore = false;

            //Ha választott pálya design, akkor betöltjük azt válaszott mennyiségszer a queue-ba, ha randomizáltat választotak a fixek közül, akkor random tötljülk be a fixeket


            //Későbbi feature lehet: menüből választhatnak pályaméretet.
            //Későbbi feature lehet: random generált pálya design.
        }

        public void LoadPlayground(string file, int rounds)
        {
            if (Directory.Exists("Playgrounds") && File.Exists(@$"Playgrounds\{file}.txt"))
            {
                string[] rows = File.ReadAllLines(@$"Playgrounds\{file}.txt");

                PlayGroundSize[0] = rows[0].Length;
                PlayGroundSize[1] = rows.Length;

                Elements = new IElement[PlayGroundSize[0], PlayGroundSize[1]];
                Powerups = new IElement[PlayGroundSize[0], PlayGroundSize[1]];

                for (int i = 0; i < rounds; i++)
                {
                    playgrounds.Enqueue(rows);
                }
                LoadNext(playgrounds.Dequeue());
            }
        }

        public void SetupSize(Point gameSize)
        {
            this.GameSize = gameSize;
            Players.ForEach(x => { x.Speed = (int)gameSize.X / 300; });
        }

        public void SetupRectSize(int gameRectSize)
        {
            //játékos reszponzivitás
            if (this.GameRectSize != gameRectSize)
            {
                FixCharacterPosition(gameRectSize);
            }
            this.GameRectSize = gameRectSize;
        }

        //karakter reszponzivitása átméretezésnél
        private void FixCharacterPosition(int gameRectSize)
        {
            //ToDo: ez csak akkor hívódjon meg, ha rossz helyre tesszük alapból a karaktert. De ha megoldjuk, akkro soha.
            if (GameRectSize != 0)
            {
                int corrigSize = gameRectSize - this.GameRectSize;
                lock (_PlayersListLockObject)
                {
                    Players.ForEach(x =>
                    {
                        bool leftPlayground = false;

                        if (x.Position.X + corrigSize < 0) { x.SetPos(0, x.Position.Y); leftPlayground = true; } //bal
                        else if (x.Position.X + (PlayerWidthRate * GameRectSize) + corrigSize > (PlayGroundSize[0] - 1) * GameRectSize) { x.SetPos((int)(((PlayGroundSize[0] - 1) * GameRectSize) - (PlayerWidthRate * GameRectSize)), x.Position.Y); leftPlayground = true; } //jobb

                        if (x.Position.Y + corrigSize < 0) { x.SetPos(x.Position.X, 0); leftPlayground = true; } //fent
                        else if (x.Position.Y + (PlayerHeightRate * gameRectSize) + corrigSize > (PlayGroundSize[1] - 1) * GameRectSize) { x.SetPos(x.Position.X, (int)(((PlayGroundSize[1] - 1) * GameRectSize) - (PlayerWidthRate * GameRectSize))); leftPlayground = true; } //lent

                        if (!leftPlayground)
                        {
                            if (CanStepToPos(x, new System.Windows.Vector(corrigSize, corrigSize)))
                            {
                                lock (_PlayersListLockObject)
                                {
                                    x.Move(corrigSize, corrigSize);
                                }
                            }
                            else
                            {
                                lock (_PlayersListLockObject)
                                {
                                    int cellX = x.Position.X / GameRectSize;
                                    int cellY = x.Position.Y / GameRectSize;
                                    x.SetPos(cellX * gameRectSize + (gameRectSize / 2), cellY * gameRectSize + (gameRectSize / 2));
                                }
                            }
                        }
                    });
                }
            }
        }

        private void LoadNext(string[] grounds)
        {
            //Betöltjük a válaszott pályadesignt = enumok (vagy fix, vagy random kérés) a választott menyniségű játékossal.
            for (int i = 0; i < PlayGroundSize[0] - 1; i++)
            {
                for (int j = 0; j < PlayGroundSize[1] - 1; j++)
                {
                    switch (grounds[j][i])
                    {
                        case 'f':
                            lock (_ElementsListLockObject)
                            {
                                Elements[i, j] = new FixWall(new Point(i, j), ElementType.FixWall);
                            }
                            break;
                        case 'w':
                            lock (_ElementsListLockObject)
                            {
                                Elements[i, j] = new Wall(new Point(i, j), ElementType.Wall);
                            }
                            lock (_PowerupsListLockObject)
                            {
                                int powerupRndNum = rnd.Next(0, 100);

                                switch (powerupRndNum)
                                {
                                    case < 20:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.BomUp);
                                        break;
                                    case < 40:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.BlastUp);
                                        break;
                                    case < 60:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.SpeedUp);
                                        break;
                                    case < 70:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.Kick);
                                        break;
                                    case < 80:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.Schedule);
                                        break;
                                    case < 88:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.Desease);
                                        break;
                                    case < 95:
                                        Powerups[i, j] = new Powerup(new Point(i, j), ElementType.SpeedDown);
                                        break;
                                    default:
                                        Powerups[i, j] = null;
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            AITaskCreator();
            CountDown(150);
        }

        private void CountDown(int seconds)
        {
            Task countDownTask = new Task(() =>
            {
                DateTime StartTime = new DateTime(1, 1, 1, 0, seconds / 60, seconds % 60);
                this.Timer = StartTime.ToString(@"mm\:ss");
                Thread.Sleep(1000);
                DateTime StartDate = DateTime.Now;

                while (!this.Timer.Equals("00:00") && !RoundOver)
                {
                    Thread.Sleep(50);
                    if (!GamePaused && !RoundOver)
                    {
                        this.Timer = StartTime.AddTicks(StartDate.Ticks - DateTime.Now.Ticks).ToString(@"mm\:ss");
                    }
                    else
                    {
                        long stopDifference = StartDate.Ticks - DateTime.Now.Ticks;

                        lock (_TimerLockObject)
                        {
                            Monitor.Wait(_TimerLockObject);
                        }
                        StartDate = DateTime.Now.AddTicks(stopDifference);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            countDownTask.ContinueWith((t) =>
            {
                int startX = 0;
                int startY = 0;
                int endX = PlayGroundSize[0] - 1;
                int endY = PlayGroundSize[1] - 1;

                while (!RoundOver && (!(startX >= endX) || !(startY >= endY)))
                {
                    //Horizontal fill top
                    for (int x = startX; x < endX; x++)
                    {
                        if (GamePaused)
                        {
                            lock (_TimerLockObject)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }

                        lock (_ElementsListLockObject)
                        {
                            Elements[x, startY] = new FixWall(new Point(x, startY), ElementType.FixWall);
                        }

                        lock (_PlayersListLockObject)
                        {
                            PlayerKiller(new Point(x, startY));
                        }

                        Thread.Sleep(50);
                    }

                    //Vertical fill right
                    for (int y = startY; y < endY; y++)
                    {
                        if (GamePaused)
                        {
                            lock (_TimerLockObject)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }

                        lock (_ElementsListLockObject)
                        {
                            Elements[endX - 1, y] = new FixWall(new Point(endX - 1, y), ElementType.FixWall);
                        }

                        lock (_PlayersListLockObject)
                        {
                            PlayerKiller(new Point(endX - 1, y));
                        }

                        Thread.Sleep(50);
                    }

                    //Horizontal fill bottom
                    for (int x = endX - 1; x >= startX; x--)
                    {
                        if (GamePaused)
                        {
                            lock (_TimerLockObject)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }

                        lock (_ElementsListLockObject)
                        {
                            Elements[x, endY - 1] = new FixWall(new Point(x, endY - 1), ElementType.FixWall);
                        }
                        lock (_PlayersListLockObject)
                        {
                            PlayerKiller(new Point(x, endY - 1));
                        }

                        Thread.Sleep(50);
                    }

                    //Vertical fill left
                    for (int y = endY - 1; y >= startY; y--)
                    {
                        if (GamePaused)
                        {
                            lock (_TimerLockObject)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }

                        lock (_ElementsListLockObject)
                        {
                            Elements[startX, y] = new FixWall(new Point(startX, y), ElementType.FixWall);
                        }

                        lock (_PlayersListLockObject)
                        {
                            PlayerKiller(new Point(startX, y));
                        }

                        Thread.Sleep(50);
                    }

                    startX++;
                    startY++;
                    endX--;
                    endY--;
                }
            });
            countDownTask.Start();
        }

        private void PlayerKiller(Point playerCoords)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (PlayerPixelToMatrixCoordinate(Players[i].Position) == playerCoords)
                {
                    Players[i].Explode = true;

                    for (int j = 0; j < 200; j++)
                    {
                        if (GamePaused)
                        {
                            lock (_TimerLockObject)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }
                        Thread.Sleep(1);
                    }
                    Players.Remove(Players[i]);
                    i--;
                    if (Players.Count == 0)
                    {
                        RoundOver = true;
                    }
                }
            }
        }

        public enum PlayerAction
        {
            up, down, left, right,
            W, A, S, D,
            bombudlr, bombwasd,
            actionudlr, actionwasd
        }

        private Point ItemPixelToMatrixCoordinate(Point position)
        {
            int playerIndexX = (int)Math.Floor((decimal)((position.X + GameRectSize / 2) / GameRectSize));
            int playerIndexY = (int)Math.Floor((decimal)((position.Y + GameRectSize / 2) / GameRectSize));

            return new Point(playerIndexX, playerIndexY);
        }

        private Point PlayerPixelToMatrixCoordinate(Point position)//Itt kell hozzáadni a két %-át az Yhoz, hogy a hitboxát nézzük ak rakternek és a render nagyságát.
        {
            int playerIndexX = (int)Math.Floor((decimal)((position.X + (GameRectSize * PlayerWidthRate) / 2) / GameRectSize));
            int playerIndexY = (int)Math.Floor((decimal)((position.Y + (GameRectSize * PlayerHeightRate) / 2) / GameRectSize));

            return new Point(playerIndexX, playerIndexY);
        }

        private Point PixelToMatrixCoordinate(Point position)
        {
            int playerCornerIndexX = (int)Math.Floor((decimal)(position.X / GameRectSize));
            int playerCornerIndexY = (int)Math.Floor((decimal)(position.Y / GameRectSize));

            return new Point(playerCornerIndexX, playerCornerIndexY);
        }

        //Odaléphet-e a játékos
        private bool CanStepToPos(Player player, System.Windows.Vector direction)
        {
            //Rectangle playerCurrentRect = new Rectangle(player.Position.X, player.Position.Y, (int)(PlayerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));
            //Rectangle playerNextRect = new Rectangle(player.Position.X + (int)direction.X, player.Position.Y + (int)direction.Y, (int)(PlayerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));

            Point playerCurrentIndexes = PlayerPixelToMatrixCoordinate(player.Position);
            Point playerNextIndexes = PlayerPixelToMatrixCoordinate(new Point(player.Position.X + (int)direction.X, player.Position.Y + (int)direction.Y));

            Bomb currentBomb = null;
            if (Elements[playerCurrentIndexes.X, playerCurrentIndexes.Y] is Bomb b)
            {
                currentBomb = b;
            }

            lock (_ElementsListLockObject)
            {
                Point playerCurrentCenterPixel = new Point((int)(player.Position.X + GameRectSize * PlayerWidthRate / 2), (int)(player.Position.Y + GameRectSize * PlayerHeightRate / 2));
                Point playerCurrentUpperRightCornerPixel = new Point((int)(playerCurrentCenterPixel.X + GameRectSize * PlayerWidthRate / 2), (int)(playerCurrentCenterPixel.Y - GameRectSize * PlayerHeightRate / 2 * 0.8));
                Point playerCurrentBottomRightCornerPixel = new Point((int)(playerCurrentCenterPixel.X + GameRectSize * PlayerWidthRate / 2), (int)(playerCurrentCenterPixel.Y + GameRectSize * PlayerHeightRate / 2 * 0.3));
                Point playerCurrentUpperLeftCornerPixel = new Point((int)(playerCurrentCenterPixel.X - GameRectSize * PlayerWidthRate / 2), (int)(playerCurrentCenterPixel.Y - GameRectSize * PlayerHeightRate / 2 * 0.8));
                Point playerCurrentBottomLeftCornerPixel = new Point((int)(playerCurrentCenterPixel.X - GameRectSize * PlayerWidthRate / 2), (int)(playerCurrentCenterPixel.Y + GameRectSize * PlayerHeightRate / 2 * 0.3));

                Point playerNextCenterPixel = new Point((int)(playerCurrentCenterPixel.X + direction.X), (int)(playerCurrentCenterPixel.Y + direction.Y));
                Point playerNextUpperRightCornerPixel = new Point((int)(playerCurrentUpperRightCornerPixel.X + direction.X), (int)(playerCurrentUpperRightCornerPixel.Y + direction.Y));
                Point playerNextBottomRightCornerPixel = new Point((int)(playerCurrentBottomRightCornerPixel.X + direction.X), (int)(playerCurrentBottomRightCornerPixel.Y + direction.Y));
                Point playerNextUpperLeftCornerPixel = new Point((int)(playerCurrentUpperLeftCornerPixel.X + direction.X), (int)(playerCurrentUpperLeftCornerPixel.Y + direction.Y));
                Point playerNextBottomLeftCornerPixel = new Point((int)(playerCurrentBottomLeftCornerPixel.X + direction.X), (int)(playerCurrentBottomLeftCornerPixel.Y + direction.Y));

                Point playerNextUpperRightCornerIndexes = PixelToMatrixCoordinate(playerNextUpperRightCornerPixel);
                Point playerNextBottomRightCornerIndexes = PixelToMatrixCoordinate(playerNextBottomRightCornerPixel);
                Point playerNextUpperLeftCornerIndexes = PixelToMatrixCoordinate(playerNextUpperLeftCornerPixel);
                Point playerNextBottomLeftCornerIndexes = PixelToMatrixCoordinate(playerNextBottomLeftCornerPixel);

                List<Point> cornerPoints = new List<Point>() {
                    playerNextUpperRightCornerIndexes,
                    playerNextBottomRightCornerIndexes,
                    playerNextUpperLeftCornerIndexes,
                    playerNextBottomLeftCornerIndexes
                };

                if (direction.X < 0)//Left
                {
                    if (cornerPoints.Any(point => point.X < 0))
                    {
                        return false;
                    }

                    if ((Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null &&
                        !(Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] is Bomb)) ||
                        (Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null &&
                        !(Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] is Bomb)) ||
                        Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null ||
                        Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null)
                    {
                        if (playerNextIndexes.X >= 0 && (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (playerNextIndexes.X >= 0 && Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else if (direction.X > 0)
                {
                    if (cornerPoints.Any(point => point.X >= PlayGroundSize[0] - 1))
                    {
                        return false;
                    }

                    if (Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null ||
                        Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null ||
                        (Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null && !(Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] is Bomb)) ||
                        (Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null && !(Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] is Bomb)))
                    {
                        if (playerNextIndexes.X < PlayGroundSize[0] && (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (playerNextIndexes.X < PlayGroundSize[0] && Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else if (direction.Y < 0)//Up
                {
                    if (cornerPoints.Any(point => point.Y < 0))
                    {
                        return false;
                    }

                    if (Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null ||
                    (Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null && !(Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] is Bomb)) ||
                    Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null ||
                    (Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null && !(Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] is Bomb)))
                    {
                        if (playerNextIndexes.Y >= 0 && (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (playerNextIndexes.Y >= 0 && Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else
                {
                    if (cornerPoints.Any(point => point.Y >= PlayGroundSize[1] - 1))
                    {
                        return false;
                    }

                    if ((Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null && !(Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] is Bomb) ||
                        Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null ||
                        (Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null && !(Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] is Bomb)) ||
                        Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null))
                    {
                        if (playerNextIndexes.Y < PlayGroundSize[0] && (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (playerNextIndexes.Y < PlayGroundSize[0] && Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                //if (Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null ||
                //    Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null ||
                //    Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null ||
                //    Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null)
                //{
                //    if ((Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                //    {
                //        return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                //    }
                //    else if (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                //    {
                //        return true;//Odaléphet a robbanásba, de belehal.
                //    }
                //    return false;
                //}
                //return true;
            }

            //lock (_ElementsListLockObject)
            //{
            //    for (int i = 0; i < Elements.GetLength(0); i++)
            //    {
            //        for (int j = 0; j < Elements.GetLength(1); j++)
            //        {
            //            if (Elements[i, j] != null)
            //            {
            //                Rectangle elementRect = new Rectangle(i * GameRectSize, j * GameRectSize, GameRectSize, GameRectSize);
            //                if (playerRect.IntersectsWith(elementRect))
            //                {
            //                    if (!(Elements[i, j] is Bomb && playerPrevRect.IntersectsWith(elementRect)))
            //                    {
            //                        return false;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    return true;
            //}
        }

        private Player GetKeyBindingForPlayer(PlayerAction playerAction)
        {
            Player pl = null;

            switch (playerAction)
            {
                case PlayerAction.up:
                case PlayerAction.down:
                case PlayerAction.left:
                case PlayerAction.right:
                case PlayerAction.bombudlr:
                case PlayerAction.actionudlr:
                    pl = Players.Where(p => p.KeyBinding == Model.KeyBinding.upDownLeftRight).FirstOrDefault();
                    break;
                case PlayerAction.W:
                case PlayerAction.S:
                case PlayerAction.A:
                case PlayerAction.D:
                case PlayerAction.bombwasd:
                case PlayerAction.actionwasd:
                    pl = Players.Where(p => p.KeyBinding == Model.KeyBinding.WSAD).FirstOrDefault();
                    break;
            }
            return pl;
        }

        public void Pause()
        {
            GamePaused = true;
            PausedWindow pausedWindow = new PausedWindow();
            if (pausedWindow.ShowDialog() == false)
            {
                //kilépés
                System.Windows.Application.Current.Shutdown();
            }
            else if (pausedWindow.ActionMainMenu)
            {
                //kilépés a menübe

            }
            else if (pausedWindow.ActionResume)
            {
                GamePaused = false;
                lock (_TimerLockObject)
                {
                    Monitor.Pulse(_TimerLockObject);
                }
            }
        }

        //A játékos mozgásának kezdete, a controller hívja meg
        public async Task StartMove(PlayerAction playerAction, Player ai = null)
        {
            Player pl = ai is null ? GetKeyBindingForPlayer(playerAction) : ai;

            if (pl != null)
            {
                switch (playerAction)
                {
                    case PlayerAction.up:
                    case PlayerAction.down:
                    case PlayerAction.W:
                    case PlayerAction.S:
                        if (pl != null && pl.MovingVertical == false)
                        {
                            pl.MovingVertical = true;
                            while (pl.MovingVertical)
                            {
                                Act(playerAction, pl);
                                await Task.Delay(1);
                            }
                        }
                        break;
                    case PlayerAction.left:
                    case PlayerAction.right:
                    case PlayerAction.A:
                    case PlayerAction.D:
                        if (pl != null && pl.MovingHorizontal == false)
                        {
                            pl.MovingHorizontal = true;
                            while (pl.MovingHorizontal)
                            {
                                Act(playerAction, pl);
                                await Task.Delay(1);
                            }
                        }
                        break;
                }
            }
        }

        //A játékos mozgásának vége, a controller hívja meg
        public async void StopMove(PlayerAction playerAction, Player ai = null)
        {
            Player pl = ai is null ? GetKeyBindingForPlayer(playerAction) : ai;

            switch (playerAction)
            {
                case PlayerAction.up:
                case PlayerAction.down:
                case PlayerAction.W:
                case PlayerAction.S:
                    if (pl != null)
                    {
                        pl.MovingVertical = false;
                    }
                    break;
                case PlayerAction.left:
                case PlayerAction.right:
                case PlayerAction.A:
                case PlayerAction.D:
                    if (pl != null)
                    {
                        pl.MovingHorizontal = false;
                    }
                    break;
                case PlayerAction.bombudlr:
                case PlayerAction.bombwasd:
                    if (pl != null)
                    {
                        pl.SetBombPressed = false;
                    }
                    break;
                case PlayerAction.actionudlr:
                case PlayerAction.actionwasd:
                    if (pl != null)
                    {
                        pl.ActionPressed = false;
                    }
                    break;
            }
        }

        //A játékos bombával kapcsolatos cselekvései
        public async Task StartAct(PlayerAction playerAction, Player ai = null)
        {
            Player pl = ai is null ? GetKeyBindingForPlayer(playerAction) : ai;
            //ToDo: ne reagáljon, ha nincs is iylen keybindinggal rendelkező játékos, vagy nullchech mindenhová
            switch (playerAction)
            {
                //Bomba lerakása
                case PlayerAction.bombudlr:
                case PlayerAction.bombwasd:
                    if (pl != null && !pl.SetBombPressed)
                    {
                        pl.SetBombPressed = true;//It is necessary, else a btn press is called more than once.
                        Act(PlayerAction.bombudlr, pl);
                        await Task.Delay(1);
                    }
                    break;

                //Action
                case PlayerAction.actionudlr:
                case PlayerAction.actionwasd:
                    if (pl != null && !pl.ActionPressed)
                    {
                        pl.ActionPressed = true;//It is necessary, else a btn press is called more than once.
                        Act(PlayerAction.actionudlr, pl);
                        await Task.Delay(1);
                    }
                    break;
            }
        }

        //A játékos mozgása
        public async void Act(PlayerAction playerAction, Player pl)
        {
            if (pl != null && !pl.Explode)
            {
                int posX = pl.Position.X;
                int posY = pl.Position.Y;

                switch (playerAction)
                {
                    case PlayerAction.up:
                    case PlayerAction.W:
                        if (posY - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(0, -1 * pl.Speed)))
                        {
                            lock (_PlayersListLockObject)
                            {
                                pl.Move(0, -pl.Speed);
                                pl.HeadDirection = PlayerDirection.up;
                            }
                        }
                        break;
                    case PlayerAction.down:
                    case PlayerAction.S:
                        if (posY + ((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize) + pl.Speed <= (PlayGroundSize[1] - 1) * GameRectSize && CanStepToPos(pl, new System.Windows.Vector(0, pl.Speed)))
                        {
                            lock (_PlayersListLockObject)
                            {
                                pl.Move(0, pl.Speed);
                                pl.HeadDirection = PlayerDirection.down;
                            }
                        }
                        break;
                    case PlayerAction.left:
                    case PlayerAction.A:
                        if (posX - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(-1 * pl.Speed, 0)))
                        {
                            lock (_PlayersListLockObject)
                            {
                                pl.Move(-pl.Speed, 0);
                                pl.HeadDirection = PlayerDirection.left;
                            }
                        }
                        break;
                    case PlayerAction.right:
                    case PlayerAction.D:
                        if (posX + (PlayerWidthRate * GameRectSize) + pl.Speed <= ((PlayGroundSize[0] - 1) * GameRectSize) && CanStepToPos(pl, new System.Windows.Vector(pl.Speed, 0)))
                        {
                            lock (_PlayersListLockObject)
                            {
                                pl.Move(pl.Speed, 0);
                                pl.HeadDirection = PlayerDirection.right;
                            }
                        }
                        break;

                    //Bomba lerakása
                    case PlayerAction.bombudlr:
                    case PlayerAction.bombwasd:
                        BombThreadStarter(pl);
                        break;

                    //Action
                    case PlayerAction.actionudlr:
                    case PlayerAction.actionwasd:
                        PlayerKickOrTriggerStarter(pl);
                        break;
                }
                EnvironmentInteractionsOnStep(pl);
            }
        }

        private void PlayerKickOrTriggerStarter(Player player)
        {//ToDo: bool állítás 
            new Task(() =>
            {
                Point playerCenter = new Point((int)(player.Position.X + (GameRectSize * PlayerWidthRate) / 2), (int)(player.Position.Y + (GameRectSize * PlayerHeightRate) / 2));
                Point playerIndexes = PlayerPixelToMatrixCoordinate(player.Position);
                switch (player.HeadDirection)
                {
                    case PlayerDirection.up:
                        if (playerIndexes.Y - 1 >= 0 && Elements[playerIndexes.X, playerIndexes.Y - 1] is Bomb b && playerCenter.Y - b.PositionPixel.Y < player.Position.Y * PlayerHeightRate * GameRectSize / 2 + GameRectSize / 2 + 10)
                        {
                            while (b.Explode != true && b.Position.Y - 1 >= 0 && Elements[b.Position.X, b.Position.Y - 1] == null)
                            {
                                b.IsMoving = true;
                                Thread.Sleep(1);
                                if (GamePaused)
                                {
                                    lock (_TimerLockObject)
                                    {
                                        Monitor.Wait(_TimerLockObject);
                                    }
                                }

                                b.PositionPixel = new Point(b.PositionPixel.X, (int)(b.PositionPixel.Y - (double)GameRectSize / 10.0));
                                Point oldPos = b.Position;
                                b.Position = ItemPixelToMatrixCoordinate(b.PositionPixel);
                                if (!oldPos.Equals(b.Position))
                                {
                                    lock (_ElementsListLockObject)
                                    {
                                        Elements[oldPos.X, oldPos.Y] = null;
                                        Elements[b.Position.X, b.Position.Y] = b;
                                    }
                                }
                            }

                            if (b != null)
                            {
                                b.IsMoving = false;
                            }
                        }
                        else
                        {
                            Thread.Sleep(300);
                            if (!player.SetBombPressed)
                            {
                                lock (player._triggerBombLockObject)
                                {
                                    Monitor.PulseAll(player._triggerBombLockObject);
                                }
                            }
                        }
                        break;
                    case PlayerDirection.down:
                        if (playerIndexes.Y + 1 < PlayGroundSize[1] - 1 && Elements[playerIndexes.X, playerIndexes.Y + 1] is Bomb bdown && playerCenter.Y + bdown.PositionPixel.Y < player.Position.Y * PlayerHeightRate * GameRectSize / 2 + GameRectSize / 2 + 10)
                        {
                            while (bdown.Explode != true && bdown.Position.Y + 1 < PlayGroundSize[1] - 1 && Elements[bdown.Position.X, bdown.Position.Y + 1] == null)
                            {
                                bdown.IsMoving = true;
                                Thread.Sleep(1);
                                if (GamePaused)
                                {
                                    lock (_TimerLockObject)
                                    {
                                        Monitor.Wait(_TimerLockObject);
                                    }
                                }

                                bdown.PositionPixel = new Point(bdown.PositionPixel.X, (int)(bdown.PositionPixel.Y + (double)GameRectSize / 10.0));
                                Point oldPos = bdown.Position;
                                bdown.Position = ItemPixelToMatrixCoordinate(bdown.PositionPixel);
                                if (!oldPos.Equals(bdown.Position))
                                {
                                    lock (_ElementsListLockObject)
                                    {
                                        Elements[oldPos.X, oldPos.Y] = null;
                                        Elements[bdown.Position.X, bdown.Position.Y] = bdown;
                                    }
                                }
                            }

                            if (bdown != null)
                            {
                                bdown.IsMoving = false;
                            }
                        }
                        else
                        {
                            if (!player.SetBombPressed)
                            {
                                lock (player._triggerBombLockObject)
                                {
                                    Monitor.PulseAll(player._triggerBombLockObject);
                                }
                            }
                        }
                        break;
                    case PlayerDirection.left:
                        if (playerIndexes.X - 1 >= 0 && Elements[playerIndexes.X - 1, playerIndexes.Y] is Bomb bleft && playerCenter.X - bleft.PositionPixel.X < player.Position.X * PlayerWidthRate * GameRectSize / 2 + GameRectSize / 2 + 10)
                        {
                            while (bleft.Explode != true && bleft.Position.X - 1 >= 0 && Elements[bleft.Position.X - 1, bleft.Position.Y] == null)
                            {
                                bleft.IsMoving = true;
                                Thread.Sleep(1);
                                if (GamePaused)
                                {
                                    lock (_TimerLockObject)
                                    {
                                        Monitor.Wait(_TimerLockObject);
                                    }
                                }

                                bleft.PositionPixel = new Point((int)(bleft.PositionPixel.X - (double)GameRectSize / 10.0), bleft.PositionPixel.Y);
                                Point oldPos = bleft.Position;
                                bleft.Position = ItemPixelToMatrixCoordinate(bleft.PositionPixel);
                                if (!oldPos.Equals(bleft.Position))
                                {
                                    lock (_ElementsListLockObject)
                                    {
                                        Elements[oldPos.X, oldPos.Y] = null;
                                        Elements[bleft.Position.X, bleft.Position.Y] = bleft;
                                    }
                                }
                            }

                            if (bleft != null)
                            {
                                bleft.IsMoving = false;
                            }
                        }
                        else
                        {
                            if (!player.SetBombPressed)
                            {
                                lock (player._triggerBombLockObject)
                                {
                                    Monitor.PulseAll(player._triggerBombLockObject);
                                }
                            }
                        }
                        break;
                    case PlayerDirection.right:
                        if (playerIndexes.X + 1 < PlayGroundSize[0] - 1 && Elements[playerIndexes.X + 1, playerIndexes.Y] is Bomb bright && playerCenter.X + bright.PositionPixel.X < player.Position.X * PlayerWidthRate * GameRectSize / 2 + GameRectSize / 2 + 10)
                        {
                            while (bright.Explode != true && bright.Position.X + 1 < PlayGroundSize[0] - 1 && Elements[bright.Position.X + 1, bright.Position.Y] == null)
                            {
                                bright.IsMoving = true;
                                Thread.Sleep(1);
                                if (GamePaused)
                                {
                                    lock (_TimerLockObject)
                                    {
                                        Monitor.Wait(_TimerLockObject);
                                    }
                                }

                                bright.PositionPixel = new Point((int)(bright.PositionPixel.X + (double)GameRectSize / 10.0), bright.PositionPixel.Y);
                                Point oldPos = bright.Position;
                                bright.Position = ItemPixelToMatrixCoordinate(bright.PositionPixel);//Korrigálni
                                if (!oldPos.Equals(bright.Position))
                                {
                                    lock (_ElementsListLockObject)
                                    {
                                        Elements[oldPos.X, oldPos.Y] = null;
                                        Elements[bright.Position.X, bright.Position.Y] = bright;
                                    }
                                }
                            }

                            if (bright != null)
                            {
                                bright.IsMoving = false;
                            }
                        }
                        else
                        {
                            if (!player.SetBombPressed)
                            {
                                lock (player._triggerBombLockObject)
                                {
                                    Monitor.PulseAll(player._triggerBombLockObject);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }, TaskCreationOptions.LongRunning).Start();
        }

        //private void TriggerScheduledBombs(Player player)
        //{
        //    lock (player._bombListLockObject)
        //    {
        //        if (player.BombList.Count != 0)
        //        {
        //            player.BombList.Where(bomb => bomb.ElementType == ElementType.ScheduledBomb).ToList().ForEach(bomb =>
        //            {
        //                bomb.Explode = true;

        //                player.BombList.Remove(bomb);

        //                int x = bomb.Position.X;
        //                int y = bomb.Position.Y;
        //                lock (_ElementsListLockObject)
        //                {
        //                    if (Elements[x, y] != null && (Elements[x, y] as Bomb).Equals(bomb))
        //                    {
        //                        Elements[x, y] = null;
        //                    }
        //                }
        //            });
        //        }
        //    }
        //}

        private void EnvironmentInteractionsOnStep(Player player)
        {
            Point playerIndexes = PlayerPixelToMatrixCoordinate(player.Position);
            //Rectangle playerRect = new Rectangle(player.Position.X, player.Position.Y, (int)(PlayerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));
            //Rectangle elementRect = new Rectangle(player.Position.X * GameRectSize, player.Position.Y * GameRectSize, GameRectSize, GameRectSize);

            int i = playerIndexes.X;
            int j = playerIndexes.Y;
            lock (_PowerupsListLockObject)
            {
                if (Powerups[i, j] != null)
                {
                    switch (Powerups[i, j].ElementType)
                    {
                        case ElementType.Kick:
                            player.CanKick = true;//ToDo: implement kick for alt?
                            break;
                        //case ElementType.Jelly:
                        //    break;
                        case ElementType.Desease:
                            player.HasDesease = true;
                            switch (rnd.Next(0, 3))
                            {
                                case 0:
                                    new Task(() =>
                                    {
                                        int originalBombAmount = player.BombAmount;
                                        player.BombAmount = 1;
                                        for (int i = 0; i < 10000; i++)
                                        {
                                            if (GamePaused)
                                            {
                                                lock (_TimerLockObject)
                                                {
                                                    Monitor.Wait(_TimerLockObject);
                                                }
                                            }
                                            Thread.Sleep(1);
                                        }
                                        player.HasDesease = false;
                                        player.BombAmount = originalBombAmount;
                                    }, TaskCreationOptions.LongRunning).Start();
                                    break;
                                case 1:
                                    new Task(() =>
                                    {
                                        int originalSpeed = player.Speed;
                                        player.Speed = (int)GameSize.X / 360;
                                        for (int i = 0; i < 10000; i++)
                                        {
                                            if (GamePaused)
                                            {
                                                lock (_TimerLockObject)
                                                {
                                                    Monitor.Wait(_TimerLockObject);
                                                }
                                            }
                                            Thread.Sleep(1);
                                        }
                                        player.HasDesease = false;
                                        player.Speed = originalSpeed;
                                    }, TaskCreationOptions.LongRunning).Start();
                                    break;
                                case 2:
                                    new Task(() =>
                                    {
                                        int originalBombExplosionRange = player.Bomb.ExplosionRange;
                                        player.Bomb.ExplosionRange = 1;
                                        for (int i = 0; i < 10000; i++)
                                        {
                                            if (GamePaused)
                                            {
                                                lock (_TimerLockObject)
                                                {
                                                    Monitor.Wait(_TimerLockObject);
                                                }
                                            }
                                            Thread.Sleep(1);
                                        }
                                        player.HasDesease = false;
                                        player.Bomb.ExplosionRange = originalBombExplosionRange;
                                    }, TaskCreationOptions.LongRunning).Start();
                                    break;
                                default://ToDo: Inverse control
                                    break;
                            }
                            break;
                        case ElementType.BomUp:
                            if (player.BombAmount < 6)
                            {
                                player.BombAmount++;
                            }
                            break;
                        case ElementType.BlastUp:
                            if (player.Bomb.ExplosionRange < 8)
                            {
                                player.Bomb.ExplosionRange++;
                            }
                            break;
                        case ElementType.SpeedUp:
                            if (player.Speed < (int)GameSize.X / 225)
                            {
                                player.Speed += (int)GameSize.X / 500;
                            }
                            break;
                        case ElementType.SpeedDown:
                            if (player.Speed > (int)GameSize.X / 300)
                            {
                                player.Speed -= (int)GameSize.X / 500;
                            }
                            break;
                        case ElementType.Schedule:
                            player.CanSchedule = true;
                            break;
                        default:
                            break;
                    }
                    Powerups[i, j] = null;
                }
            }

            lock (_ElementsListLockObject)
            {
                if (Elements[i, j] != null)
                {
                    switch (Elements[i, j].ElementType)
                    {
                        case ElementType.Bomb://Ha belesétálunk egy robbanásba, szintén meghalunk
                            lock (_PlayersListLockObject)
                            {
                                if (Elements[i, j].Explode)
                                {
                                    (Elements[i, j] as Bomb).Player.Kills++;
                                    Players.Remove(player);

                                    if (Players.Count == 0)
                                    {
                                        RoundOver = true;
                                    }
                                }
                            }
                            break;
                        //case ElementType.Player://Betegség átpasszolása :3 KÜlön listában.
                        //    break;
                        case ElementType.Teleport:
                            break;
                        case ElementType.TravelatorRight:
                            break;
                        case ElementType.TravelatorLeft:
                            break;
                        case ElementType.TravelatorUp:
                            break;
                        case ElementType.TravelatorDown:
                            break;
                        default:
                            break;
                    }
                }
            }
            //    }
            //}
            //if (Powerups[playerIndexes.X, playerIndexes.Y] != null)
            //{
            //    switch (Powerups[playerIndexes.X, playerIndexes.Y].ElementType)
            //    {
            //        case ElementType.Bomb://Ha belesétálunk egy robbanásba, szintén meghalunk
            //            if (Powerups[playerIndexes.X, playerIndexes.Y].Explode)
            //            {
            //                (Powerups[playerIndexes.X, playerIndexes.Y] as Bomb).Player.Kills++;
            //                Players.Remove(player);
            //if (Players.Count == 0)
            //{
            //    RoundOver = true;
            //}
            //            }
            //            break;
            //        case ElementType.Player://Betegség átpasszolása :3
            //            break;
            //        case ElementType.Kick:
            //            break;
            //        case ElementType.Jelly:
            //            break;
            //        case ElementType.Desease:
            //            break;
            //        case ElementType.BomUp:
            //            break;
            //        case ElementType.BlastUp:
            //            break;
            //        case ElementType.SpeedUp:
            //            break;
            //        case ElementType.SpeedDown:
            //            break;
            //        case ElementType.Schedule:
            //            break;
            //        case ElementType.Teleport:
            //            break;
            //        case ElementType.TravelatorRight:
            //            break;
            //        case ElementType.TravelatorLeft:
            //            break;
            //        case ElementType.TravelatorUp:
            //            break;
            //        case ElementType.TravelatorDown:
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //else if (Elements[playerIndexes.X, playerIndexes.Y] != null)
            //{
            //}
        }

        private void BombThreadStarter(Player pl)
        {
            if (pl.BombList.Count < pl.BombAmount)
            {
                //Itt beadható, ha scheduled, de még nem tudom, hogyan nézem meg, hogy le van e nyomva az action is közben
                Point newBombCoords = new Point(
                                    (int)Math.Floor((decimal)((pl.Position.X + (PlayerWidthRate * GameRectSize) / 2) / GameRectSize)),
                                    (int)Math.Floor((decimal)((pl.Position.Y + ((PlayerHeightRate + PlayerHeightRateHangsIn) * GameRectSize) / 2) / GameRectSize)));

                Bomb newBomb = pl.Bomb.BombCopy(
                                newBombCoords,
                                pl.ActionPressed ? ElementType.ScheduledBomb : ElementType.Bomb,
                                new Point((int)(newBombCoords.X * GameRectSize), (int)(newBombCoords.Y * GameRectSize)));
                lock (pl._bombListLockObject)
                {
                    pl.BombList.Add(newBomb);
                }

                Point xy = PlayerPixelToMatrixCoordinate(pl.Position);
                int i = xy.X;
                int j = xy.Y;


                lock (_ElementsListLockObject)
                {
                    Elements[i, j] = newBomb;
                }

                new Task(() =>
                {
                    if (newBomb.ElementType == ElementType.ScheduledBomb)
                    {
                        lock (pl._triggerBombLockObject)
                        {
                            Monitor.Wait(pl._triggerBombLockObject);
                        }
                    }
                    else
                    {
                        new Task(() =>
                        {
                            for (int i = 0; i < 3000; i++)//x másodperc múlva robban a bomba
                            {
                                lock (_TimerLockObject)
                                {
                                    if (GamePaused)
                                    {
                                        Monitor.Wait(_TimerLockObject);
                                    }
                                }
                                Thread.Sleep(1);
                            }

                            if (newBomb != null)
                            {
                                lock (newBomb._bombTriggerLock)
                                {
                                    Monitor.Pulse(newBomb._bombTriggerLock);
                                }
                            }
                        }, TaskCreationOptions.LongRunning).Start();

                        lock (newBomb._bombTriggerLock)
                        {
                            Monitor.Wait(newBomb._bombTriggerLock);
                        }
                    }

                    if (newBomb != null)
                    {
                        newBomb.Explode = true;

                        //ToDo: várjon, amíg megáll a bomba, vagy állítsa meg a rúgást.
                        //newBomb.Image.Freeze();
                        //ToDO: ha a karakter rajtamard, akkro is haljon meg. IDe.
                        int explsoionRange = pl.Bomb.ExplosionRange;

                        List<Task> explosionTasks = new List<Task>();

                        explosionTasks.Add(new Task(() =>
                        {
                            for (int row = newBomb.Position.X + 1; row <= newBomb.Position.X + explsoionRange; row++)
                            {
                                if (row < Elements.GetLength(0) - 1)
                                {
                                    ExplosionEffects(row, newBomb.Position.Y, newBomb, pl);
                                    if (BombStopper(row, newBomb.Position.Y))
                                    {
                                        break;
                                    }
                                }
                            }
                        }, TaskCreationOptions.LongRunning));
                        explosionTasks.Add(new Task(() =>
                        {
                            for (int row = newBomb.Position.X - 1; row >= newBomb.Position.X - explsoionRange; row--)
                            {
                                if (row >= 0)
                                {
                                    //ExplosionEffects(row, j, bomb, explosionImg, pl);
                                    ExplosionEffects(row, newBomb.Position.Y, newBomb, pl);
                                    if (BombStopper(row, newBomb.Position.Y))
                                    {
                                        break;
                                    }
                                }
                            }
                        }));

                        explosionTasks.Add(new Task(() =>
                        {
                            for (int col = newBomb.Position.Y + 1; col <= newBomb.Position.Y + explsoionRange; col++)
                            {
                                if (col < Elements.GetLength(1) - 1)
                                {
                                    //ExplosionEffects(i, col, bomb, explosionImg, pl);
                                    ExplosionEffects(newBomb.Position.X, col, newBomb, pl);
                                    if (BombStopper(newBomb.Position.X, col))
                                    {
                                        break;
                                    }
                                }
                            }
                        }, TaskCreationOptions.LongRunning));

                        explosionTasks.Add(new Task(() =>
                        {
                            for (int col = newBomb.Position.Y - 1; col >= newBomb.Position.Y - explsoionRange; col--)
                            {
                                if (col >= 0)
                                {
                                    //ExplosionEffects(i, col, bomb, explosionImg, pl);
                                    ExplosionEffects(newBomb.Position.X, col, newBomb, pl);

                                    if (BombStopper(newBomb.Position.X, col))
                                    {
                                        break;
                                    }
                                }
                            }
                        }, TaskCreationOptions.LongRunning));

                        Parallel.ForEach(explosionTasks, t => t.Start());

                        Trigger(pl, newBomb, newBomb.Position.X, newBomb.Position.Y, newBomb.ElementType == ElementType.ScheduledBomb ? 0 : 1500);
                    }
                }, TaskCreationOptions.LongRunning).Start();
            }
        }

        private void Trigger(Player pl, Bomb newBomb, int i, int j, int millisec)
        {
            Thread.Sleep(millisec);

            lock (pl._bombListLockObject)
            {
                if (pl.BombList.Contains(newBomb))
                {
                    pl.BombList.Remove(newBomb);
                }
            }

            lock (_ElementsListLockObject)
            {
                if (Elements[i, j] != null && (Elements[i, j] as Bomb).Equals(newBomb))//ToDo: elég az i,j-t kiszedni a bombából
                {
                    Elements[i, j] = null;
                }
            }
        }

        private void ExplosionEffects(int row, int col, Bomb bomb, Player pl)
        {
            if (!(Elements[row, col] is FixWall))
            {
                new Task(() =>
                {
                    List<Player> playersToKill = new List<Player>();//Players mert egy mezőn több player is lehet

                    if (Elements[row, col] is null)
                    {
                        //Mivel az elementsben nincsenek bene a playerek.
                        playersToKill = Players.Where(player =>
                                    (int)Math.Floor((decimal)(player.Position.X / GameRectSize)) == row &&
                                    (int)Math.Floor((decimal)(player.Position.Y / GameRectSize)) == col)
                                    .ToList();
                        playersToKill.ForEach(player => player.Explode = true);
                        if (playersToKill.Count == 0 && bomb != null)
                        {
                            bomb.Explode = true;
                            Elements[row, col] = bomb;
                        }
                    }
                    else if (Elements[row, col] is Bomb t)
                    {
                        //Mivel másik bombát is ért a robbanás ezért az is felrobban.
                        //ToDo: ez így nem jó, triggerelni kell a robbanását, nem újrakezdeni. Ami miatt kétszer akarna felrobbanni.
                        lock (t._bombTriggerLock)
                        {
                            Monitor.Pulse(t._bombTriggerLock);
                        }
                    }
                    else if (Elements[row, col] is Wall w)//ToDo: || Elements[row, col] is Skill/Booster   ami lesz a neve
                    {
                        w.Explode = true;
                    }

                    for (int i = 0; i < 1400; i++)
                    {
                        lock (_TimerLockObject)
                        {
                            if (GamePaused)
                            {
                                Monitor.Wait(_TimerLockObject);
                            }
                        }
                        Thread.Sleep(1);
                    }

                    lock (_ElementsListLockObject)
                    {
                        Elements[row, col] = null;//Ide jön a logika, hogy melyi skill töltsön be. %-os esélyeket találunk ki.

                    }

                    if (playersToKill.Count > 0)
                    {
                        lock (_PlayersListLockObject)
                        {
                            playersToKill.ForEach(player =>
                                {
                                    if (player != null)
                                    {
                                        Players.Remove(player);
                                        pl.Kills++;

                                        if (Players.Count == 0)
                                        {
                                            RoundOver = true;
                                        }
                                    }
                                }
                            );
                        }
                    }

                }, TaskCreationOptions.LongRunning).Start();
            }
        }

        private bool BombStopper(int row, int col)
        {
            return Elements[row, col] != null ||
                   Players.Any(player =>
                       (int)Math.Floor((decimal)(player.Position.X / GameRectSize)) == row &&
                       (int)Math.Floor((decimal)(player.Position.Y / GameRectSize)) == col);
        }
        private void AITaskCreator()
        {
            List<Player> ais = new List<Player>();
            ais.AddRange(Players.Where(player => player.KeyBinding == Model.KeyBinding.ai));

            List<Task> aiTasks = ais.Select(ai => new Task(() => AIWakeUp(ai), TaskCreationOptions.LongRunning)).ToList();
            //ToDo: kirakni a tasklistet;
            Parallel.ForEach(aiTasks, task => task.Start());//Hogy amennyire csak lehet egyszerre induljanak
        }

        private async void AIWakeUp(Player ai)
        {
            Thread.Sleep(1000);//1 másodperc előny a valódi játékosoknak
            while (!RoundOver)//ToDo: Majd amgí nem igaz, hogy vége
            {
                List<IElement> bombs = CollectBombs();
                Node[,] lvlMatrix = ReconstructToNodes();
                int[] targetElementIndex = FindNearestDestructible(ai.Position);

                Node target = lvlMatrix[targetElementIndex[0], targetElementIndex[1]];
                Point aiPosition = PlayerPixelToMatrixCoordinate(ai.Position);
                int aiPosX = aiPosition.X;
                int aiPosY = aiPosition.Y;

                List<Point> area = GetTargetArea(target);
                List<Point> path = FindPathToDestructible(FindNearestDestructible(ai.Position), ai.Position, lvlMatrix, area);
                foreach (var bomb in bombs)
                {
                    if (bomb != null)
                    {
                        if (GetBombArea(bomb).Contains(aiPosition) || bomb.Position.X == aiPosition.X && bomb.Position.Y == aiPosition.Y)
                        {
                            IElement currentBomb = bomb;
                            while (Elements[currentBomb.Position.X, currentBomb.Position.Y] is Bomb && aiPosY == bomb.Position.Y)
                            {
                                Hide(bomb, ai);
                            }
                        }
                    }
                }
                if (path != null)
                {
                    foreach (var pt in path)
                    {
                        if (pt.X > aiPosX && Elements[aiPosX + 1, aiPosY] == null)
                        {
                            StartMove(PlayerAction.right, ai);
                            StopMove(PlayerAction.right, ai);
                            Thread.Sleep(20);
                            break;

                        }
                        else if (pt.X < aiPosX)
                        {
                            StartMove(PlayerAction.left, ai);
                            StopMove(PlayerAction.left, ai);
                            Thread.Sleep(20);
                            break;
                        }
                        else if (pt.Y > aiPosY)
                        {
                            StartMove(PlayerAction.down, ai);
                            StopMove(PlayerAction.down, ai);
                            Thread.Sleep(20);
                            break;
                        }
                        else if (pt.Y < aiPosY)
                        {
                            StartMove(PlayerAction.up, ai);
                            StopMove(PlayerAction.up, ai);
                            Thread.Sleep(20);
                            break;
                        }
                        else if (area.Contains(pt))
                        {
                            StartAct(PlayerAction.bombudlr, ai);
                            break;
                        }
                    }
                }

                //List<IElement> pathToDestructible = FindPathToDestructible( closestElementIndex,ai.Position);
                //IElement[,] elements = new IElement[GameSize.X - 1, GameSize.Y - 1];
                /*lock (_ElementsListLockObject)
                {
                    Elements.ForEach(element => elements[element.Position.X, element.Position.Y] = element);
                }*/
                //ToDo: amíg az időből nem telt el 30 perc, addig keressen fixen skilleket. Az legyen a prioritása.
                Player nearestPlayer = NearestPlayer(ai);
                //ToDO: ha bomba van a közelében bújjon el.
                //Az Elements lista = NotAvailablePoints;






                //if (nearestPlayer != null && PositionDifference(nearestPlayer, ai) <= 20)//ToDo: ai.Bomb.explosionSize vagy ami lesz
                {
                    //ToDo: Go and Install bomb
                    //ToDo: hide and wait until Explosion + x seconds
                    // }
                    //else
                    //{
                    //ToDo: follow player
                    //ToDO: AI javítása nullcheckekkel, ha egyedül lenne mit csináljon.
                    //if (nearestPlayer != null && nearestPlayer.Position.X - ai.Position.X < 0 && CanStepToPos(ai, new System.Windows.Vector(-1 * ai.Speed, 0)))
                    //{
                    //    StartMove(PlayerAction.left, ai);
                    //    StopMove(PlayerAction.left, ai);
                    //}

                    //if (!CanStepToPos(ai, new System.Windows.Vector(-1 * ai.Speed, 0)))
                    //{
                    // int aiNextPosX = ai.Position.X + (-1 * ai.Speed);//Kiszervezni az egészet külön metódusba.
                    //int aiNextPosY = ai.Position.Y;
                    //IElement blockingElement = elements[aiNextPosX, aiNextPosY];

                    //List<Point> availablePath = FindAvailablePath(ai, aiNextPosX, aiNextPosY);
                    //List<PlayerAction> availableRoundaboutActions = FindAvailableRoundaboutActions(ai, aiNextPosX, aiNextPosY);
                    //availableRoundaboutActions.ForEach(Action => Action.Pop);
                    //ToDo: jó irány keresése a megkerüléshez, majd móaction-ök meghívása sorban.
                    //;
                    //}

                    //if (nearestPlayer.Position.Y - ai.Position.Y < 0 && CanStepToPos(ai, new System.Windows.Vector(0, -1 * ai.Speed)))
                    //{
                    //    StartMove(PlayerAction.up, ai);
                    //    StopMove(PlayerAction.up, ai);
                    //}

                    //if (nearestPlayer.Position.X - ai.Position.X > 0 && CanStepToPos(ai, new System.Windows.Vector(ai.Speed, 0)))
                    //{
                    //    StartMove(PlayerAction.right, ai);
                    //    StopMove(PlayerAction.right, ai);
                    //}

                    //if (nearestPlayer.Position.Y - ai.Position.Y > 0 && CanStepToPos(ai, new System.Windows.Vector(0, ai.Speed)))
                    //{
                    //    StartMove(PlayerAction.down, ai);
                    //    StopMove(PlayerAction.down, ai);
                    //}
                }
            }
        }

        private Player NearestPlayer(Player ai)
        {
            return Players.Where(player => player.Id != ai.Id)
                          .OrderBy(player => PositionDifference(player, ai))
                          .FirstOrDefault();
        }

        private double PositionDifference(Player player, Player ai)
        {
            return Math.Abs(player.Position.X - ai.Position.X) +
                   Math.Abs(player.Position.Y - ai.Position.Y);
        }


        private List<IElement> CollectBombs()
        {

            List<IElement> bombPlaces = new List<IElement>();
            foreach (var element in Elements)
            {
                if (element is Bomb)
                {
                    bombPlaces.Add(element);
                }
            }
            return bombPlaces;

        }

        private void Hide(IElement bomb, Player ai)
        {
            Point aiPosition = PlayerPixelToMatrixCoordinate(ai.Position);
            int aiPosX = aiPosition.X;
            int aiPosY = aiPosition.Y;

            if (aiPosX > 0 && (Elements[aiPosX - 1, aiPosY] == null || (Elements[aiPosX - 1, aiPosY] is Bomb b && Elements[aiPosX, aiPosY] is Bomb borigi && b.Equals(borigi))))
            {
                StartMove(PlayerAction.left, ai);
                //if (Math.Abs((int)Math.Floor((decimal)(ai.Position.X / GameRectSize)) - bomb.Position.X) > 5)
                //{
                //    StopMove(PlayerAction.left, ai);
                //    Thread.Sleep(4500);
                //}
            }
            else if (aiPosY < Elements.GetLength(0) && (Elements[aiPosY + 1, aiPosX] == null || (Elements[aiPosY + 1, aiPosX] is Bomb bundernext && Elements[aiPosX, aiPosY] is Bomb bunder && bundernext.Equals(bunder))))
            {
                StartMove(PlayerAction.right, ai);
            }
            else if ((aiPosX - bomb.Position.X) < 5)
            {
                if (aiPosY > 0 && (Elements[aiPosX, aiPosY - 1] == null || (Elements[aiPosX, aiPosY - 1] is Bomb bupundernext && Elements[aiPosX, aiPosY] is Bomb bupunder && bupundernext.Equals(bupunder))))
                {
                    StartMove(PlayerAction.up, ai);
                }
                else if (aiPosY < Elements.GetLength(1) && (Elements[aiPosX, aiPosY + 1] == null || (Elements[aiPosX, aiPosY + 1] is Bomb bdownundernext && Elements[aiPosX, aiPosY] is Bomb bdownunder && bdownundernext.Equals(bdownunder))))
                {
                    StartMove(PlayerAction.down, ai);
                }
            }

        }

        //Optimalizáció még erősen szükséges
        Node[,] ReconstructToNodes()
        {
            Node[,] lvlMatrix = new Node[PlayGroundSize[0], PlayGroundSize[1]];
            for (int i = 0; i < Elements.GetLength(0); i++)
            {
                for (int j = 0; j < Elements.GetLength(1); j++)
                {
                    if (Elements[i, j] == null)
                    {
                        lvlMatrix[i, j] = new Node("floor", true, new Point(i, j));
                    }
                    else if (Elements[i, j] is Wall)
                    {
                        lvlMatrix[i, j] = new Node("wall", false, Elements[i, j].Position);
                    }
                    else if (Elements[i, j] is FixWall)
                    {
                        lvlMatrix[i, j] = new Node("fixwall", false, Elements[i, j].Position);
                    }
                    else if (Elements[i, j] is Bomb)
                    {
                        lvlMatrix[i, j] = new Node("bomb", false, Elements[i, j].Position);
                    }
                }
            }
            return lvlMatrix;
        }

        private List<Point> FindPathToDestructible(int[] targetElementIndex, Point startingPosition, Node[,] lvlMatrix, List<Point> area)
        {
            Point aiPosition = PlayerPixelToMatrixCoordinate(startingPosition);
            int aiCurrentIndexX = aiPosition.X;
            int aiCurrentIndexY = aiPosition.Y;

            Node target = lvlMatrix[targetElementIndex[0], targetElementIndex[1]];
            List<Point> targetArea = area; // TODO kigyűjteni úgy a környező pontokat, hogy hasonlítani lehessen
            List<Node> openSet = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(lvlMatrix[aiCurrentIndexX, aiCurrentIndexY]);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                foreach (var x in targetArea)
                {
                    if (x.X == currentNode.Position.X && x.Y == currentNode.Position.Y && lvlMatrix[x.X, x.Y].Walkable)
                    {
                        if (aiCurrentIndexX == currentNode.Position.X && aiCurrentIndexY == currentNode.Position.Y)
                        {
                            List<Point> pathself = new List<Point>();
                            pathself.Add(new Point(aiCurrentIndexX, aiCurrentIndexY));
                            return pathself;
                        }
                        List<Point> path = PathRetrace(lvlMatrix[aiCurrentIndexX, aiCurrentIndexY], currentNode);
                        return path;
                    }
                }
                foreach (Node neighbor in GetNeighbors(currentNode, lvlMatrix))
                {
                    if (!neighbor.Walkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode.Position, neighbor.Position);
                    if (newMovementCostToNeighbour < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCostToNeighbour;
                        neighbor.hCost = GetDistance(neighbor.Position, target.Position);
                        neighbor.Parent = currentNode;
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }

        public List<Point> GetTargetArea(Node target)
        {
            List<Point> targetArea = new List<Point>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 & y == 0)
                    {
                        continue;
                    }

                    if (x != y && x != -1 * y && (target.Position.X + x) > -1 && (target.Position.Y + y) > -1)
                    {
                        int thisX = target.Position.X + x;
                        int thisY = target.Position.Y + y;
                        if (Elements[thisX, thisY] == null)
                        {
                            targetArea.Add(new Point(thisX, thisY));
                        }

                    }

                }
            }
            return targetArea;
        }
        public List<Point> GetBombArea(IElement bomb)
        {
            List<Point> targetArea = new List<Point>();
            if (bomb != null)
            {
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 & y == 0)
                        {
                            continue;
                        }

                        if (x != y && x != -1 * y && (bomb.Position.X + x) > -1 && (bomb.Position.Y + y) > -1)
                        {
                            int thisX = bomb.Position.X + x;
                            int thisY = bomb.Position.Y + y;
                            if (Elements[thisX, thisY] == null)
                            {
                                targetArea.Add(new Point(thisX, thisY));
                            }

                        }

                    }
                }
            }

            return targetArea;
        }
        public List<Node> GetNeighbors(Node node, Node[,] lvlMatrix)
        {
            List<Node> neighbors = new List<Node>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    if (x != y && x != -1 * y)
                    {
                        int checkX = node.Position.X + x;
                        int checkY = node.Position.Y + y;
                        if (checkX >= 0 && checkX < PlayGroundSize[0] && checkY >= 0 && checkY < PlayGroundSize[1])
                        {
                            neighbors.Add(lvlMatrix[checkX, checkY]);
                        }
                    }

                }
            }
            return neighbors;
        }

        List<Point> PathRetrace(Node startNode, Node targetNode)
        {
            List<Point> path = new List<Point>();
            Node currentElement = targetNode;
            while (currentElement != startNode)
            {
                path.Add(currentElement.Position);
                currentElement = currentElement.Parent;


            }
            path.Reverse();
            return path;

        }

        int GetDistance(Point pointA, Point pointB)
        {
            int distX = Math.Abs(pointA.X - pointB.X);
            int distY = Math.Abs(pointA.Y - pointB.Y);
            if (distX > distY)
            {
                return 14 + distY + 10 * (distX - distY);
                //return 14 + distX + 10 * (distY - distX);

            }
            return 1;
        }

        public bool ValidPath(int row, int col)
        {
            if (row < 0 || col < 0 || row >= PlayGroundSize[0] || col >= PlayGroundSize[1]) //Ha kint van a pályán nem valid
            {
                return false;
            }
            return true;
        }


        private int[] FindNearestDestructible(Point aiPosition)
        {
            Point aiPositionIndexes = PlayerPixelToMatrixCoordinate(aiPosition);
            int aiCurrentIndexX = aiPositionIndexes.X;
            int aiCurrentIndexY = aiPositionIndexes.Y;
            bool[,] vis = new bool[PlayGroundSize[0], PlayGroundSize[1]]; //Bool tömb, azt nézi, hogy mely elemek vannak feldolgozva
            int[] dRow = new int[] { -1, 0, 1, 0 }; // Csak arra kell, hogy végig tudjon iterálni a szomszédokon
            int[] dCol = new int[] { 0, 1, 0, -1 };
            Queue<int[]> indexQueue = new Queue<int[]>(); //A queue amibe kigyűjti az elemeket
            indexQueue.Enqueue(new int[] { aiCurrentIndexX, aiCurrentIndexY });
            vis[aiCurrentIndexX, aiCurrentIndexY] = true;
            while (indexQueue.Count != 0)
            {
                int[] cell = indexQueue.Peek(); //Megnézi a queue tetején lévő elemet
                int x = cell[0];
                int y = cell[1];
                if (Elements[x, y] is Wall)
                {
                    return cell;
                }
                else
                {
                    indexQueue.Dequeue(); //Ha nem fal dequeueoljuk
                }
                if (!(Elements[x, y] is FixWall))
                {
                    for (int i = 0; i < 4; i++) //Végig iterálunk a négy környező elemen
                    {
                        int adjx = x + dRow[i];
                        int adjy = y + dCol[i];
                        if (isValid(vis, adjx, adjy))
                        {
                            indexQueue.Enqueue(new int[] { adjx, adjy });
                            vis[adjx, adjy] = true;
                        }
                    }
                }

            }
            return new int[] { -1, -1 };
        }
        private bool isValid(bool[,] vis, int row, int col)
        {
            if (row < 0 || col < 0 || row >= PlayGroundSize[0] || col >= PlayGroundSize[1]) //Ha kint van a pályán nem valid
            {
                return false;
            }
            if (vis[row, col]) //Ha már látogattuk, nem valid
            {
                return false;
            }
            return true;
        }

    }
}
