using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FriendshipExploder.Model
{
    public enum PlayerDirection
    {
        up, down, left, right
    }

    public enum KeyBinding
    {
        upDownLeftRight, WSAD, ai
    }

    public class Player
    {
        public int Id { get; set; }
        public Point Position { get; set; }
        public bool MovingHorizontal { get; set; }
        public bool MovingVertical { get; set; }
        public int Speed { get; set; }
        public PlayerDirection HeadDirection { get; set; }
        public KeyBinding KeyBinding { get; set; }

        public Player(int id, Point position, KeyBinding keyBinding)
        {
            Position = position;
            Id = id;
            MovingHorizontal = false;
            MovingVertical = false;
            HeadDirection = PlayerDirection.right;
            KeyBinding = keyBinding;
        }

        public void Move(int x, int y)
        {
            Position = new Point(Position.X + x, Position.Y + y);
        }

        public void SetPos(int x, int y)
        {
            Position = new Point(x, y);
        }

    }
}
