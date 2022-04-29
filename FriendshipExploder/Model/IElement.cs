using System.Drawing;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public interface IElement
    {
        ImageBrush Image { get; set; }
        public Point Position { get; set; }
        public bool Explode { get; set; }

        
        //Útkereséshez kell, súlyokat tárolni
        public int gCost { get; set; }
        public int hCost { get; set; }  

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
        public IElement Parent { get; set; }
    }
}