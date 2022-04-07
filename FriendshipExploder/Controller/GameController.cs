using FriendshipExploder.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendshipExploder.Controller
{
    public class GameController
    {
        IGameControl control;

        public GameController(IGameControl control)
        {
            this.control = control;
        }

        public void KeyPressed(Key key)//async await, hogy egyszerre több gomb is nyomható legyen.
        {
            switch (key)
            {
                case Key.Up:
                    control.Act(GameLogic.PlayerAction.up);
                    break;
                case Key.W:
                    control.Act(GameLogic.PlayerAction.up);
                    break;

                case Key.Down:
                    control.Act(GameLogic.PlayerAction.down);
                    break;
                case Key.S:
                    control.Act(GameLogic.PlayerAction.down);
                    break;

                case Key.Left:
                    control.Act(GameLogic.PlayerAction.left);
                    break;
                case Key.A:
                    control.Act(GameLogic.PlayerAction.left);
                    break;

                case Key.Right:
                    control.Act(GameLogic.PlayerAction.right);
                    break;
                case Key.D:
                    control.Act(GameLogic.PlayerAction.right);
                    break;

                default:
                    break;
            }
        }
    }
}
