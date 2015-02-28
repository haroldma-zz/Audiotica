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

#ifndef FLACRT__DECODER_H
#define FLACRT__DECODER_H

#include "FLAC_winrt/format.h"
#include "FLAC/stream_decoder.h"
#include "private/deferral.h"


/** \file include/FLAC++/decoder.h
 *
 *  \brief
 *  This module contains the classes which implement the various
 *  decoders.
 *
 *  See the detailed documentation in the
 *  \link flacpp_decoder decoder \endlink module.
 */

/** \defgroup flacpp_decoder FLAC++/decoder.h: decoder classes
 *  \ingroup flacpp
 *
 *  \brief
 *  This module describes the decoder layers provided by libFLAC++.
 *
 * The libFLAC++ decoder classes are object wrappers around their
 * counterparts in libFLAC.  All decoding layers available in
 * libFLAC are also provided here.  The interface is very similar;
 * make sure to read the \link flac_decoder libFLAC decoder module \endlink.
 *
 * There are only two significant differences here.  First, instead of
 * passing in C function pointers for callbacks, you inherit from the
 * decoder class and provide implementations for the callbacks in your
 * derived class; because of this there is no need for a 'client_data'
 * property.
 *
 * Second, there are two stream decoder classes.  FLAC::Decoder::Stream
 * is used for the same cases that FLAC__stream_decoder_init_stream() /
 * FLAC__stream_decoder_init_ogg_stream() are used, and FLAC::Decoder::File
 * is used for the same cases that
 * FLAC__stream_decoder_init_FILE() and FLAC__stream_decoder_init_file() /
 * FLAC__stream_decoder_init_ogg_FILE() and FLAC__stream_decoder_init_ogg_file()
 * are used.
 */

namespace FLAC {

	namespace WindowsRuntime {

		namespace Decoder {

			/** State values for a FLAC__StreamDecoder
			*
			* The decoder's state can be obtained by calling FLAC__stream_decoder_get_state().
			*/
			public enum class StreamDecoderState {

				SearchForMetadata = FLAC__STREAM_DECODER_SEARCH_FOR_METADATA,
				/**< The decoder is ready to search for metadata. */

				ReadMetadata = FLAC__STREAM_DECODER_READ_METADATA,
				/**< The decoder is ready to or is in the process of reading metadata. */

				SearchForFrameSync = FLAC__STREAM_DECODER_SEARCH_FOR_FRAME_SYNC,
				/**< The decoder is ready to or is in the process of searching for the
				* frame sync code.
				*/

				ReadFrame = FLAC__STREAM_DECODER_READ_FRAME,
				/**< The decoder is ready to or is in the process of reading a frame. */

				EndOfStream = FLAC__STREAM_DECODER_END_OF_STREAM,
				/**< The decoder has reached the end of the stream. */

				OggError = FLAC__STREAM_DECODER_OGG_ERROR,
				/**< An error occurred in the underlying Ogg layer.  */

				SeekError = FLAC__STREAM_DECODER_SEEK_ERROR,
				/**< An error occurred while seeking.  The decoder must be flushed
				* with FLAC__stream_decoder_flush() or reset with
				* FLAC__stream_decoder_reset() before decoding can continue.
				*/

				Aborted = FLAC__STREAM_DECODER_ABORTED,
				/**< The decoder was aborted by the read callback. */

				MemoryAllocationError = FLAC__STREAM_DECODER_MEMORY_ALLOCATION_ERROR,
				/**< An error occurred allocating memory.  The decoder is in an invalid
				* state and can no longer be used.
				*/

				Uninitialized = FLAC__STREAM_DECODER_UNINITIALIZED
				/**< The decoder is in the uninitialized state; one of the
				* FLAC__stream_decoder_init_*() functions must be called before samples
				* can be processed.
				*/

			};


			/** This class is a wrapper around FLAC__StreamDecoderInitStatus.
			*/
			public enum class StreamDecoderInitStatus {

