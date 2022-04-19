﻿using FriendshipExploder.Controller;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FriendshipExploder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GameController gameCtrontroller;
        MainMenuLogic mainMenuLogic;
        GameLogic gameLogic;

        public MainWindow(GameController gameController, GameLogic gameLogic)
        {
            InitializeComponent();
            this.mainMenuLogic = new MainMenuLogic();
            this.gameLogic = gameLogic;
            this.gameCtrontroller = gameController; //Esetle gegyet írni menüre.
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();

            display.SetupModel(mainMenuLogic, gameLogic);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            display.Resize(new Size(grid.ActualWidth, grid.ActualHeight));
            display.InvalidateVisual();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            display.Resize(new Size(grid.ActualWidth, grid.ActualHeight));
            display.InvalidateVisual();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            gameCtrontroller.KeyPressed(e.Key);
            display.InvalidateVisual();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            gameCtrontroller.KeyReleased(e.Key);
            display.InvalidateVisual();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            display.InvalidateVisual();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void display_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source.ToString() == "FriendshipExploder.Renderer.Display")
            {
                int x = (int)e.GetPosition(grid).X; //Egér pozíció lekérése & eltárolása
                int y = (int)e.GetPosition(grid).Y;

                for (int i = 0; i < mainMenuLogic.MenuElements.Count; i++)
                {
                    if (x <= (mainMenuLogic.MenuElements[i].Position.X + mainMenuLogic.MenuElements[i].SizeX) && y <= (mainMenuLogic.MenuElements[i].Position.Y + mainMenuLogic.MenuElements[i].SizeY)) //Kiszámolja, hogy az egér pozíciója a kirajzolt gomb keretein belül van-e 
                    {
                        mainMenuLogic.MenuElements[i].IsClicked = true;
                        mainMenuLogic.MenuElements[i].Command.ToLower();
                    }
                    
                }
            }
        }
    }
}
