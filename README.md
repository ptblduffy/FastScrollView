# FastScrollView
A scroll view helper for Unity's EditorGUI that will only render the visible elements, which can speed up slow Unity editor tools!

Usage:
```

void OnEnable()
{
    // Create a FastScrollView
    _fastScrollView = new FastScrollView<string, bool>(DrawElement, GetElementHeight);

    // Update it with some data
    _fastScrollView.UpdateData(new List<string>() { "Hello", "World" });
}

void OnGUI()
{
    _fastScrollView.LayoutDraw();
}

private float GetElementHeight(string element) => EditorGUIUtility.singleLineHeight

private bool DrawElement(Rect elementRect, int index, string element)
{
    EditorGUI.Label(elementRect, $"{index} {element}");
}
```
