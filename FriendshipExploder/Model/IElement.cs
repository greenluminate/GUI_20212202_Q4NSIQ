using System.Drawing;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public interface IElement
    {
        ImageBrush Image { get; set; }
        public Point Position { get; set; }
    }
}