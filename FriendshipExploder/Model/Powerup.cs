using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public class Powerup : IElement
    {
        public ImageBrush Image { get; set; }
        public Point Position { get; set; }
        public bool Explode { get; set; }
        public ElementType ElementType { get; set; }
        public bool IsMoving { get; set; }
        public Point PositionPixel { get; set; }//Centerként kell megkapja
        public Powerup(ElementType ElementType)
        {
            this.Explode = false;
            this.ElementType = ElementType;
        }
    }
}
