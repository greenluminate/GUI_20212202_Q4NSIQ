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
using System.Windows.Shapes;

namespace FriendshipExploder
{
    /// <summary>
    /// Interaction logic for MenuWindows.xaml
    /// </summary>
    public partial class MenuWindows : Window
    {
        GameController gameController; //Ezeket a példányokat kell átpasszolni a többi ablakhoz, különben az irányítás beállítása nem működik!!
        GameLogic gameLogic;
        public MenuWindows()
        {
            InitializeComponent();
            this.gameLogic = new GameLogic();
            this.gameController = new GameController(gameLogic);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //BeforePlay beforePlay = new BeforePlay();
            //this.Close();
            //beforePlay.Show();
            MainWindow mainWindow = new MainWindow(gameController, gameLogic);
            mainWindow.Show();
            this.Close();

        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow options = new OptionsWindow(gameController, gameLogic);
            options.Show();
        }
    }
}
