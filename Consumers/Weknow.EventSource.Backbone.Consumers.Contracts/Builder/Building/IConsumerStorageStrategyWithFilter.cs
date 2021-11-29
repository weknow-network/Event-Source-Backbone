using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weknow.EventSource.Backbone.Building
{
    /// <summary>
    /// Responsible to load information from storage.
    /// The information can be either Segmentation or Interception.
    /// When adding it via the builder it can be arrange in a chain in order of having
    /// 'Chain of Responsibility' for saving different parts into different storage (For example GDPR's PII).
    /// Alternative, chain can serve as a cache layer.
    /// </summary>
    public interface IConsumerStorageStrategyWithFilter: IConsumerStorageStrategy
    {
        /// <summary>
        /// Determines whether is of the right target type.
        /// </summary>
        /// <param name="type">The type.</param>

        bool IsOfTargetType(EventBucketCategories type);
    }
}
