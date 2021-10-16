using NATS.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus.Internals
{
    public static class AsyncCallResponseManage
    {
        public static AsyncCallResponseManage<object> JetStreamReplyManage = new AsyncCallResponseManage<object>();
    }

    public class AsyncCallResponseManage<T>
    {
        ConcurrentDictionary<long, AsyncCallScope<T>> Hash = new ConcurrentDictionary<long, AsyncCallScope<T>>();

        long RequestId = 0;


        public long CreateRequestId()
        {
            long id = Interlocked.Increment(ref this.RequestId);
            if (id < 0) //Check if recycled
            {
                lock (this)
                {
                    if (id < 0)
                    {
                        this.RequestId = 0;
                    }
                    id = Interlocked.Increment(ref this.RequestId);
                }
            }

            return id;
        }


        public AsyncCallScope<T> RegisterRequest(long requestId, int timeout)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var scope = new AsyncCallScope<T>(requestId, this, tcs, timeout);
            Hash.TryAdd(requestId, scope);
            Timeout(requestId, tcs, timeout);
            return scope;
        }

        public void SetResult(long requestId, T value)
        {
            AsyncCallScope<T> node;
            Hash.TryGetValue(requestId, out node);
            this.Hash.TryRemove(requestId, out _);

            if (node != null)
            {
                node.Waiter.TrySetResult(value);
            }

        }

        public void Remove(long requestId)
        {
            this.Hash.TryRemove(requestId, out _);
        }

        public bool SetException(long requestId, Exception ex)
        {
            bool result = false;
            AsyncCallScope<T> node;

            Hash.TryGetValue(requestId, out node);
            this.Hash.TryRemove(requestId, out _);

            if (node != null)
            {
                result = node.Waiter.TrySetException(ex);
            }

            return result;
        }

        private void Timeout(long requestId, TaskCompletionSource<T> tsc, int milliseconds)
        {
            Task.Delay(TimeSpan.FromMilliseconds(milliseconds)).ContinueWith((task) =>
            {
                string errorMsg = string.Format("请求超时,requestId:{0}", requestId);
                SetException(requestId, new TimeoutException(errorMsg));
            });

        }
    }

}
