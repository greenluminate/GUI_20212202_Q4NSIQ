using FriendshipExploder.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FriendshipExploder.GameOver
{
    public class GameOverViewModel
    {
        IGameModel logic;

        public Brush GameOverBackground { get; set; }
        public BitmapImage FirstImage { get; set; }
        public BitmapImage SecondImage { get; set; }
        public BitmapImage ThirdImage { get; set; }

        public GameOverViewModel()
        {
            GameOverBackground = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
            FirstImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/0_player_down.png"));
            SecondImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/2_player_down.png"));
            ThirdImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/1_player_down.png"));
        }

        public void SetupLogic(IGameModel model)
        {
            this.logic = model;
        }
    }
}
