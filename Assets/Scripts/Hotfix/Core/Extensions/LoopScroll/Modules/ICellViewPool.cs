using UnityEngine;

// namespace HotfixCore.Extensions
// {
    public interface ICellViewPool
    {
        GameObject RentCellView(GameObject template);
		
        void ReturnCellView(GameObject cell);
    }
// }