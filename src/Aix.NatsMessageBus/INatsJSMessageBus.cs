using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus
{
    public interface INatsJSMessageBus : INatsPublisher, INatsRequester, INatsSubscriber, IDisposable
    {

    }
}
