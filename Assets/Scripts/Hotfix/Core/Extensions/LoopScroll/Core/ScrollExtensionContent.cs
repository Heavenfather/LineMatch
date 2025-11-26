using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// namespace HotfixCore.Extensions
// {
    /// <summary>
    /// Abstract component responsible for setting visual elements in scroll content game object. All changes should
    /// be done by the <see cref="ScrollRectExtension"/> component. Depending on which direction scroll you choose
    /// it has <see cref="ScrollVerticalContent"/> or <seealso cref="ScrollHorizontalContent"/> inheritors
    /// which one of them should be used.
    /// </summary>
    public abstract class ScrollExtensionContent : MonoBehaviour
    {
        public bool IsRevert { get; set; }
        
        /// <summary>
        /// Rect transform component of content.
        /// </summary>
        public RectTransform RTransform => transform as RectTransform;
        public GameObject First => _contentObjects.First();
        GameObject Last => _contentObjects.Last();
        
        /// <summary>
        /// Used to set padding before first element. For <seealso cref="ScrollVerticalContent"/> it's gonna be
        /// top padding and for <seealso cref="ScrollHorizontalContent"/> it's gonna be left padding.
        /// </summary>
        public virtual float FrontPadding { get; set;  }
        
        /// <summary>
        /// Used to set padding after last element. For <seealso cref="ScrollVerticalContent"/> it's gonna be
        /// bottom padding and for <seealso cref="ScrollHorizontalContent"/> it's gonna be right padding.
        /// </summary>
        public virtual float BackPadding { get; set;  }
        
        /// <summary>
        /// Spacing between elements.
        /// </summary>
        public virtual float Spacing { get; set; }
        
        /// <summary>
        /// Total count of visible elements. Important! This does not specify items in the entire scroll,
        /// only the currently visible and used items. The number of transform children under content game object
        /// can be also different from this parameter and this is perfectly normal behavior.
        /// </summary>
        public int Count => _contentObjects.Count;
        
        /// <summary>
        /// Exposed list of visible elements used to add elements by <seealso cref="ScrollExtension"/>.
        /// </summary>
        internal List<GameObject> ContentObjects => _contentObjects;
        
        /// <summary>
        /// Describes the amount of scroll before first rendered element. For <seealso cref="ScrollVerticalContent"/>
        /// it's gonna be upper part and for <seealso cref="ScrollHorizontalContent"/> it's gonna be left part.
        /// </summary>
        internal float _frontSize;
        
        /// <summary>
        /// Describes the amount of scroll after las rendered element. For <seealso cref="ScrollVerticalContent"/>
        /// it's gonna be bottom part and for <seealso cref="ScrollHorizontalContent"/> it's gonna be right part.
        internal float _backSize;
        
        /// <summary>
        /// List of visible elements. Important! This does not specify items in the entire scroll,
        /// only the currently visible and used items. The number of transform children under content game object
        /// can be also different from this parameter and this is perfectly normal behavior.
        /// </summary>
        private readonly List<GameObject> _contentObjects = new List<GameObject>();

        internal ScrollCellView GetScrollCellView(int i)
        {
            return ContentObjects[i].GetComponent<ScrollCellView>();
        }
        
        /// <summary>
        /// Safe way to get rendered element by it index. Important! Index of rendered element if not the same as
        /// index of data <seealso cref="ScrollCellView.DataIndex"/>.
        /// </summary>
        public GameObject GetContentElement(int index)
        {
            if (index > _contentObjects.Count - 1 || index < 0)
                return null;

            return _contentObjects[index];
        }

        /// <summary>
        /// Moves already rendered object to given index. Important! Index of rendered element if not the same as
        /// index of data <seealso cref="ScrollCellView.DataIndex"/>.
        /// </summary>
        public virtual void Move(GameObject rectObj, int at)
        {
            var index = _contentObjects.FindIndex(x => x.GetComponent<ScrollCellView>().RTransform == rectObj.GetComponent<ScrollCellView>().RTransform);
            if (index == -1)
                return;

            _contentObjects.RemoveAt(index);

            // The actual index could have shifted due to the removal
            if (at > index) at--;

            _contentObjects.Insert(at, rectObj);

            // Children order match their position in editor for debug purposes.
#if UNITY_EDITOR
            rectObj.transform.SetSiblingIndex(at);
#endif
        }

        /// <summary>
        /// Add new object to content at given index. Important!
        /// - Index of rendered element if not the same as index of data <seealso cref="ScrollCellView.DataIndex"/>;
        /// - This does not create new game object. In the case of this extension, it is managed
        /// by <seealso cref="ScrollRectExtension.RentCellViewFromPool"/>;
        /// </summary>
        public virtual void AddObjectWithoutRefreshing(GameObject cellView, int at)
        {
            if (ContentObjects.Contains(cellView))
                return;

            if (at < ContentObjects.Count)
                ContentObjects.Insert(at, cellView);
            else
                ContentObjects.Add(cellView);
        }

        /// <summary>
        /// Removes object from content. Important! Does not destroys game object. In the case of this extension,
        /// it is managed by <seealso cref="ScrollRectExtension.ReturnCellViewToPool"/>. 
        /// </summary>
        public void RemoveObjectWithoutRefreshing(GameObject cellView)
        {
            if (!ContentObjects.Contains(cellView))
                return;
                
            ContentObjects.Remove(cellView);
        }

        /// <summary>
        /// Add new object to content at given index <seealso cref="AddObjectWithoutRefreshing"/> and also rebuilds
        /// and recalculates positions of rendered objects <seealso cref="Refresh"/>.
        /// </summary>
        public void Add(GameObject rectObj, int at)
        {
            AddObjectWithoutRefreshing(rectObj, at);
            Refresh();
        }

        /// <summary>
        /// Removes object from content <seealso cref="RemoveObjectWithoutRefreshing"/> and also rebuilds
        /// and recalculates positions of rendered objects <seealso cref="Refresh"/>.
        /// </summary>
        public void Remove(GameObject rectObj)
        {
            RemoveObjectWithoutRefreshing(rectObj);
            Refresh();
        }

        /// <summary>
        /// Rebuild and recalculates position of each rendered object. For <seealso cref="ScrollVerticalContent"/>
        /// sets the objects from up to bottom and for <seealso cref="ScrollHorizontalContent"/>, from left to right.
        /// Important! Should be called just once per frame. If couple object need to be modified, to not invoke
        /// unnecessary ui rebuilds use <seealso cref="AddObjectWithoutRefreshing"/> and
        /// <seealso cref="RemoveObjectWithoutRefreshing"/> and after call refresh manually.
        /// </summary>
        public abstract void Refresh();
    }
// }