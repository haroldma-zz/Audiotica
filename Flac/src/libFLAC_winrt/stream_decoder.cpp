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

#include "FLAC_winrt/decoder.h"
#include "FLAC/assert.h"
#include "private/helper.h"


namespace FLAC {

	namespace WindowsRuntime {

		namespace Decoder {

			StreamDecoder::StreamDecoder() :
				file_stream_(nullptr), file_reader_(nullptr)
			{
				decoder_ = ::FLAC__stream_decoder_new();
			}

			StreamDecoder::~StreamDecoder()
			{
				if (0 != decoder_) {
					(void)::FLAC__stream_decoder_finish(decoder_);
					::FLAC__stream_decoder_delete(decoder_);
				}

				if (nullptr != file_reader_) {
					(void)file_reader_->DetachStream();
					delete file_reader_;
				}
			}

			bool StreamDecoder::IsValid::get()
			{
				return (0 != decoder_) && (ReferenceEquals(file_stream_, nullptr) == ReferenceEquals(file_reader_, nullptr));
			}

			bool StreamDecoder::SetOggSerialNumber(int value)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_ogg_serial_number(decoder_, value));
			}

			bool StreamDecoder::SetMd5Checking(bool value)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_md5_checking(decoder_, value));
			}

			bool StreamDecoder::SetMetadataRespond(Format::MetadataType type)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_respond(decoder_, (::FLAC__MetadataType)(int)type));
			}

			bool StreamDecoder::SetMetadataRespondApplication(const Platform::Array<FLAC__byte>^ id)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_respond_application(decoder_, id->Data));
			}

			bool StreamDecoder::SetMetadataRespondAll()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_respond_all(decoder_));
			}

			bool StreamDecoder::SetMetadataIgnore(Format::MetadataType type)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_ignore(decoder_, (::FLAC__MetadataType)(int)type));
			}

			bool StreamDecoder::SetMetadataIgnoreApplication(const Platform::Array<FLAC__byte>^ id)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_ignore_application(decoder_, id->Data));
			}

			bool StreamDecoder::SetMetadataIgnoreAll()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_set_metadata_ignore_all(decoder_));
			}

			StreamDecoderState StreamDecoder::GetState()
			{
				FLAC__ASSERT(IsValid);
				return (StreamDecoderState)(int)::FLAC__stream_decoder_get_state(decoder_);
			}

			bool StreamDecoder::GetMd5Checking()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_get_md5_checking(decoder_));
			}

			FLAC__uint64 StreamDecoder::GetTotalSamples()
			{
				FLAC__ASSERT(IsValid);
				return ::FLAC__stream_decoder_get_total_samples(decoder_);
			}

			unsigned StreamDecoder::GetChannels()
			{
				FLAC__ASSERT(IsValid);
				return ::FLAC__stream_decoder_get_channels(decoder_);
			}

			Format::Frames::ChannelAssignment StreamDecoder::GetChannelAssignment()
			{
				FLAC__ASSERT(IsValid);
				return (Format::Frames::ChannelAssignment)(int)::FLAC__stream_decoder_get_channel_assignment(decoder_);
			}

			unsigned StreamDecoder::GetBitsPerSample()
			{
				FLAC__ASSERT(IsValid);
				return ::FLAC__stream_decoder_get_bits_per_sample(decoder_);
			}

			unsigned StreamDecoder::GetSampleRate()
			{
				FLAC__ASSERT(IsValid);
				return ::FLAC__stream_decoder_get_sample_rate(decoder_);
			}

			unsigned StreamDecoder::GetBlocksize()
			{
				FLAC__ASSERT(IsValid);
				return ::FLAC__stream_decoder_get_blocksize(decoder_);
			}

			bool StreamDecoder::GetDecodePosition(FLAC__uint64 *position)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_get_decode_position(decoder_, position));
			}

			StreamDecoderInitStatus StreamDecoder::Init()
			{
				FLAC__ASSERT(IsValid);
				return (StreamDecoderInitStatus)(int)::FLAC__stream_decoder_init_stream(decoder_, read_callback_, seek_callback_, tell_callback_, length_callback_, eof_callback_, write_callback_, metadata_callback_, error_callback_, /*client_data=*/(void*)this);
			}

			StreamDecoderInitStatus StreamDecoder::Init(Windows::Storage::Streams::IRandomAccessStream^ fileStream)
			{
				FLAC__ASSERT(IsValid);
				file_stream_ = fileStream;
				file_stream_->Seek(0);
				file_reader_ = ref new Windows::Storage::Streams::DataReader(file_stream_);
				return (StreamDecoderInitStatus)(int)::FLAC__stream_decoder_init_stream(decoder_, read_callback_, seek_callback_, tell_callback_, length_callback_, eof_callback_, write_callback_, metadata_callback_, error_callback_, /*client_data=*/(void*)this);
			}

			StreamDecoderInitStatus StreamDecoder::InitOgg()
			{
				FLAC__ASSERT(IsValid);
				return (StreamDecoderInitStatus)(int)::FLAC__stream_decoder_init_ogg_stream(decoder_, read_callback_, seek_callback_, tell_callback_, length_callback_, eof_callback_, write_callback_, metadata_callback_, error_callback_, /*client_data=*/(void*)this);
			}

			StreamDecoderInitStatus StreamDecoder::InitOgg(Windows::Storage::Streams::IRandomAccessStream^ fileStream)
			{
				FLAC__ASSERT(IsValid);
				file_stream_ = fileStream;
				file_stream_->Seek(0);
				file_reader_ = ref new Windows::Storage::Streams::DataReader(file_stream_);
				return (StreamDecoderInitStatus)(int)::FLAC__stream_decoder_init_ogg_stream(decoder_, read_callback_, seek_callback_, tell_callback_, length_callback_, eof_callback_, write_callback_, metadata_callback_, error_callback_, /*client_data=*/(void*)this);
			}

			bool StreamDecoder::Finish()
			{
				FLAC__ASSERT(IsValid);
				if (nullptr != file_reader_) {
					(void)file_reader_->DetachStream();
					delete file_reader_;
				}
				file_reader_ = nullptr;
				file_stream_ = nullptr;
				return !!(::FLAC__stream_decoder_finish(decoder_));
			}

			bool StreamDecoder::Flush()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_flush(decoder_));
			}

			bool StreamDecoder::Reset()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_reset(decoder_));
			}

			bool StreamDecoder::ProcessSingle()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_process_single(decoder_));
			}

			bool StreamDecoder::ProcessUntilEndOfMetadata()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_process_until_end_of_metadata(decoder_));
			}

			bool StreamDecoder::ProcessUntilEndOfStream()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_process_until_end_of_stream(decoder_));
			}

			bool StreamDecoder::SkipSingleFrame()
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_skip_single_frame(decoder_));
			}

			bool StreamDecoder::SeekAbsolute(FLAC__uint64 sample)
			{
				FLAC__ASSERT(IsValid);
				return !!(::FLAC__stream_decoder_seek_absolute(decoder_, sample));
			}


			::FLAC__StreamDecoderReadStatus StreamDecoder::read_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__byte buffer[], size_t *bytes, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				if (instance->file_stream_) {
					if (*bytes > 0) {
						*bytes = perform_synchronously(instance->file_reader_->LoadAsync(*bytes));
						if (*bytes == 0)
							return FLAC__STREAM_DECODER_READ_STATUS_END_OF_STREAM;
						instance->file_reader_->ReadBytes(Platform::ArrayReference<FLAC__byte>(buffer, *bytes));
						return FLAC__STREAM_DECODER_READ_STATUS_CONTINUE;
					}
					return FLAC__STREAM_DECODER_READ_STATUS_ABORT;
				}

				Callbacks::StreamDecoderReadEventArgs^ args = ref new Callbacks::StreamDecoderReadEventArgs(buffer, bytes);
				instance->ReadCallback(instance, args);
				perform_synchronously(args->WaitForDeferralsAsync());

				return args->Result;
			}

			::FLAC__StreamDecoderSeekStatus StreamDecoder::seek_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 absolute_byte_offset, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				if (instance->file_stream_) {
					if (absolute_byte_offset > instance->file_stream_->Size)
						return FLAC__STREAM_DECODER_SEEK_STATUS_ERROR;
					instance->file_stream_->Seek(absolute_byte_offset);
					return FLAC__STREAM_DECODER_SEEK_STATUS_OK;
				}

				Callbacks::StreamDecoderSeekEventArgs^ args = ref new Callbacks::StreamDecoderSeekEventArgs(absolute_byte_offset);
				instance->SeekCallback(instance, args);
				return args->Result;
			}

			::FLAC__StreamDecoderTellStatus StreamDecoder::tell_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 *absolute_byte_offset, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				if (instance->file_stream_) {
					*absolute_byte_offset = instance->file_stream_->Position;
					return FLAC__STREAM_DECODER_TELL_STATUS_OK;
				}

				Callbacks::StreamDecoderTellEventArgs^ args = ref new Callbacks::StreamDecoderTellEventArgs(absolute_byte_offset);
				instance->TellCallback(instance, args);
				return args->Result;
			}

			::FLAC__StreamDecoderLengthStatus StreamDecoder::length_callback_(const ::FLAC__StreamDecoder *decoder, FLAC__uint64 *stream_length, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				if (instance->file_stream_) {
					*stream_length = instance->file_stream_->Size;
					return FLAC__STREAM_DECODER_LENGTH_STATUS_OK;
				}

				Callbacks::StreamDecoderLengthEventArgs^ args = ref new Callbacks::StreamDecoderLengthEventArgs(stream_length);
				instance->LengthCallback(instance, args);
				return args->Result;
			}

			FLAC__bool StreamDecoder::eof_callback_(const ::FLAC__StreamDecoder *decoder, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				if (instance->file_stream_)
					return (instance->file_stream_->Position < instance->file_stream_->Size) ? FALSE : TRUE;

				Callbacks::StreamDecoderEofEventArgs^ args = ref new Callbacks::StreamDecoderEofEventArgs();
				instance->EofCallback(instance, args);
				return args->Result;
			}

			::FLAC__StreamDecoderWriteStatus StreamDecoder::write_callback_(const ::FLAC__StreamDecoder *decoder, const ::FLAC__Frame *frame, const FLAC__int32 * const buffer[], void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);

				Callbacks::StreamDecoderWriteEventArgs^ args = ref new Callbacks::StreamDecoderWriteEventArgs(buffer, frame);
				instance->WriteCallback(instance, args);
				perform_synchronously(args->WaitForDeferralsAsync());

				return args->Result;
			}

			void StreamDecoder::metadata_callback_(const ::FLAC__StreamDecoder *decoder, const ::FLAC__StreamMetadata *metadata, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);
				instance->MetadataCallback(instance, ref new Callbacks::StreamDecoderMetadataEventArgs(metadata));
			}

			void StreamDecoder::error_callback_(const ::FLAC__StreamDecoder *decoder, ::FLAC__StreamDecoderErrorStatus status, void *client_data)
			{
				(void)decoder;
				FLAC__ASSERT(0 != client_data);
				StreamDecoder^ instance = reinterpret_cast<StreamDecoder^>(client_data);
				FLAC__ASSERT(nullptr != instance && instance->IsValid);
				instance->ErrorCallback(instance, ref new Callbacks::StreamDecoderErrorEventArgs(status));
			}


			namespace Callbacks {

				Windows::Storage::Streams::IBuffer^ StreamDecoderWriteEventArgs::GetBuffer()
				{
					if (!buffer_) {
						uint32 count = frame_->Header->Blocksize * ((frame_->Header->Channels * frame_->Header->BitsPerSample) >> 3);
						buffer_ = ref new Windows::Storage::Streams::Buffer(count);
						buffer_->Length = pack_sample(data_, frame_->Header->Blocksize, frame_->Header->Channels, buffer_, frame_->Header->BitsPerSample);
					}
					return buffer_;
				}

				Platform::Array<FLAC__int32>^ StreamDecoderWriteEventArgs::GetData(unsigned index)
				{
					if (index > frame_->Header->Channels)
						throw ref new Platform::OutOfBoundsException();

					if (!data_array_) {
						data_array_ = ref new Platform::Array<Platform::Object^>(frame_->Header->Channels);
						for (unsigned i = 0; i < frame_->Header->Channels; i++) {
							data_array_[i] = ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(data_[i]), frame_->Header->Blocksize);
						}
					}

					return safe_cast<Platform::Array<FLAC__int32>^>(data_array_[index]);
				}

			}

		}
	}
}
