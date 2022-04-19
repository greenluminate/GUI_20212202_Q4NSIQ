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
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        GameController gameController;
        GameLogic gameLogic;
        int counter = 0; //Számolja, hogy hol tart, így könnyen tudhatjuk, hogy a lista mely eleme melyik iránynak felelhet meg (fel:0, le:1 stb)
        public OptionsWindow(GameController gameController, GameLogic gameLogic)
        {
            InitializeComponent();
            this.gameController = gameController;
            this.gameLogic = gameLogic;
            this.PreviewKeyDown += new KeyEventHandler(HandleKey);

        }

        private void HandleKey(object sender, KeyEventArgs e) 
        {
            Key pressed = e.Key;
            switch (counter)
            {
                case 0:
                    instructionsText.Text = "Le";
                    gameController.keys.Add(pressed);
                    counter++;
                    break;
                case 1:
                    instructionsText.Text = "Bal";
                    gameController.keys.Add(pressed);
                    counter++;
                    break;
                case 2:
                    instructionsText.Text = "Jobb";
                    gameController.keys.Add(pressed);
                    counter++;
                    break;
                case 3:
                    gameController.keys.Add(pressed);
                    this.Close();
                    break;
            }
        }
    }
}
