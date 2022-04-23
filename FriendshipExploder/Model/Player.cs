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
        public Bomb Bomb { get; set; }//Ezt másolva teszi le a bombát billentyűkombinációtól függően változó típussal.
        public List<Bomb> BombList { get; set; }//Időzített robbantáskor innen kikeresve robban fel az összes időzítetten lerakott bomba.
        public int BombAmount { get; set; } //1-5 db bombája lehet a játékosnak a pályán egyszerre.
        public bool CanKick { get; set; } //Amikor a játékos megnyom egy adott gombot a bomba felé fordulva, előtte állva, a bomba addig halad, amíg akadályt nem ér.
        public bool CanSchedule { get; private set; } //Csak akkor tehet le gomb kombinációval időzített bombát, ha képes ilyenre.

        public Player(int id, Point position, KeyBinding keyBinding)
        {
            Position = position;
            Id = id;
            MovingHorizontal = false;
            MovingVertical = false;
            HeadDirection = PlayerDirection.right;
            KeyBinding = keyBinding;
            Bomb = new Bomb(this);
            BombList = new List<Bomb>();
            BombAmount = 1;
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
