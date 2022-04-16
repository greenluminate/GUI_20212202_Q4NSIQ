using FriendshipExploder.Controller;
using FriendshipExploder.Logic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FriendshipExploder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameController gameCtrontroller;
        MainMenuLogic mainMenuLogic;
        GameLogic gameLogic;
        
        public MainWindow()
        {
            InitializeComponent();
            this.mainMenuLogic = new MainMenuLogic();
            this.gameLogic = new GameLogic();
            this.gameCtrontroller = new GameController(gameLogic); //Esetle gegyet írni menüre.

            display.SetupModel(mainMenuLogic, gameLogic);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            display.Resize(new Size(grid.ActualWidth, grid.ActualHeight));
            display.InvalidateVisual();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            display.Resize(new Size(grid.ActualWidth, grid.ActualHeight));
            display.InvalidateVisual();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            gameCtrontroller.KeyPressed(e.Key);
            display.InvalidateVisual();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            gameCtrontroller.KeyReleased(e.Key);
            display.InvalidateVisual();
        }
    }
}
