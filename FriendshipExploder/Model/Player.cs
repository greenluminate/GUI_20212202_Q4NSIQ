using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public class Player
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ImageBrush Image { get; set; }
        public bool Moving { get; set; }
        public int Speed { get; set; }

        public Player(int x, int y, ImageBrush image)
        {
            X = x;
            Y = y;
            Image = image;
            Moving = false;
        }

    }
}
