using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;

namespace Audiotica.Windows.CustomTriggers
{
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        //private variables
        private DeviceHelper.Family _queriedDeviceFamily;
        //Public property
        public DeviceHelper.Family DeviceFamily
        {
            get { return _queriedDeviceFamily; }
            set
            {
                _queriedDeviceFamily = value;
                SetActive(DeviceHelper.IsType(_queriedDeviceFamily));
            }
        }
    }
}