using System.Drawing;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public enum ElementType
    {
        Bomb, ScheduledBomb, FixWall, Floor, Player, Wall,
        Kick, Jelly, Desease, BomUp, BlastUp, SpeedUp, SpeedDown, Schedule,//Powerups
        Teleport, TravelatorRight, TravelatorLeft, TravelatorUp, TravelatorDown//Active grounds
    }

    public interface IElement
    {
        ImageBrush Image { get; set; }
        public Point Position { get; set; }
        public Point PositionPixel { get; set; }//Centerként kell megkapja
        public bool Explode { get; set; }
        public ElementType ElementType { get; set; }
        public bool IsMoving { get; set; }
    }
}