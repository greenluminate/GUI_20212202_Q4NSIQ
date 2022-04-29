using System.Drawing;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public enum ElementType
    {
        Bomb, FixWall, Floor, Player, Wall,
        Kick, Jelly, Desease, BomUp, BlastUp, SpeedUp, SpeedDown, Schedule,//Powerups
        Teleport, TravelatorRight, TravelatorLeft, TravelatorUp, TravelatorDown//Active grounds
    }

    public interface IElement
    {
        ImageBrush Image { get; set; }
        public Point Position { get; set; }
        public bool Explode { get; set; }
        public ElementType ElementType { get; set; }
    }
}