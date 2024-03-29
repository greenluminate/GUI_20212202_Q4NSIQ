﻿using FriendshipExploder.Model;
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
        public Task StartMove(PlayerAction playerAction, Player ai = null);
        public Task StartAct(PlayerAction playerAction, Player ai = null);
        public void StopMove(PlayerAction playerAction, Player ai = null);
        public void Pause();
    }
}
