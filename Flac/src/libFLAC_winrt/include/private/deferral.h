#pragma once

#include <ppltasks.h>

namespace FLAC {

	namespace WindowsRuntime {

		namespace Decoder {

			namespace Callbacks {

				public interface struct IDeferral
				{
					void Complete();
				};


				class AsyncCountdownEvent
				{
				public:
					AsyncCountdownEvent(int count) :
						tce_(Concurrency::task_completion_event<void>()),
						task_(Concurrency::create_task(tce_)),
						count_(count)
					{
					}

					void AddCount()
					{
						if (!ModifyCount(1))
							throw ref new Platform::COMException(E_NOT_VALID_STATE);
					}

					void Signal()
					{
						if (!ModifyCount(-1))
							throw ref new Platform::COMException(E_NOT_VALID_STATE);
					}

					Concurrency::task<void> WaitAsync()
					{
						return task_;
					}

				private:
					bool ModifyCount(long signalCount)
					{
						while (true) {
							long oldCount = InterlockedCompareExchange(&count_, 0, 0);
							if (0 == oldCount) return false;

							long newCount = oldCount + signalCount;
							if (newCount < 0) return false;

							if (oldCount == InterlockedCompareExchange(&count_, newCount, oldCount)) {
								if (0 == newCount) tce_.set();
								return true;
							}
						}
					}

					Concurrency::task_completion_event<void> tce_;
					Concurrency::task<void> task_;
					long count_;
				};


				ref class Deferral sealed : public IDeferral
				{
				internal:
					Deferral(AsyncCountdownEvent *count) : count_(count)
					{
					}

				public:
					virtual void Complete()
					{
						if (nullptr != count_) {
							count_->Signal();
							count_ = nullptr;
						}
					}

				private:
					AsyncCountdownEvent *count_;
				};


				class DeferralManager
				{
				public:
					DeferralManager() : count_(nullptr)
					{
					}

					IDeferral^ GetDeferral()
					{
						if (nullptr == count_)
							count_ = new AsyncCountdownEvent(1);
						IDeferral^ deferral = ref new Deferral(count_);
						count_->AddCount();
						return deferral;
					}

					Concurrency::task<void> SignalAndWaitAsync()
					{
						if (nullptr == count_)
							return Concurrency::task_from_result();
						count_->Signal();
						return count_->WaitAsync();
					}

					virtual ~DeferralManager()
					{
						if (nullptr != count_) {
							delete count_;
							count_ = nullptr;
						}
					}

				private:
					AsyncCountdownEvent *count_;
				};

			}
		}
	}
}
