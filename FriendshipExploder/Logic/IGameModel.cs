using FriendshipExploder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static FriendshipExploder.Logic.GameLogic;

namespace FriendshipExploder.Logic
{
    public interface IGameModel
    {
        List<IElement> Elements { get; set; }
        int[] PlayGroundSize { get; set; }
        public List<Player> Players { get; set; }
        public void SetupSize(Vector gameRectSize);
    }
}
