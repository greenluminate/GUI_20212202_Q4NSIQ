using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public interface IElement
    {
        int Id { get; set; }
        ImageBrush Image { get; set; }
        int PosX { get; set; }
        int PosY { get; set; }
    }
}