				OK = FLAC__STREAM_DECODER_INIT_STATUS_OK,
				/**< Initialization was successful. */

				UnsupportedContainer = FLAC__STREAM_DECODER_INIT_STATUS_UNSUPPORTED_CONTAINER,
				/**< The library was not compiled with support for the given container
				* format.
				*/

				InvalidCallbacks = FLAC__STREAM_DECODER_INIT_STATUS_INVALID_CALLBACKS,
				/**< A required callback was not supplied. */

				MemoryAllocationError = FLAC__STREAM_DECODER_INIT_STATUS_MEMORY_ALLOCATION_ERROR,
				/**< An error occurred allocating memory. */

				ErrorOpeningFile = FLAC__STREAM_DECODER_INIT_STATUS_ERROR_OPENING_FILE,
				/**< fopen() failed in FLAC__stream_decoder_init_file() or
				* FLAC__stream_decoder_init_ogg_file(). */

				AlreadyInitialized = FLAC__STREAM_DECODER_INIT_STATUS_ALREADY_INITIALIZED
				/**< FLAC__stream_decoder_init_*() was called when the decoder was
				* already initialized, usually because
				* FLAC__stream_decoder_finish() was not called.
				*/
			};


			namespace Callbacks {

				/** Return values for the FLAC__StreamDecoder read callback.
				*/
				public enum class StreamDecoderReadStatus {

					Continue = FLAC__STREAM_DECODER_READ_STATUS_CONTINUE,
					/**< The read was OK and decoding can continue. */

					EndOfStream = FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM,
					/**< The read was attempted while at the end of the stream.  Note that
					* the client must only return this value when the read callback was
					* called when already at the end of the stream.  Otherwise, if the read
					* itself moves to the end of the stream, the client should still return
					* the data and \c FLAC__STREAM_DECODER_READ_STATUS_CONTINUE, and then on
					* the next read callback it should return
					* \c FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM with a byte count
					* of \c 0.
					*/

					Abort = FLAC__STREAM_DECODER_READ_STATUS_ABORT
					/**< An unrecoverable error occurred.  The decoder will return from the process call. */

				};


				/** Return values for the FLAC__StreamDecoder seek callback.
				*/
				public enum class StreamDecoderSeekStatus {

					OK = FLAC__STREAM_DECODER_SEEK_STATUS_OK,
					/**< The seek was OK and decoding can continue. */

					Error = FLAC__STREAM_DECODER_SEEK_STATUS_ERROR,
					/**< An unrecoverable error occurred.  The decoder will return from the process call. */

					Unsupported = FLAC__STREAM_DECODER_SEEK_STATUS_UNSUPPORTED
					/**< Client does not support seeking. */

				};


				/** Return values for the FLAC__StreamDecoder tell callback.
				*/
				public enum class StreamDecoderTellStatus {

					OK = FLAC__STREAM_DECODER_TELL_STATUS_OK,
					/**< The tell was OK and decoding can continue. */

					Error = FLAC__STREAM_DECODER_TELL_STATUS_ERROR,
					/**< An unrecoverable error occurred.  The decoder will return from the process call. */

					Unsupported = FLAC__STREAM_DECODER_TELL_STATUS_UNSUPPORTED
					/**< Client does not support telling the position. */

				};


				/** Return values for the FLAC__StreamDecoder length callback.
				*/
				public enum class StreamDecoderLengthStatus {

					OK = FLAC__STREAM_DECODER_LENGTH_STATUS_OK,
					/**< The length call was OK and decoding can continue. */

					Error = FLAC__STREAM_DECODER_LENGTH_STATUS_ERROR,
					/**< An unrecoverable error occurred.  The decoder will return from the process call. */

					Unsupported = FLAC__STREAM_DECODER_LENGTH_STATUS_UNSUPPORTED
					/**< Client does not support reporting the length. */

				};


