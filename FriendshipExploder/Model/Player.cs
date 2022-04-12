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
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ImageBrush Image { get; set; }

        public Player(int id, int x, int y, ImageBrush image)
        {
            Id = id;
            X = x;
            Y = y;
            Image = image;
        }

    }
}
