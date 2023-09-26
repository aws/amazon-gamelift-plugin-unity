using UnityEngine.UIElements;

namespace AmazonGameLift.Editor
{
    public class StatusBox
    {
        public VisualElement BoxElement { get; set; }
        public bool ShowElement { get; set; }
        
        public StatusBox(string name, VisualElement boxElement, bool showElement)
        {
            BoxElement = boxElement;
            ShowElement = showElement;
        }
    }
}