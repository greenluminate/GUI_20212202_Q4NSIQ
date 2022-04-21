using FriendshipExploder.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FriendshipExploder.Logic.GameLogic;

namespace FriendshipExploder.Logic
{
    public interface IGameModel
    {
        List<IElement> Elements { get; set; }
        int[] PlayGroundSize { get; set; }
        public List<Player> Players { get; set; }
        public void SetupSize(Point gameSize, int gameRectSize);
        public string Timer { get; set; }
    }
}
