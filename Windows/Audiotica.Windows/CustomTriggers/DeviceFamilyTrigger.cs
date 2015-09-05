using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Audiotica.Core.Windows.Helpers;

namespace Audiotica.Windows.CustomTriggers
{
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        //private variables
        private DeviceFamily _queriedDeviceFamily;
        //Public property
        public DeviceFamily DeviceFamily
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