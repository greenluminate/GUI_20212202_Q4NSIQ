using FriendshipExploder.Logic;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FriendshipExploder.Menu
{
    public class MainMenuViewModel : ObservableRecipient
    {
        IGameModel logic;

        public Brush MenuBackground { get; set; }
        public List<string> Playgrounds { get; set; }
        public string SelectedPlayground { get; set; }
        public ICommand NewGameCommand { get; set; }
        public ICommand NextCommand { get; set; }

        private bool newGameEnabled;
        public bool NewGameEnabled
        {
            get { return newGameEnabled; }
            set { SetProperty(ref newGameEnabled, value); }
        }

        private bool exitEnabled;
        public bool ExitEnabled
        {
            get { return exitEnabled; }
            set { SetProperty(ref exitEnabled, value); }
        }

        private bool nextEnabled;
        public bool NextEnabled
        {
            get { return nextEnabled; }
            set { SetProperty(ref nextEnabled, value); }
        }

        private double firstColumnOpacity;
        public double FirstColumnOpacity
        {
            get { return firstColumnOpacity; }
            set { SetProperty(ref firstColumnOpacity, value); }
        }

        private double secondColumnOpacity;
        public double SecondColumnOpacity
        {
            get { return secondColumnOpacity; }
            set { SetProperty(ref secondColumnOpacity, value); }
        }

        private double thirdColumnOpacity;
        public double ThirdColumnOpacity
        {
            get { return thirdColumnOpacity; }
            set { SetProperty(ref thirdColumnOpacity, value); }
        }

        public List<string> Players { get; set; }
        public string SelectedPlayer { get; set; }
        public List<string> KeyBindings { get; set; }


        public MainMenuViewModel()
        {
            MenuBackground = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
            LoadPlaygrounds();
            NewGameEnabled = true;
            ExitEnabled = true;
            nextEnabled = false;
            FirstColumnOpacity = 1;
            SecondColumnOpacity = 0.2;
            ThirdColumnOpacity = 0.2;

            Players = new List<string>();
            Players.Add("Player 1");
            Players.Add("Player 2");
            Players.Add("Player 3");

            KeyBindings = new List<string>();
            KeyBindings.Add("[Disabled]");
            KeyBindings.Add("[Up][Down][Left][Right]");
            KeyBindings.Add("[W][S][A][D]");
            KeyBindings.Add("[Ai]");

            NewGameCommand = new RelayCommand(() =>
                {
                    FirstColumnOpacity = 0.2;
                    SecondColumnOpacity = 1;
                    NewGameEnabled = false;
                    ExitEnabled = false;
                    NextEnabled = true;
                });

            NextCommand = new RelayCommand(() =>
            {
                SecondColumnOpacity = 0.2;
                ThirdColumnOpacity= 1;
                NextEnabled = false;
            });
        }

        public void SetupLogic(IGameModel model)
        {
            this.logic = model;
        }

        //Pályák betöltése fájlból
        private void LoadPlaygrounds()
        {
            Playgrounds = new List<string>();
            string[] files = Directory.GetFiles("Playgrounds", "*.txt");
            foreach (var playground in files)
            {
                Playgrounds.Add(Path.GetFileNameWithoutExtension(@"Playgrounds\" + playground));
            }
        }

    }
}
