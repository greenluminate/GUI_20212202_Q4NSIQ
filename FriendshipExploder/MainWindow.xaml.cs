using FriendshipExploder.Controller;
using FriendshipExploder.Logic;
using FriendshipExploder.Menu;
using FriendshipExploder.GameOver;
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
using System.Windows.Threading;

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

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();

            display.SetupModel(mainMenuLogic, gameLogic);

            MainMenu menu = new MainMenu(gameLogic);
            if (menu.ShowDialog() == false)
            {
                Application.Current.Shutdown();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Size size = new Size(grid.ActualWidth, grid.ActualHeight);
            display.Resize(size);
            gameLogic.SetupSize(new System.Drawing.Point((int)size.Width, (int)(size.Height - size.Height * 0.05)));
            display.InvalidateVisual();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Size size = new Size(grid.ActualWidth, grid.ActualHeight);
            display.Resize(size);
            gameLogic.SetupSize(new System.Drawing.Point((int)size.Width, (int)(size.Height - size.Height * 0.05)));
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

        private void Timer_Tick(object sender, EventArgs e)
        {
            //ToDo: Monitorozni, hogy csak akkor fusson le, ha nincs másik ablak megnyitva, különben várjon.
            display.InvalidateVisual();
        }
    }
}
