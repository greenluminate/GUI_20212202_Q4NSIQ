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
        public List<IElement> Elements { get; set; }

        public List<Player> Players { get; set; }

        //játéktér mérete (cella x, cella y)
        public int[] PlayGroundSize { get; set; }

        //kockák mérete
        private int GameRectSize { get; set; }

        private Point GameSize { get; set; }


        public GameLogic()
        {
            Elements = new List<IElement>();
            Players = new List<Player>();

            //Queue példányosítás
            playgrounds = new Queue<string[]>();
            PlayGroundSize = new int[2];

            string[] ground = LoadPlayground("playground_1.txt");

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

            //Ha válaszott pálya design, akkor betöltjük azt válaszott mennyiségszer a queue-ba, ha randomizáltat választotak a fixek közül, akkor random tötljülk be a fixeket
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

                    if (x.Position.X - gameRectSize / 4 + corrigSize < 0) { x.SetPos(gameRectSize / 4, x.Position.Y); leftPlayground = true; } //bal
                    else if (x.Position.X + gameRectSize / 4 + corrigSize > (PlayGroundSize[0] - 1) * GameRectSize) { x.SetPos(((PlayGroundSize[0] - 1) * GameRectSize) - (gameRectSize / 4), x.Position.Y); leftPlayground = true; } //jobb

                    if (x.Position.Y - gameRectSize / 4 + corrigSize < 0) { x.SetPos(x.Position.X, gameRectSize / 4); leftPlayground = true; } //fent
                    else if (x.Position.Y + gameRectSize / 4 + corrigSize > (PlayGroundSize[1] - 1) * GameRectSize) { x.SetPos(x.Position.X, ((PlayGroundSize[1] - 1) * GameRectSize) - (gameRectSize / 4)); leftPlayground = true; } //lent

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
                            Elements.Add(new FixWall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        case 'w':
                            Elements.Add(new Wall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                    }
                }
            }

            Players.Add(new Player(0, new Point(15, 10), Model.KeyBinding.upDownLeftRight));
            //Players.Add(new Player(1, new Point(15, 60), Model.KeyBinding.WSAD));
            Players.Add(new Player(2, new Point(15, 120), Model.KeyBinding.ai));
            Players.Add(new Player(1, new Point(15, 180), Model.KeyBinding.ai));
            //ToDo: TaskCreator => TODO: Async void-> while everything
            AITaskCreator();
        }

        public enum PlayerAction
        {
            up, down, left, right, W, A, S, D, bomb, kick //Később: explode ha lesz időzítettünk
        }

        //Odaléphet-e a játékos
        private bool CanStepToPos(Player player, System.Windows.Vector direction)
        {
            bool canStep = true;
            Rectangle playerRect = new Rectangle(player.Position.X + (int)direction.X - (GameRectSize / 4), player.Position.Y + (int)direction.Y - (GameRectSize / 4), GameRectSize / 2, GameRectSize / 2);

            foreach (var element in Elements)
            {
                if (element is Wall || element is FixWall)
                {
                    Rectangle elementRect = new Rectangle(element.Position.X * GameRectSize, element.Position.Y * GameRectSize, GameRectSize, GameRectSize);
                    if (playerRect.IntersectsWith(elementRect))
                    {
                        canStep = false;
                    }
                }
            }
            return canStep;
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
                    pl = Players.Where(p => p.KeyBinding == Model.KeyBinding.upDownLeftRight).FirstOrDefault();
                    break;
                case PlayerAction.W:
                case PlayerAction.S:
                case PlayerAction.A:
                case PlayerAction.D:
                    pl = Players.Where(p => p.KeyBinding == Model.KeyBinding.WSAD).FirstOrDefault();
                    break;
            }
            return pl;
        }

        //A játékos mozgásának kezdete, a controller hívja meg
        public async Task StartMove(PlayerAction playerAction)
        {
            Player pl = GetKeyBindingForPlayer(playerAction);

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
        public void StopMove(PlayerAction playerAction)
        {
            Player pl = GetKeyBindingForPlayer(playerAction);

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

        //A játékos mozgása
        public void Act(PlayerAction playerAction, Player pl)
        {
            int posX = Players[0].Position.X;
            int posY = Players[0].Position.Y;

            switch (playerAction)
            {
                case PlayerAction.up:
                case PlayerAction.W:
                    if (posY - GameRectSize / 4 - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(0, -1 * pl.Speed)))
                    {
                        pl.Move(0, -pl.Speed);
                        pl.HeadDirection = PlayerDirection.up;
                    }
                    break;
                case PlayerAction.down:
                case PlayerAction.S:
                    if (posY + GameRectSize / 4 + pl.Speed <= (PlayGroundSize[1] - 1) * GameRectSize && CanStepToPos(pl, new System.Windows.Vector(0, pl.Speed)))
                    {
                        pl.Move(0, pl.Speed);
                        pl.HeadDirection = PlayerDirection.down;
                    }
                    break;
                case PlayerAction.left:
                case PlayerAction.A:
                    if (posX - GameRectSize / 4 - pl.Speed >= 0 && CanStepToPos(pl, new System.Windows.Vector(-1 * pl.Speed, 0)))
                    {
                        pl.Move(-pl.Speed, 0);
                        pl.HeadDirection = PlayerDirection.left;
                    }
                    break;
                case PlayerAction.right:
                case PlayerAction.D:
                    if (posX + GameRectSize / 4 + pl.Speed <= ((PlayGroundSize[0] - 1) * GameRectSize) && CanStepToPos(pl, new System.Windows.Vector(pl.Speed, 0)))
                    {
                        pl.Move(pl.Speed, 0);
                        pl.HeadDirection = PlayerDirection.right;
                    }
                    break;
            }
        }

        private void AITaskCreator()
        {
            List<Player> ais = new List<Player>();
            ais.AddRange(Players.Where(player => player.KeyBinding == Model.KeyBinding.ai));

            List<Task> aiTasks = ais.Select(ai => new Task(() => AIWakeUp(ai), TaskCreationOptions.LongRunning)).ToList();
            //ToDO kirakni a tasklistet;
            Parallel.ForEach(aiTasks, task => task.Start());//Hogy amennyire csak lehet egyszerre induljanak
        }

        private async void AIWakeUp(Player ai)
        {
            Thread.Sleep(10000);
            while (true)//Majd EndGame ha kész lesz
            {
                Act(PlayerAction.down, ai);
            }
        }
    }
}
