using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Common
{
    public class IoCContainer : IDisposable
    {
        public IServiceCollection ServiceCollection { get; } = new ServiceCollection();

        protected IoCContainer()
        {
        }

        private IServiceProvider _spRoot;
        private IServiceScope _scopeRoot;

        public IServiceProvider ServiceProvider => _scopeRoot?.ServiceProvider;

        public void Build()
        {
            _spRoot = ServiceCollection?.BuildServiceProvider();
            _scopeRoot = _spRoot.CreateScope();
        }

        public void Dispose()
        {
            _scopeRoot?.Dispose();
            _scopeRoot = null;
            _spRoot = null;
        }

        public T GetService<T>()
        {
            return ServiceProvider.GetService<T>();
        }

        public void AddSingleton<T>(T instance) where T : class
        {
            IoCContainer.Instance.ServiceCollection.AddSingleton<T>(instance);
        }

        public void TryAddSingletonAddSingleton<T>(T instance) where T : class
        {
            IoCContainer.Instance.ServiceCollection.TryAddSingleton<T>(instance);
        }

        private static IoCContainer _container;
        private static object _locker = new object();

        public static IoCContainer Instance
        {
            get
            {
                if (_container == null)
                {
                    lock (_locker)
                    {
                        if (_container == null)
                        {
                            _container = new IoCContainer();
                        }
                    }
                }

                return _container;
            }
        }

    }
}
