using System;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Audiotica.Triggers
{
    public class NetworkAvailableStateTrigger : StateTriggerBase
    {
        public static readonly DependencyProperty ConnectionStateProperty =
            DependencyProperty.Register("ConnectionState", typeof (ConnectionState),
                typeof (NetworkAvailableStateTrigger),
                new PropertyMetadata(ConnectionState.Connected, OnConnectionStatePropertyChanged));

        public NetworkAvailableStateTrigger()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            UpdateState();
        }

        public ConnectionState ConnectionState
        {
            get { return (ConnectionState) GetValue(ConnectionStateProperty); }
            set { SetValue(ConnectionStateProperty, value); }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateState);
        }

        private void UpdateState()
        {
            var isConnected = false;
            var profile = NetworkInformation.GetInternetConnectionProfile();
            // TODO: complete check
            if (profile != null)
                isConnected = profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            SetActive(
                isConnected && ConnectionState == ConnectionState.Connected ||
                !isConnected && ConnectionState == ConnectionState.Disconnected);
        }

        private static void OnConnectionStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (NetworkAvailableStateTrigger) d;
            obj.UpdateState();
        }
    }

    public enum ConnectionState
    {
        Connected,
        Disconnected
    }
}