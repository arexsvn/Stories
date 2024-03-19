using UnityEditor;
using UnityEngine;

namespace VirtualList.Editor
{
   [CustomEditor(typeof(AbstractVirtualList), true)]
   public class AbstractVirtualListEditor : UnityEditor.Editor 
   {
      public override void OnInspectorGUI()
      {
         DrawDefaultInspector();

         if(!Application.isPlaying && GUILayout.Button("Preview Layout"))
         {
            ((VirtualListView)target).PreviewLayout();
         }
      }

   }
}
