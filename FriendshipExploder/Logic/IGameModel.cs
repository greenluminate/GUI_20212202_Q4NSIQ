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
        public IElement[,] Elements { get; set; }
        public IElement[,] Powerups { get; set; }
        int[] PlayGroundSize { get; set; }
        public List<Player> Players { get; set; }
        public void SetupSize(Point gameSize, int gameRectSize);
        public string Timer { get; set; }
        public object _ElementsListLockObject { get; set; }
        public object _PlayersListLockObject { get; set; }
        public double PlayerHeightRate { get; set; }
        public double PlayerHeightRateHangsIn { get; set; }
        public double PlayerWidthRate { get; set; }
        public bool GamePaused { get; set; }
        public void LoadPlayground(string file, int rounds);
        public bool RoundOver { get; set; }
        public bool RoundScore { get; set; }
    }
}
