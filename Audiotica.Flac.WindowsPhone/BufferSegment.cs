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
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace Audiotica.Flac.WindowsPhone
{
    /// <summary>
    /// Delimits a section of a Windows Runtime buffer.
    /// </summary>
    public struct BufferSegment
    {
        private readonly IBuffer _buffer;
        private readonly uint _count;
        private readonly uint _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferSegment" /> structure
        /// that delimits all the elements in the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to wrap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        public BufferSegment(IBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            this._buffer = buffer;
            this._offset = 0;
            this._count = buffer.Length;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferSegment" /> structure
        /// that delimits the specified range of the elements in the specified array.
        /// </summary>
        /// <param name="buffer">The buffer containing the range of elements to delimit.</param>
        /// <param name="offset">The zero-based index of the first element in the range.</param>
        /// <param name="count">The number of elements in the range.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="offset" /> and <paramref name="count" /> do not specify
        /// a valid range in <paramref name="buffer" />.
        /// </exception>
        public BufferSegment(IBuffer buffer, uint offset, uint count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Invalid offset or count specified.");

            this._buffer = buffer;
            this._offset = offset;
            this._count = count;
        }

        /// <summary>
        /// Gets the original buffer containing the range of elements that the buffer segment delimits.
        /// </summary>
        public IBuffer Buffer
        {
            get
            {
                Contract.Assert(
                    (null == this._buffer && 0 == this._offset && 0 == this._count) ||
                    (null != this._buffer && this._offset + this._count <= this._buffer.Length),
                    "ArraySegment is invalid");

                return this._buffer;
            }
        }

        /// <summary>
        /// Gets the position of the first element in the range delimited by the buffer segment,
        /// relative to the start of the original buffer.
        /// </summary>
        public uint Offset
        {
            get
            {
                Contract.Assert(
                    (null == this._buffer && 0 == this._offset && 0 == this._count) ||
                    (null != this._buffer && this._offset + this._count <= this._buffer.Length),
                    "ArraySegment is invalid");

                return this._offset;
            }
        }

        /// <summary>
        /// Gets the number of elements in the range delimited by the buffer segment.
        /// </summary>
        public uint Count
        {
            get
            {
                Contract.Assert(
                    (null == this._buffer && 0 == this._offset && 0 == this._count) ||
                    (null != this._buffer && this._offset + this._count <= this._buffer.Length),
                    "ArraySegment is invalid");

                return this._count;
            }
        }

        /// <summary>
        /// Returns the hash code for the current instance.
        /// </summary>
        public override int GetHashCode()
        {
            return this._buffer == null ? 0 : unchecked((int) (this._buffer.GetHashCode() ^ this._offset ^ this._count));
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        public override bool Equals(Object obj)
        {
            return obj is BufferSegment && this.Equals((BufferSegment) obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="BufferSegment" /> structure is equal to the current instance.
        /// </summary>
        public bool Equals(BufferSegment obj)
        {
            return (obj._buffer == this._buffer || obj._buffer.IsSameData(this._buffer))
                   && obj._offset == this._offset && obj._count == this._count;
        }

        /// <summary>
        /// Indicates whether two <see cref="BufferSegment" /> structures are equal.
        /// </summary>
        public static bool operator ==(BufferSegment left, BufferSegment right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two <see cref="BufferSegment" /> structures are unequal.
        /// </summary>
        public static bool operator !=(BufferSegment left, BufferSegment right)
        {
            return !(left == right);
        }
    }
}
