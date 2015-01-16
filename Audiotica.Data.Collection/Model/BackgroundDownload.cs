#region

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Networking.BackgroundTransfer;

#endregion

namespace Audiotica.Data.Collection.Model
{
    public class BackgroundDownload : INotifyPropertyChanged
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
            CancellationTokenSrc = new CancellationTokenSource();
        }

        #endregion

        #region Public Properties

        public double BytesToReceive
        {
            get { return _bytesToReceive; }
            set
            {
                _bytesToReceive = value;
                OnPropertyChanged();
            }
        }

        public double BytesReceived
        {
            get { return _bytesReceived; }
            set
            {
                _bytesReceived = value;
                OnPropertyChanged();
            }
        }

        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public DownloadOperation DownloadOperation { get; private set; }

        public CancellationTokenSource CancellationTokenSrc { get; set; }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}