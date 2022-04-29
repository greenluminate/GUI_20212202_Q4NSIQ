﻿using System;
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

        public void SetupSize(Point gameSize, int gameRectSize)
        {
            //játékos reszponzivitás
            FixCharacterPosition(gameRectSize);

            this.GameRectSize = gameRectSize;
            this.GameSize = gameSize;
            Players.ForEach(x => { x.Speed = (int)gameSize.X / 300; });
        }

        //karakter reszponzivitása átméretezésnél
        private void FixCharacterPosition(int gameRectSize)
        {
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
                                        //Powerups[i, j] = new Powerup(ElementType.BomUp);
                                        Powerups[i, j] = null;
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
                    if (!GamePaused)
                    {
                        this.Timer = StartTime.AddTicks(StartDate.Ticks - DateTime.Now.Ticks).ToString(@"mm\:ss");
                    }
                    else
                    {
                        int stopDifference = new DateTime(StartDate.Ticks - DateTime.Now.Ticks).Second;

                        lock (_TimerLockObject)
                        {
                            Monitor.Wait(_TimerLockObject);
                        }
                        StartDate = DateTime.Now.AddSeconds(stopDifference);
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

        //Odaléphet-e a játékos
        private bool CanStepToPos(Player player, System.Windows.Vector direction)
        {
            Rectangle playerPrevRect = new Rectangle(player.Position.X, player.Position.Y, (int)(PlayerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));
            Rectangle playerRect = new Rectangle(player.Position.X + (int)direction.X, player.Position.Y + (int)direction.Y, (int)(PlayerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));

            int playerCurrentIndexX = (int)Math.Floor((decimal)(player.Position.X / GameRectSize));
            int playerCurrentIndexY = (int)Math.Floor((decimal)(player.Position.Y / GameRectSize));

            //int playerNextIndexX = (int)Math.Floor((decimal)((player.Position.X + direction.X - (GameRectSize / 4) + (GameRectSize / 2)) / GameRectSize));
            //int playerNextIndexY = (int)Math.Floor((decimal)((player.Position.Y + direction.Y - (GameRectSize / 4) + (GameRectSize / 2)) / GameRectSize));

            //if (direction.Y > 0)//Ha lefelé szeretnénk menni, ne lógjunk belea cellába.
            //{
            //    playerNextIndexY = (int)Math.Floor((decimal)((player.Position.Y + direction.Y + (GameRectSize / 4)) / GameRectSize * 1.15));
            //}

            lock (_ElementsListLockObject)
            {
                IElement BombUnderPlayer = null;
                if (Elements[playerCurrentIndexX, playerCurrentIndexY] is Bomb)
                {
                    BombUnderPlayer = Elements[playerCurrentIndexX, playerCurrentIndexY];
                }

                bool canStep = true;

                for (int i = 0; i < Elements.GetLength(0); i++)
                {
                    for (int j = 0; j < Elements.GetLength(1); j++)
                    {
                        if (Elements[i, j] != null)
                        {
                            Rectangle elementRect = new Rectangle(i * GameRectSize, j * GameRectSize, GameRectSize, GameRectSize);
                            if (playerRect.IntersectsWith(elementRect))
                            {
                                if (!(Elements[i, j] is Bomb && playerPrevRect.IntersectsWith(elementRect)))
                                {
                                    canStep = false;
                                }

                                //return false;
                            }
                        }
                    }
                }
                return canStep;
            }

            //return true;
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
                    break;
            }
        }

        private void BombThreadStarter(Player pl)
        {
            if (pl.BombList.Count < pl.BombAmount)
            {
                //Itt beadható, ha scheduled, de még nem tudom, hogyan nézem meg, higy le van e nyomva az action is közben
                Bomb newBomb = pl.Bomb.BombCopy(
                                new Point(
                                    (int)Math.Floor((decimal)((pl.Position.X + (PlayerWidthRate * GameRectSize) / 2) / GameRectSize)),
                                    (int)Math.Floor((decimal)((pl.Position.Y + ((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize) / 2) / GameRectSize))));
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
                        //bomb = (Bomb)Array.Find(Elements, bomb => bomb.Equals(newBomb));
                    }

                    //ImageBrush explosionImg = new ImageBrush(
                    //                    new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "FireParts", "explosion.png"),
                    //                    UriKind.RelativeOrAbsolute)));

                    if (bomb != null)
                    {
                        //bomb.Image = explosionImg;
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
                            if (row < Elements.GetLength(0))
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
                            if (col < Elements.GetLength(1))
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
