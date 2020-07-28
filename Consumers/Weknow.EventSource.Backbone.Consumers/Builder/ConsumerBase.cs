using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public class ConsumerBase<T>
    {
        private readonly ConsumerParameters _parameters;
        private readonly Func<ShardMetadata, T> _factory;

        public ConsumerBase(ConsumerParameters parameters, Func<ShardMetadata, T> factory)
        {
            _parameters = parameters;
            _factory = factory;
            _parameters.Channel.Init(_parameters.Options);
            _parameters.Channel.ReceiveAsync(HandleAsync);
        }

        private async ValueTask HandleAsync(Announcement arg)
        {
            var instance = _factory(new ShardMetadata(arg.Metadata, null));
            var method = instance.GetType().GetMethod(arg.Metadata.Operation, BindingFlags.Public | BindingFlags.Instance);
            var parameters = method.GetParameters();
            var arguments = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo? parameter = parameters[i];
                var unclassify = this.GetType().GetMethod("Unclassify", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(parameter.ParameterType);
                arguments[i] = await (ValueTask<object>)unclassify.Invoke(this, new object[] { arg, parameter.Name });
            }
            method.Invoke(instance, arguments);
        }

        private async ValueTask<object> Unclassify<T>(Announcement arg, string argumentName)
        {
            foreach (var strategy in _parameters.SegmentationStrategies)
            {
                var (isValid, value) = await strategy.TryUnclassifyAsync<T>(arg.Segments, arg.Metadata.Operation, argumentName, _parameters.Options);
                if (isValid)
                    return value;
            }
            throw new NotSupportedException();
        }
    }
}
