using System;
using System.Collections.Generic;
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
        private int[] playerKeyBinding = { 0, 0, 0};

        public MainMenu()
        {
            InitializeComponent();
            AfterSelect();
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void NewGame(object sender, RoutedEventArgs e)
        {
            activeColumn = 1;
            AfterSelect();
        }

        private void menu_Loaded(object sender, RoutedEventArgs e)
        {
            menu.Background = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
        }

        private void AfterSelect()
        {
            switch (activeColumn)
            {
                case 0:
                    firstColumn.Opacity = 1;
                    secondColumn.Opacity = 0.2;
                    break;
                case 1:
                    ChangeActivePlayer();
                    firstColumn.Opacity = 0.2;
                    secondColumn.Opacity = 1;
                    newGame.IsEnabled = false;
                    exitGame.IsEnabled = false;
                    break;
                case 2:
                    firstColumn.Opacity = 0.2;
                    secondColumn.Opacity = 0.2;
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
                if (playerKeyBinding[activePlayer] < 3)
                {
                    playerKeyBinding[activePlayer]++;

                }
            }
            else
            {
                if (playerKeyBinding[activePlayer] > 0)
                {
                    playerKeyBinding[activePlayer]--;

                }
            }
            
            player0KeyBinding.Content = keyBinding[playerKeyBinding[0]];
            player1KeyBinding.Content = keyBinding[playerKeyBinding[1]];
            player2KeyBinding.Content = keyBinding[playerKeyBinding[2]];
        }

        private void menu_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (activeColumn == 1 && activePlayer > 0)
                    {
                        activePlayer--;
                        ChangeActivePlayer();
                    }
                    break;
                case Key.Down:
                    if (activeColumn == 1 && activePlayer < 2)
                    {
                        activePlayer++;
                        ChangeActivePlayer();
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
            }
        }
    }
}
