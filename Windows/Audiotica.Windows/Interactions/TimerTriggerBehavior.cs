using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Microsoft.Xaml.Interactivity;

namespace Audiotica.Windows.Interactions
{
    [ContentProperty(Name = "Actions")]
    public sealed class TimerTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof (ActionCollection),
                typeof (TimerTriggerBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty MillisecondsPerTickProperty =
            DependencyProperty.Register("MillisecondsPerTick", typeof (double),
                typeof (TimerTriggerBehavior), new PropertyMetadata(1000.0));

        public static readonly DependencyProperty TotalTicksProperty =
            DependencyProperty.Register("TotalTicks", typeof (int),
                typeof (TimerTriggerBehavior), new PropertyMetadata(-1));

        private int _tickCount;
        private DispatcherTimer _timer;

        public ActionCollection Actions
        {
            get
            {
                var actions = (ActionCollection) GetValue(ActionsProperty);
                if (actions == null)
                {
                    actions = new ActionCollection();
                    SetValue(ActionsProperty, actions);
                }
                return actions;
            }
        }

        public double MillisecondsPerTick
        {
            get { return (double) GetValue(MillisecondsPerTickProperty); }
            set { SetValue(MillisecondsPerTickProperty, value); }
        }

        public int TotalTicks
        {
            get { return (int) GetValue(TotalTicksProperty); }
            set { SetValue(TotalTicksProperty, value); }
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            if ((associatedObject != AssociatedObject) && !DesignMode.DesignModeEnabled)
            {
                if (AssociatedObject != null)
                    throw new InvalidOperationException("Cannot attach behavior multiple times.");
                AssociatedObject = associatedObject;
                StartTimer();
            }
        }

        public void Detach()
        {
            StopTimer();
        }

        private void OnTimerTick(object sender, object e)
        {
            if (TotalTicks > 0 && ++_tickCount >= TotalTicks)
            {
                StopTimer();
            }

            // Raise the actions
            Interaction.ExecuteActions(AssociatedObject, Actions, null);
        }

        internal void StartTimer()
        {
            _timer = new DispatcherTimer {Interval = (TimeSpan.FromMilliseconds(MillisecondsPerTick))};
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        internal void StopTimer()
        {
            _timer.Stop();
        }
    }
}