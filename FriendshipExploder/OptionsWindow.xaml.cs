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
        List<Key> keys = new List<Key>(); //Key elemek kigyűjtése egy listába, később valahogyan átadni a logicnak
        int counter = 0; //Számolja, hogy hol tart, így könnyen tudhatjuk, hogy a lista mely eleme melyik iránynak felelhet meg (fel:0, le:1 stb)
        public OptionsWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += new KeyEventHandler(HandleKey);

        }

        private void HandleKey(object sender, KeyEventArgs e) 
        {
            Key pressed = e.Key;
            switch (counter)
            {
                case 0:
                    instructionsText.Text = "Le";
                    keys.Add(pressed);
                    counter++;
                    break;
                case 1:
                    instructionsText.Text = "Bal";
                    keys.Add(pressed);
                    counter++;
                    break;
                case 2:
                    instructionsText.Text = "Jobb";
                    keys.Add(pressed);
                    counter++;
                    break;
                case 3:
                    this.Close();
                    break;
            }
        }
    }
}
