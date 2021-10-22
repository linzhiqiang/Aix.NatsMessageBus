using Aix.NatsMessageBus.Model;
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

  

}
