using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FriendshipExploder.Logic;

namespace FriendshipExploder.Logic
{
    public interface IGameControl
    {
        void Act(GameLogic.PlayerAction playerAction);
    }
}
