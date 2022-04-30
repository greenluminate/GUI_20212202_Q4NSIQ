using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FriendshipExploder.Model
{

    public class Node
    {
        public Node(string type, bool walkable, Point position)
        {
            Type = type;
            Walkable = walkable;
            Position = position;
        }

        public string Type { get; set; }
        public bool Walkable { get; set; }

        public int gCost { get; set; }
        public int hCost { get; set; }

        public int fCost
        {
            get
            {
                return gCost + hCost;
            }
        }
        public Node Parent { get; set; }

        public Point Position { get; set; }


    }
}
