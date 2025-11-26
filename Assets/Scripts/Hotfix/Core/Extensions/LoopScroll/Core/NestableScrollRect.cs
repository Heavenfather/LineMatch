using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// namespace HotfixCore.Extensions
// {
	public class NestableScrollRect : ScrollRect
	{
		private bool _routeToParent = false;

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			DoParentEventSystemHandler<IInitializePotentialDragHandler>((parent) =>
			{
				parent.OnInitializePotentialDrag(eventData);
			});
			
			DoEventSystemHandler<IInitializePotentialDragHandler>(transform, (parent) =>
				{
					if(parent as NestableScrollRect == this)
						base.OnInitializePotentialDrag(eventData);
					else
						parent.OnInitializePotentialDrag(eventData);
				});
		}

		public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (_routeToParent)
			{
				DoParentEventSystemHandler<IDragHandler>((parent) =>
				{
					parent.OnDrag(eventData);
				});
			}
			else
			{
				DoEventSystemHandler<IDragHandler>(transform, (parent) =>
				{
					if(parent as NestableScrollRect == this)
						base.OnDrag(eventData);
					else
						parent.OnDrag(eventData);
				});
			}
		}

		public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (!horizontal && Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
				_routeToParent = true;
			else if (!vertical && Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
				_routeToParent = true;
			else
				_routeToParent = false;

			if (_routeToParent)
			{
				DoParentEventSystemHandler<IBeginDragHandler>((parent) =>
				{
					parent.OnBeginDrag(eventData);
				});
			}
			else
			{
				DoEventSystemHandler<IBeginDragHandler>(transform, (parent) =>
					{
						if(parent as NestableScrollRect == this)
							base.OnBeginDrag(eventData);
						else
							parent.OnBeginDrag(eventData);
					});
			}
		}

		public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
		{
			if (_routeToParent)
			{
				DoParentEventSystemHandler<IEndDragHandler>((parent) =>
				{
					parent.OnEndDrag(eventData);
				});
			}
			else
			{
				DoEventSystemHandler<IEndDragHandler>(transform, (parent) =>
					{
						if(parent as NestableScrollRect == this)
							base.OnEndDrag(eventData);
						else
							parent.OnEndDrag(eventData);
					});
			}
			_routeToParent = false;
		}
		
		void DoParentEventSystemHandler<T>(Action<T> action) where T:IEventSystemHandler
		{
			Transform parent = transform.parent;
			while (parent != null)
			{
				DoEventSystemHandler<T>(parent, action);
				parent = parent.parent;
			}
		}
		
		void DoEventSystemHandler<T>(Transform self, Action<T> action) where T:IEventSystemHandler
		{
			List<Component> components = new List<Component>();
			self.GetComponents<Component>(components);
			foreach (Component c in components)
			{
				if (c is T)
				{
					action((T)(IEventSystemHandler)c);
				}
			}
		}
	}
// }