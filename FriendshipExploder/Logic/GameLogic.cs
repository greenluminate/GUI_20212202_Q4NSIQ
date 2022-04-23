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
using FriendshipExploder.Model;

namespace FriendshipExploder.Logic
{
    public class GameLogic : IGameModel, IGameControl
    {
        private Queue<string[]> playgrounds; //path-okat tartalmaz, előre generált pálxák? //Mert vagy beletesszük ak iválaszott pályát választott meccs számszor, vagy előre legeneráljuk a random pélykat, csak beletesszük, hogy melyik fix és melyik, mely random. VAgy kuka az egész és mindig más laapján generálunk random.
        //Lehet ide kéne betenn ia köztes képernyőket is pl.: MainMenu, playground, who win image, curren leaderboard image, next playground és így körbe.

        //pályán lévő elemek (játékos, fal, stb.)
        public IElement[,] Elements { get; set; }

        public List<Player> Players { get; set; }

        //játéktér mérete (cella x, cella y)
        public int[] PlayGroundSize { get; set; }

        //kockák mérete
        private int GameRectSize { get; set; }

        private Point GameSize { get; set; }

        public string Timer { get; set; }

        public object _ElementsListLockObject { get; set; }
        public object _PlayersListLockObject { get; set; }
        public double PlayerHeightRate { get; set; }
        public double PlayerHeightRateHangsIn { get; set; }
        public double playerWidthRate { get; set; }

        public GameLogic()
        {
            _ElementsListLockObject = new object();
            _PlayersListLockObject = new object();
            Players = new List<Player>();

            playgrounds = new Queue<string[]>();
            PlayGroundSize = new int[2];

            PlayerHeightRate = 0.8;
            PlayerHeightRateHangsIn = 0.2;
            playerWidthRate = 0.6;

            string[] ground = LoadPlayground("playground_1.txt");

            Elements = new IElement[PlayGroundSize[0], PlayGroundSize[1]];

            /*PlayGroundSize[0] = 18;
            PlayGroundSize[1] = 14;*/

            /*string[] grounds = new string[] {
                "0wfwfwfwfwfwfwfww",
                "0w0wwwwwwwwwww00w",
                "00fwfwfwfwfwfwf0w",
                "0wwwwwwwwwwwwwwww",
                "0wfwfwfwfwfwfwfww",
                "0wwwwwwwwwwwwwwww",
                "0wfwfwfwfwfwfwfww",
                "0wwwwwwwwwwwwwwww",
                "0wfwfwfwfwfwfwfww",
                "0wwwwwwwwwwwwwwww",
                "00fwfwfwfwfwfwf0w",
                "0wwwwwwwwwwwwwwww",
                "0wfwfwfwfwfwfwfww"
            };*/

            //Ha választott pálya design, akkor betöltjük azt válaszott mennyiségszer a queue-ba, ha randomizáltat választotak a fixek közül, akkor random tötljülk be a fixeket
            for (int i = 0; i < 3; i++)
            {
                playgrounds.Enqueue(ground);
            }

            LoadNext(playgrounds.Dequeue());

            //Későbbi feature lehet: menüből választhatnak pályaméretet.
            //Későbbi feature lehet: random generált pálya design.
        }

