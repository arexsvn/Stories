using UnityEngine;
using UnityEngine.UI;

namespace VirtualList
{
    public class VirtualListView : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private float _buffer;
        [SerializeField] private VirtualListType _type = VirtualListType.Vertical;
        [SerializeField] private RectOffset _rectOffset;
        [SerializeField] private float _spacing;
        [SerializeField] private bool _autoInit;
        
        public AbstractVirtualList List { get; private set; }

        public enum VirtualListType
        {
            Vertical,
            Horizontal,
            Grid
        }
        void Awake()
        {
            if (_autoInit)
            {
                Init();
            }
        }

        public void Set(ScrollRect scrollRect, VirtualListType type, RectOffset rectOffset, float spacing = 0f, float buffer = 0)
        {
            _scrollRect = scrollRect;
            _type = type;
            _rectOffset = rectOffset;
            _spacing = spacing;
            _buffer = buffer;

            Init();
        }
        
        public void Init()
        {
            if (_scrollRect == null)
            {
                Debug.LogWarning("VirtualList has no ScrollRect component. Please set one via Inspector.");
                return;
            }
            
            createList();
        }

        public void Clear(bool clearPool = false)
        {
            List.Clear(clearPool);
        }
        
        private void createList()
        {
            switch (_type)
            {
                case VirtualListType.Vertical :
                    List = new VirtualVerticalList();
                    break;
                
                case VirtualListType.Horizontal :
                    List = new VirtualHorizontalList();
                    break;
                
                case VirtualListType.Grid :
                    List = new VirtualGridList();
                    break;
            }

            List.Init(_scrollRect, _buffer, _rectOffset, _spacing);
        }
        
#if UNITY_EDITOR
        public void PreviewLayout()
        {
            List.PreviewLayout();
        }
#endif
    }
}
