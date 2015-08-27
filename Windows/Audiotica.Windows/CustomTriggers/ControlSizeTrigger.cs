using Windows.UI.Xaml;

namespace Audiotica.Windows.CustomTriggers
{
    public class ControlSizeTrigger : StateTriggerBase
    {
        private double _currentHeight, _currentWidth;
        //private variables
        private FrameworkElement _targetElement;
        //public properties to set from XAML
        public double MinHeight { get; set; }
        public double MinWidth { get; set; } = -1;

        public FrameworkElement TargetElement
        {
            get { return _targetElement; }
            set
            {
                _targetElement = value;
                _targetElement.SizeChanged += _targetElement_SizeChanged;
            }
        }

        //Handle event to get current values
        private void _targetElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _currentHeight = e.NewSize.Height;
            _currentWidth = e.NewSize.Width;
            UpdateTrigger();
        }

        //Logic to evaluate and apply trigger value
        private void UpdateTrigger()
        {
            //if target is set and either minHeight or minWidth is set, proceed
            if (_targetElement != null && (MinWidth > 0 || MinHeight > 0))
            {
                //if both minHeight and minWidth are set, then both conditions must be satisfied
                if (MinHeight > 0 && MinWidth > 0)
                {
                    SetActive((_currentHeight >= MinHeight) && (_currentWidth >= MinWidth));
                }
                //if only one of them is set, then only that condition needs to be satisfied
                else if (MinHeight > 0)
                {
                    SetActive(_currentHeight >= MinHeight);
                }
                else
                {
                    SetActive(_currentWidth >= MinWidth);
                }
            }
            else
            {
                SetActive(false);
            }
        }
    }
}