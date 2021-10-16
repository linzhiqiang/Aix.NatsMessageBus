using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus.Internals
{
    public class AsyncCallScope<T> : IDisposable
    {
        public TaskCompletionSource<T> Waiter;
        long RequestId;
        private int Timeout;
        AsyncCallResponseManage<T> ResponseManage;

        public AsyncCallScope(long requestId, AsyncCallResponseManage<T> responseManage, TaskCompletionSource<T> tcs, int timeout)
        {
            RequestId = requestId;
            ResponseManage = responseManage;
            this.Waiter = tcs;
            this.Timeout = timeout;
        }

        public void Dispose()
        {
            this.ResponseManage.Remove(RequestId);

            this.Waiter.TrySetCanceled();
        }

    }
}
