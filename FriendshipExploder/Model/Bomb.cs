using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public enum BombType { Normal, Scheduled }
    public class Bomb : IElement
    {
        public Point Position { get; set; }
        public Point PositionPixel { get; set; }//Centerként kell megkapja
        public ImageBrush Image { get; set; }
        public BombType Type { get; set; }
        public Player Player { get; private set; }
        //public bool IsScheduled { get; set; }//Akkor robban fel, ha lenyom egy adott gombot a játékos.
        public bool IsBouncey { get; set; }//Akadályt érve elindul az ellenkező irányba, amíg fel nem robban. Permanens tulajdonság.
        public int ExplosionRange { get; set; }//1-7 tile long.
        public bool Explode { get; set; }
        public ElementType ElementType { get; set; }

        public Bomb(Player player, ElementType ElementType, BombType type = BombType.Normal)
        {
            this.Type = type;
            this.Player = player;
            this.ExplosionRange = 3;
            Explode = false;
            this.ElementType = ElementType;
            PositionPixel = new Point();
        }

        public Bomb BombCopy(Point position, BombType type = BombType.Normal, Point point = new Point())
        {
            Bomb newBomb = new Bomb(this.Player, this.ElementType, type);
            newBomb.IsBouncey = this.IsBouncey;
            newBomb.ExplosionRange = this.ExplosionRange;
            newBomb.Position = position;
            PositionPixel = point;
            return newBomb;
        }

        public override bool Equals(object obj)
        {
            Bomb bomb = (obj as Bomb);
            return this.Position.X == bomb.Position.X && this.Position.Y == bomb.Position.Y && this.Player == bomb.Player;
        }
    }
}
