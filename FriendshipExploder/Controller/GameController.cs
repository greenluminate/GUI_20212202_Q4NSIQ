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

        public async void KeyPressed(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    await control.StartMove(GameLogic.PlayerAction.up);
                    break;
                case Key.W:
                    await control.StartMove(GameLogic.PlayerAction.W);
                    break;

                case Key.Down:
                    await control.StartMove(GameLogic.PlayerAction.down);
                    break;
                case Key.S:
                    await control.StartMove(GameLogic.PlayerAction.S);
                    break;

                case Key.Left:
                    await control.StartMove(GameLogic.PlayerAction.left);
                    break;
                case Key.A:
                    await control.StartMove(GameLogic.PlayerAction.A);
                    break;

                case Key.Right:
                    await control.StartMove(GameLogic.PlayerAction.right);
                    break;
                case Key.D:
                    await control.StartMove(GameLogic.PlayerAction.D);
                    break;

                //Bomba lerakása
                case Key.RightShift:
                    await control.StartAct(GameLogic.PlayerAction.bombudlr);
                    break;
                case Key.LeftShift:
                    await control.StartAct(GameLogic.PlayerAction.bombwasd);
                    break;

                //Action
                case Key.RightCtrl:
                    await control.StartAct(GameLogic.PlayerAction.actionudlr);
                    break;
                case Key.LeftCtrl:
                    await control.StartAct(GameLogic.PlayerAction.actionwasd);
                    break;

                //Játék megállítása
                case Key.Escape:
                    control.Pause();
                    break;
            }
        }

        public void KeyReleased(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    control.StopMove(GameLogic.PlayerAction.up);
                    break;
                case Key.W:
                    control.StopMove(GameLogic.PlayerAction.W);
                    break;

                case Key.Down:
                    control.StopMove(GameLogic.PlayerAction.down);
                    break;
                case Key.S:
                    control.StopMove(GameLogic.PlayerAction.S);
                    break;

                case Key.Left:
                    control.StopMove(GameLogic.PlayerAction.left);
                    break;
                case Key.A:
                    control.StopMove(GameLogic.PlayerAction.A);
                    break;

                case Key.Right:
                    control.StopMove(GameLogic.PlayerAction.right);
                    break;
                case Key.D:
                    control.StopMove(GameLogic.PlayerAction.D);
                    break;
            }
        }
    }
}
