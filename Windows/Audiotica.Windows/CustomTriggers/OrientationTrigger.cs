using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;
using Audiotica.Windows.Common;

namespace Audiotica.Windows.CustomTriggers
{
    /// <summary>
    ///     A trigger that changes state based on the orientation of the current window.
    /// </summary>
    public class OrientationTrigger : StateTriggerBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new <see cref="OrientationTrigger" /> instance.
        /// </summary>
        public OrientationTrigger()
        {
            // Create a weak subscription to the SizeChanged event so that we don't pin the trigger or page in memory
            WeakEvent.Subscribe<WindowSizeChangedEventHandler>(Window.Current, "SizeChanged", Window_SizeChanged);

            // Calculate the initial state
            CalculateState();
        }

        #endregion // Constructors

        #region Internal Methods

        private void CalculateState()
        {
            if (MobileOnly && !DeviceHelper.IsType(DeviceHelper.Family.Mobile))
            {
                WeakEvent.Unsubscribe<WindowSizeChangedEventHandler>(Window.Current, "SizeChanged", Window_SizeChanged);
                return;
            }

            var currentOrientation = ApplicationView.GetForCurrentView().Orientation;
            SetActive(currentOrientation == _orientation);
        }

        #endregion // Internal Methods

        #region Overrides / Event Handlers

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine(string.Format("Size Changed {0}", this.GetHashCode()));
            CalculateState();
        }

        #endregion // Overrides / Event Handlers

        #region Public Properties

        private ApplicationViewOrientation _orientation;

        /// <summary>
        ///     Gets or sets the orientation that will satisfy the trigger.
        /// </summary>
        /// <value>
        ///     The orientation that will satisfy the trigger.
        /// </value>
        public ApplicationViewOrientation Orientation
        {
            get { return _orientation; }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    CalculateState();
                }
            }
        }

        public bool MobileOnly { get; set; }

        #endregion // Public Properties
    }
}