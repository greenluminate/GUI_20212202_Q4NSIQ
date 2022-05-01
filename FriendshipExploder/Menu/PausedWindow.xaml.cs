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
    /// Interaction logic for PausedWindow.xaml
    /// </summary>
    public partial class PausedWindow : Window
    {
        public bool ActionResume { get; set; }
        public bool ActionMainMenu { get; set; }

        public PausedWindow()
        {
            InitializeComponent();
            int fontSize = (int)SystemParameters.PrimaryScreenWidth / 80;
            title_lb.FontSize = fontSize;
            resume_bt.FontSize = fontSize;
            exit_bt.FontSize = fontSize;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pausedWindow.Background = new ImageBrush(new BitmapImage(new Uri(System.IO.Path.Combine("..", "..", "..", "Images", "GameBackground", "pausedBackground.png"),UriKind.RelativeOrAbsolute)));
        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Resume(object sender, RoutedEventArgs e)
        {
            ActionResume = true;
            this.DialogResult = true;
        }

        private void MainMenu(object sender, RoutedEventArgs e)
        {
            ActionMainMenu = true;
            this.DialogResult = true;
        }
    }
}
