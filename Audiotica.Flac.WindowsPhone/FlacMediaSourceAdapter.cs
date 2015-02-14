using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace Audiotica.Flac.WindowsPhone
{
    public sealed class FlacMediaSourceAdapter : IDisposable
    {
        private ConcurrentQueue<IBuffer> _buffersQueue;
        private double _currentTime;
        private FlacMediaDecoder _mediaDecoder;
        private MediaStreamSource _mediaSource;
        private const int SAMPLE_BUFFER_SIZE = 2048;

        private FlacMediaSourceAdapter()
        {
            _buffersQueue = new ConcurrentQueue<IBuffer>();
            _mediaDecoder = new FlacMediaDecoder();
        }

        public IMediaSource MediaSource { get { return _mediaSource; } }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public static async Task<FlacMediaSourceAdapter> CreateAsync(string filePath)
        {
            var storageFile = await StorageFile.GetFileFromPathAsync(filePath);
            return await CreateAsync(storageFile);
        }

        public static async Task<FlacMediaSourceAdapter> CreateAsync(IStorageFile file)
        {
            var fileStream = await file.OpenAsync(FileAccessMode.Read);
            var adapter = new FlacMediaSourceAdapter();
            adapter.Initialize(fileStream);

            return adapter;
        }

        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_mediaDecoder != null)
                {
                    _mediaDecoder.Dispose();
                    _mediaDecoder = null;
                }

                _buffersQueue = null;
                _mediaSource = null;
            }
        }

        private IBuffer GetBuffer()
        {
            IBuffer buffer;
            var dequeued = _buffersQueue.TryDequeue(out buffer);
            if (!dequeued) buffer = new Buffer(SAMPLE_BUFFER_SIZE);
            return buffer;
        }

        private void Initialize(IRandomAccessStream fileStream)
        {
            _mediaDecoder.Initialize(fileStream);
            var streamInfo = _mediaDecoder.GetStreamInfo();

            var encodingProperties = AudioEncodingProperties.CreatePcm(
                streamInfo.SampleRate, streamInfo.ChannelCount, streamInfo.BitsPerSample);

            _mediaSource = new MediaStreamSource(new AudioStreamDescriptor(encodingProperties));
            _mediaSource.Starting += OnMediaSourceStarting;
            _mediaSource.SampleRequested += OnMediaSourceSampleRequested;
            _mediaSource.Closed += OnMediaSourceClosed;

            _mediaSource.Duration = TimeSpan.FromSeconds(streamInfo.Duration);
            _mediaSource.BufferTime = TimeSpan.Zero;
            _mediaSource.CanSeek = true;
        }

        private void OnMediaSourceClosed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs e)
        {
            _currentTime = 0.0;
            if (_mediaDecoder != null)
            {
                _mediaDecoder.Finish();
            }
        }

        private void OnMediaSourceSampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs e)
        {
            var instantBuffer = GetBuffer();
            var buffer = _mediaDecoder.ReadSample(instantBuffer, instantBuffer.Capacity);

            MediaStreamSample sample = null;

            if (buffer.Length > 0)
            {
                sample = MediaStreamSample.CreateFromBuffer(buffer, TimeSpan.FromSeconds(_currentTime));
                sample.Processed += OnSampleProcessed;

                var duration = _mediaDecoder.GetDurationFromBufferSize(buffer.Length);
                sample.Duration = TimeSpan.FromSeconds(duration);

                _currentTime += duration;
            }
            else
            {
                _currentTime = 0.0;
                _mediaDecoder.Seek(0);
            }

            e.Request.Sample = sample;
        }

        private void OnMediaSourceStarting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs e)
        {
            if (e.Request.StartPosition.HasValue)
            {
                _currentTime = e.Request.StartPosition.Value.TotalSeconds;
                _mediaDecoder.Seek(e.Request.StartPosition.Value);
            }
            e.Request.SetActualStartPosition(TimeSpan.FromSeconds(_currentTime));
        }

        private void OnSampleProcessed(MediaStreamSample sender, object args)
        {
            _buffersQueue.Enqueue(sender.Buffer);
            sender.Processed -= OnSampleProcessed;
        }

        ~FlacMediaSourceAdapter()
        {
            Dispose(false);
        }
    }
}