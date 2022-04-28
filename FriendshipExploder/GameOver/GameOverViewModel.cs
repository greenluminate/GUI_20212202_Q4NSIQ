using FriendshipExploder.Logic;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FriendshipExploder.GameOver
{
    public class GameOverViewModel : ObservableRecipient
    {
        IGameModel logic;

        public Brush GameOverBackground { get; set; }

        public string FirstName { get; set; }
        public BitmapImage FirstImage { get; set; }
        public string FirstKill { get; set; }

        public string SecondName { get; set; }
        public BitmapImage SecondImage { get; set; }
        public string SecondKill { get; set; }

        public string ThirdName { get; set; }
        public BitmapImage ThirdImage { get; set; }
        public string ThirdKill { get; set; }


        public GameOverViewModel()
        {
            GameOverBackground = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));

            var players = logic.Players.OrderByDescending(pl => pl.Kills).ToList();

            if (players.Count > 0)
            {
                FirstName = $"Player {players[0].Id + 1}";
                FirstImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[0].Id}_player_down.png"));
                FirstKill = players[0].Kills.ToString();
            }

            if (players.Count > 1)
            {
                SecondName = $"Player {players[1].Id + 1}";
                SecondImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[1].Id}_player_down.png"));
                SecondKill = players[1].Kills.ToString();
            }

            if (players.Count > 2)
            {
                ThirdName = $"Player {players[2].Id + 1}";
                ThirdImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[2].Id}_player_down.png"));
                ThirdKill = players[2].Kills.ToString();
            }
        }

        public void SetupLogic(IGameModel model)
        {
            this.logic = model;
        }
    }
}
