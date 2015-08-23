using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Audiotica.Windows.CustomTriggers
{
    public class InputTypeTrigger : StateTriggerBase
    {
        private PointerDeviceType _lastPointerType;
        //private variables
        private FrameworkElement _targetElement;
        //public properties to set from XAML
        public FrameworkElement TargetElement
        {
            get { return _targetElement; }
            set
            {
                _targetElement = value;
                _targetElement.AddHandler(UIElement.PointerPressedEvent,
                    new PointerEventHandler(_targetElement_PointerPressed), true);
            }
        }

        public PointerDeviceType PointerType { get; set; }
        //Handle event to get current values
        private void _targetElement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _lastPointerType = e.Pointer.PointerDeviceType;
            UpdateTrigger();
        }

        //Logic to evaluate and apply trigger value
        public void UpdateTrigger()
        {
            SetActive(PointerType == _lastPointerType);
        }
    }
}