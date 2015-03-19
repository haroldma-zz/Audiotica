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

#ifndef FLACRT__FORMAT_H
#define FLACRT__FORMAT_H

#include <malloc.h>

#include "FLAC/format.h"
#include "FLAC/ordinals.h"
#include "share/win_utf8_io.h"


/** \file include/FLAC/format.h
 *
 *  \brief
 *  This module contains structure definitions for the representation
 *  of FLAC format components in memory.  These are the basic
 *  structures used by the rest of the interfaces.
 *
 *  See the detailed documentation in the
 *  \link flac_format format \endlink module.
 */

/** \defgroup flac_format FLAC/format.h: format components
 *  \ingroup flac
 *
 *  \brief
 *  This module contains structure definitions for the representation
 *  of FLAC format components in memory.  These are the basic
 *  structures used by the rest of the interfaces.
 *
 *  First, you should be familiar with the
 *  <A HREF="../format.html">FLAC format</A>.  Many of the values here
 *  follow directly from the specification.  As a user of libFLAC, the
 *  interesting parts really are the structures that describe the frame
 *  header and metadata blocks.
 *
 *  The format structures here are very primitive, designed to store
 *  information in an efficient way.  Reading information from the
 *  structures is easy but creating or modifying them directly is
 *  more complex.  For the most part, as a user of a library, editing
 *  is not necessary; however, for metadata blocks it is, so there are
 *  convenience functions provided in the \link flac_metadata metadata
 *  module \endlink to simplify the manipulation of metadata blocks.
 *
 * \note
 * It's not the best convention, but symbols ending in _LEN are in bits
 * and _LENGTH are in bytes.  _LENGTH symbols are \#defines instead of
 * global variables because they are usually used when declaring byte
 * arrays and some compilers require compile-time knowledge of array
 * sizes when declared on the stack.
 *
 * \{
 */

/* convert UTF-8 to String */
static
Platform::String^ string_from_utf8(const char *str)
{
	wchar_t *widestr;
	Platform::String^ ret;

	if (!(widestr = wchar_from_utf8(str))) throw ref new Platform::InvalidArgumentException();
	ret = ref new Platform::String(widestr);
	free(widestr);

	return ret;
}

namespace FLAC {

	namespace WindowsRuntime {

		namespace Format {

			/// <summary>
			/// An enumeration of the available subframe types.
			/// </summary>
			public enum class SubframeType {
				/// <summary>
				/// Constant signal.
				/// </summary>
				Constant = FLAC__SUBFRAME_TYPE_CONSTANT,

				/// <summary>
				/// Uncompressed signal.
				/// </summary>
				Verbatim = FLAC__SUBFRAME_TYPE_VERBATIM,

				/// <summary>
				/// Fixed polynomial prediction.
				/// </summary>
				Fixed = FLAC__SUBFRAME_TYPE_FIXED,

				/// <summary>
				/// Linear prediction.
				/// </summary>
				LPC = FLAC__SUBFRAME_TYPE_LPC
			};


			namespace Subframes {

				/// <summary>
				/// An enumeration of the available entropy coding methods.
				/// </summary>
				public enum class EntropyCodingMethodType {
					/// <summary>
					/// Residual is coded by partitioning into contexts, each with it's own
					/// 4-bit Rice parameter.
					/// </summary>
					PartitionedRice = FLAC__ENTROPY_CODING_METHOD_PARTITIONED_RICE,

					/// <summary>
					/// Residual is coded by partitioning into contexts, each with it's own
					/// 5-bit Rice parameter.
					/// </summary>
					PartitionedRice2 = FLAC__ENTROPY_CODING_METHOD_PARTITIONED_RICE2
				};


				/// <summary>
				/// Contents of a Rice partitioned residual.
				/// </summary>
				public ref class PartitionedRiceContents sealed {
				public:
					/// <summary>
					/// The Rice parameters for each context.
					/// </summary>
					property Platform::Array<unsigned>^ Parameters {
						Platform::Array<unsigned>^ get() {
							return parameters_ ? parameters_ : (parameters_ =
								ref new Platform::Array<unsigned>(source_.parameters, (2 ^ source_.capacity_by_order)));
						}
					}

					/// <summary>
					/// Widths for escape-coded partitions.  Will be non-zero for escaped
					/// partitions and zero for unescaped partitions.
					/// </summary>
					property Platform::Array<unsigned>^ RawBits {
						Platform::Array<unsigned>^ get() {
							return raw_bits_ ? raw_bits_ : (raw_bits_ =
								ref new Platform::Array<unsigned>(source_.raw_bits, (2 ^ source_.capacity_by_order)));
						}
					}

				internal:
					PartitionedRiceContents(const FLAC__EntropyCodingMethod_PartitionedRiceContents &src)
						: source_(src), parameters_(nullptr), raw_bits_(nullptr) { }

				private:
					const FLAC__EntropyCodingMethod_PartitionedRiceContents &source_;

					Platform::Array<unsigned>^ parameters_;
					Platform::Array<unsigned>^ raw_bits_;
				};


