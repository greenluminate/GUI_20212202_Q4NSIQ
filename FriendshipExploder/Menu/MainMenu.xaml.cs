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
            InitializeComponent();
            (this.DataContext as MainMenuViewModel).SetupLogic(model);
        }

        //Kilépés gomb
        private void Exit(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
