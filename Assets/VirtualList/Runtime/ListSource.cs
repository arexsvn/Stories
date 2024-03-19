using System.Collections.Generic;
using UnityEngine;

namespace VirtualList
{
   public sealed class SourceData<T>:ISourceData
   {
      public GameObject prefab { get; }
      private readonly System.Action<T> _setup;
      
      public SourceData(GameObject prefab, System.Action<T> setup)
      {
         this.prefab = prefab;
         _setup = setup;
      }
      
      public void Set(GameObject view)
      {
         T comp = view.GetComponent<T>();
         _setup?.Invoke(comp);
      }
   }

   public interface ISourceData
   {
      void Set(GameObject view);
      GameObject prefab { get; }
   }

   public class ListSource : IListSource
   { 
      public int Count => _data.Count;
      private readonly List<ISourceData> _data = new List<ISourceData>();
      
      public virtual void Add<T>(GameObject prefab, System.Action<T> setup) where T : MonoBehaviour
      {
         _data.Add(new SourceData<T>(prefab, setup));
      }
      
      public virtual void Add<T>(T prefab, System.Action<T> setup) where T : MonoBehaviour
      {
         _data.Add(new SourceData<T>(prefab.gameObject, setup));
      }

      public virtual void Update<T>(int index, T prefab, System.Action<T> setup) where T : MonoBehaviour
      {
         _data[index] = new SourceData<T>(prefab.gameObject, setup);
      }
      
      public virtual void Clear()
      {
         _data.Clear();
      }
      
      public virtual void SetItem(GameObject view, int index)
      {
         _data[index].Set(view);
      }
      
      public virtual Vector2 SizeAt(int index)
      {
         RectTransform rectTransform = PrefabAt(index).transform as RectTransform;
         
         return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
      }

      public virtual GameObject PrefabAt(int index)
      {
         return _data[index].prefab;
      }
      
      public virtual bool DynamicSizeAt(GameObject view, int index)
      {
         return false;
      }
   }
}