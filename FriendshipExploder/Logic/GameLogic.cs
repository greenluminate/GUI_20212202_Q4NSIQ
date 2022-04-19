using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FriendshipExploder.Model;

namespace FriendshipExploder.Logic
{
    public class GameLogic : IGameModel, IGameControl
    {
        public enum PlaygroundItem
        {
            player0, player1, palyer2, bomb, fire, fixWall, wall, floor, modderFloor, booster //Ezekhez mind van mappa
        }

        public enum InfobarItem
        {
            counter, score //killnum; socer darabszáma = játékosok száma a menüben kiválasztás után.
        }

        public enum Playground
        {
            pg0, pg1, pg2, pg3 //pg0 = Random válogatott
        }

        //Pálya1, Pálya2, Pálya3, RandomGenerált, RandomSorsoltAMeglévőkből
        //Fájlokból töltsük a három db fix pályatervet kiválasztás alapján, vagy hardcodeoljuk a három fix pályát? Hosz a random generált pályát úgy is kódban kénem egírni.


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

            //Gueue példányosítás
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
            this.GameRectSize = gameRectSize;
            this.GameSize = gameSize;
            Players.ForEach(x => { x.Speed = (int)gameSize.X / 300; });
        }

        private void LoadNext(string[] grounds)
        {
            //Betöltjük a válaszott pályadesignt = enumok (vagy fix, vagy random kérés) a választott menyniségű játékossal.
            for (int i = 0; i < PlayGroundSize[0]-1; i++)
            {
                for (int j = 0; j < PlayGroundSize[1]-1; j++)
                {
                    switch (grounds[j][i])
                    {
                        case 'f':
                            Elements.Add(new FixWall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        case '0':
                            Elements.Add(new Floor(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        case 'w':
                            Elements.Add(new Wall(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        default:
                            Elements.Add(new Floor(new Point(i, j), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                    }
                }
            }

            Players.Add(new Player(new Point(10, 10), new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Players", "0_Player.png"), UriKind.RelativeOrAbsolute)))));
        }

        public enum PlayerAction //Action foglalt = beépített név
        {
            up, down, left, right, bomb, kick //Később: explode ha lesz időzítettünk
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

        //A játékos mozgásának kezdete, a controller hívja meg
        public async Task StartMove(PlayerAction playerAction, int playerId)
        {
            switch (playerAction)
            {
                case PlayerAction.up:
                case PlayerAction.down:
                    if (Players[playerId].MovingVertical == false)
                    {
                        Players[playerId].MovingVertical = true;
                        while (Players[playerId].MovingVertical)
                        {
                            Act(playerAction, playerId);
                            await Task.Delay(1);
                        }
                    }
                    break;
                case PlayerAction.left:
                case PlayerAction.right:
                    if (Players[playerId].MovingHorizontal == false)
                    {
                        Players[playerId].MovingHorizontal = true;
                        while (Players[playerId].MovingHorizontal)
                        {
                            Act(playerAction, playerId);
                            await Task.Delay(1);
                        }
                    }
                    break;
            }
            
        }

        //A játékos mozgásának vége, a controller hívja meg
        public void StopMove(PlayerAction playerAction, int playerId)
        {
            switch (playerAction)
            {
                case PlayerAction.up:
                case PlayerAction.down:
                    Players[playerId].MovingVertical = false;
                    break;
                case PlayerAction.left:
                case PlayerAction.right:
                    Players[playerId].MovingHorizontal = false;
                    break;
            }
        }

        //A játékos mozgása
        public void Act(PlayerAction playerAction, int playerId)
        {
            int posX = Players[0].Position.X;
            int posY = Players[0].Position.Y;

            switch (playerAction)
            {
                case PlayerAction.up:
                    if (posY - GameRectSize / 4 - Players[playerId].Speed >= 0 && CanStepToPos(Players[playerId], new System.Windows.Vector(0, -1*Players[playerId].Speed)))
                    {
                        Players[0].Move(0, -Players[playerId].Speed);
                    }
                    break;
                case PlayerAction.down:
                    if (posY + GameRectSize / 4 + Players[playerId].Speed <= (PlayGroundSize[1] - 1) * GameRectSize && CanStepToPos(Players[playerId], new System.Windows.Vector(0, Players[playerId].Speed)))
                    {
                        Players[0].Move(0, Players[playerId].Speed);
                    }
                    break;
                case PlayerAction.left:
                    if (posX - GameRectSize / 4 - Players[playerId].Speed >= 0 && CanStepToPos(Players[playerId], new System.Windows.Vector(-1*Players[playerId].Speed, 0)))
                    {
                        Players[0].Move(-Players[playerId].Speed, 0);
                    }
                    break;
                case PlayerAction.right:
                    if (posX + GameRectSize / 4 + Players[playerId].Speed <= ((PlayGroundSize[0] - 1) * GameRectSize) && CanStepToPos(Players[playerId], new System.Windows.Vector(Players[playerId].Speed, 0)))
                    {
                        Players[0].Move(Players[playerId].Speed, 0);
                    }
                    break;
            }
        }

    }
}
