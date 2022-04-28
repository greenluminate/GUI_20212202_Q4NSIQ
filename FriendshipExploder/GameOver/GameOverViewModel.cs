using FriendshipExploder.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipExploder.GameOver
{
    public class GameOverViewModel
    {
        IGameModel logic;

        public GameOverViewModel()
        {

        }

        public void SetupLogic(IGameModel model)
        {
            this.logic = model;
        }
    }
}
