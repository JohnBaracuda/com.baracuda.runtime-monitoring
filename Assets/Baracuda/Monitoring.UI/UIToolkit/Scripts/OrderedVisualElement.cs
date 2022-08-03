using UnityEngine.UIElements;

namespace Baracuda.Monitoring.UI.UIToolkit.Scripts
{
    internal class OrderedVisualElement : VisualElement, IOrder
    {
        public int Order { get; set; }
    }
}