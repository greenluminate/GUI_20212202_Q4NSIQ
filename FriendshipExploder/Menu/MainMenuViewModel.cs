using FriendshipExploder.Logic;
using FriendshipExploder.Model;
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

        private string selectedPlayground;
        public string SelectedPlayground { 
            get { return selectedPlayground; }
            set {
                SetProperty(ref selectedPlayground, value);
                (StartCommand as RelayCommand).NotifyCanExecuteChanged();
                UpdatePreviewImage();
            } 
        }
        private bool playgroundsEnabled;
        public bool PlaygroundsEnabled
        {
            get { return playgroundsEnabled; }
            set { SetProperty(ref playgroundsEnabled, value); }
        }

        private int buttonFontSize;
        public int ButtonFontSize
        {
            get { return buttonFontSize; }
            set { SetProperty(ref buttonFontSize, value); }
        }

        private double labelFontSize;
        public double LabelFontSize
        {
            get { return labelFontSize; }
            set { SetProperty(ref labelFontSize, value); }
        }


        public ICommand NewGameCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand StartCommand { get; set; }

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

        public List<string> KeyBindings { get; set; }
        private string player1SelectedKeyBinding;
        public string Player1SelectedKeyBinding
        {
            get { return player1SelectedKeyBinding; }
            set {
                SetProperty(ref player1SelectedKeyBinding, value);
                (NextCommand as RelayCommand).NotifyCanExecuteChanged();
            }
        }

        private string player2SelectedKeyBinding;
        public string Player2SelectedKeyBinding
        {
            get { return player2SelectedKeyBinding; }
            set {
                SetProperty(ref player2SelectedKeyBinding, value);
                (NextCommand as RelayCommand).NotifyCanExecuteChanged();
            }
        }

        private string player3SelectedKeyBinding;
        public string Player3SelectedKeyBinding
        {
            get { return player3SelectedKeyBinding; }
            set { 
                SetProperty(ref player3SelectedKeyBinding, value);
                (NextCommand as RelayCommand).NotifyCanExecuteChanged();
            }
        }

        private bool player1KeyBindingEnabled;
        public bool Player1KeyBindingEnabled
        {
            get { return player1KeyBindingEnabled; }
            set { SetProperty(ref player1KeyBindingEnabled, value); }
        }
        private bool player2KeyBindingEnabled;
        public bool Player2KeyBindingEnabled
        {
            get { return player2KeyBindingEnabled; }
            set { SetProperty(ref player2KeyBindingEnabled, value); }
        }
        private bool player3KeyBindingEnabled;
        public bool Player3KeyBindingEnabled
        {
            get { return player3KeyBindingEnabled; }
            set { SetProperty(ref player3KeyBindingEnabled, value); }
        }

        public string RoundsSelected { get; set; }
        private bool roundsEnabled;
        public bool RoundsEnabled
        {
            get { return roundsEnabled; }
            set { SetProperty(ref roundsEnabled, value); }
        }

        private bool startEnabled;
        public bool StartEnabled
        {
            get { return startEnabled; }
            set { SetProperty(ref startEnabled, value); }
        }


        private string playgroundImage;
        public string PlaygroundImage
        {
            get { return playgroundImage; }
            set { SetProperty(ref playgroundImage, value); }
        }


        public MainMenuViewModel()
        {
            ButtonFontSize = (int)System.Windows.SystemParameters.PrimaryScreenWidth / 80;
            LabelFontSize = (int)System.Windows.SystemParameters.PrimaryScreenWidth / 96;
            MenuBackground = new ImageBrush(new BitmapImage(new Uri($"pack://application:,,,/Images/GameBackground/0_GameBackground.jpg")));
            LoadPlaygrounds();
            NewGameEnabled = true;
            ExitEnabled = true;
            nextEnabled = false;
            FirstColumnOpacity = 1;
            SecondColumnOpacity = 0.2;
            ThirdColumnOpacity = 0.2;
            Player1KeyBindingEnabled = false;
            Player2KeyBindingEnabled = false;
            Player3KeyBindingEnabled = false;
            RoundsEnabled = false;
            RoundsSelected = "0";
            PlaygroundsEnabled = false;
            StartEnabled = false;

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
                    Player1KeyBindingEnabled = true;
                    Player2KeyBindingEnabled = true;
                    Player3KeyBindingEnabled = true;
                });

            NextCommand = new RelayCommand(() =>
            {
                Player1KeyBindingEnabled = false;
                Player2KeyBindingEnabled = false;
                Player3KeyBindingEnabled = false;
                SecondColumnOpacity = 0.2;
                ThirdColumnOpacity= 1;
                NextEnabled = false;
                RoundsEnabled = true;
                PlaygroundsEnabled = true;
            }, () => CheckNextCanExecute());

            StartCommand = new RelayCommand(() =>
            {
                BuildPlayers();
                logic.LoadPlayground(SelectedPlayground, int.Parse(RoundsSelected) + 1);
            }, () => SelectedPlayground != null);
        }

        private bool CheckNextCanExecute()
        {
            bool canExec = true;

            if (Player1SelectedKeyBinding == KeyBindings[0] && Player2SelectedKeyBinding == KeyBindings[0] && Player3SelectedKeyBinding == KeyBindings[0])
            {
                //minden player Disabled
                canExec = false;
            }
            else
            {
                if ((Player1SelectedKeyBinding == KeyBindings[0] && Player2SelectedKeyBinding == KeyBindings[0]) || 
                    (Player1SelectedKeyBinding == KeyBindings[0] && Player3SelectedKeyBinding == KeyBindings[0]) || 
                    (Player2SelectedKeyBinding == KeyBindings[0] && Player3SelectedKeyBinding == KeyBindings[0]) ||
                    (Player1SelectedKeyBinding == KeyBindings[3] && Player2SelectedKeyBinding == KeyBindings[0] && Player3SelectedKeyBinding == KeyBindings[0]) ||
                    (Player1SelectedKeyBinding == KeyBindings[0] && Player2SelectedKeyBinding == KeyBindings[3] && Player3SelectedKeyBinding == KeyBindings[0]) ||
                    (Player1SelectedKeyBinding == KeyBindings[0] && Player2SelectedKeyBinding == KeyBindings[0] && Player3SelectedKeyBinding == KeyBindings[3]))
                {
                    //csak 1 player van
                    canExec = false;
                }
                else
                {
                    if ((Player1SelectedKeyBinding != KeyBindings[0]) && (Player1SelectedKeyBinding != KeyBindings[3]))
                    {
                        if (Player1SelectedKeyBinding == Player2SelectedKeyBinding || Player1SelectedKeyBinding == player3SelectedKeyBinding)
                        {
                            //ugyan az
                            canExec = false;
                        }
                    }
                    else
                    {
                        //Disabled vagy Ai
                        if ((Player2SelectedKeyBinding != KeyBindings[0]) && (Player2SelectedKeyBinding != KeyBindings[3]))
                        {
                            if (Player2SelectedKeyBinding == Player3SelectedKeyBinding)
                            {
                                //ugyan az
                                canExec = false;
                            }
                        }
                    }
                }
            }
            
            return canExec;
        }

        private void BuildPlayers()
        {
            string[] selectedKeys = { Player1SelectedKeyBinding, Player2SelectedKeyBinding , Player3SelectedKeyBinding };

            for (int i = 0; i < selectedKeys.Length; i++)
            {
                string pl = selectedKeys[i];
                if (pl != "[Disabled]")
                {
                    int bindingNum = KeyBindings.IndexOf(pl);
                    logic.Players.Add(
                                    new Player(i,
                                    new System.Drawing.Point(2, i * 120),
                                    (Model.KeyBinding)bindingNum - 1)
                                    );
                }
            }
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

        private void UpdatePreviewImage()
        {
            PlaygroundImage = $"../Playgrounds/Preview/{SelectedPlayground}.jpg";
        }
    }
}