        private string[] LoadPlayground(string file)
        {
            if (Directory.Exists("Playgrounds") && File.Exists(@$"Playgrounds\{file}"))
            {
                string[] rows = File.ReadAllLines(@$"Playgrounds\{file}");

                PlayGroundSize[0] = rows[0].Length;
                PlayGroundSize[1] = rows.Length;
                return rows;
            }
            else
            {
                //hiba
                return null;
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
                Players.ForEach(x =>
                {
                    bool leftPlayground = false;

                    if (x.Position.X + corrigSize < 0) { x.SetPos(0, x.Position.Y); leftPlayground = true; } //bal
                    else if (x.Position.X + (playerWidthRate * GameRectSize) + corrigSize > (PlayGroundSize[0] - 1) * GameRectSize) { x.SetPos((int)(((PlayGroundSize[0] - 1) * GameRectSize) - (playerWidthRate * GameRectSize)), x.Position.Y); leftPlayground = true; } //jobb

                    if (x.Position.Y + corrigSize < 0) { x.SetPos(x.Position.X, 0); leftPlayground = true; } //fent
                    else if (x.Position.Y + (PlayerHeightRate * gameRectSize) + corrigSize > (PlayGroundSize[1] - 1) * GameRectSize) { x.SetPos(x.Position.X, (int)(((PlayGroundSize[1] - 1) * GameRectSize) - (playerWidthRate * GameRectSize))); leftPlayground = true; } //lent

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
                                Elements[i,j] = new FixWall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute))));
                            }
                            break;
                        case 'w':
                            lock (_ElementsListLockObject)
                            {
                                Elements[i, j] = new Wall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute))));
                            }
                            break;
                    }
                }
            }

            Players.Add(new Player(0, new Point(0, 0), Model.KeyBinding.upDownLeftRight));
            Players.Add(new Player(1, new Point(0, 1), Model.KeyBinding.WSAD));
            Players.Add(new Player(2, new Point(0, 8), Model.KeyBinding.ai));
            Players.Add(new Player(3, new Point(0, 9), Model.KeyBinding.ai));

            AITaskCreator();
            CountDown(150);

        }

        private void CountDown(int seconds)
        {
            Task countDownTask = new Task(() =>
            {
                DateTime StartTime = new DateTime(1, 1, 1, 0, seconds / 60, seconds % 60);
                this.Timer = StartTime.ToString(@"mm\:ss");
                Thread.Sleep(2000);
                DateTime StartDate = DateTime.Now;
                while (!this.Timer.Equals("00:00"))
                {
                    this.Timer = StartTime.AddSeconds(-(DateTime.Now.Second - StartDate.Second)).ToString(@"mm\:ss");
                }
            }, TaskCreationOptions.LongRunning);
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

            Rectangle playerRect = new Rectangle(player.Position.X + (int)direction.X, player.Position.Y + (int)direction.Y, (int)(playerWidthRate * GameRectSize), (int)((PlayerHeightRate - PlayerHeightRateHangsIn) * GameRectSize));
            
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

                for (int i = 0; i < Elements.GetLength(0); i++)
                {
                    for (int j = 0; j < Elements.GetLength(1); j++)
                    {
                        if (Elements[i, j] != null)
                        {
                            Rectangle elementRect = new Rectangle(i * GameRectSize, j * GameRectSize, GameRectSize, GameRectSize);
                            if (playerRect.IntersectsWith(elementRect))
                            {
                                if (Elements[i, j] is Bomb && Elements[i, j] == BombUnderPlayer)
                                {
                                    return true;
                                }
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
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
                    if (posX + (playerWidthRate * GameRectSize) + pl.Speed <= ((PlayGroundSize[0] - 1) * GameRectSize) && CanStepToPos(pl, new System.Windows.Vector(pl.Speed, 0)))
                    {
                        pl.Move(pl.Speed, 0);
                        pl.HeadDirection = PlayerDirection.right;
                    }
                    break;

                //Bomba lerakása
                case PlayerAction.bombudlr:
                case PlayerAction.bombwasd:
                    if (pl.BombList.Count < pl.BombAmount)
                    {
                        //Itt beadható, ha scheduled, de még nem tudom, hogyan nézem meg, higy le van e nyomva az action is közben
                        Bomb newBomb = pl.Bomb.BombCopy(
                                        new Point(
                                            (int)Math.Floor((decimal)(pl.Position.X / GameRectSize)),
                                            (int)Math.Floor((decimal)(pl.Position.Y / GameRectSize))),
                                        new ImageBrush(
                                            new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Bombs", "bomb.png"),
                                            UriKind.RelativeOrAbsolute))));
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

                        new Task(() =>
                        {
                            Thread.Sleep(3000);//x másodperc múlva robba na bomba
                            Bomb bomb = null;

                            lock (_ElementsListLockObject)
                            {
                                bomb = (Bomb)Elements[i, j];
                                //bomb = (Bomb)Array.Find(Elements, bomb => bomb.Equals(newBomb));
                            }

                            if (bomb != null)
                            {
                                bomb.Image = new ImageBrush(
                                                new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "FireParts", "explosion.png"),
                                                UriKind.RelativeOrAbsolute)));
                            }

                            newBomb.Image.Freeze();
                            Thread.Sleep(500);

                            lock (pl._bombListLockObject)
                            {
                                pl.BombList.Remove(newBomb);
                            }

                            lock (_ElementsListLockObject)
                            {
                                Elements[i, j] = null;
                                //Elements.Remove(bomb);
                            }

                        }, TaskCreationOptions.LongRunning).Start();
                    }
                    //Rövid taskkal felrobbantani.
                    break;

                //Action
                case PlayerAction.actionudlr:
                case PlayerAction.actionwasd:
                    break;
            }
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
                if (PositionDifference(nearestPlayer, ai) <= 20)//ToDo: ai.Bomb.explosionSize vagy ami lesz
                {
                    //ToDo: Go and Install bomb
                    //ToDo: hide and wait until Explosion + x seconds
                }
                else
                {
                    //ToDo: follow player
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
            return Players.OrderBy(player => PositionDifference(player, ai))
                          .Where(player => player.Id != ai.Id)
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
