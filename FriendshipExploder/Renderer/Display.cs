using FriendshipExploder.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            base.OnRender(drawingContext);
            if (gameModel != null && size.Width > 50 && size.Height > 50)//ToDo: Ennél kisebbre ne lehessen állítani az ablakot, mint megkötés a windownál.
            {
                drawingContext.DrawRectangle(
                    new ImageBrush(
                        new BitmapImage(
                            new Uri(
                                Path.Combine("..", "..", "..", "Images", "GameBackground", "0_GameBackground.jpg"),
                                UriKind.RelativeOrAbsolute))),
                    null, new Rect(0, 0, size.Width, size.Height));
                //kocka méretének meghatározása
                double gameRectWidth = (size.Width / gameModel.PlayGroundSize[0]);
                double gameRectHeight = ((size.Height - size.Height * 0.05) / gameModel.PlayGroundSize[1]);
                double startY = (size.Height * 0.05);

                double gameRectSize = gameRectWidth < gameRectHeight ? gameRectWidth : gameRectHeight;

                double startX = ((size.Width - (gameRectSize * (gameModel.PlayGroundSize[0]))) / 2);

                //állapotsáv
                //drawingContext.DrawRectangle(Brushes.Beige, new Pen(Brushes.Black, 1),new Rect(0, 0, size.Width, size.Height * 0.05)); //5%-a a magasságnak


                //keret renderelése
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)));

                //sarkok
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX, startY, gameRectSize / 2, gameRectSize / 2)); //bal felső
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX + (gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY, gameRectSize / 2, gameRectSize / 2)); //jobb felső
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize / 2)); //bal alsó
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX + (gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize / 2)); //jobb alsó

                for (int i = 0; i < gameModel.PlayGroundSize[0] - 1; i++)
                {
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX + (i * gameRectSize) + gameRectSize / 2, startY, gameRectSize, gameRectSize / 2)); //felső sor
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX + (i * gameRectSize) + gameRectSize / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize, gameRectSize / 2)); //alsó sor
                }
                for (int i = 0; i < gameModel.PlayGroundSize[1] - 1; i++)
                {
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX, startY + (i * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize)); //bal oldal
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX + (gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY + (i * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize)); //jobb oldal
                }


                //játéktér kezdete a bal felső sarokban a fal nélkül
                startX += gameRectSize / 2;
                startY = startY + gameRectSize / 2;

                //háttér renderelése
                brush = new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Floors", "0_Floor.png"), UriKind.RelativeOrAbsolute)));
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(startX, startY, gameRectSize * (gameModel.PlayGroundSize[0] - 1), gameRectSize * (gameModel.PlayGroundSize[1] - 1))); //bal felső


                //kockaméret átadása a logic részére
                gameModel.SetupSize(new System.Drawing.Point((int)size.Width, (int)(size.Height - size.Height * 0.05)), (int)gameRectSize);


                //elemek kirajzolása
                foreach (var element in gameModel.Elements)
                {
                    double x = startX + element.Position.X * gameRectSize;
                    double y = startY + element.Position.Y * gameRectSize;

                    drawingContext.DrawRectangle(
                        element.Image,
                        new Pen(Brushes.Black, 0),
                        new Rect(x, y, gameRectSize, gameRectSize)
                    );
                }

                //játékosok kirajzolása
                foreach (var player in gameModel.Players)
                {
                    double x = startX + player.Position.X;
                    double y = startY + player.Position.Y;

                    string dir = player.HeadDirection.ToString();
                    ImageBrush playerImage = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{player.Id}_player_{dir}.png")));

                    drawingContext.DrawRectangle(
                        playerImage,
                        new Pen(Brushes.Black, 0),
                        new Rect(x - gameRectSize / 4, y - gameRectSize / 4, gameRectSize / 2, gameRectSize / 2)
                    );
                }
                drawingContext.DrawText(new FormattedText(gameModel.Timer, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Verdana"), 30, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip), new Point(size.Width / 2 - (30 * 1.5), size.Height * 0.025));
            }
        }

    }
}
