#region

using System;
using Windows.ApplicationModel;

#endregion

namespace Audiotica.Core.Common
{
    public class AppVersion : IComparable<AppVersion>
    {
        public AppVersion()
        {
        }

        public AppVersion(int release, int month, int patch, int revision)
        {
            ReleaseNumber = release;
            MonthNumber = month;
            PatchNumber = patch;
            RevisionNumber = revision;
        }

        public int ReleaseNumber { get; set; }
        public int MonthNumber { get; set; }
        public int PatchNumber { get; set; }
        public int RevisionNumber { get; set; }

        public int CompareTo(AppVersion other)
        {
            // check if less than
            if (ReleaseNumber < other.ReleaseNumber)
                return -1;
            if (MonthNumber < other.MonthNumber)
                return -1;
            if (PatchNumber < other.PatchNumber)
                return -1;
            if (RevisionNumber < other.RevisionNumber)
                return -1;

            // check if equal
            if (ReleaseNumber == other.ReleaseNumber
                && MonthNumber == other.MonthNumber
                && PatchNumber == other.PatchNumber
                && RevisionNumber == other.RevisionNumber)
                return 0;

            // if we're here, than this object is greater
            return 1;
        }

        public static implicit operator AppVersion(PackageVersion version)
        {
            return new AppVersion(version.Major, version.Minor, version.Build, version.Revision);
        }

        public override string ToString()
        {
            return ReleaseNumber + "." + MonthNumber + "." + PatchNumber + "." + RevisionNumber;
        }
    }
}