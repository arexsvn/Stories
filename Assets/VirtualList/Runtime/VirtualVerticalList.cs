using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualList
{
   public sealed class VirtualVerticalList : AbstractVirtualList
   {
      private Dictionary<int, float> _cellY = new Dictionary<int, float>();
      private float _totalHeight;
      private Vector2 _lastIndices;
      
      protected override void OnInvalidate()
      {
         RecalculateSize();
      }

      private void RecalculateSize()
      {
         float size = _padding.vertical + _totalHeight - (_spacing + _padding.top);
         _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
      }

      protected override void UpdateSource(IListSource source)
      {
         // Reset the total height when adding a new source.
         _totalHeight = _padding.top;
         _cellY.Clear();
         _lastIndices = Vector2.zero;
         
         base.UpdateSource(source);
      }
      
      protected override void AddCell(Vector2 cellSize, int index)
      {
         base.AddCell(cellSize, index);
         
         _cellY[index] = _totalHeight;
         _totalHeight += cellSize.y + _spacing;
      }
      
      protected override void PositionCell(RectTransform cellRect, int index)
      {
         float primaryPos = _cellY[index];
         
         cellRect.anchorMin = new Vector2(0, 1); // left-top
         cellRect.anchorMax = new Vector2(1, 1); // right-top
         cellRect.sizeDelta = new Vector2(-_padding.horizontal, _cellSizes[index].y);
         cellRect.pivot = new Vector2(0f, 1f); // anchor to top-left
         cellRect.anchoredPosition = new Vector2(_padding.left, -primaryPos);
      }

      /**
       * For cells with the dynamic size flag set, check that their size matches the stored size and recalculate total height if not.
       */
      protected override void CheckForDynamicSizeCell(RectTransform cellRect, int index)
      {
         Canvas.ForceUpdateCanvases();

         Vector2 currentSize = _cellSizes[index];
         var rect = cellRect.rect;
         
         // If the current stored size does not match the size of a dynamic cell, update the stored size and invalidate the list.
         if (!Mathf.Approximately(rect.height, currentSize.y))
         {
            _cellSizes[index] = new Vector2(rect.width, rect.height);
            
            _totalHeight = _padding.top;
            for(int n = 0; n < _cellSizes.Count; n++)
            {
               _cellY[n] = _totalHeight;
               _totalHeight += _cellSizes[n].y + _spacing;
            }

            Invalidate();
            
            /*
            OnInvalidate();

            if (index < _cellSizes.Count - 1)
            {
               AdjustActiveCellsPosition(index + 1, new Vector2(0f, rect.height - currentSize.y));
            }
            */
         }
      }
      
      protected override Vector2 CalculateRawIndices(Rect window)
      {
         var pos = window.position;
         var size = window.size;
         const int axis = 1;
         float windowTop = pos[axis];
         float windowBottom = pos[axis] + size[axis] + _spacing;
         int minIndex = 0;
         int maxIndex = _cellY.Count;
         bool minSet = false;
         bool maxSet = false;
         
         for (int n = 0; n < _cellY.Count; n++)
         {
            if (!minSet && _cellY[n] >= windowTop)
            {
               minIndex = Math.Max(n - 1, 0);
               minSet = true;
            }
            
            if (!maxSet && _cellY[n] > windowBottom)
            {
               maxIndex = n;
               maxSet = true;
            }

            if (minSet && maxSet)
            {
               break;
            }
         }
         
         _lastIndices.Set(minIndex, maxIndex);
         
         return _lastIndices;
      }

      
      protected override Vector2 GetCenteredScrollPosition(int index)
      {
         float primaryPos = _cellY[index];
         var rect = Viewport.rect;
         float offset = primaryPos - ((rect.size.y - _cellSizes[index].y) * 0.5f);
         return new Vector2(0, Mathf.Clamp(offset, 0, _scrollRect.content.rect.size.y - rect.size.y));
      }
   }
}