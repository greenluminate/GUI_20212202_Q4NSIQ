using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public class Wall : IElement
    {
        public int Id { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public ImageBrush Image { get; set; }

        public Wall(int id, int posX, int posY, ImageBrush image)
        {
            Id = id;
            PosX = posX;
            PosY = posY;
            Image = image;
        }

    }
}
