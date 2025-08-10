using Aix.NatsMessageBus.Model;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{
    public interface INatsMessageBus : INatsPublisher, INatsRequester, INatsSubscriber, IDisposable
    {

    }

    public interface IMySubscription
    {

        void Unsubscribe();

    }

    public class NatsSubscription : IMySubscription
    {
        private ISubscription _subscription;
        public NatsSubscription(ISubscription subscription)
        {
            _subscription = subscription;
        }

        public void Unsubscribe()
        {
            _subscription.Unsubscribe();
        }
    }

}
