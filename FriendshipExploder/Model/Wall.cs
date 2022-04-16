using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public class Wall : IElement
    {
        public Point Position { get; set; }
        public ImageBrush Image { get; set; }

        public Wall(Point position, ImageBrush image)
        {
            Position = position;
            Image = image;
        }

    }
}
