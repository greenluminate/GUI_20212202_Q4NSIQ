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
using System.Windows.Media.Effects;
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
                gameModel.SetupRectSize((int)gameRectSize);

                //saját betütípus
                Typeface typeface = new Typeface(new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#Bomberman"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);


                //elemek kirajzolása
                //new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "FixWalls", "0_FixWall.png"), UriKind.RelativeOrAbsolute)))
                //new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Walls", "0_Wall.png"), UriKind.RelativeOrAbsolute)))
                //new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Bombs", "Bomb.png"),UriKind.RelativeOrAbsolute)))
                lock (gameModel._ElementsListLockObject)
                {
                    for (int i = 0; i < gameModel.Elements.GetLength(0); i++)
                    {
                        for (int j = 0; j < gameModel.Elements.GetLength(1); j++)
                        {
                            double x = startX + i * gameRectSize;
                            double y = startY + j * gameRectSize;
                            if (gameModel.Elements[i, j] != null)
                            {
                                if (!gameModel.Elements[i, j].Explode)
                                {
                                    drawingContext.DrawRectangle(
                                            new ImageBrush(
                                            new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", $"{gameModel.Elements[i, j].ElementType}s", $"0_{gameModel.Elements[i, j].ElementType}.png"),
                                            UriKind.RelativeOrAbsolute))),
                                            new Pen(Brushes.Black, 0),
                                            new Rect(x, y, gameRectSize, gameRectSize)
                                        );
                                }
                                else
                                {
                                    //Felrobbanás állapota
                                    drawingContext.DrawRectangle(
                                        new ImageBrush(
                                        new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", $"{gameModel.Elements[i, j].ElementType}s", $"0_{gameModel.Elements[i, j].ElementType}Explode.png"),
                                        UriKind.RelativeOrAbsolute))),
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectSize, gameRectSize)
                                    );
                                }
                            }
                            else if (gameModel.Powerups[i, j] != null)
                            {
                                //Powerups
                                drawingContext.DrawRectangle(
                                        new ImageBrush(
                                            new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "Powerups", $"{gameModel.Powerups[i, j].ElementType}.png"),
                                            UriKind.RelativeOrAbsolute))),
                                        new Pen(Brushes.Black, 0),
                                        new Rect(x, y, gameRectSize, gameRectSize)
                                    );
                            }
                        }
                        //játékosok kirajzolása
                        lock (gameModel._PlayersListLockObject)
                        {
                            foreach (var player in gameModel.Players)
                            {
                                double x = startX + player.Position.X;
                                double y = startY + player.Position.Y;

                                string dir = player.HeadDirection.ToString();
                                ImageBrush playerImage = new ImageBrush(new BitmapImage(new Uri(System.IO.Path.Combine("..", "..", "..", "Images", "Players", $"{player.Id}_player_{dir}.png"), UriKind.RelativeOrAbsolute)));

                                drawingContext.DrawRectangle(
                                    playerImage,
                                    new Pen(Brushes.Black, 0),
                                    new Rect(x, y - (gameRectSize * gameModel.PlayerHeightRateHangsIn), gameModel.PlayerWidthRate * gameRectSize, gameModel.PlayerHeightRate * gameRectSize)
                                //new Rect(x - gameRectSize * playerHeightRate / 4, y - gameRectSize * playerHeightRate / 4, gameRectSize * playerWidthRate, gameRectSize * playerHeightRate)
                                );
                            }
                        }
                        brush = new ImageBrush(new BitmapImage(new Uri(Path.Combine("..", "..", "..", "Images", "GameBackground", "0_timerbg.png"), UriKind.RelativeOrAbsolute)));
                        drawingContext.DrawRectangle(brush, new Pen(Brushes.Black, 0), new Rect(size.Width / 2 - (gameRectSize * 1.5), 0, gameRectSize * 3, size.Height * 0.05 + gameRectSize / 2));
                        drawingContext.DrawText(new FormattedText(gameModel.Timer, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 30, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip), new Point(size.Width / 2 - (30 * 1.5), size.Height * 0.025));
                        //30-ast lecserélni responsive értékre.


                        //futam vége
                        if (gameModel.RoundOver)
                        {
                            drawingContext.DrawRectangle(Brushes.LightGray, new Pen(Brushes.Red, 4), new Rect((size.Width / 2) - (size.Width / 4), (size.Height / 2) - (size.Height / 12), size.Width / 2, size.Height / 6));
                            FormattedText text = new FormattedText("Round over!", CultureInfo.GetCultureInfo("hu-hu"), FlowDirection.LeftToRight, typeface, size.Width / 24, Brushes.Red, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                            Point textLocation = new Point((size.Width / 2) - (text.WidthIncludingTrailingWhitespace / 2), (size.Height / 2) - (text.Height / 2));
                            drawingContext.DrawText(text, textLocation);
                        }

                        //futam eredmény
                        if (gameModel.RoundScore)
                        {
                            ImageBrush backgr = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/scboard.png")));

                            drawingContext.DrawRectangle(backgr, new Pen(Brushes.Black, 0), new Rect((size.Width / 2) - (size.Width / 4), (size.Height / 2) - (size.Height / 10), size.Width / 2, size.Height / 5));
                            FormattedText scoreText = new FormattedText("Score", CultureInfo.GetCultureInfo("hu-hu"), FlowDirection.LeftToRight, typeface, size.Width / 40, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                            Point textLocation = new Point((size.Width / 2) - (scoreText.WidthIncludingTrailingWhitespace / 2), (size.Height / 2) - (size.Height / 10) + 10);
                            drawingContext.DrawText(scoreText, textLocation);

                            double startHeight = scoreText.Height + 10;

                            var pl = gameModel.Players.Where(pl => pl.Id == 0).FirstOrDefault();
                            if (pl != null)
                            {
                                //Pl 1
                                FormattedText player1Text = new FormattedText($"Player 1: {pl.Kills} kill", CultureInfo.GetCultureInfo("hu-hu"), FlowDirection.LeftToRight, typeface, size.Width / 65, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                                textLocation = new Point((size.Width / 2) - (size.Width / 4) + 20, (size.Height / 2) - (size.Height / 10) + startHeight);
                                drawingContext.DrawText(player1Text, textLocation);
                                startHeight += player1Text.Height + 10;
                            }

                            pl = gameModel.Players.Where(pl => pl.Id == 1).FirstOrDefault();
                            if (pl != null)
                            {
                                //Pl 2
                                FormattedText player2Text = new FormattedText($"Player 2: {pl.Kills} kill", CultureInfo.GetCultureInfo("hu-hu"), FlowDirection.LeftToRight, typeface, size.Width / 65, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                                textLocation = new Point((size.Width / 2) - (size.Width / 4) + 20, (size.Height / 2) - (size.Height / 10) + startHeight);
                                drawingContext.DrawText(player2Text, textLocation);
                                startHeight += player2Text.Height + 10;
                            }

                            pl = gameModel.Players.Where(pl => pl.Id == 2).FirstOrDefault();
                            if (pl != null)
                            {
                                //Pl 3
                                FormattedText player3Text = new FormattedText($"Player 3: {pl.Kills} kill", CultureInfo.GetCultureInfo("hu-hu"), FlowDirection.LeftToRight, typeface, size.Width / 65, Brushes.White, VisualTreeHelper.GetDpi(this).PixelsPerDip);
                                textLocation = new Point((size.Width / 2) - (size.Width / 4) + 20, (size.Height / 2) - (size.Height / 10) + startHeight);
                                drawingContext.DrawText(player3Text, textLocation);
                            }
                        }


                        //megállítva
                        if (gameModel.GamePaused)
                        {
                            drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)), null, new Rect(0, 0, size.Width, size.Height));
                        }
                    }
                }
            }
        }
    }
}
