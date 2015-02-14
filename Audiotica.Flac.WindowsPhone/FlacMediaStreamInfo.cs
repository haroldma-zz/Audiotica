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

namespace Audiotica.Flac.WindowsPhone
{
    /// <summary>
    /// Represents media stream info.
    /// </summary>
    public sealed class FlacMediaStreamInfo
    {
        public FlacMediaStreamInfo(double duration, long bytesPerSec,
            uint bitsPerSample, uint sampleRate, uint channelCount)
        {
            this.Duration = duration;
            this.BytesPerSecond = bytesPerSec;
            this.BitsPerSample = bitsPerSample;
            this.SampleRate = sampleRate;
            this.ChannelCount = channelCount;
        }

        /// <summary>
        /// Gets a duration of an audio stream.
        /// </summary>
        public double Duration { get; private set; }

        /// <summary>
        /// Get an average bytes per second rate.
        /// </summary>
        public long BytesPerSecond { get; private set; }

        /// <summary>
        /// Gets bits per sample rate.
        /// </summary>
        public uint BitsPerSample { get; private set; }

        /// <summary>
        /// Get sample rate.
        /// </summary>
        public uint SampleRate { get; private set; }

        /// <summary>
        /// Gets audio channels count.
        /// </summary>
        public uint ChannelCount { get; private set; }
    }
}
