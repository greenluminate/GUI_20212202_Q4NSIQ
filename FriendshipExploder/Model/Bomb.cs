using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipExploder.Model
{
    public enum BombType { Normal, Scheduled }
    public class Bomb
    {
        public BombType Type { get; set; }
        public Player Player { get; private set; }
        //public bool IsScheduled { get; set; }//Akkor robban fel, ha lenyom egy adott gombot a játékos.
        public bool IsBouncey { get; set; }//Akadályt érve elindul az ellenkező irányba, amíg fel nem robban. Permanens tulajdonság.
        public int ExplosionRange { get; set; }//1-7 tile long.

        public Bomb(Player player)
        {
            this.Player = player;
        }
    }
}
