using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FriendshipExploder.Model;

namespace FriendshipExploder.Logic
{
    public class GameLogic : IGameModel, IGameControl
    {
        private KeyBinding player1_keyBinding;
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
        public PlaygroundItem[,] GameMatrix { get; set; } //Fájlokból töltsük a három db fix pályatervet kiválasztás alapján, vagy hardcodeoljuk a három fix pályát? Hosz a random generált pályát úgy is kódban kénem egírni.
        public InfobarItem[,] InfobarMatrix { get; set; }

        private Queue<string[]> playgrounds; //path-okat tartalmaz, előre generált pálxák? //Mert vagy beletesszük ak iválaszott pályát választott meccs számszor, vagy előre legeneráljuk a random pélykat, csak beletesszük, hogy melyik fix és melyik, mely random. VAgy kuka az egész és mindig más laapján generálunk random.
        //Lehet ide kéne betenn ia köztes képernyőket is pl.: MainMenu, playground, who win image, curren leaderboard image, next playground és így körbe.

        //pályán lévő elemek (játékos, fal, stb.)
        public List<IElement> Elements { get; set; }

        public List<Player> Players { get; set; }

        //játéktér mérete (cella x, cella y)
        public int[] PlayGroundSize { get; set; }

        //játéktér kezdete
        private Vector StartPos { get; set; }
        private Vector GameRectSize { get; set; }

        public GameLogic()
        {
            Elements = new List<IElement>();
            Players = new List<Player>();

            //Gueue példányosítás
            playgrounds = new Queue<string[]>();

            //Itt kéne a GameMátrixot példányosítani a fix mérettel.
            PlayGroundSize = new int[2];
            PlayGroundSize[0] = 18;
            PlayGroundSize[1] = 14;

            GameMatrix = new PlaygroundItem[52, 68]; //4* méret (13x17)

            InfobarMatrix = new InfobarItem[2, 17];

            string[] grounds = new string[] {
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
            };

            //Ha válaszott pálya design, akkor betöltjük azt válaszott mennyiségszer a queue-ba, ha randomizáltat választotak a fixek közül, akkor random tötljülk be a fixeket
            for (int i = 0; i < 3; i++)
            {
                playgrounds.Enqueue(grounds);
            }

            LoadNext(playgrounds.Dequeue());

            //Későbbi feature lehet: menüből választhatnak pályaméretet.
            //Későbbi feature lehet: random generált pálya design.
        }

        public void SetupSize(Vector startPos, Vector gameRectSize)
        {
            this.StartPos = startPos;
            this.GameRectSize = gameRectSize;
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
                            Elements.Add(new FixWall(0, i, j, new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        case '0':
                            Elements.Add(new Floor(0, i, j, new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        case 'w':
                            Elements.Add(new Wall(0, i, j, new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                        default:
                            Elements.Add(new Floor(0, i, j, new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)))));
                            break;
                    }
                }
            }

            Players.Add(new Player(0, 200, 100, new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "Players", "0_Player.png"), UriKind.RelativeOrAbsolute))))); //ez itt biztosan nincs jó helyen
        }


        /*private PlaygroundItem ConvertToEnum(char chr)
        {
            //Visszadjuk a chr alapján az adott pályaelemet, lehet hogy mi nem chr-rel dolgozunk, hanem stringgel, vag dicttel
            switch (chr)
            {
                case 'f': return PlaygroundItem.fixWall;
                case 'p': return PlaygroundItem.player0;//majd tobbre irni
                case '0': return PlaygroundItem.floor;
                case 'w': return PlaygroundItem.wall;
                //case 'w': return PlaygroundItem.wall;
                default:
                    return PlaygroundItem.floor;
            }
        }*/

        public enum PlayerAction //Action foglalt = beépített név
        {
            up, down, left, right, bomb, kick //Később: explode ha lesz időzítettünk
        }

        private bool CanStepToPos(Vector pos, Player player)
        {
            //odaléphet-e, de optimális lenne nem végigmenni minden elemen
            return true;
        }

        public void Act(PlayerAction playerAction)//Sender is which player?
        {
            int posX = Players[0].X; //de töb palyer is van. Valahogy meg kell adni, hogy melyik. VAgy arra is enum.
            int posY = Players[0].Y;

            switch (playerAction)
            {
                case PlayerAction.up:
                    if (posY - 5 > 0 && CanStepToPos(new Vector(Players[0].X, Players[0].Y - 5), Players[0]))
                    {
                        Players[0].Y = Players[0].Y - 5;
                    }
                    break;
                case PlayerAction.down:
                    if (posY + 5 < ((PlayGroundSize[1] - 2) * GameRectSize.Y) && CanStepToPos(new Vector(Players[0].X, Players[0].Y + 5), Players[0]))
                    {
                        Players[0].Y = Players[0].Y + 5;
                    }
                    break;
                case PlayerAction.left:
                    if (posX - 5 > 0 && CanStepToPos(new Vector(Players[0].X - 5, Players[0].Y), Players[0]))
                    {
                        Players[0].X = Players[0].X - 5;
                    }
                    break;
                case PlayerAction.right:
                    if (posX + 5 < ((PlayGroundSize[0] - 2) * GameRectSize.X) && CanStepToPos(new Vector(Players[0].X + 5, Players[0].Y), Players[0]))
                    {
                        Players[0].X = Players[0].X + 5;
                    }
                    break;
            }


            /*switch (playerAction)
            {
                case PlayerAction.up:
                    if (row - 1 >= 0 &&
                        GameMatrix[row - 1, col] != PlaygroundItem.fixWall &&
                        GameMatrix[row - 1, col] != PlaygroundItem.wall)
                    {
                        row--;
                    }
                    break;

                case PlayerAction.down:
                    if (row + 1 <= GameMatrix.GetLength(0) - 1 &&
                         GameMatrix[row + 1, col] != PlaygroundItem.fixWall &&
                         GameMatrix[row + 1, col] != PlaygroundItem.wall)
                    {
                        row++;
                    }
                    break;

                case PlayerAction.left:
                    if (col - 1 >= 0 &&
                         GameMatrix[row, col - 1] != PlaygroundItem.fixWall &&
                         GameMatrix[row, col - 1] != PlaygroundItem.wall)
                    {
                        col--;
                    }
                    break;

                case PlayerAction.right:
                    if (col + 1 <= GameMatrix.GetLength(1) - 1 &&
                         GameMatrix[row, col + 1] != PlaygroundItem.fixWall &&
                         GameMatrix[row, col + 1] != PlaygroundItem.wall)
                    {
                        col++;
                    }
                    break;

                case PlayerAction.bomb:
                    break;

                case PlayerAction.kick:
                    break;

                default:
                    break;
            }*/
        }

    }
}
