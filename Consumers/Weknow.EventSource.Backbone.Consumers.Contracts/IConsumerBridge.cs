using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone
{
    public interface IConsumerBridge
    {
        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <typeparam name="TParam">The type of the parameter.</typeparam>
        /// <param name="arg">The argument.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <returns></returns>
        ValueTask<TParam> GetParameterAsync<TParam>(Announcement arg, string argumentName);
    }
}
