/* libFLAC_winrt - FLAC library for Windows Runtime
 * Copyright (C) 2014  Alexander Ovchinnikov
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 * - Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 *
 * - Neither the name of copyright holder nor the names of project's
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT HOLDER
 * OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using FLAC.WindowsRuntime.Decoder;
using FLAC.WindowsRuntime.Decoder.Callbacks;
using FLAC.WindowsRuntime.Format;
using Buffer = Windows.Storage.Streams.Buffer;

namespace Audiotica.Flac.WindowsPhone
{
    public sealed class FlacMediaDecoder
    {
        private static readonly BufferSegment _noCurrentData = new BufferSegment(new Buffer(0));

        private readonly StreamDecoder _streamDecoder;
        private BufferSegment _currentData;

        private IRandomAccessStream _fileStream;

        private bool _isMetadataRead;
        private FlacMediaStreamInfo _streamInfo;

        public FlacMediaDecoder()
        {
            this._streamDecoder = new StreamDecoder();
            this._streamDecoder.WriteCallback += this.WriteCallback;
            this._streamDecoder.MetadataCallback += this.MetadataCallback;
            this._currentData = _noCurrentData;
        }

        public ulong Position
        {
            get { return this._fileStream != null ? this._fileStream.Position : 0; }
        }

        public void Dispose()
        {
            this.Finish();

            this._streamDecoder.WriteCallback -= this.WriteCallback;
            this._streamDecoder.MetadataCallback -= this.MetadataCallback;

            this._streamDecoder.Dispose();
        }

        public void Initialize(IRandomAccessStream fileStream)
        {
            if (!this._streamDecoder.IsValid)
                throw new InvalidOperationException("Decoder is not valid.");

            this._fileStream = fileStream;

            StreamDecoderInitStatus decoderInitStatus = this._streamDecoder.Init(this._fileStream);
            if (decoderInitStatus != StreamDecoderInitStatus.OK)
            {
                this._streamDecoder.Finish();
                this._streamDecoder.Dispose();
                throw new InvalidOperationException("Failed to initialize decoder.");
            }
        }

        public FlacMediaStreamInfo GetStreamInfo()
        {
            this.EnsureMetadataRead();
            return this._streamInfo;
        }

        public IBuffer ReadSample(IBuffer buffer, uint count)
        {
            if (buffer == null)
                throw new ArgumentNullException();

            if (count > buffer.Capacity)
                throw new ArgumentOutOfRangeException();

            if (this._currentData.Count >= count)
            {
                this._currentData.Buffer.CopyTo(this._currentData.Offset, buffer, 0, count);
                this._currentData = new BufferSegment(this._currentData.Buffer,
                    this._currentData.Offset + count, this._currentData.Count - count);
                buffer.Length = count;
                return buffer;
            }

            uint read = this._currentData.Count;
            if (read > 0)
                this._currentData.Buffer.CopyTo(this._currentData.Offset, buffer, 0, this._currentData.Count);
            this._currentData = _noCurrentData;

            while (this.RequestSample())
            {
                uint rest = count - read;
                if (this._currentData.Count >= rest)
                {
                    this._currentData.Buffer.CopyTo(0, buffer, read, rest);
                    read += rest;
                    this._currentData = new BufferSegment(this._currentData.Buffer, rest, this._currentData.Count - rest);
                    break;
                }
                this._currentData.Buffer.CopyTo(0, buffer, read, this._currentData.Count);
                read += this._currentData.Count;
            }

            buffer.Length = read;
            return buffer;
        }

        private bool RequestSample()
        {
            this.EnsureMetadataRead();

            bool result = this._streamDecoder.ProcessSingle();
            if (!result)
                this._currentData = _noCurrentData;

            return this._currentData.Count > 0;
        }

        public void Seek(TimeSpan position)
        {
            var count = _streamDecoder.GetTotalSamples();
            var sample = _streamInfo.SampleRate * position.TotalSeconds;
            if (sample > count)
                sample = count;

            _streamDecoder.SeekAbsolute((ulong)sample);
        }

        public void Seek(ulong position)
        {
            if (this.Position == position)
                return;

            this.EnsureMetadataRead();
            if (this._streamInfo.BitsPerSample == 0)
                throw new InvalidOperationException("Cannot seek current stream.");

            bool result = this._streamDecoder.SeekAbsolute(position/this._streamInfo.BitsPerSample);
            if (!result)
                throw new ArgumentOutOfRangeException("position", "Position overflow.");
        }

        public void Finish()
        {
            this._streamDecoder.Finish();
            this._fileStream = null;
        }

        private void EnsureMetadataRead()
        {
            if (this._isMetadataRead)
                return;

            bool result = this._streamDecoder.ProcessUntilEndOfMetadata();
            StreamDecoderState state = this._streamDecoder.GetState();

            if (!result || state == StreamDecoderState.EndOfStream)
                throw new EndOfStreamException("No metadata found, or unexpected call.");

            this._isMetadataRead = true;
        }

        private void WriteCallback(object sender, StreamDecoderWriteEventArgs e)
        {
            IBuffer currentSample = e.GetBuffer();
            GC.AddMemoryPressure(currentSample.Capacity);
            this._currentData = new BufferSegment(currentSample);
            e.SetResult(StreamDecoderWriteStatus.Continue);
        }

        private void MetadataCallback(object sender, StreamDecoderMetadataEventArgs e)
        {
            if (e.Metadata.Type == MetadataType.StreamInfo && e.Metadata.StreamInfo != null)
            {
                uint blockAlign = e.Metadata.StreamInfo.Channels*(e.Metadata.StreamInfo.BitsPerSample/8);
                uint avgBytesPerSec = e.Metadata.StreamInfo.SampleRate*blockAlign;

                double duration = (double) e.Metadata.StreamInfo.TotalSamples/e.Metadata.StreamInfo.SampleRate;

                this._streamInfo = new FlacMediaStreamInfo(
                    duration, avgBytesPerSec,
                    e.Metadata.StreamInfo.BitsPerSample,
                    e.Metadata.StreamInfo.SampleRate,
                    e.Metadata.StreamInfo.Channels);
            }
        }

        /// <summary>
        /// Converts specified sample's buffer size to a sample's duration.
        /// </summary>
        /// <param name="bufferSize">Sample's buffer size.</param>
        /// <returns>Sample's duration.</returns>
        /// <exception cref="EndOfStreamException">This stream contains no data.</exception>
        public double GetDurationFromBufferSize(uint bufferSize)
        {
            FlacMediaStreamInfo streamInfo = this.GetStreamInfo();

            if (streamInfo.BytesPerSecond == 0)
                return 0;

            return (double) bufferSize/streamInfo.BytesPerSecond;
        }

        /// <summary>
        /// Converts specified sample's duration to a sample's buffer size.
        /// </summary>
        /// <param name="duration">Sample's duration.</param>
        /// <returns>Sample's buffer size.</returns>
        /// <exception cref="System.IO.EndOfStreamException">This stream contains no data.</exception>
        public uint GetBufferSizeFromDuration(double duration)
        {
            FlacMediaStreamInfo streamInfo = this.GetStreamInfo();
            return (uint) (duration*streamInfo.BytesPerSecond);
        }
    }
}