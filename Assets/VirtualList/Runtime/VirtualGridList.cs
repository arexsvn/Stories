using UnityEngine;

namespace VirtualList
{
   public class VirtualGridList : AbstractVirtualList
   {
      public enum Axis { Horizontal = 0, Vertical = 1 }

      public RectOffset padding = new RectOffset(10, 10, 10, 10);
      public Axis axis = Axis.Vertical;
      public Vector2 cellSize = new Vector2(100, 100);
      public Vector2 spacing = new Vector2(10, 10);
      public int limit = 4;
      private int _axis;

      protected override void OnInvalidate()
      {
         _axis = (int)axis;
         RecalculateSize();
      }

      private void RecalculateSize()
      {
         int primary = Mathf.CeilToInt(ItemCount() / (float)limit);
         int otherAxis = 1 - _axis;

         Vector2 size = Vector2.zero;
         size[_axis] = cellSize[_axis] * primary + Mathf.Max(0, primary - 1) * spacing[_axis];
         size[otherAxis] = cellSize[otherAxis] * limit + Mathf.Min(0, limit - 1) * spacing[otherAxis];
         size.x += padding.horizontal;
         size.y += padding.vertical;
         _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
         _scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
      }

      protected override void PositionCell(RectTransform cellRect, int index)
      {
         cellRect.SetParent(_scrollRect.content, false);

         int otherAxis = 1 - _axis;
         int primary = index / limit;
         int secondary = index % limit;

         float primaryPos = primary * (cellSize[_axis] + spacing[_axis]) + PaddingForAxis(_axis);
         float secondaryPos = secondary * (cellSize[otherAxis] + spacing[otherAxis]) + PaddingForAxis(otherAxis);

         cellRect.SetInsetAndSizeFromParentEdge(EdgeForAxis(_axis), primaryPos, cellSize[_axis]);
         cellRect.SetInsetAndSizeFromParentEdge(EdgeForAxis(otherAxis), secondaryPos, cellSize[otherAxis]);
      }

      private float PaddingForAxis(int ax)
      {
         return ax == 0 ? padding.left : padding.top;
      }

      private RectTransform.Edge EdgeForAxis(int ax)
      {
         return ax == 1 ? RectTransform.Edge.Top : RectTransform.Edge.Left;
      }

      protected override Vector2 CalculateRawIndices(Rect window)
      {
         var pos = window.position;
         var size = window.size;

         float pad = PaddingForAxis(_axis);
         float lowestPosVisible = pos[_axis] - pad;
         float highestPosVisible = pos[_axis] + size[_axis] + cellSize[_axis] - pad;
         float rowSize = cellSize[_axis] + spacing[_axis];

         int min = limit * RowAtPos(lowestPosVisible, rowSize);
         int max = limit * RowAtPos(highestPosVisible, rowSize);
         return new Vector2(min, max);
      }

      private int RowAtPos(float pos, float rowSize)
      {
         return (int)(pos / rowSize);
      }

      protected override Vector2 GetCenteredScrollPosition(int index)
      {
         int primary = index / limit;
         float primaryPos = primary * (cellSize[_axis] + spacing[_axis]) + PaddingForAxis(_axis);

         float offset = primaryPos - ((Viewport.rect.size[_axis] - cellSize[_axis]) * 0.5f);
         offset = Mathf.Clamp(offset, 0, _scrollRect.content.rect.size[_axis] - Viewport.rect.size[_axis]);

         if (axis == Axis.Vertical)
         {
            return new Vector2(0, offset);
         }
         else
         {
            return new Vector2(-offset, 0);
         }
      }
   }
}
