using System;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Audiotica.Windows.CustomTriggers
{
    public class NetworkAvailableStateTrigger : StateTriggerBase
    {
        public enum ConnectionStates
        {
            Connected,
            Disconnected
        }

        private ConnectionStates _connectionState;

        public NetworkAvailableStateTrigger()
        {
            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            UpdateState();
        }

        public ConnectionStates ConnectionState
        {
            get { return _connectionState; }
            set
            {
                if (_connectionState == value)
                    return;
                _connectionState = value;
                UpdateState();
            }
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateState);
        }

        private void UpdateState()
        {
            var isConnected = false;
            var profile = NetworkInformation.GetInternetConnectionProfile();
            if (profile != null)
                isConnected = profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            SetActive(
                isConnected && ConnectionState.Equals(ConnectionStates.Connected) ||
                !isConnected && ConnectionState.Equals(ConnectionStates.Disconnected));
        }
    }
}