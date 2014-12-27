#region

using Windows.Networking.BackgroundTransfer;
using GalaSoft.MvvmLight;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class BackgroundDownload : ObservableObject
    {
        #region Private Fields

        private double _bytesReceived;
        private double _bytesToReceive = 10;
        private string _status = "Waiting";

        #endregion

        #region Constructor

        public BackgroundDownload(DownloadOperation downloadOperation)
        {
            DownloadOperation = downloadOperation;
        }

        #endregion

        #region Public Properties

        public double BytesToReceive
        {
            get { return _bytesToReceive; }
            set { Set(ref _bytesToReceive, value); }
        }

        public double BytesReceived
        {
            get { return _bytesReceived; }
            set { Set(ref _bytesReceived, value); }
        }

        public string Status
        {
            get { return _status; }
            set { Set(ref _status, value); }
        }

        public DownloadOperation DownloadOperation { get; private set; }

        #endregion
    }
}