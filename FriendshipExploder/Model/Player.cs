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
        public bool CanSchedule { get; set; } //Csak akkor tehet le gomb kombinációval időzített bombát, ha képes ilyenre.
        public int Kills { get; set; }
        public int SumOfKills { get; set; }

        public object _bombListLockObject;

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
            _bombListLockObject = new object();
            BombAmount = 1;
            Kills = 0;//Lehet azelőző körös killszámot át kéne adni, de nem feltélen, sőt külön fájlba is menthetnénk a körök között az addigi killeket, vagy mindegy hová.
            SumOfKills = 0;
        }

        public void SetupKeyBinding(int binding)
        {
            switch (binding)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }
        }

        public void Move(int x, int y)
        {
            Position = new Point(Position.X + x, Position.Y + y);
        }

        public void SetPos(int x, int y)
        {
            Position = new Point(x, y);
        }

        public void ResetPlayer()
        {
            SumOfKills += Kills;
            Kills = 0;

            Position = new System.Drawing.Point(2, Id * 120);
            MovingHorizontal = false;
            MovingVertical = false;
            HeadDirection = PlayerDirection.right;
            Bomb = new Bomb(this);
            BombList = new List<Bomb>();
            _bombListLockObject = new object();
            BombAmount = 1;
        }

    }
}
