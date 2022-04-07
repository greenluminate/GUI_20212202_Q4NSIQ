using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public GameLogic()
        {
            //Gueue példányosítás
            playgrounds = new Queue<string[]>();

            //Itt kéne a GameMátrixot példányosítani a fix mérettel.
            GameMatrix = new PlaygroundItem[13, 17];

            InfobarMatrix = new InfobarItem[2, 17];

            string[] grounds = new string[] {
                "fffffffffffffffff",
                "fp0wwwwwwwwwww00f",
                "f0fwfwfwfwfwfwf0f",
                "fwwwwwwwwwwwwwwwf",
                "fwfwfwfwfwfwfwfwf",
                "fwwwwwwwwwwwwwwwf",
                "fwfwfwfwfwfwfwfwf",
                "fwwwwwwwwwwwwwwwf",
                "fwfwfwfwfwfwfwfwf",
                "fwwwwwwwwwwwwwwwf",
                "f0fwfwfwfwfwfwf0f",
                "f00wwwwwwwwwww00f",
                "fffffffffffffffff"
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

        private void LoadNext(string[] grounds)
        {
            //Betöltjük a válaszott pályadesignt = enumok (vagy fix, vagy random kérés) a választott menyniségű játékossal.
            for (int i = 0; i < GameMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < GameMatrix.GetLength(1); j++)
                {
                    GameMatrix[i, j] = ConvertToEnum(grounds[i][j]);
                }
            }
        }


        private PlaygroundItem ConvertToEnum(char chr)
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
        }

        public enum PlayerAction //Action foglalt = beépített név
        {
            up, down, left, right, bomb, kick //Később: explode ha lesz időzítettünk
        }

        private int[] PlaygroundItemIndexes(PlaygroundItem item)
        {
            for (int row = 0; row < GameMatrix.GetLength(0); row++)
            {
                for (int col = 0; col < GameMatrix.GetLength(1); col++)
                {
                    if (GameMatrix[row, col] == item)// new int[] {i, j }
                    {
                        return new int[] { row, col };
                    }
                }
            }

            return new int[] { -1, -1 };
        }

        public void Act(PlayerAction playerAction)//Sender is which player?
        {
            int[] playerIndexes = PlaygroundItemIndexes(PlaygroundItem.player0);//de töb palyer is van. Valahogy meg kell adni, hogy melyik. VAgy arra is enum.

            int row = playerIndexes[0];
            int col = playerIndexes[1];

            int previousRow = row;
            int previousCol = col;

            switch (playerAction)
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
            }

            if (GameMatrix[row, col] == PlaygroundItem.floor)
            {
                //ToDo: alap mindig legyen, igazából minden féle pályaelem lehet külön rétegen, aztán ott keressük. Nem nagy dolog. És akkor tényleg lehet a karakternek nagyobb mátrixa.
                GameMatrix[row, col] = PlaygroundItem.player0;
                GameMatrix[previousRow, previousCol] = PlaygroundItem.floor;
            }
            else
            {
                //Csak fordulás abba az irányba.
            }
        }

    }
}
