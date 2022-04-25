using FriendshipExploder.Logic;
using FriendshipExploder.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FriendshipExploder.Menu
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        private int activeColumn = 0;
        private int activePlayer = 0;
        private string[] keyBinding = { "[Disabled]", "[Up][Down][Left][Right]", "[W][S][A][D]", "[Ai]" };
        private List<int> playerKeyBinding = Enumerable.Range(0, 3).Select(n => 0).ToList();
        private List<string> playGrounds = new List<string>();
        private int activePlayground = 0;

        private IGameModel GameModel { get; set; }

        public MainMenu(IGameModel model)
        {
            this.GameModel = model;
            InitializeComponent();
            ChangeColumn();
            LoadPlaygrounds();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NewGame(object sender, RoutedEventArgs e)
        {
            activeColumn = 1;
            ChangeColumn();
        }

        private void menu_Loaded(object sender, RoutedEventArgs e)
        {
            menu.Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
        }

        private void ChangeColumn()
        {
            switch (activeColumn)
            {
                case 0:
                    firstColumn.Opacity = 1;
                    secondColumn.Opacity = 0.2;
                    thirdColumn.Opacity = 0.2;
                    break;
                case 1:
                    ChangeActivePlayer();
                    firstColumn.Opacity = 0.2;
                    secondColumn.Opacity = 1;
                    thirdColumn.Opacity = 0.2;
                    newGame.IsEnabled = false;
                    exitGame.IsEnabled = false;
                    break;
                case 2:
                    firstColumn.Opacity = 0.2;
                    secondColumn.Opacity = 0.2;
                    thirdColumn.Opacity = 1;
                    break;
            }
        }

        private void ChangeActivePlayer()
        {
            switch (activePlayer)
            {
                case 0:
                    player0.Opacity = 1;
                    player1.Opacity = 0.2;
                    player2.Opacity = 0.2;
                    break;
                case 1:
                    player0.Opacity = 0.2;
                    player1.Opacity = 1;
                    player2.Opacity = 0.2;
                    break;
                case 2:
                    player0.Opacity = 0.2;
                    player1.Opacity = 0.2;
                    player2.Opacity = 1;
                    break;
            }
        }

        private void ChangeKeyBinding(bool increase)
        {
            if (increase)
            {
                if (playerKeyBinding[activePlayer] == keyBinding.Length - 1)
                {
                    playerKeyBinding[activePlayer] = 0;
                }
                else if (playerKeyBinding.Any(bd => bd == playerKeyBinding[activePlayer] + 1) &&
                        playerKeyBinding[activePlayer] + 1 != 0 &&
                        playerKeyBinding[activePlayer] + 1 != 3)
                {
                    //Már ki van osztva az adott keybinding
                    playerKeyBinding[activePlayer]++;
                    ChangeKeyBinding(increase);
                }
                else
                {
                    playerKeyBinding[activePlayer]++;
                }
            }
            else
            {
                if (playerKeyBinding[activePlayer] == 0)
                {
                    playerKeyBinding[activePlayer] = keyBinding.Length - 1;
                }
                else if (playerKeyBinding.Any(bd => bd == playerKeyBinding[activePlayer] - 1) &&
                        playerKeyBinding[activePlayer] - 1 != 0 &&
                        playerKeyBinding[activePlayer] - 1 != 3)
                {
                    //Már ki van osztva az adott keybinding
                    playerKeyBinding[activePlayer]--;
                    ChangeKeyBinding(false);
                }
                else
                {
                    playerKeyBinding[activePlayer]--;
                }
            }

            player0KeyBinding.Content = keyBinding[playerKeyBinding[0]];
            player1KeyBinding.Content = keyBinding[playerKeyBinding[1]];
            player2KeyBinding.Content = keyBinding[playerKeyBinding[2]];
        }

        private void ChangeActivePlayground()
        {
            playgrounds.SelectedIndex = activePlayground;
        }

        private void LoadPlaygrounds()
        {
            string[] files = Directory.GetFiles("Playgrounds", "*.txt");
            foreach (var playground in files)
            {
                Border br = new Border();
                br.BorderBrush = new SolidColorBrush(Colors.LightGray);
                br.BorderThickness = new Thickness(2);
                br.Margin = new Thickness(10);
                br.HorizontalAlignment = HorizontalAlignment.Stretch;

                Label pl = new Label();
                pl.Foreground = new SolidColorBrush(Colors.White);
                pl.FontSize = 24;
                pl.FontWeight = FontWeights.Bold;
                pl.Content = System.IO.Path.GetFileNameWithoutExtension(@"Playgrounds\" + playground);
                br.Child = pl;

                playgrounds.Items.Add(br);
                playGrounds.Add(playground);
            }
            playgrounds.SelectedIndex = 0;
        }

        private void menu_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (activeColumn == 1 && activePlayer == 0)
                    {
                        activePlayer = playerKeyBinding.Count() - 1;
                        ChangeActivePlayer();
                    }
                    else if (activeColumn == 1 && activePlayer > 0)
                    {
                        activePlayer--;
                        ChangeActivePlayer();
                    }
                    else if (activeColumn == 2 && activePlayground > 0)
                    {
                        activePlayground--;
                        ChangeActivePlayground();
                    }
                    break;
                case Key.Down:
                    if (activeColumn == 1 && activePlayer < playerKeyBinding.Count() - 1)
                    {
                        activePlayer++;
                        ChangeActivePlayer();
                    }
                    else if (activeColumn == 1)
                    {
                        activePlayer = 0;
                        ChangeActivePlayer();
                    }
                    else if (activeColumn == 2 && activePlayground < playGrounds.Count)
                    {
                        activePlayground++;
                        ChangeActivePlayground();
                    }
                    break;
                case Key.Left:
                    if (activeColumn == 1)
                    {
                        ChangeKeyBinding(false);
                    }
                    break;
                case Key.Right:
                    if (activeColumn == 1)
                    {
                        ChangeKeyBinding(true);
                    }
                    break;
                case Key.Enter:
                    if (activeColumn == 1)
                    {
                        activeColumn++;
                        ChangeColumn();
                    }
                    else if (activeColumn == 2)
                    {
                        //Játékosok létrehozása
                        playerKeyBinding.Where(bindnum => bindnum != 0)
                            .ToList()
                            .ForEach(bindnum => GameModel.Players.Add(
                                new Player(playerKeyBinding.IndexOf(bindnum),
                                new System.Drawing.Point(2, playerKeyBinding.IndexOf(bindnum) * 120),
                                (Model.KeyBinding)bindnum - 1)));

                        //Pálya betöltése
                        GameModel.LoadPlayground(playGrounds[playgrounds.SelectedIndex]);
                        this.DialogResult = true;
                    }
                    break;
            }
        }
    }
}
