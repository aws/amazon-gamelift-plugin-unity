using UnityEngine.UIElements;

namespace Editor.Window
{
    public abstract class StatefulInput
    {
        private const string InactiveFoldoutElementName = "foldout--hidden";

        //Should be called with a visual element to hide this element on the screen
        protected static void Hide(VisualElement element) => element.AddToClassList(InactiveFoldoutElementName);
        //Should be called with a visual element to display this item on the screen
        protected static void Show(VisualElement element) => element.RemoveFromClassList(InactiveFoldoutElementName);

        //Should be called after a change in the UI state to update the UI to reflect the change
        protected abstract void UpdateGUI();
    }
}