				/** Header for a Rice partitioned residual.  (c.f. <A HREF="../format.html#partitioned_rice">format specification</A>)
				*/
				public ref class PartitionedRice sealed {
				public:
					property Platform::Array<PartitionedRiceContents^>^ Contents {
						Platform::Array<PartitionedRiceContents^>^ get() {
							return contents_ ? contents_ : (contents_ = InitializeContents());
						}
					}
					/**< The context's Rice parameters and/or raw bits. */

				internal:
					PartitionedRice(const FLAC__EntropyCodingMethod_PartitionedRice &src) : source_(src) { }

				private:
					inline Platform::Array<PartitionedRiceContents^>^ InitializeContents() {
						Platform::Array<PartitionedRiceContents^>^ arr = ref new Platform::Array<PartitionedRiceContents^>(2 ^ source_.order);
						for (unsigned i = 0; i < arr->Length; i++) {
							arr[i] = ref new PartitionedRiceContents(source_.contents[i]);
						}
						return arr;
					}

					const FLAC__EntropyCodingMethod_PartitionedRice &source_;

					Platform::Array<PartitionedRiceContents^>^ contents_;
				};


				/** Header for the entropy coding method.  (c.f. <A HREF="../format.html#residual">format specification</A>)
				*/
				public ref class EntropyCodingMethod sealed {
				public:
					property EntropyCodingMethodType Type {
						EntropyCodingMethodType get() { return (EntropyCodingMethodType)(int)source_.type; }
					}

					property Subframes::PartitionedRice^ PartitionedRice {
						Subframes::PartitionedRice^ get() {
							return partitioned_rice_ ? partitioned_rice_ : (partitioned_rice_ =
								ref new Subframes::PartitionedRice(source_.data.partitioned_rice));
						}
					}

				internal:
					EntropyCodingMethod(const FLAC__EntropyCodingMethod &src) : source_(src) { }

				private:
					const FLAC__EntropyCodingMethod &source_;

					Subframes::PartitionedRice^ partitioned_rice_;
				};


				/** CONSTANT subframe.  (c.f. <A HREF="../format.html#subframe_constant">format specification</A>)
				*/
				public ref class SubframeConstant sealed {
				public:
					property FLAC__int32 Value {
						FLAC__int32 get() { return source_.value; }
					}
					/**< The constant signal value. */

				internal:
					SubframeConstant(const FLAC__Subframe_Constant &src) : source_(src) { }

				private:
					const FLAC__Subframe_Constant &source_;
				};


				/** VERBATIM subframe.  (c.f. <A HREF="../format.html#subframe_verbatim">format specification</A>)
				*/
				public ref class SubframeVerbatim sealed {
				public:
					property Platform::Array<FLAC__int32>^ Data {
						Platform::Array<FLAC__int32>^ get() {
							return data_ ? data_ : (data_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(source_.data), blocksize_));
						}
					}
					/**< A pointer to verbatim signal. */

				internal:
					SubframeVerbatim(const FLAC__Subframe_Verbatim &src, const unsigned &blocksize)
						: source_(src), blocksize_(blocksize) { }

				private:
					const FLAC__Subframe_Verbatim &source_;
					const unsigned &blocksize_;

					Platform::Array<FLAC__int32>^ data_;
				};


				/** FIXED subframe.  (c.f. <A HREF="../format.html#subframe_fixed">format specification</A>)
				*/
				public ref class SubframeFixed sealed {
				public:
					property Subframes::EntropyCodingMethod^ EntropyCodingMethod {
						Subframes::EntropyCodingMethod^ get() {
							return entropy_coding_method_ ? entropy_coding_method_ : (entropy_coding_method_ =
								ref new Subframes::EntropyCodingMethod(source_.entropy_coding_method));
						}
					}
					/**< The residual coding method. */

					property Platform::Array<FLAC__int32>^ Warmup {
						Platform::Array<FLAC__int32>^ get() {
							return warmup_ ? warmup_ : (warmup_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(source_.warmup), source_.order));
						}
					}
					/**< Warmup samples to prime the predictor, length == order. */

					property Platform::Array<FLAC__int32>^ Residual {
						Platform::Array<FLAC__int32>^ get() {
							return residual_ ? residual_ : (residual_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32*>(source_.residual), (blocksize_ - source_.order)));
						}
					}
					/**< The residual signal, length == (blocksize minus order) samples. */

				internal:
					SubframeFixed(const FLAC__Subframe_Fixed &src, const unsigned &blocksize)
						: source_(src), blocksize_(blocksize) { }

				private:
					const FLAC__Subframe_Fixed &source_;
					const unsigned &blocksize_;

					Subframes::EntropyCodingMethod^	entropy_coding_method_;
					Platform::Array<FLAC__int32>^	warmup_;
					Platform::Array<FLAC__int32>^	residual_;
				};


				/** LPC subframe.  (c.f. <A HREF="../format.html#subframe_lpc">format specification</A>)
				*/
				public ref class SubframeLPC sealed {
				public:
					property Subframes::EntropyCodingMethod^ EntropyCodingMethod {
						Subframes::EntropyCodingMethod^ get() {
							return entropy_coding_method_ ? entropy_coding_method_ : (entropy_coding_method_ =
								ref new Subframes::EntropyCodingMethod(source_.entropy_coding_method));
						}
					}
					/**< The residual coding method. */

					property unsigned QlpCoeffPrecision {
						unsigned get() { return source_.qlp_coeff_precision; }
					}
					/**< Quantized FIR filter coefficient precision in bits. */

					property int QuantizationLevel {
						int get() { return source_.quantization_level; }
					}
					/**< The qlp coeff shift needed. */

					property Platform::Array<FLAC__int32>^ QlpCoeff {
						Platform::Array<FLAC__int32>^ get() {
							return qlp_coeff_ ? qlp_coeff_ : (qlp_coeff_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(source_.qlp_coeff), FLAC__MAX_LPC_ORDER));
						}
					}
					/**< FIR filter coefficients. */

