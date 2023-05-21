using Refit;

namespace EventSource.Backbone.WebEventTest
{

    /// <summary>
    /// Structure of the target migration service
    /// </summary>
    /// <remarks>read more: https://github.com/reactiveui/refit</remarks>
    public interface IEventsMigration
    {
        /// <summary>
        /// Forward events (announcements)
        /// </summary>
        /// <param name="announcement">The announcement.</param>
        /// <returns></returns>
        [Post("")]
        Task ForwardEventAsync([Body] Announcement announcement);
    }
}
