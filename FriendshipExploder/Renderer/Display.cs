using FriendshipExploder.Logic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FriendshipExploder.Renderer
{
    class Display : FrameworkElement
    {
        IMainMenuModel mainMenuModel;
        IGameModel gameModel;
        Size size;

        public void Resize(Size size)
        {
            this.size = size;
        }

        public void SetupModel(IMainMenuModel mainMenuModel, IGameModel gameModel)
        {
            this.mainMenuModel = mainMenuModel;
            this.gameModel = gameModel;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            #region ToDO
            //
            #endregion
            base.OnRender(drawingContext);
            if (gameModel != null && size.Width > 50 && size.Height > 50)//ToDo: Ennél kisebbre ne lehessen állítani az ablakot, mint megkötés a windownál.
            {
                ImageBrush brush = new ImageBrush();
                //x = col; y = row!!!
                //ToDO: megkeresni az enumokhoz tartozó képeket, az alapján, hogy mely pálya jön
                //local brus variabel and creation
                //telejs kéőpernyőt kitöltő téglalap kell-e?
                double gameRectWith = size.Width / 16;
                double gameRectHeight = size.Height / 14;

                double x;
                double y;
                for (int i = 0; i < gameModel.GameMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < gameModel.GameMatrix.GetLength(1); j++)
                    {
                        x = j * gameRectWith - (gameRectWith / 2);
                        y = (i + 2) * gameRectHeight - (gameRectHeight / 2);
                        drawingContext.DrawRectangle(
                            new ImageBrush(
                                new BitmapImage(
                                    new Uri(Path.Combine("Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)
                                    )
                                ),
                            new Pen(Brushes.Black, 0),
                           new Rect(x, y, gameRectWith, gameRectHeight)
                        );

                        switch (gameModel.GameMatrix[i, j])
                        {
                            case GameLogic.PlaygroundItem.player0:
                                brush = new ImageBrush
                                   (new BitmapImage(new Uri(Path.Combine("Images", "Players", "0_Player.png"), UriKind.RelativeOrAbsolute)));

                                drawingContext.DrawRectangle(
                                    brush,
                                    new Pen(Brushes.Black, 0),
                                    new Rect(x, y, gameRectWith, gameRectHeight * 1.5)
                                );//ere metódust írni és beadni neki az értékeket.
                                break;

                            case GameLogic.PlaygroundItem.player1:
                                break;
                            case GameLogic.PlaygroundItem.palyer2:
                                break;
                            case GameLogic.PlaygroundItem.bomb:
                                break;
                            case GameLogic.PlaygroundItem.fire:
                                break;

                            case GameLogic.PlaygroundItem.fixWall:
                                brush = new ImageBrush
                                   (new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)));

                                if ((x == 2 * gameRectHeight && y == 0) ||
                                    (x == 2 * gameRectHeight && y == gameRectHeight) ||
                                    (x == gameRectWith && y == gameRectHeight) ||
                                    (x == gameRectWith && y == 0))//Left-top, Left-bottom, Rigth-bottom, Right-top
                                {
                                    drawingContext.DrawRectangle(
                                        brush,
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectWith / 2, gameRectHeight / 2)
                                        );
                                }
                                else if (y == 0 || x == gameRectHeight)//Top, Bottom
                                {
                                    drawingContext.DrawRectangle(
                                        brush,
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectWith, gameRectHeight / 2)
                                        );
                                }
                                else if (x == 2 * gameRectHeight || y == gameRectHeight)//Left, Right
                                {
                                    drawingContext.DrawRectangle(
                                        brush,
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectWith / 2, gameRectHeight)
                                        );
                                }
                                else
                                {
                                    drawingContext.DrawRectangle(
                                        brush,
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectWith, gameRectHeight)
                                    );
                                }
                                break;

                            case GameLogic.PlaygroundItem.wall:
                                brush = new ImageBrush
                                   (new BitmapImage(new Uri(Path.Combine("Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute)));

                                drawingContext.DrawRectangle(
                                    brush,
                                    new Pen(Brushes.Black, 0),
                                    new Rect(x, y, gameRectWith, gameRectHeight)
                                );
                                break;

                            case GameLogic.PlaygroundItem.floor:
                                //Igazából már kirajzoltam, szóval mindegy elvileg.
                                break;

                            case GameLogic.PlaygroundItem.modderFloor:
                                break;
                            case GameLogic.PlaygroundItem.booster:
                                break;
                            default:
                                break;
                        }

                        drawingContext.DrawRectangle(
                            Brushes.Black,
                            new Pen(Brushes.Black, 0),
                            new Rect(0, 0, size.Width, gameRectHeight * 2));

                    }
                }
            }
        }

    }
}
