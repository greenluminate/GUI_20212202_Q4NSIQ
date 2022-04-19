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
        public List<Key> keys = new List<Key>();
        
        public GameController(IGameControl control)
        {
            this.control = control;
        }

        public async void KeyPressed(Key key)
        {
            //if (key == keys[0]) //Ki van kommentelve mert gettós, de amúgy ezzel működik (vagyis elindul ugyanezt be kéne rakni a KeyReleasedhez is)
            //{
            //    await control.StartMove(GameLogic.PlayerAction.up, 0);
            //}
            //else if (key == keys[1])
            //{
            //    await control.StartMove(GameLogic.PlayerAction.down, 0);
            //}
            //else if(key == keys[2])
            //{
            //    await control.StartMove(GameLogic.PlayerAction.left, 0);
            //}
            //else if(key == keys[3])
            //{
            //    await control.StartMove(GameLogic.PlayerAction.right, 0);
            //}
            switch (key)
            {
                case Key.Up:
                    await control.StartMove(GameLogic.PlayerAction.up, 0);
                    break;
                case Key.W:
                    await control.StartMove(GameLogic.PlayerAction.up, 1);
                    break;

                case Key.Down:
                    await control.StartMove(GameLogic.PlayerAction.down, 0);
                    break;
                case Key.S:
                    await control.StartMove(GameLogic.PlayerAction.down, 1);
                    break;

                case Key.Left:
                    await control.StartMove(GameLogic.PlayerAction.left, 0);
                    break;
                case Key.A:
                    await control.StartMove(GameLogic.PlayerAction.left, 1);
                    break;

                case Key.Right:
                    await control.StartMove(GameLogic.PlayerAction.right, 0);
                    break;
                case Key.D:
                    await control.StartMove(GameLogic.PlayerAction.right, 1);
                    break;
            }
        }

        public void KeyReleased(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    control.StopMove(GameLogic.PlayerAction.up, 0);
                    break;
                case Key.W:
                    control.StopMove(GameLogic.PlayerAction.up, 1);
                    break;

                case Key.Down:
                    control.StopMove(GameLogic.PlayerAction.down, 0);
                    break;
                case Key.S:
                    control.StopMove(GameLogic.PlayerAction.down, 1);
                    break;

                case Key.Left:
                    control.StopMove(GameLogic.PlayerAction.left, 0);
                    break;
                case Key.A:
                    control.StopMove(GameLogic.PlayerAction.left, 1);
                    break;

                case Key.Right:
                    control.StopMove(GameLogic.PlayerAction.right, 0);
                    break;
                case Key.D:
                    control.StopMove(GameLogic.PlayerAction.right, 1);
                    break;
            }
        }
    }
}