				/** Return values for the FLAC__StreamDecoder write callback.
				*/
				public enum class StreamDecoderWriteStatus {

					Continue = FLAC__STREAM_DECODER_WRITE_STATUS_CONTINUE,
					/**< The write was OK and decoding can continue. */

					Abort = FLAC__STREAM_DECODER_WRITE_STATUS_ABORT
					/**< An unrecoverable error occurred.  The decoder will return from the process call. */

				};


				/** Possible values passed back to the FLAC__StreamDecoder error callback.
				*  \c FLAC__STREAM_DECODER_ERROR_STATUS_LOST_SYNC is the generic catch-
				*  all.  The rest could be caused by bad sync (false synchronization on
				*  data that is not the start of a frame) or corrupted data.  The error
				*  itself is the decoder's best guess at what happened assuming a correct
				*  sync.  For example \c FLAC__STREAM_DECODER_ERROR_STATUS_BAD_HEADER
				*  could be caused by a correct sync on the start of a frame, but some
				*  data in the frame header was corrupted.  Or it could be the result of
				*  syncing on a point the stream that looked like the starting of a frame
				*  but was not.  \c FLAC__STREAM_DECODER_ERROR_STATUS_UNPARSEABLE_STREAM
				*  could be because the decoder encountered a valid frame made by a future
				*  version of the encoder which it cannot parse, or because of a false
				*  sync making it appear as though an encountered frame was generated by
				*  a future encoder.
				*/
				public enum class StreamDecoderErrorStatus {

					LostSync = FLAC__STREAM_DECODER_ERROR_STATUS_LOST_SYNC,
					/**< An error in the stream caused the decoder to lose synchronization. */

					BadHeader = FLAC__STREAM_DECODER_ERROR_STATUS_BAD_HEADER,
					/**< The decoder encountered a corrupted frame header. */

					FrameCRCMismatch = FLAC__STREAM_DECODER_ERROR_STATUS_FRAME_CRC_MISMATCH,
					/**< The frame's data did not match the CRC in the footer. */

					UnparseableStream = FLAC__STREAM_DECODER_ERROR_STATUS_UNPARSEABLE_STREAM
					/**< The decoder encountered reserved fields in use in the stream. */

				};


