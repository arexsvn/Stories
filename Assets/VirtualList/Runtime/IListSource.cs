using UnityEngine;

namespace VirtualList
{
   // Interface describing the list of items displayed by a virtual list
   public interface IListSource
   {
      // Number of items
      int Count
      {
         get;
      }

      // Set up the view to display the item at the given index
      //
      // The view may be reused multiple times
      void SetItem(GameObject view, int index);
      
      GameObject PrefabAt(int index);
      
      Vector2 SizeAt(int index);

      bool DynamicSizeAt(GameObject view, int index);
   }
}
