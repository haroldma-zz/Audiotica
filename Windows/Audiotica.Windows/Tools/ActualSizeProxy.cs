using System.ComponentModel;
using Windows.UI.Xaml;

namespace Audiotica.Windows.Tools
{
    public class ActualSizePropertyProxy : FrameworkElement, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register("Element", typeof (FrameworkElement), typeof (ActualSizePropertyProxy),
                new PropertyMetadata(null, OnElementPropertyChanged));

        public FrameworkElement Element
        {
            get { return (FrameworkElement) GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }

        public double ActualHeightValue => Element?.ActualHeight ?? 0;

        public double ActualWidthValue => Element?.ActualWidth ?? 0;
        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ActualSizePropertyProxy) d).OnElementChanged(e);
        }

        private void OnElementChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldElement = (FrameworkElement) e.OldValue;
            var newElement = (FrameworkElement) e.NewValue;

            newElement.SizeChanged += Element_SizeChanged;
            if (oldElement != null)
            {
                oldElement.SizeChanged -= Element_SizeChanged;
            }
            NotifyPropChange();
        }

        private void Element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NotifyPropChange();
        }

        private void NotifyPropChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("ActualWidthValue"));
                PropertyChanged(this, new PropertyChangedEventArgs("ActualHeightValue"));
            }
        }
    }
}