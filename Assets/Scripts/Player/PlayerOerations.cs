using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Player
{
    public enum PlayerOperation
    {
        None, // 用于表示没有操作
        All, // 用于表示所有操作
        Run,
        Sprint,
        Jump,
        Slide,
        MoveLeft,
        MoveRight,
    }
}
