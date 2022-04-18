using FriendshipExploder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FriendshipExploder.Logic.GameLogic;

namespace FriendshipExploder.Logic
{
    public interface IGameControl
    { 
        public Task StartMove(PlayerAction playerAction);
        public void StopMove(PlayerAction playerAction);
    }
}
