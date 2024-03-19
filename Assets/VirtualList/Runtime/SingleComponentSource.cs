using System.Collections.Generic;
using UnityEngine;

namespace VirtualList
{
   // An interface used by the SimpleSource
   public interface IViewFor<T>
   {
      void Set(T value);
   }

   // A simple data source backed by an IList (which can be an array)
   public class SingleComponentSource<TData, TView> : IListSource where TView : Component, IViewFor<TData>
   {
      private readonly IList<TData> _list;
      private GameObject _prefab;
      public SingleComponentSource(IList<TData> list, GameObject prefab)
      {
         _list = list;
         _prefab = prefab;
      }

      // Number of items
      public int Count
      {
         get
         {
            if (_list != null)
               return _list.Count;
            else
               return 0;
         }
      }

      public void SetItem(GameObject view, int index)
      {
         var element = _list[index];
         var display = view.GetComponent<TView>();
         display.Set(element);
      }
      
      public GameObject PrefabAt(int index)
      {
         return _prefab;
      }

      public Vector2 SizeAt(int index)
      {
         return _prefab.GetComponent<RectTransform>().sizeDelta;
      }

      public bool DynamicSizeAt(GameObject view, int index)
      {
         return false;
      }
   }
}
