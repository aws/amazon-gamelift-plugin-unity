using AmazonGameLift.Editor;
using UnityEngine.UIElements;

namespace Editor.Resources.EditorWindow
{
    public class ElementLocalizer
    {
        private readonly VisualElement _root;
        private readonly TextProvider _textProvider = TextProviderFactory.Create();

        public ElementLocalizer(VisualElement root)
        {
            _root = root;
        }

        public void SetElementText(string elementName, string textKey)
        {
            var element = _root.Q<TextElement>(elementName);
            if (element != default)
            {
                element.text = _textProvider.Get(textKey);
            }
        }
    }
}