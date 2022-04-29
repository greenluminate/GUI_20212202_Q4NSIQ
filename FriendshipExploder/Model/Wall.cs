﻿using System;
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
        public bool Explode { get; set; }
        public ElementType ElementType { get; set; }

        public Wall(Point position, ElementType ElementType)
        {
            Position = position;
            Explode = false;
            this.ElementType = ElementType;
        }
    }
}
