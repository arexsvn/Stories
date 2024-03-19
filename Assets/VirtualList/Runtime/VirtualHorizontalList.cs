using System;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualList
{
   public class VirtualHorizontalList : AbstractVirtualList
   {
      private Dictionary<int, float> _cellX = new Dictionary<int, float>();
      private float _totalWidth;
      private Vector2 _lastIndices;
      
      protected override void OnInvalidate()
      {
         RecalculateSize();
      }

      private void RecalculateSize()
      {
         //int primary = ItemCount();
         //float size = _padding.horizontal + cellSize * primary + Mathf.Max(0, primary - 1) * _spacing;
         float size = _padding.horizontal + _totalWidth - (_spacing + _padding.left);
         _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
      }

      protected override void UpdateSource(IListSource source)
      {
         _totalWidth = _padding.left;
         
         base.UpdateSource(source);
      }
      
      protected override void AddCell(Vector2 cellSize, int index)
      {
         base.AddCell(cellSize, index);
         
         _cellX[index] = _totalWidth;
         _totalWidth += cellSize.x + _spacing;
      }
      
      protected override void PositionCell(RectTransform cellRect, int index)
      {
         //cellRect.SetParent(_scrollRect.content, false);

         //float primaryPos = index * (cellSize + _spacing) + _padding.left;
         float primaryPos = _cellX[index];
         
         cellRect.anchorMin = new Vector2(0, 0); // bottom-left
         cellRect.anchorMax = new Vector2(0, 1); // bottom-right
         cellRect.sizeDelta = new Vector2(_cellSizes[index].x, -_padding.vertical);
         cellRect.pivot = new Vector2(0f, 1f); // anchor to bottom-right
         cellRect.anchoredPosition = new Vector2(primaryPos, _padding.top);
      }

      protected override Vector2 CalculateRawIndices(Rect window)
      {
         /*
         var pos = window.position;
         var size = window.size;

         const int axis = 0;
         float pad = _padding.left;
         float lowestPosVisible = pos[axis] - pad;
         float highestPosVisible = pos[axis] + size[axis] + cellSize - pad;
         float colSize = cellSize + _spacing;

         int min = (int)(lowestPosVisible / colSize);
         int max = (int)(highestPosVisible / colSize);
         _lastIndicies.Set(min, max);

         return _lastIndices;
         */
         var pos = window.position;
         var size = window.size;
         const int axis = 0;
         float windowLeft = pos[axis];
         float windowRight = pos[axis] + size[axis] + _spacing;
         int minIndex = 0;
         int maxIndex = _cellX.Count;
         bool minSet = false;
         bool maxSet = false;
         
         for (int n = 0; n < _cellX.Count; n++)
         {
            if (!minSet && _cellX[n] >= windowLeft)
            {
               minIndex = Math.Max(n - 1, 0);
               minSet = true;
            }
            
            if (!maxSet && _cellX[n] > windowRight)
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
         
         //Debug.Log($"Activate Cell min/max visible {_lastIndices}");
         
         return _lastIndices;
      }

      protected override Vector2 GetCenteredScrollPosition(int index)
      {
         /*
         float primaryPos = -(index * (cellSize + _spacing) + _padding.left);
         var rect = Viewport.rect;
         float offset = primaryPos + ((rect.size.x- cellSize) * 0.5f);
         return new Vector2(Mathf.Clamp(offset, -Mathf.Max(0, _scrollRect.content.rect.size.x - rect.size.x), 0), 0);
         */
         float primaryPos = _cellX[index];
         var rect = Viewport.rect;
         float offset = primaryPos - ((rect.size.x - _cellSizes[index].x) * 0.5f);
         return new Vector2(0, Mathf.Clamp(offset, 0, _scrollRect.content.rect.size.x - rect.size.x));
      }
   }
}
