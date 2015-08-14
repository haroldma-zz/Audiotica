using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Audiotica.Windows.Common;

namespace Audiotica.Windows.CustomTriggers
{
    /// <summary>
    ///     Enables a state if an Object is <c>null</c> or a String/IEnumerable is empty
    /// </summary>
    public class IsNullOrEmptyStateTrigger : StateTriggerBase
    {
        /// <summary>
        ///     Identifies the <see cref="Value" /> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof (object), typeof (IsNullOrEmptyStateTrigger),
                new PropertyMetadata(true, OnValuePropertyChanged));

        /// <summary>
        ///     Gets or sets the value used to check for <c>null</c> or empty.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (IsNullOrEmptyStateTrigger) d;
            var val = e.NewValue;

            obj.SetActive(IsNullOrEmpty(val));

            if (val == null)
                return;

            // Try to listen for various notification events
            // Starting with INorifyCollectionChanged
            var valNotifyCollection = val as INotifyCollectionChanged;
            if (valNotifyCollection != null)
            {
                WeakEvent.Subscribe<NotifyCollectionChangedEventHandler>(valNotifyCollection, "CollectionChanged",
                    (sender, args) => obj.SetActive(IsNullOrEmpty(valNotifyCollection)));
                return;
            }

            // Not INotifyCollectionChanged, try IObservableVector
            var valObservableVector = val as IObservableVector<object>;
            if (valObservableVector != null)
            {
                WeakEvent.Subscribe<VectorChangedEventHandler<object>>(valObservableVector, "VectorChanged",
                    (sender, args) => obj.SetActive(IsNullOrEmpty(valObservableVector)));
                return;
            }

            // Not INotifyCollectionChanged, try IObservableMap
            var valObservableMap = val as IObservableMap<object, object>;
            if (valObservableMap != null)
            {
                WeakEvent.Subscribe<MapChangedEventHandler<object, object>>(valObservableMap, "MapChanged",
                    (sender, args) => obj.SetActive(IsNullOrEmpty(valObservableMap)));
            }
        }

        private static bool IsNullOrEmpty(object val)
        {
            if (val == null) return true;

            // Object is not null, check for an empty string
            var valString = val as string;
            if (valString != null)
            {
                return (valString.Length == 0);
            }

            // Object is not a string, check for an empty ICollection (faster)
            var valCollection = val as ICollection;
            if (valCollection != null)
            {
                return (valCollection.Count == 0);
            }

            // Object is not an ICollection, check for an empty IEnumerable
            var valEnumerable = val as IEnumerable;
            if (valEnumerable != null)
            {
                return !valEnumerable.Cast<object>().Any();
            }

            // Not null and not a known type to test for emptiness
            return false;
        }
    }

    /// <summary>
    ///     Trigger for switching when the screen orientation changes
    /// </summary>
    public class OrientationStateTrigger : StateTriggerBase
    {
        /// <summary>
        ///     Orientations
        /// </summary>
        public enum Orientations
        {
            /// <summary>
            ///     none
            /// </summary>
            None,

            /// <summary>
            ///     landscape
            /// </summary>
            Landscape,

            /// <summary>
            ///     portrait
            /// </summary>
            Portrait
        }

        /// <summary>
        ///     Identifies the <see cref="Orientation" /> parameter.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof (Orientations), typeof (OrientationStateTrigger),
                new PropertyMetadata(Orientations.None, OnOrientationPropertyChanged));

        /// <summary>
        ///     Initializes a new instance of the <see cref="OrientationStateTrigger" /> class.
        /// </summary>
        public OrientationStateTrigger()
        {
            if (!DesignMode.DesignModeEnabled)
            {
                WeakEvent.Subscribe<TypedEventHandler<DisplayInformation, object>>(
                    DisplayInformation.GetForCurrentView(), "OrientationChanged",
                    OrientationStateTrigger_OrientationChanged);
            }
        }

        /// <summary>
        ///     Gets or sets the orientation to trigger on.
        /// </summary>
        public Orientations Orientation
        {
            get { return (Orientations) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        private void OrientationStateTrigger_OrientationChanged(DisplayInformation sender, object args)
        {
            UpdateTrigger(sender.CurrentOrientation);
        }

        private void UpdateTrigger(DisplayOrientations orientation)
        {
            switch (orientation)
            {
                case DisplayOrientations.None:
                    SetActive(false);
                    break;
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    SetActive(Orientation == Orientations.Landscape);
                    break;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    SetActive(Orientation == Orientations.Portrait);
                    break;
            }
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (OrientationStateTrigger) d;
            if (!DesignMode.DesignModeEnabled)
            {
                var orientation = DisplayInformation.GetForCurrentView().CurrentOrientation;
                obj.UpdateTrigger(orientation);
            }
        }
    }
}