					property Platform::Array<FLAC__int32>^ Warmup {
						Platform::Array<FLAC__int32>^ get() {
							return warmup_ ? warmup_ : (warmup_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(source_.warmup), source_.order));
						}
					}
					/**< Warmup samples to prime the predictor, length == order. */

					property Platform::Array<FLAC__int32>^ Residual {
						Platform::Array<FLAC__int32>^ get() {
							return residual_ ? residual_ : (residual_ =
								ref new Platform::Array<FLAC__int32>(const_cast<FLAC__int32 *>(source_.residual), blocksize_ - source_.order));
						}
					}
					/**< The residual signal, length == (blocksize minus order) samples. */

				internal:
					SubframeLPC(const FLAC__Subframe_LPC &src, const unsigned &blocksize)
						: source_(src), blocksize_(blocksize) { }

				private:
					const FLAC__Subframe_LPC &source_;
					const unsigned &blocksize_;

					Subframes::EntropyCodingMethod^	entropy_coding_method_;
					Platform::Array<FLAC__int32>^	qlp_coeff_;
					Platform::Array<FLAC__int32>^	warmup_;
					Platform::Array<FLAC__int32>^	residual_;
				};

			}


			/** FLAC subframe structure.  (c.f. <A HREF="../format.html#subframe">format specification</A>)
			*/
			public ref class Subframe sealed {
			public:
				property SubframeType Type {
					SubframeType get() { return (SubframeType)(int)source_.type; }
				}

				property Subframes::SubframeConstant^ Constant {
					Subframes::SubframeConstant^ get() {
						return constant_ ? constant_ : (constant_ = (FLAC__SUBFRAME_TYPE_CONSTANT == source_.type)
							? ref new Subframes::SubframeConstant(source_.data.constant) : nullptr);
					}
				}

				property Subframes::SubframeFixed^ Fixed {
					Subframes::SubframeFixed^ get() {
						return fixed_ ? fixed_ : (fixed_ = (FLAC__SUBFRAME_TYPE_FIXED == source_.type)
							? ref new Subframes::SubframeFixed(source_.data.fixed, blocksize_) : nullptr);
					}
				}

				property Subframes::SubframeLPC^ LPC {
					Subframes::SubframeLPC^ get() {
						return lpc_ ? lpc_ : (lpc_ = (FLAC__SUBFRAME_TYPE_LPC == source_.type)
							? ref new Subframes::SubframeLPC(source_.data.lpc, blocksize_) : nullptr);
					}
				}

				property Subframes::SubframeVerbatim^ Verbatim {
					Subframes::SubframeVerbatim^ get() {
						return verbatim_ ? verbatim_ : (verbatim_ = (FLAC__SUBFRAME_TYPE_VERBATIM == source_.type)
							? ref new Subframes::SubframeVerbatim(source_.data.verbatim, blocksize_) : nullptr);
					}
				}

				property unsigned WastedBits {
					unsigned get() { return source_.wasted_bits; }
				}

			internal:
				Subframe(const FLAC__Subframe &src, const unsigned &blocksize)
					: source_(src), blocksize_(blocksize) { }

			private:
				const FLAC__Subframe &source_;
				const unsigned &blocksize_;

				Subframes::SubframeConstant^ constant_;
				Subframes::SubframeFixed^	 fixed_;
				Subframes::SubframeLPC^		 lpc_;
				Subframes::SubframeVerbatim^ verbatim_;
			};


			namespace Frames {

				/// <summary>
				/// An enumeration of the available channel assignments.
				/// </summary>
				public enum class ChannelAssignment {
					Independent = FLAC__CHANNEL_ASSIGNMENT_INDEPENDENT, /**< independent channels */
					LeftSide = FLAC__CHANNEL_ASSIGNMENT_LEFT_SIDE, /**< left+side stereo */
					RightSide = FLAC__CHANNEL_ASSIGNMENT_RIGHT_SIDE, /**< right+side stereo */
					MidSide = FLAC__CHANNEL_ASSIGNMENT_MID_SIDE /**< mid+side stereo */
				};


				/// <summary>
				/// An enumeration of the possible frame numbering methods.
				/// </summary>
				public enum class FrameNumberType {
					FrameNumber = FLAC__FRAME_NUMBER_TYPE_FRAME_NUMBER, /**< number contains the frame number */
					SampleNumber = FLAC__FRAME_NUMBER_TYPE_SAMPLE_NUMBER /**< number contains the sample number of first sample in frame */
				};


				/** FLAC frame header structure.  (c.f. <A HREF="../format.html#frame_header">format specification</A>)
				*/
				public ref class FrameHeader sealed {
				public:
					property unsigned Blocksize {
						unsigned get() { return source_.blocksize; }
					}
					/**< The number of samples per subframe. */

					property unsigned SampleRate {
						unsigned get() { return source_.sample_rate; }
					}
					/**< The sample rate in Hz. */

					property unsigned Channels {
						unsigned get() { return source_.channels; }
					}
					/**< The number of channels (== number of subframes). */

					property Frames::ChannelAssignment ChannelAssignment {
						Frames::ChannelAssignment get() { return (Frames::ChannelAssignment)(int)source_.channel_assignment;; }
					}
					/**< The channel assignment for the frame. */

					property unsigned BitsPerSample {
						unsigned get() { return source_.bits_per_sample; }
					}
					/**< The sample resolution. */

					property FrameNumberType NumberType {
						FrameNumberType get() { return (FrameNumberType)(int)source_.number_type; }
					}
					/**< The numbering scheme used for the frame.  As a convenience, the
					* decoder will always convert a frame number to a sample number because
					* the rules are complex. */

					property FLAC__uint32 FrameNumber {
						FLAC__uint32 get() { return (FLAC__FRAME_NUMBER_TYPE_FRAME_NUMBER == source_.number_type) ? source_.number.frame_number : 0; }
					}

					property FLAC__uint64 SampleNumber {
						FLAC__uint64 get() { return (FLAC__FRAME_NUMBER_TYPE_SAMPLE_NUMBER == source_.number_type) ? source_.number.sample_number : 0; }
					}
					/**< The frame number or sample number of first sample in frame;
					* use the \a number_type value to determine which to use. */

					property FLAC__uint8 CRC {
						FLAC__uint8 get() { return source_.crc; }
					}
					/**< CRC-8 (polynomial = x^8 + x^2 + x^1 + x^0, initialized with 0)
					* of the raw frame header bytes, meaning everything before the CRC byte
					* including the sync code.
					*/

				internal:
					FrameHeader(const FLAC__FrameHeader &src) : source_(src) { }

				private:
					const FLAC__FrameHeader &source_;
				};


				/** FLAC frame footer structure.  (c.f. <A HREF="../format.html#frame_footer">format specification</A>)
				*/
				public ref class FrameFooter sealed {
				public:
					property FLAC__uint16 CRC {
						FLAC__uint16 get() { return source_.crc; }
					}
					/**< CRC-16 (polynomial = x^16 + x^15 + x^2 + x^0, initialized with
					* 0) of the bytes before the crc, back to and including the frame header
					* sync code.
					*/

				internal:
					FrameFooter(const FLAC__FrameFooter &src) : source_(src) { }

				private:
					const FLAC__FrameFooter &source_;
				};

			}


			/** FLAC frame structure.  (c.f. <A HREF="../format.html#frame">format specification</A>)
			*/
			public ref class Frame sealed {
			public:
				property Frames::FrameHeader^ Header {
					Frames::FrameHeader^ get() {
						return header_ ? header_ : (header_ = ref new Frames::FrameHeader(source_->header));
					}
				}

				property Platform::Array<Subframe^>^ Subframes {
					Platform::Array<Subframe^>^ get() {
						return subframes_ ? subframes_ : (subframes_ = InitializeSubframes());
					}
				}

				property Frames::FrameFooter^ Footer {
					Frames::FrameFooter^ get() {
						return footer_ ? footer_ : (footer_ = ref new Frames::FrameFooter(source_->footer));
					}
				}

			internal:
				Frame(const FLAC__Frame *src) : source_(src) { }

			private:
				inline Platform::Array<Subframe^>^ InitializeSubframes() {
					Platform::Array<Subframe^>^ arr = ref new Platform::Array<Subframe^>(FLAC__MAX_CHANNELS);
					for (unsigned i = 0; i < arr->Length; i++) {
						arr[i] = ref new Subframe(source_->subframes[i], source_->header.blocksize);
					}
					return arr;
				}

				const FLAC__Frame *source_;

				Frames::FrameHeader^		header_;
				Platform::Array<Subframe^>^	subframes_;
				Frames::FrameFooter^		footer_;
			};


			/// <summary>
			/// An enumeration of the available metadata block types.
			/// </summary>
			public enum class MetadataType {

				StreamInfo = FLAC__METADATA_TYPE_STREAMINFO,
				/**< <A HREF="../format.html#metadata_block_streaminfo">STREAMINFO</A> block */

				Padding = FLAC__METADATA_TYPE_PADDING,
				/**< <A HREF="../format.html#metadata_block_padding">PADDING</A> block */

				Application = FLAC__METADATA_TYPE_APPLICATION,
				/**< <A HREF="../format.html#metadata_block_application">APPLICATION</A> block */

				Seektable = FLAC__METADATA_TYPE_SEEKTABLE,
				/**< <A HREF="../format.html#metadata_block_seektable">SEEKTABLE</A> block */

				VorbisComment = FLAC__METADATA_TYPE_VORBIS_COMMENT,
				/**< <A HREF="../format.html#metadata_block_vorbis_comment">VORBISCOMMENT</A> block (a.k.a. FLAC tags) */

				Cuesheet = FLAC__METADATA_TYPE_CUESHEET,
				/**< <A HREF="../format.html#metadata_block_cuesheet">CUESHEET</A> block */

				Picture = FLAC__METADATA_TYPE_PICTURE,
				/**< <A HREF="../format.html#metadata_block_picture">PICTURE</A> block */

				Undefined = FLAC__METADATA_TYPE_UNDEFINED
				/**< marker to denote beginning of undefined type range; this number will increase as new metadata types are added */

			};


			namespace Metadata {

				/** FLAC STREAMINFO structure.  (c.f. <A HREF="../format.html#metadata_block_streaminfo">format specification</A>)
				 */
				public ref class StreamInfo sealed {
				public:
					property unsigned MinBlocksize {
						unsigned get() { return source_.min_blocksize; }
					}

					property unsigned MaxBlocksize {
						unsigned get() { return source_.max_blocksize; }
					}

					property unsigned MinFramesize {
						unsigned get() { return source_.min_framesize; }
					}

					property unsigned MaxFramesize {
						unsigned get() { return source_.max_framesize; }
					}

					property unsigned SampleRate {
						unsigned get() { return source_.sample_rate; }
					}

					property unsigned Channels {
						unsigned get() { return source_.channels; }
					}

					property unsigned BitsPerSample {
						unsigned get() { return source_.bits_per_sample; }
					}

					property FLAC__uint64 TotalSamples {
						FLAC__uint64 get() { return source_.total_samples; }
					}

					property Platform::Array<FLAC__byte>^ Md5Sum {
						Platform::Array<FLAC__byte>^ get() {
							return md5sum_ ? md5sum_ : (md5sum_ =
								ref new Platform::Array<FLAC__byte>(const_cast<FLAC__byte *>(source_.md5sum), 16));
						}
					}

				internal:
					StreamInfo(const FLAC__StreamMetadata_StreamInfo &src) : source_(src) { }

				private:
					const FLAC__StreamMetadata_StreamInfo &source_;

					Platform::Array<FLAC__byte>^ md5sum_;
				};


				/** FLAC APPLICATION structure.  (c.f. <A HREF="../format.html#metadata_block_application">format specification</A>)
				 */
				public ref class Application sealed {
				public:
					property Platform::Array<FLAC__byte>^ ID {
						Platform::Array<FLAC__byte>^ get() {
							return id_ ? id_ : (id_ = ref new Platform::Array<FLAC__byte>(const_cast<FLAC__byte *>(source_.id), 4));
						}
					}

					property Platform::Array<FLAC__byte>^ Data {
						Platform::Array<FLAC__byte>^ get() {
							return data_ ? data_ : (data_ =
								ref new Platform::Array<FLAC__byte>(const_cast<FLAC__byte *>(source_.data), blocksize_ - 4));
						}
					}

				internal:
					Application(const FLAC__StreamMetadata_Application &src, const unsigned &blocksize)
						: source_(src), blocksize_(blocksize) { }

				private:
					const FLAC__StreamMetadata_Application &source_;
					const unsigned &blocksize_;

					Platform::Array<FLAC__byte>^ id_;
					Platform::Array<FLAC__byte>^ data_;
				};


				/** SeekPoint structure used in SEEKTABLE blocks.  (c.f. <A HREF="../format.html#seekpoint">format specification</A>)
				 */
				public ref class SeekPoint sealed {
				public:
					property FLAC__uint64 SampleNumber {
						FLAC__uint64 get() { return source_.sample_number; }
					}
					/**<  The sample number of the target frame. */

					property FLAC__uint64 StreamOffset {
						FLAC__uint64 get() { return source_.stream_offset; }
					}
					/**< The offset, in bytes, of the target frame with respect to
					 * beginning of the first frame. */

					property unsigned FrameSamples {
						unsigned get() { return source_.frame_samples; }
					}
					/**< The number of samples in the target frame. */

				internal:
					SeekPoint(const FLAC__StreamMetadata_SeekPoint &src) : source_(src) { }

				private:
					const FLAC__StreamMetadata_SeekPoint &source_;
				};


				/** FLAC SEEKTABLE structure.  (c.f. <A HREF="../format.html#metadata_block_seektable">format specification</A>)
				 *
				 * \note From the format specification:
				 * - The seek points must be sorted by ascending sample number.
				 * - Each seek point's sample number must be the first sample of the
				 *   target frame.
				 * - Each seek point's sample number must be unique within the table.
				 * - Existence of a SEEKTABLE block implies a correct setting of
				 *   total_samples in the stream_info block.
				 * - Behavior is undefined when more than one SEEKTABLE block is
				 *   present in a stream.
				 */
				public ref class SeekTable sealed {
				public:
					property Platform::Array<SeekPoint^>^ Points {
						Platform::Array<SeekPoint^>^ get() { return points_ ? points_ : (points_ = InitializePoints()); }
					}

				internal:
					SeekTable(const FLAC__StreamMetadata_SeekTable &src) : source_(src) { }

				private:
					Platform::Array<SeekPoint^>^ InitializePoints() {
						Platform::Array<SeekPoint^>^ arr = ref new Platform::Array<SeekPoint^>(source_.num_points);
						for (unsigned i = 0; i < arr->Length; i++) {
							arr[i] = ref new SeekPoint(source_.points[i]);
						}
						return arr;
					}

					const FLAC__StreamMetadata_SeekTable &source_;

					Platform::Array<SeekPoint^>^ points_;
				};


				/** FLAC VORBIS_COMMENT structure.  (c.f. <A HREF="../format.html#metadata_block_vorbis_comment">format specification</A>)
				 */
				public ref class VorbisComment sealed {
				public:
					property Platform::String^ VendorString {
						Platform::String^ get() {
							return vendor_string_ ? vendor_string_ : (vendor_string_ =
								string_from_utf8(reinterpret_cast<char *>(source_.vendor_string.entry)));
						}
					}

					property Platform::Array<Platform::String^>^ Comments {
						Platform::Array<Platform::String^>^ get() {
							return comments_ ? comments_ : (comments_ = InitializeComments());
						}
					}

				internal:
					VorbisComment(const FLAC__StreamMetadata_VorbisComment &src) : source_(src) { }

				private:
					inline Platform::Array<Platform::String^>^ InitializeComments() {
						Platform::Array<Platform::String^>^ arr = ref new Platform::Array<Platform::String^>(source_.num_comments);
						for (unsigned i = 0; i < arr->Length; i++) {
							arr[i] = string_from_utf8(reinterpret_cast<char *>(source_.comments[i].entry));
						}
						return arr;
					}

					const FLAC__StreamMetadata_VorbisComment &source_;

					Platform::String^ vendor_string_;
					Platform::Array<Platform::String^>^ comments_;
				};


				/** FLAC CUESHEET track index structure.  (See the
				 * <A HREF="../format.html#cuesheet_track_index">format specification</A> for
				 * the full description of each field.)
				 */
				public ref class CueSheetIndex sealed {
				public:
					property FLAC__uint64 Offset {
						FLAC__uint64 get() { return source_.offset; }
					}
					/**< Offset in samples, relative to the track offset, of the index
					 * point.
					 */

					property FLAC__byte Number {
						FLAC__byte get() { return source_.number; }
					}
					/**< The index point number. */

				internal:
					CueSheetIndex(const FLAC__StreamMetadata_CueSheet_Index &src) : source_(src) { }

				private:
					const FLAC__StreamMetadata_CueSheet_Index &source_;
				};


				/** FLAC CUESHEET track structure.  (See the
				 * <A HREF="../format.html#cuesheet_track">format specification</A> for
				 * the full description of each field.)
				 */
				public ref class CueSheetTrack sealed {
				public:
					property FLAC__uint64 Offset {
						FLAC__uint64 get() { return source_.offset; }
					}
					/**< Track offset in samples, relative to the beginning of the FLAC audio stream. */

					property FLAC__byte Number {
						FLAC__byte get() { return source_.number; }
					}
					/**< The track number. */

					property Platform::String^ ISRC {
						Platform::String^ get() { return isrc_ ? isrc_ : (isrc_ = string_from_utf8(source_.isrc)); }
					}
					/**< Track ISRC.  This is a 12-digit alphanumeric code */

					property unsigned Type {
						unsigned get() { return source_.type; }
					}
					/**< The track type: 0 for audio, 1 for non-audio. */

					property unsigned PreEmphasis {
						unsigned get() { return source_.pre_emphasis; }
					}
					/**< The pre-emphasis flag: 0 for no pre-emphasis, 1 for pre-emphasis. */

					property Platform::Array<CueSheetIndex^>^ Indices {
						Platform::Array<CueSheetIndex^>^ get() { return indices_ ? indices_ : (indices_ = InitializeIndices()); }
					}
					/**< NULL if num_indices == 0, else pointer to array of index points. */

				internal:
					CueSheetTrack(const FLAC__StreamMetadata_CueSheet_Track &src) : source_(src) { }

				private:
					inline Platform::Array<CueSheetIndex^>^ InitializeIndices() {
						Platform::Array<CueSheetIndex^>^ arr = ref new Platform::Array<CueSheetIndex^>(source_.num_indices);
						for (unsigned i = 0; i < arr->Length; i++) {
							arr[i] = ref new CueSheetIndex(source_.indices[i]);
						}
						return arr;
					}

					const FLAC__StreamMetadata_CueSheet_Track &source_;

					Platform::String^ isrc_;
					Platform::Array<CueSheetIndex^>^ indices_;
				};


				/** FLAC CUESHEET structure.  (See the
				 * <A HREF="../format.html#metadata_block_cuesheet">format specification</A>
				 * for the full description of each field.)
				 */
				public ref class CueSheet sealed {
				public:
					property Platform::String^ MediaCatalogNumber {
						Platform::String^ get() {
							return media_catalog_number_ ? media_catalog_number_ :
								(media_catalog_number_ = string_from_utf8(source_.media_catalog_number));
						}
					}
					/**< Media catalog number, in ASCII printable characters 0x20-0x7e.  In
					 * general, the media catalog number may be 0 to 128 bytes long; any
					 * unused characters should be right-padded with NUL characters.
					 */

					property FLAC__uint64 LeadIn {
						FLAC__uint64 get() { return source_.lead_in; }
					}
					/**< The number of lead-in samples. */

					property bool IsCD {
						bool get() { return !!source_.is_cd; }
					}
					/**< \c true if CUESHEET corresponds to a Compact Disc, else \c false. */

					property Platform::Array<CueSheetTrack^>^ Tracks {
						Platform::Array<CueSheetTrack^>^ get() { return tracks_ ? tracks_ : (tracks_ = InitializeTracks()); }
					}
					/**< NULL if num_tracks == 0, else pointer to array of tracks. */

				internal:
					CueSheet(const FLAC__StreamMetadata_CueSheet &src) : source_(src) { }

				private:
					inline Platform::Array<CueSheetTrack^>^ InitializeTracks() {
						Platform::Array<CueSheetTrack^>^ arr = ref new Platform::Array<CueSheetTrack^>(source_.num_tracks);
						for (unsigned i = 0; i < arr->Length; i++) {
							arr[i] = ref new CueSheetTrack(source_.tracks[i]);
						}
						return arr;
					}

					const FLAC__StreamMetadata_CueSheet &source_;

					Platform::String^ media_catalog_number_;
					Platform::Array<CueSheetTrack^>^ tracks_;
				};


				/** An enumeration of the PICTURE types (see FLAC__StreamMetadataPicture and id3 v2.4 APIC tag). */
				public enum class PictureType {
					Other = FLAC__STREAM_METADATA_PICTURE_TYPE_OTHER, /**< Other */
					FileIconStandard = FLAC__STREAM_METADATA_PICTURE_TYPE_FILE_ICON_STANDARD, /**< 32x32 pixels 'file icon' (PNG only) */
					FileIcon = FLAC__STREAM_METADATA_PICTURE_TYPE_FILE_ICON, /**< Other file icon */
					FrontCover = FLAC__STREAM_METADATA_PICTURE_TYPE_FRONT_COVER, /**< Cover (front) */
					BackCover = FLAC__STREAM_METADATA_PICTURE_TYPE_BACK_COVER, /**< Cover (back) */
					LeafletPage = FLAC__STREAM_METADATA_PICTURE_TYPE_LEAFLET_PAGE, /**< Leaflet page */
					Media = FLAC__STREAM_METADATA_PICTURE_TYPE_MEDIA, /**< Media (e.g. label side of CD) */
					LeadArtist = FLAC__STREAM_METADATA_PICTURE_TYPE_LEAD_ARTIST, /**< Lead artist/lead performer/soloist */
					Artist = FLAC__STREAM_METADATA_PICTURE_TYPE_ARTIST, /**< Artist/performer */
					Conductor = FLAC__STREAM_METADATA_PICTURE_TYPE_CONDUCTOR, /**< Conductor */
					Band = FLAC__STREAM_METADATA_PICTURE_TYPE_BAND, /**< Band/Orchestra */
					Composer = FLAC__STREAM_METADATA_PICTURE_TYPE_COMPOSER, /**< Composer */
					Lyricist = FLAC__STREAM_METADATA_PICTURE_TYPE_LYRICIST, /**< Lyricist/text writer */
					RecordingLocation = FLAC__STREAM_METADATA_PICTURE_TYPE_RECORDING_LOCATION, /**< Recording Location */
					DuringRecording = FLAC__STREAM_METADATA_PICTURE_TYPE_DURING_RECORDING, /**< During recording */
					DuringPerformance = FLAC__STREAM_METADATA_PICTURE_TYPE_DURING_PERFORMANCE, /**< During performance */
					VideoScreenCapture = FLAC__STREAM_METADATA_PICTURE_TYPE_VIDEO_SCREEN_CAPTURE, /**< Movie/video screen capture */
					Fish = FLAC__STREAM_METADATA_PICTURE_TYPE_FISH, /**< A bright coloured fish */
					Illustration = FLAC__STREAM_METADATA_PICTURE_TYPE_ILLUSTRATION, /**< Illustration */
					BandLogotype = FLAC__STREAM_METADATA_PICTURE_TYPE_BAND_LOGOTYPE, /**< Band/artist logotype */
					PublisherLogotype = FLAC__STREAM_METADATA_PICTURE_TYPE_PUBLISHER_LOGOTYPE, /**< Publisher/Studio logotype */
					Undefined = FLAC__STREAM_METADATA_PICTURE_TYPE_UNDEFINED
				};


				/** FLAC PICTURE structure.  (See the
				 * <A HREF="../format.html#metadata_block_picture">format specification</A>
				 * for the full description of each field.)
				 */
				public ref class Picture sealed {
				public:
					property PictureType Type {
						PictureType get() { return (PictureType)(int)source_.type; }
					}
					/**< The kind of picture stored. */

					property Platform::String^ MimeType {
						Platform::String^ get() {
							return mime_type_ ? mime_type_ : (mime_type_ = string_from_utf8(source_.mime_type));
						}
					}
					/**< Picture data's MIME type, in ASCII printable characters
					 * 0x20-0x7e, NUL terminated.  For best compatibility with players,
					 * use picture data of MIME type \c image/jpeg or \c image/png.  A
					 * MIME type of '-->' is also allowed, in which case the picture
					 * data should be a complete URL.  In file storage, the MIME type is
					 * stored as a 32-bit length followed by the ASCII string with no NUL
					 * terminator, but is converted to a plain C string in this structure
					 * for convenience.
					 */

					property Platform::String^ Description {
						Platform::String^ get() {
							return description_ ? description_ : (description_ =
								string_from_utf8(reinterpret_cast<char *>(source_.description)));
						}
					}
					/**< Picture's description in UTF-8, NUL terminated.  In file storage,
					 * the description is stored as a 32-bit length followed by the UTF-8
					 * string with no NUL terminator, but is converted to a plain C string
					 * in this structure for convenience.
					 */

					property FLAC__uint32 Width {
						FLAC__uint32 get() { return source_.width; }
					}
					/**< Picture's width in pixels. */

					property FLAC__uint32 Height {
						FLAC__uint32 get() { return source_.height; }
					}
					/**< Picture's height in pixels. */

					property FLAC__uint32 Depth {
						FLAC__uint32 get() { return source_.depth; }
					}
					/**< Picture's color depth in bits-per-pixel. */

					property FLAC__uint32 Colors {
						FLAC__uint32 get() { return source_.colors; }
					}
					/**< For indexed palettes (like GIF), picture's number of colors (the
					 * number of palette entries), or \c 0 for non-indexed (i.e. 2^depth).
					 */

					property Platform::Array<FLAC__byte>^ Data {
						Platform::Array<FLAC__byte>^ get() {
							return data_ ? data_ : (data_ =
								ref new Platform::Array<FLAC__byte>(source_.data, source_.data_length));
						}
					}
					/**< Binary picture data. */

				internal:
					Picture(const FLAC__StreamMetadata_Picture &src) : source_(src) { }

				private:
					const FLAC__StreamMetadata_Picture &source_;

					Platform::String^ mime_type_;
					Platform::String^ description_;
					Platform::Array<FLAC__byte>^ data_;
				};


				/** Structure that is used when a metadata block of unknown type is loaded.
				 *  The contents are opaque.  The structure is used only internally to
				 *  correctly handle unknown metadata.
				 */
				public ref class Unknown sealed {
				public:
					property Platform::Array<FLAC__byte>^ Data {
						Platform::Array<FLAC__byte>^ get() {
							return data_ ? data_ : (data_ =
								ref new Platform::Array<FLAC__byte>(source_.data, blocksize_));
						}
					}

				internal:
					Unknown(const FLAC__StreamMetadata_Unknown &src, const unsigned &blocksize)
						: source_(src), blocksize_(blocksize) { }

				private:
					const FLAC__StreamMetadata_Unknown &source_;
					const unsigned &blocksize_;

					Platform::Array<FLAC__byte>^ data_;
				};

			}


			/** FLAC metadata block structure.  (c.f. <A HREF="../format.html#metadata_block">format specification</A>)
			 */
			public ref class StreamMetadata sealed {
			public:
				property MetadataType Type {
					MetadataType get() { return (MetadataType)(int)source_->type; }
				}
				/**< The type of the metadata block; used determine which member of the
				 * \a data union to dereference.  If type >= FLAC__METADATA_TYPE_UNDEFINED
				 * then \a data.unknown must be used. */

				property bool IsLast {
					bool get() { return !!source_->is_last; }
				}
				/**< \c true if this metadata block is the last, else \a false */

				property unsigned Length {
					unsigned get() { return source_->length; }
				}
				/**< Length, in bytes, of the block data as it appears in the stream. */

				property Metadata::StreamInfo^ StreamInfo {
					Metadata::StreamInfo^ get() {
						return stream_info_ ? stream_info_ : (stream_info_ = (FLAC__METADATA_TYPE_STREAMINFO == source_->type)
							? ref new Metadata::StreamInfo(source_->data.stream_info) : nullptr);
					}
				}

				property Metadata::Application^ Application {
					Metadata::Application^ get() {
						return application_ ? application_ : (application_ = (FLAC__METADATA_TYPE_APPLICATION == source_->type)
							? ref new Metadata::Application(source_->data.application, source_->length) : nullptr);
					}
				}

				property Metadata::SeekTable^ SeekTable {
					Metadata::SeekTable^ get() {
						return seek_table_ ? seek_table_ : (seek_table_ = (FLAC__METADATA_TYPE_SEEKTABLE == source_->type)
							? ref new Metadata::SeekTable(source_->data.seek_table) : nullptr);
					}
				}

				property Metadata::VorbisComment^ VorbisComment {
					Metadata::VorbisComment^ get() {
						return vorbis_comment_ ? vorbis_comment_ : (vorbis_comment_ = (FLAC__METADATA_TYPE_VORBIS_COMMENT == source_->type)
							? ref new Metadata::VorbisComment(source_->data.vorbis_comment) : nullptr);
					}
				}

				property Metadata::CueSheet^ CueSheet {
					Metadata::CueSheet^ get() {
						return cue_sheet_ ? cue_sheet_ : (cue_sheet_ = (FLAC__METADATA_TYPE_CUESHEET == source_->type)
							? ref new Metadata::CueSheet(source_->data.cue_sheet) : nullptr);
					}
				}

				property Metadata::Picture^ Picture {
					Metadata::Picture^ get() {
						return picture_ ? picture_ : (picture_ = (FLAC__METADATA_TYPE_PICTURE == source_->type)
							? ref new Metadata::Picture(source_->data.picture) : nullptr);
					}
				}

				property Metadata::Unknown^ Unknown {
					Metadata::Unknown^ get() {
						return unknown_ ? unknown_ : (unknown_ = (FLAC__METADATA_TYPE_UNDEFINED == source_->type)
							? ref new Metadata::Unknown(source_->data.unknown, source_->length) : nullptr);
					}
				}

				/**< Polymorphic block data; use the \a type value to determine which
				 * to use. */

			internal:
				StreamMetadata(const FLAC__StreamMetadata *src) : source_(src) { }

			private:
				const FLAC__StreamMetadata *source_;

				Metadata::StreamInfo^ stream_info_;
				Metadata::Application^ application_;
				Metadata::SeekTable^ seek_table_;
				Metadata::VorbisComment^ vorbis_comment_;
				Metadata::CueSheet^ cue_sheet_;
				Metadata::Picture^ picture_;
				Metadata::Unknown^ unknown_;
			};

		}
	}
}

#endif
