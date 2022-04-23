﻿using System;
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
        public ImageBrush Image { get; set; }
        public BombType Type { get; set; }
        public Player Player { get; private set; }
        //public bool IsScheduled { get; set; }//Akkor robban fel, ha lenyom egy adott gombot a játékos.
        public bool IsBouncey { get; set; }//Akadályt érve elindul az ellenkező irányba, amíg fel nem robban. Permanens tulajdonság.
        public int ExplosionRange { get; set; }//1-7 tile long.

        public Bomb(Player player, BombType type = BombType.Normal)
        {
            this.Type = type;
            this.Player = player;
        }

        public Bomb BombCopy(Point position, ImageBrush image, BombType type = BombType.Normal)
        {
            Bomb newBomb = new Bomb(this.Player, type);
            newBomb.IsBouncey = this.IsBouncey;
            newBomb.ExplosionRange = this.ExplosionRange;
            newBomb.Position = position;
            newBomb.Image = image;

            return newBomb;

        }

        public override bool Equals(object obj)
        {
            return this.Position.X == (obj as Bomb).Position.X && this.Position.Y == (obj as Bomb).Position.Y;
        }
    }
}