using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public abstract class InfoBox
    {
        public VisualElement BoxElement { get; set; }
        public bool ShowElement { get; set; }
        
        public InfoBox(VisualElement boxElement, bool showElement)
        {
            BoxElement = boxElement;
            ShowElement = showElement;
        }
    }
}