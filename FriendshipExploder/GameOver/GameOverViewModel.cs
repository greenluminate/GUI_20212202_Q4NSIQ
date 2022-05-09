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

        private Brush gameOverBackground;
        public Brush GameOverBackground
        {
            get { return gameOverBackground; }
            set { SetProperty(ref gameOverBackground, value); }
        }


        private string firstName;
        public string FirstName
        {
            get { return firstName; }
            set { SetProperty(ref firstName, value); }
        }

        private BitmapImage firstImage;
        public BitmapImage FirstImage
        {
            get { return firstImage; }
            set { SetProperty(ref firstImage, value); }
        }

        private string firstKill;
        public string FirstKill
        {
            get { return firstKill; }
            set { SetProperty(ref firstKill, value); }
        }


        private string secondName;
        public string SecondName
        {
            get { return secondName; }
            set { SetProperty(ref secondName, value); }
        }

        private BitmapImage secondImage;
        public BitmapImage SecondImage
        {
            get { return secondImage; }
            set { SetProperty(ref secondImage, value); }
        }

        private string secondKill;
        public string SecondKill
        {
            get { return secondKill; }
            set { SetProperty(ref secondKill, value); }
        }


        private string thirdName;
        public string ThirdName
        {
            get { return thirdName; }
            set { SetProperty(ref thirdName, value); }
        }

        private BitmapImage thirdImage;
        public BitmapImage ThirdImage
        {
            get { return thirdImage; }
            set { SetProperty(ref thirdImage, value); }
        }

        private string thirdKill;
        public string ThirdKill
        {
            get { return thirdKill; }
            set { SetProperty(ref thirdKill, value); }
        }


        private int fontSize;
        public int FontSize
        {
            get { return fontSize; }
            set { SetProperty(ref fontSize, value); }
        }

        private double placeFontSize;
        public double PlaceFontSize
        {
            get { return placeFontSize; }
            set { SetProperty(ref placeFontSize, value); }
        }


        public GameOverViewModel()
        {
            FontSize = (int)System.Windows.SystemParameters.PrimaryScreenWidth / 80;
            PlaceFontSize = (int)System.Windows.SystemParameters.PrimaryScreenWidth / 32;
            GameOverBackground = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
        }

        private void LoadScore()
        {
            var players = logic.PlayersForScore.OrderByDescending(pl => pl.SumOfKills).ToList();
            
            if (players.Count > 0)
            {
                FirstName = $"Player {players[0].Id + 1}";
                FirstImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[0].Id}_player_down.png"));
                FirstKill = players[0].SumOfKills.ToString();
            }

            if (players.Count > 1)
            {
                SecondName = $"Player {players[1].Id + 1}";
                SecondImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[1].Id}_player_down.png"));
                SecondKill = players[1].SumOfKills.ToString();
            }

            if (players.Count > 2)
            {
                ThirdName = $"Player {players[2].Id + 1}";
                ThirdImage = new BitmapImage(new Uri($"pack://application:,,,/Images/Players/{players[2].Id}_player_down.png"));
                ThirdKill = players[2].SumOfKills.ToString();
            }
        }

        public void SetupLogic(IGameModel model)
        {
            this.logic = model;
            LoadScore();
        }
    }
}