				/** Signature for the read callback.
				*
				*  A function pointer matching this signature must be passed to
				*  FLAC__stream_decoder_init*_stream(). The supplied function will be
				*  called when the decoder needs more input data.  The address of the
				*  buffer to be filled is supplied, along with the number of bytes the
				*  buffer can hold.  The callback may choose to supply less data and
				*  modify the byte count but must be careful not to overflow the buffer.
				*  The callback then returns a status code chosen from
				*  FLAC__StreamDecoderReadStatus.
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  buffer   A pointer to a location for the callee to store
				*                  data to be decoded.
				* \param  bytes    A pointer to the size of the buffer.  On entry
				*                  to the callback, it contains the maximum number
				*                  of bytes that may be stored in \a buffer.  The
				*                  callee must set it to the actual number of bytes
				*                  stored (0 in case of error or end-of-stream) before
				*                  returning.
				* \retval FLAC__StreamDecoderReadStatus
				*    The callee's return status.  Note that the callback should return
				*    \c FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM if and only if
				*    zero bytes were read and there is no more data to be read.
				*/
				public ref class StreamDecoderReadEventArgs sealed {
				public:
					IDeferral^ GetDeferral() {
						return deferral_manager_.GetDeferral();
					}

					property Platform::Array<FLAC__byte>^ Buffer {
						Platform::Array<FLAC__byte>^ get() {
							return Platform::ArrayReference<FLAC__byte>(buffer_, *bytes_);
						}
					}

					void SetResult(size_t bytes, StreamDecoderReadStatus result) {
						*bytes_ = bytes;
						result_ = (::FLAC__StreamDecoderReadStatus)(int)result;
						handled_ = true;
					}

				internal:
					StreamDecoderReadEventArgs(FLAC__byte *buffer, size_t *bytes)
						: buffer_(buffer), bytes_(bytes), deferral_manager_(DeferralManager()) { }

					property ::FLAC__StreamDecoderReadStatus Result {
						::FLAC__StreamDecoderReadStatus get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

					Concurrency::task<void> WaitForDeferralsAsync() {
						return deferral_manager_.SignalAndWaitAsync();
					}

				private:
					DeferralManager deferral_manager_;

					FLAC__byte *buffer_;
					size_t *bytes_;

					bool handled_;
					::FLAC__StreamDecoderReadStatus result_;
				};


				/** Signature for the seek callback.
				*
				*  A function pointer matching this signature may be passed to
				*  FLAC__stream_decoder_init*_stream().  The supplied function will be
				*  called when the decoder needs to seek the input stream.  The decoder
				*  will pass the absolute byte offset to seek to, 0 meaning the
				*  beginning of the stream.
				*
				* Here is an example of a seek callback for stdio streams:
				* \code
				* FLAC__StreamDecoderSeekStatus seek_cb(const FLAC__StreamDecoder *decoder, FLAC__uint64 absolute_byte_offset, void *client_data)
				* {
				*   FILE *file = ((MyClientData*)client_data)->file;
				*   if(file == stdin)
				*     return FLAC__STREAM_DECODER_SEEK_STATUS_UNSUPPORTED;
				*   else if(fseeko(file, (off_t)absolute_byte_offset, SEEK_SET) < 0)
				*     return FLAC__STREAM_DECODER_SEEK_STATUS_ERROR;
				*   else
				*     return FLAC__STREAM_DECODER_SEEK_STATUS_OK;
				* }
				* \endcode
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  absolute_byte_offset  The offset from the beginning of the stream
				*                               to seek to.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				* \retval FLAC__StreamDecoderSeekStatus
				*    The callee's return status.
				*/
				public ref class StreamDecoderSeekEventArgs sealed {
				public:
					property FLAC__uint64 AbsoluteByteOffset {
						FLAC__uint64 get() { return absoluteByteOffset_; }
					}

					void SetResult(StreamDecoderSeekStatus result) {
						result_ = (::FLAC__StreamDecoderSeekStatus)(int)result;
						handled_ = true;
					}

				internal:
					StreamDecoderSeekEventArgs(const FLAC__uint64 &absoluteByteOffset)
						: absoluteByteOffset_(absoluteByteOffset) { }

					property ::FLAC__StreamDecoderSeekStatus Result {
						::FLAC__StreamDecoderSeekStatus get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

				private:
					const FLAC__uint64 &absoluteByteOffset_;

					bool handled_;
					::FLAC__StreamDecoderSeekStatus result_;
				};


				/** Signature for the tell callback.
				*
				*  A function pointer matching this signature may be passed to
				*  FLAC__stream_decoder_init*_stream().  The supplied function will be
				*  called when the decoder wants to know the current position of the
				*  stream.  The callback should return the byte offset from the
				*  beginning of the stream.
				*
				* Here is an example of a tell callback for stdio streams:
				* \code
				* FLAC__StreamDecoderTellStatus tell_cb(const FLAC__StreamDecoder *decoder, FLAC__uint64 *absolute_byte_offset, void *client_data)
				* {
				*   FILE *file = ((MyClientData*)client_data)->file;
				*   off_t pos;
				*   if(file == stdin)
				*     return FLAC__STREAM_DECODER_TELL_STATUS_UNSUPPORTED;
				*   else if((pos = ftello(file)) < 0)
				*     return FLAC__STREAM_DECODER_TELL_STATUS_ERROR;
				*   else {
				*     *absolute_byte_offset = (FLAC__uint64)pos;
				*     return FLAC__STREAM_DECODER_TELL_STATUS_OK;
				*   }
				* }
				* \endcode
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  absolute_byte_offset  A pointer to storage for the current offset
				*                               from the beginning of the stream.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				* \retval FLAC__StreamDecoderTellStatus
				*    The callee's return status.
				*/
				public ref class StreamDecoderTellEventArgs sealed {
				public:
					void SetResult(FLAC__uint64 absoluteByteOffset, StreamDecoderTellStatus result) {
						*absoluteByteOffset_ = absoluteByteOffset;
						result_ = (::FLAC__StreamDecoderTellStatus)(int)result;
						handled_ = true;
					}

				internal:
					StreamDecoderTellEventArgs(FLAC__uint64 *absoluteByteOffset)
						: absoluteByteOffset_(absoluteByteOffset) { }

					property ::FLAC__StreamDecoderTellStatus Result {
						::FLAC__StreamDecoderTellStatus get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

				private:
					FLAC__uint64 *absoluteByteOffset_;

					bool handled_;
					::FLAC__StreamDecoderTellStatus result_;
				};


				/** Signature for the length callback.
				*
				*  A function pointer matching this signature may be passed to
				*  FLAC__stream_decoder_init*_stream().  The supplied function will be
				*  called when the decoder wants to know the total length of the stream
				*  in bytes.
				*
				* Here is an example of a length callback for stdio streams:
				* \code
				* FLAC__StreamDecoderLengthStatus length_cb(const FLAC__StreamDecoder *decoder, FLAC__uint64 *stream_length, void *client_data)
				* {
				*   FILE *file = ((MyClientData*)client_data)->file;
				*   struct stat filestats;
				*
				*   if(file == stdin)
				*     return FLAC__STREAM_DECODER_LENGTH_STATUS_UNSUPPORTED;
				*   else if(fstat(fileno(file), &filestats) != 0)
				*     return FLAC__STREAM_DECODER_LENGTH_STATUS_ERROR;
				*   else {
				*     *stream_length = (FLAC__uint64)filestats.st_size;
				*     return FLAC__STREAM_DECODER_LENGTH_STATUS_OK;
				*   }
				* }
				* \endcode
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  stream_length  A pointer to storage for the length of the stream
				*                        in bytes.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				* \retval FLAC__StreamDecoderLengthStatus
				*    The callee's return status.
				*/
				public ref class StreamDecoderLengthEventArgs sealed {
				public:
					void SetResult(FLAC__uint64 streamLength, StreamDecoderLengthStatus result) {
						*streamLength_ = streamLength;
						result_ = (::FLAC__StreamDecoderLengthStatus)(int)result;
						handled_ = true;
					}

				internal:
					StreamDecoderLengthEventArgs(FLAC__uint64 *streamLength)
						: streamLength_(streamLength) { }

					property ::FLAC__StreamDecoderLengthStatus Result {
						::FLAC__StreamDecoderLengthStatus get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

				private:
					FLAC__uint64 *streamLength_;

					bool handled_;
					::FLAC__StreamDecoderLengthStatus result_;
				};


				/** Signature for the EOF callback.
				*
				*  A function pointer matching this signature may be passed to
				*  FLAC__stream_decoder_init*_stream().  The supplied function will be
				*  called when the decoder needs to know if the end of the stream has
				*  been reached.
				*
				* Here is an example of a EOF callback for stdio streams:
				* FLAC__bool eof_cb(const FLAC__StreamDecoder *decoder, void *client_data)
				* \code
				* {
				*   FILE *file = ((MyClientData*)client_data)->file;
				*   return feof(file)? true : false;
				* }
				* \endcode
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				* \retval FLAC__bool
				*    \c true if the currently at the end of the stream, else \c false.
				*/
				public ref class StreamDecoderEofEventArgs sealed {
				public:
					void SetResult(bool result) {
						result_ = result ? TRUE : FALSE;
						handled_ = true;
					}

				internal:
					StreamDecoderEofEventArgs() { }

					property ::FLAC__bool Result {
						::FLAC__bool get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

				private:
					bool handled_;
					::FLAC__bool result_;
				};


				/** Signature for the write callback.
				*
				*  A function pointer matching this signature must be passed to one of
				*  the FLAC__stream_decoder_init_*() functions.
				*  The supplied function will be called when the decoder has decoded a
				*  single audio frame.  The decoder will pass the frame metadata as well
				*  as an array of pointers (one for each channel) pointing to the
				*  decoded audio.
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  frame    The description of the decoded frame.  See
				*                  FLAC__Frame.
				* \param  buffer   An array of pointers to decoded channels of data.
				*                  Each pointer will point to an array of signed
				*                  samples of length \a frame->header.blocksize.
				*                  Channels will be ordered according to the FLAC
				*                  specification; see the documentation for the
				*                  <A HREF="../format.html#frame_header">frame header</A>.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				* \retval FLAC__StreamDecoderWriteStatus
				*    The callee's return status.
				*/
				public ref class StreamDecoderWriteEventArgs sealed {
				public:
					IDeferral^ GetDeferral() {
						return deferral_manager_.GetDeferral();
					}

					property Format::Frame^ Frame {
						Format::Frame^ get() { return frame_; }
					}

					Windows::Storage::Streams::IBuffer^ GetBuffer();

					Platform::Array<FLAC__int32>^ GetData(unsigned index);

					void SetResult(StreamDecoderWriteStatus result) {
						result_ = (::FLAC__StreamDecoderWriteStatus)(int)result;
						handled_ = true;
					}

				internal:
					StreamDecoderWriteEventArgs(const FLAC__int32 *const *data, const ::FLAC__Frame *frame)
						: data_(data), buffer_(nullptr), data_array_(nullptr), deferral_manager_(DeferralManager()) {
						frame_ = ref new Format::Frame(frame);
					}

					property ::FLAC__StreamDecoderWriteStatus Result {
						::FLAC__StreamDecoderWriteStatus get() {
							if (handled_) return result_;
							throw ref new Platform::COMException(E_NOT_SET);
						}
					}

					Concurrency::task<void> WaitForDeferralsAsync() {
						return deferral_manager_.SignalAndWaitAsync();
					}

				private:
					DeferralManager deferral_manager_;

					const FLAC__int32 *const *data_;

					Format::Frame^ frame_;
					Windows::Storage::Streams::IBuffer^ buffer_;
					Platform::Array<Platform::Object^>^ data_array_;

					bool handled_;
					::FLAC__StreamDecoderWriteStatus result_;
				};


				/** Signature for the metadata callback.
				*
				*  A function pointer matching this signature must be passed to one of
				*  the FLAC__stream_decoder_init_*() functions.
				*  The supplied function will be called when the decoder has decoded a
				*  metadata block.  In a valid FLAC file there will always be one
				*  \c STREAMINFO block, followed by zero or more other metadata blocks.
				*  These will be supplied by the decoder in the same order as they
				*  appear in the stream and always before the first audio frame (i.e.
				*  write callback).  The metadata block that is passed in must not be
				*  modified, and it doesn't live beyond the callback, so you should make
				*  a copy of it with FLAC__metadata_object_clone() if you will need it
				*  elsewhere.  Since metadata blocks can potentially be large, by
				*  default the decoder only calls the metadata callback for the
				*  \c STREAMINFO block; you can instruct the decoder to pass or filter
				*  other blocks with FLAC__stream_decoder_set_metadata_*() calls.
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  metadata The decoded metadata block.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				*/
				public ref class StreamDecoderMetadataEventArgs sealed {
				public:
					property Format::StreamMetadata^ Metadata {
						Format::StreamMetadata^ get() { return metadata_; }
					}

				internal:
					StreamDecoderMetadataEventArgs(const ::FLAC__StreamMetadata *metadata) {
						metadata_ = ref new Format::StreamMetadata(metadata);
					}

				private:
					Format::StreamMetadata^ metadata_;
				};


				/** Signature for the error callback.
				*
				*  A function pointer matching this signature must be passed to one of
				*  the FLAC__stream_decoder_init_*() functions.
				*  The supplied function will be called whenever an error occurs during
				*  decoding.
				*
				* \note In general, FLAC__StreamDecoder functions which change the
				* state should not be called on the \a decoder while in the callback.
				*
				* \param  decoder  The decoder instance calling the callback.
				* \param  status   The error encountered by the decoder.
				* \param  client_data  The callee's client data set through
				*                      FLAC__stream_decoder_init_*().
				*/
				public ref class StreamDecoderErrorEventArgs sealed {
				public:
					property StreamDecoderErrorStatus Status {
						StreamDecoderErrorStatus get() { return (StreamDecoderErrorStatus)(int)status_; }
					}

				internal:
					StreamDecoderErrorEventArgs(const ::FLAC__StreamDecoderErrorStatus &status)
						: status_(status) { }

				private:
					const ::FLAC__StreamDecoderErrorStatus &status_;
				};

			}


			/** \ingroup flacpp_decoder
			 *  \brief
			 *  This class wraps the ::FLAC__StreamDecoder.  If you are
			 *  decoding from a file, FLAC::Decoder::File may be more
			 *  convenient.
			 *
			 * The usage of this class is similar to FLAC__StreamDecoder,
			 * except instead of providing callbacks to
			 * FLAC__stream_decoder_init*_stream(), you will inherit from this
			 * class and override the virtual callback functions with your
			 * own implementations, then call init() or init_ogg().  The rest
			 * of the calls work the same as in the C layer.
			 *
			 * Only the read, write, and error callbacks are mandatory.  The
			 * others are optional; this class provides default
			 * implementations that do nothing.  In order for seeking to work
			 * you must overide seek_callback(), tell_callback(),
			 * length_callback(), and eof_callback().
			 */
			public ref class StreamDecoder sealed {
			public:
				StreamDecoder();
				virtual ~StreamDecoder();

				//@{
				/** Call after construction to check the that the object was created
				 *  successfully.  If not, use GetState() to find out why not.
				 */
				property bool IsValid { bool get(); }
				//@}

				bool SetOggSerialNumber(int value);											///< See FLAC__stream_decoder_set_ogg_serial_number()
				bool SetMd5Checking(bool value);											///< See FLAC__stream_decoder_set_md5_checking()
				bool SetMetadataRespond(Format::MetadataType type);							///< See FLAC__stream_decoder_set_metadata_respond()
				bool SetMetadataRespondApplication(const Platform::Array<FLAC__byte>^ id);	///< See FLAC__stream_decoder_set_metadata_respond_application()
				bool SetMetadataRespondAll();												///< See FLAC__stream_decoder_set_metadata_respond_all()
				bool SetMetadataIgnore(Format::MetadataType type);							///< See FLAC__stream_decoder_set_metadata_ignore()
				bool SetMetadataIgnoreApplication(const Platform::Array<FLAC__byte>^ id);	///< See FLAC__stream_decoder_set_metadata_ignore_application()
				bool SetMetadataIgnoreAll();												///< See FLAC__stream_decoder_set_metadata_ignore_all()

				StreamDecoderState GetState();								///< See FLAC__stream_decoder_get_state()
				bool GetMd5Checking();										///< See FLAC__stream_decoder_get_md5_checking()
				FLAC__uint64 GetTotalSamples();								///< See FLAC__stream_decoder_get_total_samples()
				unsigned GetChannels();										///< See FLAC__stream_decoder_get_channels()
				Format::Frames::ChannelAssignment GetChannelAssignment();	///< See FLAC__stream_decoder_get_channel_assignment()
				unsigned GetBitsPerSample();								///< See FLAC__stream_decoder_get_bits_per_sample()
				unsigned GetSampleRate();									///< See FLAC__stream_decoder_get_sample_rate()
				unsigned GetBlocksize();									///< See FLAC__stream_decoder_get_blocksize()
				bool GetDecodePosition(FLAC__uint64 *position);				///< See FLAC__stream_decoder_get_decode_position()

				StreamDecoderInitStatus Init();			///< Seek FLAC__stream_decoder_init_stream()
				StreamDecoderInitStatus Init(Windows::Storage::Streams::IRandomAccessStream^ fileStream);
				StreamDecoderInitStatus InitOgg();		///< Seek FLAC__stream_decoder_init_ogg_stream()
				StreamDecoderInitStatus InitOgg(Windows::Storage::Streams::IRandomAccessStream^ fileStream);

				bool Finish();	///< See FLAC__stream_decoder_finish()

				bool Flush();	///< See FLAC__stream_decoder_flush()
				bool Reset();	///< See FLAC__stream_decoder_reset()

				bool ProcessSingle();					///< See FLAC__stream_decoder_process_single()
				bool ProcessUntilEndOfMetadata();		///< See FLAC__stream_decoder_process_until_end_of_metadata()
				bool ProcessUntilEndOfStream();			///< See FLAC__stream_decoder_process_until_end_of_stream()
				bool SkipSingleFrame();					///< See FLAC__stream_decoder_skip_single_frame()

				bool SeekAbsolute(FLAC__uint64 sample);	///< See FLAC__stream_decoder_seek_absolute()

				/// see FLAC__StreamDecoderReadCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderReadEventArgs^>^ ReadCallback;

				/// see FLAC__StreamDecoderSeekCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderSeekEventArgs^>^ SeekCallback;

				/// see FLAC__StreamDecoderTellCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderTellEventArgs^>^ TellCallback;

				/// see FLAC__StreamDecoderLengthCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderLengthEventArgs^>^ LengthCallback;

				/// see FLAC__StreamDecoderEofCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderEofEventArgs^>^ EofCallback;

				/// see FLAC__StreamDecoderWriteCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderWriteEventArgs^>^ WriteCallback;

				/// see FLAC__StreamDecoderMetadataCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderMetadataEventArgs^>^ MetadataCallback;

				/// see FLAC__StreamDecoderErrorCallback
				event Windows::Foundation::TypedEventHandler<Platform::Object^, Callbacks::StreamDecoderErrorEventArgs^>^ ErrorCallback;

			private:
				::FLAC__StreamDecoder *decoder_;
				Windows::Storage::Streams::IRandomAccessStream^ file_stream_;
				Windows::Storage::Streams::DataReader^ file_reader_;

				static ::FLAC__StreamDecoderReadStatus read_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__byte buffer[], size_t *bytes, void *client_data);
				static ::FLAC__StreamDecoderSeekStatus seek_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 absolute_byte_offset, void *client_data);
				static ::FLAC__StreamDecoderTellStatus tell_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 *absolute_byte_offset, void *client_data);
				static ::FLAC__StreamDecoderLengthStatus length_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 *stream_length, void *client_data);
				static FLAC__bool eof_callback_(const ::FLAC__StreamDecoder *decoder, void *client_data);
				static ::FLAC__StreamDecoderWriteStatus write_callback_(const ::FLAC__StreamDecoder *decoder, const ::FLAC__Frame *frame, const FLAC__int32 * const buffer[], void *client_data);
				static void metadata_callback_(const ::FLAC__StreamDecoder *decoder, const ::FLAC__StreamMetadata *metadata, void *client_data);
				static void error_callback_(const ::FLAC__StreamDecoder *decoder, ::FLAC__StreamDecoderErrorStatus status, void *client_data);

			private:
				// Private and undefined so you can't use them:
				StreamDecoder(const StreamDecoder^&);
				void operator=(const StreamDecoder^&);
			};
		}
	}
}

#endif
