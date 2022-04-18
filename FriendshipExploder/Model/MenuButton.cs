using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FriendshipExploder.Model
{
    internal class MenuButton : IMenuElement
    {
        public MenuButton(string command, bool isClicked, Point position, Brush brush)
        {
            Command = command;
            IsClicked = isClicked;
            Position = position;
            Brush = brush;
            
        }

        public string Command { get; set; }

        public bool IsClicked { get; set; }
        
        public Point Position { get; set; }
        public Brush Brush { get; set; }

        public int SizeX { get; set; }

        public int SizeY { get; set; }
    }
}
