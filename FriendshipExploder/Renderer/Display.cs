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
            base.OnRender(drawingContext);
            bool canStart = false;
            foreach (var element in mainMenuModel.MenuElements) //Végig járja a menü elemeket, azaz a gombokat
            {
                element.SizeX = 50;
                element.SizeY = 50;
                drawingContext.DrawRectangle(
                    element.Brush,
                    new Pen(Brushes.Gray, 0),
                    new Rect(element.Position.X, element.Position.Y, element.SizeX, element.SizeY)
                    );
                if (element.IsClicked == true && element.Command == "start")
                {
                    canStart = true;
                }
            }
            
                if (gameModel != null && size.Width > 50 && size.Height > 50 && canStart == true )//ToDo: Ennél kisebbre ne lehessen állítani az ablakot, mint megkötés a windownál   Marci: Megnézi, hogy a megfelelő gomb lett-e megnyomva, hogy elkezdje a pálya renderelését
                {
                    //kocka méretének meghatározása
                    int gameRectWidth = (int)(size.Width / gameModel.PlayGroundSize[0]);
                    int gameRectHeight = (int)((size.Height - size.Height * 0.05) / gameModel.PlayGroundSize[1]);
                    int startY = (int)(size.Height * 0.05);

                    int gameRectSize = gameRectWidth < gameRectHeight ? gameRectWidth : gameRectHeight;
                    //állapotsáv
                    drawingContext.DrawRectangle(Brushes.Beige, new Pen(Brushes.Black, 1), new Rect(0, 0, size.Width, size.Height * 0.05)); //5%-a a magasságnak


                    //keret renderelése
                    ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)));

                    //sarkok

                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY, gameRectSize / 2, gameRectSize / 2)); //bal felső
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY, gameRectSize / 2, gameRectSize / 2)); //jobb felső
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize / 2)); //bal alsó
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize / 2)); //jobb alsó

                    for (int i = 0; i < gameModel.PlayGroundSize[0] - 1; i++)
                    {
                        drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((i * gameRectSize) + gameRectSize / 2, startY, gameRectSize, gameRectSize / 2)); //felső sor
                        drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((i * gameRectSize) + gameRectSize / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectSize) + gameRectSize / 2, gameRectSize, gameRectSize / 2)); //alsó sor
                    }
                    for (int i = 0; i < gameModel.PlayGroundSize[1] - 1; i++)
                    {
                        drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY + (i * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize)); //bal oldal
                        drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectSize + gameRectSize / 2, startY + (i * gameRectSize) + gameRectSize / 2, gameRectSize / 2, gameRectSize)); //jobb oldal
                    }


                    //játéktér kezdete a bal felső sarokban a fal nélkül
                    int startX = gameRectSize / 2;
                    startY = startY + gameRectSize / 2;

                    //kockaméret átadása a logic részére
                    gameModel.SetupSize(new System.Drawing.Point((int)size.Width, (int)(size.Height - size.Height * 0.05)), gameRectSize);


                    //elemek kirajzolása
                    foreach (var element in gameModel.Elements)
                    {
                        int x = startX + element.Position.X * gameRectSize;
                        int y = startY + element.Position.Y * gameRectSize;

                        drawingContext.DrawRectangle(
                            element.Image,
                            new Pen(Brushes.Black, 0),
                            new Rect(x, y, gameRectSize, gameRectSize)
                        );
                    }





                    //játékosok kirajzolása
                    foreach (var player in gameModel.Players)
                    {
                        int x = startX + player.Position.X;
                        int y = startY + player.Position.Y;

                        drawingContext.DrawRectangle(
                            player.Image,
                            new Pen(Brushes.Black, 0),
                            new Rect(x - gameRectSize / 4, y - gameRectSize / 4, gameRectSize / 2, gameRectSize / 2)
                        );
                    }
                }
            
            
        }

    }
}
