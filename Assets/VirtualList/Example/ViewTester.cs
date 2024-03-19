using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualList
{
   public sealed class ViewTester : MonoBehaviour
   {
      public VirtualListView listView;
      public Button source1Button;
      public Button source2Button;
      public Button source3Button;
      public Button clearButton;
      public Button invalidateButton;
      public TestDisplay defaultTilePrefab;
      public TestDisplay tallTilePrefab;
      public TestDisplay dynamicSizeTilePrefab;

      public void Start()
      {
         source1Button.onClick.AddListener(() =>
         {
            string[] someNames =
            {
               "One",
               "Two",
               "Three",
               "Four",
               "Five",
               "Six"
            };
            listView.List.SetSource(new SingleComponentSource<string, TestDisplay>(someNames, defaultTilePrefab.gameObject));
         });

         source2Button.onClick.AddListener(() =>
         {
            List<CustomSizeSourceData> data = new List<CustomSizeSourceData>
            {
               new CustomSizeSourceData("zero", "tall"),
               new CustomSizeSourceData("one", "tall"),
               new CustomSizeSourceData("two", "tall"),
               new CustomSizeSourceData("three", "tall"),
               new CustomSizeSourceData("four", "tall"),
               new CustomSizeSourceData("five"),
               new CustomSizeSourceData("six", "tall"),
               new CustomSizeSourceData("seven"),
               new CustomSizeSourceData("eight", "dynamic"),
               new CustomSizeSourceData("nine"),
               new CustomSizeSourceData("ten", null, new Vector2(200f, 300f)),
               new CustomSizeSourceData("11"),
               new CustomSizeSourceData("12"),
               new CustomSizeSourceData("13"),
               new CustomSizeSourceData("14"),
               new CustomSizeSourceData("15"),
               new CustomSizeSourceData("16"),
               new CustomSizeSourceData("17", "dynamic"),
               new CustomSizeSourceData("18"),
               new CustomSizeSourceData("19"),
               new CustomSizeSourceData("20")
            };

            Dictionary<string, GameObject> prefabOverrides = new Dictionary<string, GameObject>
            {
               {
                  "tall", tallTilePrefab.gameObject
               },
               {
                  "dynamic", dynamicSizeTilePrefab.gameObject
               }
            };

            CustomSourceExample customSource = new CustomSourceExample();
            customSource.Init(data, defaultTilePrefab.gameObject, prefabOverrides);
            listView.List.SetSource(customSource);
         });

         source3Button.onClick.AddListener(() =>
         {
            DynamicSizedSourceExample dynaSource = new DynamicSizedSourceExample();

            dynaSource.Add(defaultTilePrefab, o => o.Set("dyno1"));
            dynaSource.Add(tallTilePrefab, o => o.Set("dyno2"));
            dynaSource.Add(dynamicSizeTilePrefab, o => o.Set("dyno3"));
            dynaSource.Add(tallTilePrefab, o => o.Set("dyno4"));
            dynaSource.Add(defaultTilePrefab, o => o.Set("dyno5"));
            dynaSource.Add(tallTilePrefab, o => o.Set("dyno6"));
            dynaSource.Add(defaultTilePrefab, o => o.Set("dyno7"));
            dynaSource.Add(dynamicSizeTilePrefab, o => o.Set("dyno8"));

            listView.List.SetSource(dynaSource);
         });
         clearButton.onClick.AddListener(() => listView.Clear(false));
         invalidateButton.onClick.AddListener(listView.List.Invalidate);
      }
   }

   public class CustomSourceExample : IListSource
   {
      private List<CustomSizeSourceData> _data;
      private Dictionary<string, GameObject> _prefabs;
      private GameObject _defaultPrefab;
      public int Count => _data.Count;

      public void Init(List<CustomSizeSourceData> data, GameObject defaultPrefab,
         Dictionary<string, GameObject> prefabs)
      {
         _data = data;
         _defaultPrefab = defaultPrefab;
         _prefabs = prefabs;
      }

      public void SetItem(GameObject view, int index)
      {
         CustomSizeSourceData customSizeSourceData = _data[index];
         TestDisplay testDisplay = view.GetComponent<TestDisplayDynamicSize>();
         if (testDisplay == null)
         {
            testDisplay = view.GetComponent<TestDisplay>();
         }
         else
         {
            (testDisplay as TestDisplayDynamicSize).dynamicSizeSet =
               (view.transform as RectTransform).rect.height > SizeAt(index).y;
         }

         testDisplay.Set(customSizeSourceData.label);
      }

      public Vector2 SizeAt(int index)
      {
         CustomSizeSourceData customSizeSourceData = _data[index];
         if (customSizeSourceData.sizeOverride != default)
         {
            return customSizeSourceData.sizeOverride;
         }

         RectTransform rectTransform = PrefabAt(index).transform as RectTransform;

         return new Vector2(rectTransform.rect.width, rectTransform.rect.height);
      }

      public GameObject PrefabAt(int index)
      {
         CustomSizeSourceData customSizeSourceData = _data[index];

         if (!string.IsNullOrEmpty(customSizeSourceData.prefabOverride))
         {
            return _prefabs[customSizeSourceData.prefabOverride];
         }

         return _defaultPrefab;
      }

      public bool DynamicSizeAt(GameObject view, int index)
      {
         TestDisplayDynamicSize testDisplay = view.GetComponent<TestDisplayDynamicSize>();
         if (testDisplay != null)
         {
            return !testDisplay.dynamicSizeSet;
         }

         return false;
      }
   }

   public class CustomSizeSourceData
   {
      public string label;
      public string prefabOverride;
      public Vector2 sizeOverride;

      public CustomSizeSourceData(string label, string prefabOverride = null, Vector2 sizeOverride = default)
      {
         this.label = label;
         this.prefabOverride = prefabOverride;
         this.sizeOverride = sizeOverride;
      }
   }
   
   public class DynamicSizedSourceExample : ListSource
   {
      public override void SetItem(GameObject view, int index)
      {
         base.SetItem(view, index);

         // Handle any special case setup, deal with dynamic-sized elements, etc...
         TestDisplayDynamicSize testDisplay = view.GetComponent<TestDisplayDynamicSize>();
         if (testDisplay != null)
         {
            testDisplay.dynamicSizeSet = (view.transform as RectTransform).rect.height > SizeAt(index).y;
         }
      }
      
      public override bool DynamicSizeAt(GameObject view, int index)
      {
         TestDisplayDynamicSize testDisplay = view.GetComponent<TestDisplayDynamicSize>();
         if (testDisplay != null)
         {
            return !testDisplay.dynamicSizeSet;
         }

         return false;
      }
   }
}