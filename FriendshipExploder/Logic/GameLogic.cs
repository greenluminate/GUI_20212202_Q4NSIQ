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
                                x.Move(corrigSize, corrigSize);
                            }
                            else
                            {
                                int cellX = x.Position.X / GameRectSize;
                                int cellY = x.Position.Y / GameRectSize;
                                x.SetPos(cellX * gameRectSize + (gameRectSize / 2), cellY * gameRectSize + (gameRectSize / 2));
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
                                        Powerups[i, j] = new Powerup(ElementType.BomUp);
                                        break;
                                    case < 40:
                                        Powerups[i, j] = new Powerup(ElementType.BlastUp);
                                        break;
                                    case < 60:
                                        Powerups[i, j] = new Powerup(ElementType.SpeedUp);
                                        break;
                                    case < 70:
                                        Powerups[i, j] = new Powerup(ElementType.Kick);
                                        break;
                                    case < 80:
                                        Powerups[i, j] = new Powerup(ElementType.Schedule);
                                        break;
                                    case < 88:
                                        Powerups[i, j] = new Powerup(ElementType.Desease);
                                        break;
                                    case < 95:
                                        Powerups[i, j] = new Powerup(ElementType.SpeedDown);
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

                while (!this.Timer.Equals("00:00"))
                {
                    Thread.Sleep(50);
                    if (!GamePaused)
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
            //Task contonuation Elkezdenek a fix falak betölteni.
            countDownTask.Start();
        }

        public enum PlayerAction
        {
            up, down, left, right,
            W, A, S, D,
            bombudlr, bombwasd,
            actionudlr, actionwasd
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

                if (direction.X < 0)//Left
                {
                    if ((Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null && !(Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] is Bomb)) ||
                    (Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null && !(Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] is Bomb)) ||
                    Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null ||
                    Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null)
                    {
                        if ((Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else if (direction.X > 0)
                {
                    if (Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null ||
                        Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null ||
                        (Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null && !(Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] is Bomb)) ||
                        (Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null && !(Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] is Bomb)))
                    {
                        if ((Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else if (direction.Y < 0)//Up
                {
                    if (Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null ||
                    (Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null && !(Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] is Bomb)) ||
                    Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null ||
                    (Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null && !(Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] is Bomb)))
                    {
                        if ((Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
                        {
                            return true;//Odaléphet a robbanásba, de belehal.
                        }
                        return false;
                    }
                    return true;
                }
                else
                {
                    if ((Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] != null && !(Elements[playerNextUpperRightCornerIndexes.X, playerNextUpperRightCornerIndexes.Y] is Bomb) ||
                        Elements[playerNextBottomRightCornerIndexes.X, playerNextBottomRightCornerIndexes.Y] != null ||
                        (Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] != null && !(Elements[playerNextUpperLeftCornerIndexes.X, playerNextUpperLeftCornerIndexes.Y] is Bomb)) ||
                        Elements[playerNextBottomLeftCornerIndexes.X, playerNextBottomLeftCornerIndexes.Y] != null))
                    {
                        if ((Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb nextBomb) && currentBomb != null && nextBomb.Equals(currentBomb))
                        {
                            return true;//Lejöhet arról a bombáról, ami alá került lerakásra.
                        }
                        else if (Elements[playerNextIndexes.X, playerNextIndexes.Y] is Bomb c && c.Explode)
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
                    Act(PlayerAction.bombudlr, pl);
                    await Task.Delay(1);
                    break;

                //Action
                case PlayerAction.actionudlr:
                case PlayerAction.actionwasd:
                    Act(PlayerAction.bombudlr, pl);
                    await Task.Delay(1);
                    break;
            }
        }

        //A játékos mozgása
        public async void Act(PlayerAction playerAction, Player pl)
        {
            if (pl != null)
            {
                int posX = pl.Position.X;
                int posY = pl.Position.Y;

                switch (playerAction)
                {
                    case PlayerAction.up:
                    case PlayerAction.W:
                        if (posY - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(0, -1 * pl.Speed)))
                        {
                            pl.Move(0, -pl.Speed);
                            pl.HeadDirection = PlayerDirection.up;
                        }
                        break;
                    case PlayerAction.down:
                    case PlayerAction.S:
                        if (posY + ((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize) + pl.Speed <= (PlayGroundSize[1] - 1) * GameRectSize && CanStepToPos(pl, new System.Windows.Vector(0, pl.Speed)))
                        {
                            pl.Move(0, pl.Speed);
                            pl.HeadDirection = PlayerDirection.down;
                        }
                        break;
                    case PlayerAction.left:
                    case PlayerAction.A:
                        if (posX - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(-1 * pl.Speed, 0)))
                        {
                            pl.Move(-pl.Speed, 0);
                            pl.HeadDirection = PlayerDirection.left;
                        }
                        break;
                    case PlayerAction.right:
                    case PlayerAction.D:
                        if (posX + (PlayerWidthRate * GameRectSize) + pl.Speed <= ((PlayGroundSize[0] - 1) * GameRectSize) && CanStepToPos(pl, new System.Windows.Vector(pl.Speed, 0)))
                        {
                            pl.Move(pl.Speed, 0);
                            pl.HeadDirection = PlayerDirection.right;
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
                        BombKickStarter(pl);
                        break;
                }
                EnvironmentInteractionsOnStep(pl);
            }
        }

        private void BombKickStarter(Player player)
        {
            new Task(() =>
            {
                Point playerCenter = new Point((int)(player.Position.X + (GameRectSize * PlayerWidthRate) / 2), (int)(player.Position.Y + (GameRectSize * PlayerHeightRate) / 2));
                Point playerIndexes = PlayerPixelToMatrixCoordinate(player.Position);
                switch (player.HeadDirection)
                {
                    case PlayerDirection.up:
                        if (Elements[playerIndexes.X, playerIndexes.Y - 1] is Bomb b && playerCenter.Y - b.PositionPixel.Y < player.Position.Y * PlayerHeightRateHangsIn - 2)
                        {
                            while (Elements[b.Position.X, b.Position.Y - 1] == null)
                            {
                                Thread.Sleep(1);
                                lock (_TimerLockObject)
                                {
                                    Monitor.Wait(_TimerLockObject);
                                }
                                b.PositionPixel = new Point(b.PositionPixel.X, b.PositionPixel.Y - (int)GameRectSize / 8);
                                b.Position = PlayerPixelToMatrixCoordinate(b.PositionPixel);
                            }
                        }
                        break;
                    case PlayerDirection.down:
                        break;
                    case PlayerDirection.left:
                        break;
                    case PlayerDirection.right:
                        break;
                    default:
                        break;
                }
            }, TaskCreationOptions.LongRunning);
        }

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
                                        for (int i = 0; i < 3000; i++)
                                        {
                                            if (!GamePaused)
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
                                    }, TaskCreationOptions.LongRunning);
                                    break;
                                case 1:
                                    new Task(() =>
                                    {
                                        int originalSpeed = player.Speed;
                                        player.Speed = (int)GameSize.X / 360;
                                        for (int i = 0; i < 3000; i++)
                                        {
                                            if (!GamePaused)
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
                                    }, TaskCreationOptions.LongRunning);
                                    break;
                                case 2:
                                    new Task(() =>
                                    {
                                        int originalBombExplosionRange = player.Bomb.ExplosionRange;
                                        player.Bomb.ExplosionRange = 1;
                                        for (int i = 0; i < 3000; i++)
                                        {
                                            if (!GamePaused)
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
                                    }, TaskCreationOptions.LongRunning);
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
                            if (player.Speed < (int)GameSize.X / 200)
                            {
                                player.Speed += (int)GameSize.X / 20;
                            }
                            break;
                        case ElementType.SpeedDown:
                            if (player.Speed > (int)GameSize.X / 300)
                            {
                                player.Speed -= (int)GameSize.X / 20;
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
                Bomb newBomb = pl.Bomb.BombCopy(
                                new Point(
                                    (int)Math.Floor((decimal)((pl.Position.X + (PlayerWidthRate * GameRectSize) / 2) / GameRectSize)),
                                    (int)Math.Floor((decimal)((pl.Position.Y + ((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize) / 2) / GameRectSize))),
                                BombType.Normal,
                                new Point((int)(pl.Position.X + (GameRectSize * PlayerWidthRate) / 2), (int)(pl.Position.Y + (GameRectSize * (PlayerHeightRate - PlayerHeightRateHangsIn)) / 2)));
                lock (pl._bombListLockObject)
                {
                    pl.BombList.Add(newBomb);
                }

                int i = (int)Math.Floor((decimal)(pl.Position.X / GameRectSize));
                int j = (int)Math.Floor((decimal)(pl.Position.Y / GameRectSize));


                lock (_ElementsListLockObject)
                {
                    Elements[i, j] = newBomb;
                }

                new Task(async () =>
                {
                    Thread.Sleep(3000);//x másodperc múlva robban a bomba
                    Bomb bomb = null;

                    lock (_ElementsListLockObject)
                    {
                        bomb = (Bomb)Elements[i, j];
                    }

                    if (bomb != null)
                    {
                        bomb.Explode = true;
                    }

                    //newBomb.Image.Freeze();
                    //ToDO: ha a karakter rajtamard, akkro is haljon meg. IDe.
                    int explsoionRange = pl.Bomb.ExplosionRange;

                    List<Task> explosionTasks = new List<Task>();

                    explosionTasks.Add(new Task(() =>
                    {
                        for (int row = i + 1; row <= i + explsoionRange; row++)
                        {
                            if (row < Elements.GetLength(0) - 1)
                            {
                                ExplosionEffects(row, j, bomb, pl);
                                //ExplosionEffects(row, j, bomb, explosionImg, pl);
                                if (BombStopper(row, j))
                                {
                                    break;
                                }
                            }
                        }
                    }));
                    explosionTasks.Add(new Task(() =>
                    {
                        for (int row = i - 1; row >= i - explsoionRange; row--)
                        {
                            if (row >= 0)
                            {
                                //ExplosionEffects(row, j, bomb, explosionImg, pl);
                                ExplosionEffects(row, j, bomb, pl);
                                if (BombStopper(row, j))
                                {
                                    break;
                                }
                            }
                        }
                    }));

                    explosionTasks.Add(new Task(() =>
                    {
                        for (int col = j + 1; col <= j + explsoionRange; col++)
                        {
                            if (col < Elements.GetLength(1) - 1)
                            {
                                //ExplosionEffects(i, col, bomb, explosionImg, pl);
                                ExplosionEffects(i, col, bomb, pl);
                                if (BombStopper(i, col))
                                {
                                    break;
                                }
                            }
                        }
                    }));

                    explosionTasks.Add(new Task(() =>
                    {
                        for (int col = j - 1; col >= j - explsoionRange; col--)
                        {
                            if (col >= 0)
                            {
                                //ExplosionEffects(i, col, bomb, explosionImg, pl);
                                ExplosionEffects(i, col, bomb, pl);

                                if (BombStopper(i, col))
                                {
                                    break;
                                }
                            }
                        }
                    }));

                    Parallel.ForEach(explosionTasks, t => t.Start());
                    await Task.Delay(1);//Ez nem megoldás.
                    Trigger(pl, newBomb, i, j, 1500);

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
                if (Elements[i, j] != null && (Elements[i, j] as Bomb).Equals(newBomb))
                {
                    Elements[i, j] = null;
                }
            }
        }

        private void ExplosionEffects(int row, int col, Bomb bomb, Player pl)//ImageBrush image,
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

                        if (playersToKill.Count == 0 && bomb != null)
                        {
                            bomb.Explode = true;
                            Elements[row, col] = bomb;
                        }

                        playersToKill.ForEach(player =>
                            {
                                int x = (int)Math.Floor((decimal)(player.Position.X / GameRectSize));
                                int y = (int)Math.Floor((decimal)(player.Position.Y / GameRectSize));
                                //player.Image = Halál image helye;
                                //player.Image.Freeze();
                                //ToDo: vagy a playerbe tárolnia ssaját képének elérését, vagy máshogy megoldani.
                                //ToDo: Player image cserélhetősége a haláluk miatt is fontos.
                            });
                    }
                    else if (Elements[row, col] is Bomb t)
                    {
                        //Mivel másik bombát is ért a robbanás ezért az is felrobban.
                        //ToDo: ez így nem jó, triggerelni kell a robbanását, nem újrakezdeni. Ami miatt kétszer akarna felrobbanni.
                        Trigger(t.Player, t, row, col, 1);
                    }
                    else if (Elements[row, col] is Wall)//ToDo: || Elements[row, col] is Skill/Booster   ami lesz a neve
                    {
                        //Elements[row, col].Image = image;
                        //Elements[row, col].Image.Freeze();
                        //Elements[row, col].Image.Freeze();
                    }

                    Thread.Sleep(1400);

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
            while (true)//ToDo: Majd amgí nem igaz, hogy vége
            {
                //IElement[,] elements = new IElement[GameSize.X - 1, GameSize.Y - 1];
                /*lock (_ElementsListLockObject)
                {
                    Elements.ForEach(element => elements[element.Position.X, element.Position.Y] = element);
                }*/
                //ToDo: amíg az időből nem telt el 30 perc, addig keressen fixen skilleket. Az legyen a prioritása.
                Player nearestPlayer = NearestPlayer(ai);
                //ToDO: ha bomba van a közelében bújjon el.
                //Az Elements lista = NotAvailablePoints;
                if (nearestPlayer != null && PositionDifference(nearestPlayer, ai) <= 20)//ToDo: ai.Bomb.explosionSize vagy ami lesz
                {
                    //ToDo: Go and Install bomb
                    //ToDo: hide and wait until Explosion + x seconds
                }
                else
                {
                    //ToDo: follow player
                    //ToDO: AI javítása nullcheckekkel, ha egyedül lenne mit csináljon.
                    if (nearestPlayer.Position.X - ai.Position.X < 0 && CanStepToPos(ai, new System.Windows.Vector(-1 * ai.Speed, 0)))
                    {
                        StartMove(PlayerAction.left, ai);
                        StopMove(PlayerAction.left, ai);
                    }

                    if (!CanStepToPos(ai, new System.Windows.Vector(-1 * ai.Speed, 0)))
                    {
                        int aiNextPosX = ai.Position.X + (-1 * ai.Speed);//Kiszervezni az egészet külön metódusba.
                        int aiNextPosY = ai.Position.Y;
                        //IElement blockingElement = elements[aiNextPosX, aiNextPosY];
                        List<Point> availablePath = FindAvailablePath(ai, aiNextPosX, aiNextPosY);
                        List<PlayerAction> availableRoundaboutActions = FindAvailableRoundaboutActions(ai, aiNextPosX, aiNextPosY);
                        //availableRoundaboutActions.ForEach(Action => Action.Pop);
                        //ToDo: jó irány keresése a megkerüléshez, majd móaction-ök meghívása sorban.
                        ;
                    }

                    if (nearestPlayer.Position.Y - ai.Position.Y < 0 && CanStepToPos(ai, new System.Windows.Vector(0, -1 * ai.Speed)))
                    {
                        StartMove(PlayerAction.up, ai);
                        StopMove(PlayerAction.up, ai);
                    }

                    if (nearestPlayer.Position.X - ai.Position.X > 0 && CanStepToPos(ai, new System.Windows.Vector(ai.Speed, 0)))
                    {
                        StartMove(PlayerAction.right, ai);
                        StopMove(PlayerAction.right, ai);
                    }

                    if (nearestPlayer.Position.Y - ai.Position.Y > 0 && CanStepToPos(ai, new System.Windows.Vector(0, ai.Speed)))
                    {
                        StartMove(PlayerAction.down, ai);
                        StopMove(PlayerAction.down, ai);
                    }
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

        private List<Point> FindAvailablePath(Player ai, int aiNextPosX, int aiNextPosY)
        {
            List<Point> availablePath = new List<Point>();
            for (int x = -1 * (GameRectSize / 2); x < (GameRectSize / 2); x++)
            {
                for (int y = -1 * (GameRectSize / 2); y < (GameRectSize / 2); y++)
                {
                    if (!(ai.Position.X + x == aiNextPosX && ai.Position.Y + y == aiNextPosY) && CanStepToPos(ai, new System.Windows.Vector(x * ai.Speed, y * ai.Speed)))
                    {
                        availablePath.Add(new Point(ai.Position.X + (x * ai.Speed), ai.Position.Y + (y * ai.Speed)));
                    }
                }
            }
            return availablePath;
        }

        private List<PlayerAction> FindAvailableRoundaboutActions(Player ai, int aiNextPosX, int aiNextPosY)
        {
            List<PlayerAction> availableActions = new List<PlayerAction>();
            for (int x = -1 * (GameRectSize / 2); x < (GameRectSize / 2); x++)
            {
                for (int y = -1 * (GameRectSize / 2); y < (GameRectSize / 2); y++)
                {
                    if (!(ai.Position.X + x == aiNextPosX && ai.Position.Y + y == aiNextPosY) && CanStepToPos(ai, new System.Windows.Vector(x * ai.Speed, y * ai.Speed)))
                    {
                        if (ai.Position.X + x - ai.Position.X < 0)
                        {
                            availableActions.Add(PlayerAction.left);
                        }

                        if (ai.Position.Y + y - ai.Position.Y < 0)
                        {
                            availableActions.Add(PlayerAction.up);
                        }

                        if (ai.Position.X + x - ai.Position.X > 0)
                        {
                            availableActions.Add(PlayerAction.right);
                        }

                        if (ai.Position.Y + y - ai.Position.Y > 0)
                        {
                            availableActions.Add(PlayerAction.down);
                        }
                    }
                }
            }
            return availableActions;
        }
    }
}
