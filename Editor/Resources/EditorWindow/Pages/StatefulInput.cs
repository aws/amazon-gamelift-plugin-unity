using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow.Pages
{
    public abstract class StatefulInput
    {
        private const string InactiveClassName = "foldout--hidden";

        protected static void Hide(VisualElement element) => element.AddToClassList(InactiveClassName);
        protected static void Show(VisualElement element) => element.RemoveFromClassList(InactiveClassName);

        protected abstract void UpdateGUI();
    }
}