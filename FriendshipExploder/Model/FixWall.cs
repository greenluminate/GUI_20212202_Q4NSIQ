﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public class FixWall : IElement
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public ImageBrush Image { get; set; }
        public bool Explode { get; set; }
        public int gCost { get; set; }
        public int hCost { get; set; }
        public IElement Parent { get; set; }

        public FixWall(Point position, ImageBrush image)
        {
            Position = position;
            Image = image;
            Explode = false;
        }
    }
}
