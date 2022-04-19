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
        public MenuWindows()
        {
            InitializeComponent();
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //BeforePlay beforePlay = new BeforePlay();
            //this.Close();
            //beforePlay.Show();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();

        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            OptionsWindow options = new OptionsWindow();
            options.Show();
        }
    }
}
