using System.Collections.Generic;
using HotfixCore.Extensions;
using UnityEngine;

// namespace HotfixCore.Extensions
// {
	public class CellViewPool : ICellViewPool
	{
		private readonly Dictionary<int, Stack<GameObject>> _pool = new Dictionary<int, Stack<GameObject>>();
		private readonly Transform _root;
		public CellViewPool(Transform root)
		{
			_root = root;
		}

		public Dictionary<int, Stack<GameObject>> GetPoolGameObjects()
		{
			return _pool;
		}
		
		public GameObject RentCellView(GameObject template)
		{
			var rectTransform = template.GetComponent<RectTransform>();
			var id = rectTransform.GetInstanceID();
			if (_pool.TryGetValue(id, out Stack<GameObject> stack) && 0 < stack.Count)
				return stack.Pop();
			var cellGameObject = GameObject.Instantiate(rectTransform, _root, false);
			var cell = cellGameObject.gameObject.GetOrAddComponent<ScrollCellView>();
			cell.TemplateId = id;
			
			cell.RTransform.localPosition = Vector3.zero;
			cell.RTransform.localRotation = Quaternion.identity;
			cell.RTransform.localScale = Vector3.zero;
			cell.RTransform.gameObject.SetActive(true);
			
			return cell.gameObject;
		}
		
		public void ReturnCellView(GameObject cell)
		{
			var cellView = cell.GetComponent<ScrollCellView>();
			if (cellView == null)
			{
				return;
			}
			int id = cellView.TemplateId;
			if (!_pool.TryGetValue(id, out Stack<GameObject> stack))
			{
				stack = new Stack<GameObject>();
				_pool.Add(id, stack);
			}
			
			cellView.RTransform.localPosition = Vector3.zero;
			cellView.RTransform.localRotation = Quaternion.identity;
			cellView.RTransform.localScale = Vector3.zero;

			if (!stack.Contains(cell))
				stack.Push(cell);
		}
	}
// }