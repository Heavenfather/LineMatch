using System;
using System.Collections.Generic;

namespace HotfixLogic.Match
{
    public interface IValidate
    {
        void Validate(ElementDestroyContext context,List<GridItem> gridItems, Action<bool> callback);
    }
}