using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualList
{
   public abstract class AbstractVirtualList
   {
      protected ScrollRect _scrollRect;
      protected RectOffset _padding;  // Padding around scrolling list.
      protected float _spacing;       // Space between cells.
      protected readonly Dictionary<int, Vector2> _cellSizes = new Dictionary<int, Vector2>();  // Stores the sizes of cells for visibility calculations.
      
      private float _buffer;          // size in pixels to preload tiles
      private float _minScrollDelta;  // min scroll distance before we recheck which cells to activate/deactivate. Defaults to .5 buffer size.
      //private Transform _poolParent;  // Container for storing inactive cell pool.
      private IListSource _source;    // Datasource for list cells.
      private readonly List<Cell> _pool = new List<Cell>();
      private int _poolCommits;       // number of elements that have been committed to the the pool
      private readonly Dictionary<int, Cell> _activeCells = new Dictionary<int, Cell>();
      private Vector2 _activeIndices = Vector2.zero;  // Track the first and last index of currently active tiles [start, end)
      private Vector2 _lastPosition;                  // Last scroll position that the cells were activated/deactivated.
      
      public void Init(ScrollRect scrollRect, float buffer, RectOffset padding, float spacing)
      {
         _scrollRect = scrollRect;
         _buffer = buffer;
         _minScrollDelta = buffer * 0.5f;
         _padding = padding;
         _spacing = spacing;
         
         Invalidate();
         
         scrollRect.onValueChanged.AddListener(OnScrollbarValue);
      }
      
      // Sets the data source for the virtual list
      public void SetSource(IListSource source)
      {
         UpdateSource(source);
         Invalidate();
      }
      
      // Removes the data source for the virtual list. Note, this will not destroy any pooled elements so they can
      // be reused on a subsequent `SetSource()` call.
      public void RemoveSource()
      {
         SetSource(null);
      }

      public void SetSourceAndCenterOn(IListSource source, int index)
      {
         UpdateSource(source);
         OnInvalidate();
         _scrollRect.content.anchoredPosition = GetCenteredScrollPosition(index);
         UpdateVisibilityDisjoint();
      }
      
      // Refreshes view
      // Call if contents of source changes in a way not handled by cells
      public void Invalidate()
      {
         OnInvalidate();
         UpdateVisibilityDisjoint();
      }

      // Clears the list and destroy pooled elements
      public void Clear(bool clearPool)
      {
         _source = null;

         _cellSizes.Clear();
         
         foreach (var pair in _activeCells)
         {
            Object.Destroy(pair.Value.View);
         }
         _activeCells.Clear();
         _activeIndices = Vector2.zero;

         if (clearPool)
         {
            /*
            if (_poolParent != null)
            {
               Object.Destroy(_poolParent.gameObject);
               _poolParent = null;
            }
            */
            foreach (var pooledCell in _pool)
            {
               Object.Destroy(pooledCell.View.gameObject);
            }
            _pool.Clear();
         }
      }

      // methods for manually iterating over visible cells
      public int StartIndex => (int) _activeIndices.x;

      // index after the last visible index
      public int EndIndex => (int) _activeIndices.y;

      public GameObject GetCell(int index)
      {
         _activeCells.TryGetValue(index, out Cell cell);
         return cell?.View;
      }
      
      protected virtual void UpdateSource(IListSource source)
      {
         _source = source;
         
         _cellSizes.Clear();
         
         foreach (var pair in _activeCells)
         {
            ReturnCellToPool(pair.Value);
         }
         _activeCells.Clear();
         _activeIndices = Vector2.zero;
         
         for(int n = 0; n < _source.Count; n++)
         {
            AddCell(_source.SizeAt(n), n);
         }
      }
      
      protected virtual void AddCell(Vector2 cellSize, int index)
      {
         _cellSizes[index] = cellSize;
      }
      
      protected abstract void OnInvalidate();
      protected abstract void PositionCell(RectTransform cellRect, int index);
      protected abstract Vector2 CalculateRawIndices(Rect window);
      protected abstract Vector2 GetCenteredScrollPosition(int index);

      private Vector2 CalculateActiveIndices()
      {
         var viewportRectSize = Viewport.rect.size;
         // Expand the viewport by the buffer size to cause cells outside the actual viewport to be activated.
         //   This gives smoother scrolling and prevents 'pop-in' of cells at the cost of activating extra cells.
         var viewportSize = new Vector2(viewportRectSize.x + _buffer * 2f, viewportRectSize.y + _buffer * 2f);
         var content = _scrollRect.content;
         var anchoredPos = content.anchoredPosition;
         var sizeDelta = content.sizeDelta;
         var pivot = content.pivot;
         var viewX = -anchoredPos.x + (sizeDelta.x * pivot.x) - _buffer;
         var viewY = anchoredPos.y + (sizeDelta.y * (1 - pivot.y)) - _buffer;
         var viewportPosition = new Vector2(viewX, viewY);
         var raw = CalculateRawIndices(new Rect(viewportPosition, viewportSize));

         int count = ItemCount();
         int min = Mathf.Max((int)Mathf.Min(raw.x, raw.y), 0);
         int max = Mathf.Min((int)Mathf.Max(raw.x, raw.y), count);
         
         return new Vector2(min, max);
      }

      private void OnScrollbarValue(Vector2 scrollValue)
      {
         UpdateVisibility();
      }

      private void UpdateVisibility()
      {
         if (_minScrollDelta > 0f)
         {
            // Don't recheck indices unless scroll delta is high enough.
            //   Assumes EITHER vertical or horizontal scrolling
            var scrollDelta = _lastPosition - _scrollRect.content.anchoredPosition;
            float deltaMax = Mathf.Max(Mathf.Abs(scrollDelta.x), Mathf.Abs(scrollDelta.y));

            if (_lastPosition != default && deltaMax < _minScrollDelta)
            {
               return;
            }
            _lastPosition = _scrollRect.content.anchoredPosition;
         }
         
         Vector2 newActiveIndices = CalculateActiveIndices();

         if (_activeIndices == newActiveIndices)
         {
            return;
         }

         //Special case for no overlap
         if (_activeIndices.y <= newActiveIndices.x || _activeIndices.x >= newActiveIndices.y)
         {
            UpdateVisibilityDisjoint(newActiveIndices);
         }
         else
         {
            //Deactivate first
            for (int i = (int)_activeIndices.x; i < newActiveIndices.x; i++)
               ActivateCell(i, false);
            for (int i = (int)_activeIndices.y; i >= newActiveIndices.y; i--)
               ActivateCell(i, false);
                
            //Then activate
            for (int i = (int)Mathf.Min(newActiveIndices.y, _activeIndices.x) - 1; i >= newActiveIndices.x; i--)
               ActivateCell(i, true);
            for (int i = (int)Mathf.Max(newActiveIndices.x, _activeIndices.y); i < newActiveIndices.y; i++)
               ActivateCell(i, true);

         }
         
         _activeIndices = newActiveIndices;
         CommitToPool();
      }

      private void UpdateVisibilityDisjoint()
      {
         _lastPosition = Vector2.zero;
         Vector2 newActiveIndices = CalculateActiveIndices();
         UpdateVisibilityDisjoint(newActiveIndices);
         _activeIndices = newActiveIndices;
         CommitToPool();
      }

      private void UpdateVisibilityDisjoint(Vector2 newActiveIndices)
      {
         for (int i = (int)_activeIndices.x; i < _activeIndices.y; i++)
         {
            if(i < newActiveIndices.x || i >= newActiveIndices.y)
            {
               ActivateCell(i, false);
            }
         }

         for (int i = (int)newActiveIndices.x; i < newActiveIndices.y; i++)
         {
            ActivateCell(i, true);
         }
      }

      private GameObject PrefabAt(int index)
      {
         return _source.PrefabAt(index);
      }

      private void ActivateCell(int index, bool activate)
      {
         _activeCells.TryGetValue(index, out Cell cell);
         
         if (activate)
         {
            var prefab = PrefabAt(index);
            
            if(cell != null && cell.Prefab != prefab)
            {
               ReturnCellToPool(cell);
               cell = null;
            }
            
            if (cell == null)
            {
               cell = GetCellFromPool(prefab);
               _activeCells[index] = cell;
            }
            
            PositionCell(cell.RectTransform, index);
            
            // Don't call SetItem on cells which have been set to a particular index already. This prevents pooled cells with unchanged
            //   data from getting SetItem called unnecessarily.
            bool cellSet = cell.index == index;
            
            if (!cellSet && _source != null)
            {
               cell.index = index;
               
               _source.SetItem(cell.View, index);

               if (_source.DynamicSizeAt(cell.View, index))
               {
                  CheckForDynamicSizeCell(cell.RectTransform, index);
               }
            }
         }
         else if (cell != null)
         {
            ReturnCellToPool(cell);
            _activeCells.Remove(index);
         }
      }

      protected virtual void CheckForDynamicSizeCell(RectTransform cellRect, int index)
      {

      }

      // Not currently used - A more efficient approach to adjusting the position of active cells relative to a delta but probably not worth it over Invalidate.
      protected void AdjustActiveCellsPosition(int initialCell, Vector2 sizeDelta)
      {
         for(int n = initialCell; n < _cellSizes.Count; n++)
         {
            _activeCells.TryGetValue(n, out Cell cell);

            if (cell != null)
            {
               cell.RectTransform.localPosition = new Vector2(cell.RectTransform.localPosition.x + sizeDelta.x, cell.RectTransform.localPosition.y - sizeDelta.y);
            }
         }
      }
      
      protected int ItemCount()
      {
         return _source != null ? _source.Count : 0;
      }

      private Cell GetCellFromPool(GameObject prefab)
      {
         for(int i = _pool.Count - 1; i >= 0; --i)
         {
            var cell = _pool[i];
            if(cell.Prefab == prefab)
            {
               _pool.RemoveAt(i);
               if(i < _poolCommits)
               {
                  _poolCommits -= 1;
               }
               //cell.RectTransform.SetParent(_scrollRect.content, false);  // TODO : verify this is slower than SetActive
               cell.View.SetActive(true);
               return cell;
            }
         }
         return new Cell(Object.Instantiate(prefab, _scrollRect.content), prefab);
      }
      
      private void ReturnCellToPool(Cell pooledObject)
      {
         if (pooledObject == null)
            return;

         pooledObject.index = -1;
         _pool.Add(pooledObject);
      }

      private void CommitToPool()
      {
         for (int i = _poolCommits; i < _pool.Count; ++i)
         {
            var pooledObject = _pool[i].View;

            // @see the comments in the getter for PoolParent. The following
            //   effectively disables the component hierarchy of pooledObject.
            //pooledObject.transform.SetParent(PoolParent, false); // TODO : verify this is slower than SetActive
            pooledObject.SetActive(false);
         }
         _poolCommits = _pool.Count;
      }

      /*
      private Transform PoolParent
      {
         get
         {
            if (_poolParent == null)
            {
               // Cells that are moved to the pool are reparented with the following.
               // Because the following object is not active, reparenting has the effect
               //   of disabling the component hierarchy of the cell moving to the pool.
               var go = new GameObject("PoolParent", typeof(RectTransform));
               go.SetActive(false);
               _poolParent = go.transform;
               _poolParent.SetParent(_scrollRect.transform.parent, false);
            }

            return _poolParent;
         }
      }
*/
      private RectTransform _viewport;
      protected RectTransform Viewport
      {
         get
         {
            if (_viewport == null)
            {
               _viewport = _scrollRect.GetComponent<RectTransform>();
            }
            return _viewport;
         }
      }
      
   #if UNITY_EDITOR
      public void PreviewLayout()
      {
         if(Application.isPlaying) return;
         
         while (_scrollRect.transform.parent.childCount > 0)
         {
            Transform t = _scrollRect.transform.parent.GetChild(0);
            t.SetParent(null);
            Object.DestroyImmediate(t.gameObject);
         }

         OnInvalidate();

         /*
         for (int i = 0; i < 20; i++)
         {
            var cell = Object.Instantiate(_defaultTilePrefab);
            cell.name = "Temporary Preview";
            cell.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            PositionCell(cell, i);
         }
         */
      }
   #endif
      
      private sealed class Cell
      {
         public int index;
         public readonly GameObject View;
         public readonly GameObject Prefab;
         public readonly RectTransform RectTransform;

         public Cell(GameObject view, GameObject prefab)
         {
            View = view;
            RectTransform = view.GetComponent<RectTransform>();
            Prefab = prefab;
            index = -1;
         }
      }
   }
}
