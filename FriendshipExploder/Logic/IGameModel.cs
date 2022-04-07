using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FriendshipExploder.Logic.GameLogic;

namespace FriendshipExploder.Logic
{
    public interface IGameModel
    {
        PlaygroundItem[,] GameMatrix { get; set; }
        InfobarItem[,] InfobarMatrix { get; set; }
    }
}
