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
                //kocka méretének meghatározása
                int gameRectWidth = (int)(size.Width / gameModel.PlayGroundSize[0]);
                int gameRectHeight = (int)((size.Height - size.Height * 0.05) / gameModel.PlayGroundSize[1]);
                int startY = (int)(size.Height * 0.05);


                //állapotsáv
                drawingContext.DrawRectangle(Brushes.Beige, new Pen(Brushes.Black, 1),new Rect(0, 0, size.Width, size.Height * 0.05)); //5%-a a magasságnak


                //keret renderelése
                ImageBrush brush = new ImageBrush(new BitmapImage(new Uri(Path.Combine("Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)));

                //sarkok
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY, gameRectWidth / 2, gameRectHeight / 2)); //bal felső
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectWidth + gameRectWidth / 2, startY, gameRectWidth / 2, gameRectHeight / 2)); //jobb felső
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectHeight) + gameRectHeight / 2, gameRectWidth / 2, gameRectHeight / 2)); //bal alsó
                drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectWidth + gameRectWidth / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectHeight) + gameRectHeight / 2, gameRectWidth / 2, gameRectHeight / 2)); //jobb alsó

                for (int i = 0; i < gameModel.PlayGroundSize[0] - 1; i++)
                {
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((i * gameRectWidth) + gameRectWidth / 2, startY, gameRectWidth, gameRectHeight / 2)); //felső sor
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((i * gameRectWidth) + gameRectWidth / 2, startY + ((gameModel.PlayGroundSize[1] - 1) * gameRectHeight) + gameRectHeight / 2, gameRectWidth, gameRectHeight / 2)); //alsó sor
                }
                for (int i = 0; i < gameModel.PlayGroundSize[1] - 1; i++)
                {
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(0, startY + (i * gameRectHeight) + gameRectHeight / 2, gameRectWidth / 2, gameRectHeight)); //bal oldal
                    drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect((gameModel.PlayGroundSize[0] - 1) * gameRectWidth + gameRectWidth / 2, startY + (i * gameRectHeight) + gameRectHeight / 2, gameRectWidth / 2, gameRectHeight)); //jobb oldal
                }


                //játéktér kezdete a bal felső sarokban a fal nélkül
                int startX = gameRectWidth / 2;
                startY = startY + gameRectHeight / 2;

                //kezdet és kockaméret átadása a logic részére
                gameModel.SetupSize(new Vector(startX, startY), new Vector(gameRectWidth, gameRectHeight));


                //elemek kirajzolása
                foreach (var element in gameModel.Elements)
                {
                    int x = startX + element.PosX * gameRectWidth;
                    int y = startY + element.PosY * gameRectHeight;

                    drawingContext.DrawRectangle(
                        element.Image,
                        new Pen(Brushes.Black, 0),
                        new Rect(x, y, gameRectWidth, gameRectHeight)
                    );
                }

                //játékosok kirajzolása
                foreach (var player in gameModel.Players)
                {
                    int x = startX + player.X;
                    int y = startY + player.Y;

                    drawingContext.DrawRectangle(
                        player.Image,
                        new Pen(Brushes.Black, 0),
                        new Rect(x, y, gameRectWidth, gameRectHeight)
                    );
                }
            }
        }

    }
}
