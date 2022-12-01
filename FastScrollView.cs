using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EditorUtils
{
    
    public class FastScrollView<TElement, TReturn>
    {
        public delegate TReturn DrawElementReturn(Rect rect, int index, TElement element);

        private readonly DrawElementReturn draw;
        private readonly Func<TElement, float> getHeight;
        private Vector2 simpleScrollPos;
        private float elementSpacing = 2f;
        private readonly List<ElementLayout> elementLayout = new List<ElementLayout>();
        private readonly List<TElement> elements = new List<TElement>();

        private Dictionary<TElement, TReturn> _results = new Dictionary<TElement, TReturn>();
        public IReadOnlyDictionary<TElement, TReturn> Results => _results;

        public FastScrollView(DrawElementReturn draw, Func<TElement, float> getHeight)
        {
            this.elements = elements;
            this.draw = draw;
            this.getHeight = getHeight;
        }

        public FastScrollView<TElement, TReturn> UpdateData(List<TElement> newElements)
        {
            this.elements.Clear();
            this.elements.AddRange(newElements);
            this.elements.TrimExcess();
            this.elementLayout.Clear();
            this.elementLayout.TrimExcess();
            return this;
        }

        public void LayoutDraw()
        {
            var rect = EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight,
                GUILayout.ExpandHeight(true), GUILayout.MinHeight(EditorGUIUtility.singleLineHeight)
                );
            Draw(rect);
        }

        private struct ElementLayout
        {
            public readonly float ElementStart;
            public readonly float ElementEnd;
            public float ElementHeight => ElementEnd - ElementStart;

            public ElementLayout(float elementStart, float elementEnd)
            {
                ElementStart = elementStart;
                ElementEnd = elementEnd;
            }

            public bool ContainsY(float y)
            {
                return y >= ElementStart && y < ElementEnd;
            }
        }

        public void Draw(Rect rect)
        {
            _results.Clear();
            
            var totalHeight = 0f;

            elementLayout.Clear();
            for (var index = 0; index < elements.Count; index++)
            {
                var element = elements[index];
                var elementStart = totalHeight;
                var elementHeight = getHeight(element);
                var elementEnd = elementStart + elementHeight;
                elementLayout.Add(new ElementLayout(elementStart, elementEnd));
                totalHeight += elementHeight;
                if (index < elements.Count - 1)
                {
                    totalHeight += elementSpacing;
                }
            }

            var viewRect = new Rect(rect);
            viewRect.width -= 16f;
            viewRect.height = totalHeight;
            using (var scrollScope = new GUI.ScrollViewScope(rect, simpleScrollPos, viewRect))
            {
                simpleScrollPos = scrollScope.scrollPosition;

                FindStarting(simpleScrollPos.y, out var startIndex, out var startY);

                var elementRect = new Rect(viewRect)
                {
                    y = startY + viewRect.y,
                };
                
                int drawn = 0;
                if (elements.Count > 0)
                {
                    int j = startIndex;
                    while (j < elements.Count && (elementLayout[j].ElementStart - simpleScrollPos.y <= rect.height))
                    {
                        var element = elements[j];
                        elementRect.height = elementLayout[j].ElementHeight;
                        _results[element] = DrawElement(elementRect, j, elements[j]);
                        elementRect.y += elementRect.height;
                        elementRect.y += elementSpacing;
                        j++;
                        drawn++;
                    }
                }
            }
        }

        private void FindStarting(float scrollY, out int startingIndex, out float startingY)
        {
            startingIndex = 0;
            startingY = 0f;
            for (int i = 0; i < elements.Count; i++)
            {
                var layout = elementLayout[i];

                if (layout.ContainsY(scrollY))
                {
                    startingIndex = i;
                    startingY = layout.ElementStart;
                    return;
                }

                if (i + 1 < elements.Count)
                {
                    if (scrollY >= layout.ElementEnd && scrollY < elementLayout[i + 1].ElementStart)
                    {
                        startingIndex = i;
                        startingY = layout.ElementStart;
                        return;
                    }
                }
            }
        }

        private TReturn DrawElement(Rect elementRect, int index, TElement element)
        {
            try
            {
                return draw.Invoke(elementRect, index, element);
            }
            catch (Exception)
            {
                Debug.LogError($"Error drawing element {element} at index {index}");
                throw;
            }
        }
    }

    public class FastScrollView<TElement> : FastScrollView<TElement, bool>
    {
        public delegate void DrawSingleElement(Rect rect, int index, TElement element);

        public FastScrollView(DrawSingleElement draw, Func<TElement, float> getHeight) : base(Wrap(draw), getHeight) { }

        private static DrawElementReturn Wrap(DrawSingleElement drawSingleElement)
        {
            return (rect, index, element) =>
            {
                drawSingleElement(rect, index, element);
                return true;
            };
        }
    }
}