using FriendshipExploder.Model;
using System.Collections.Generic;

namespace FriendshipExploder.Logic
{
    public interface IMainMenuModel
    {
        public List<IMenuElement> MenuElements { get; set; }
    }
}