namespace EventSourcing.Backbone
{
    /// <summary>
    /// Handle the producing pipeline
    /// CodeGenerator : generate class which inherit from ProducerPipeline
    /// ---------- ProducerPipeline - pipeline which invoke on each call  -----------
    ///          classify-commands = 
    ///             parameters.Select
    ///                 CreateClassificationAdaptor(operation, argumentName, producedData)
    ///                     return ClassifyArgumentAsync
    ///          SendAsync(operation, classifyAdaptors) // recursive
    ///             Channel.SendAsync(announcement)
    /// </summary>
    public abstract class ProducerPipeline
    {
        private readonly IProducerPlan _plan;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        [Obsolete("Reflection", true)]
        public ProducerPipeline(ref IProducerPlanBuilder plan)
        {
            _plan = plan.Build();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        public ProducerPipeline(IProducerPlan plan)
        {
            _plan = plan;
        }

        #endregion // Ctor

        #region CreateClassificationAdaptor

        /// <summary>
        /// Classify the operation payload from method arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        /// <remarks>
        /// MUST BE PROTECTED, called from the generated code
        /// </remarks>
        protected Func<IProducerPlan, Bucket, ValueTask<Bucket>> CreateClassificationAdaptor<T>(
                                            string operation,
                                            string argumentName,
                                            T producedData)
        {
            return (IProducerPlan plan, Bucket payload) =>
                                 ClassifyArgumentAsync(
                                                 plan,
                                                 payload,
                                                 operation,
                                                 argumentName,
                                                 producedData);
        }

        #endregion // CreateClassificationAdaptor

        #region ClassifyArgumentAsync

        /// <summary>
        /// Classifies the operation's argument.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="plan">The plan.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        private async ValueTask<Bucket> ClassifyArgumentAsync<T>(
                                                IProducerPlan plan,
                                                Bucket payload,
                                                string operation,
                                                string argumentName,
                                                T producedData)
        {
            foreach (var strategy in plan.SegmentationStrategies)
            {
                Bucket newSerments = await ClassifyArgumentAsync(strategy, plan.Options, operation, argumentName, producedData);

                #region Validation

                if (newSerments == Bucket.Empty)
                    continue;

                #endregion // Validation

                payload = payload.AddRange(newSerments);
            }

            return payload;
        }

        #endregion // ClassifyArgumentAsync

        #region ClassifyArgumentAsync

        /// <summary>
        /// Bridge classification of single operation's argument.
        /// Get the argument data and pass it to the segmentation strategies.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strategy">The strategy.</param>
        /// <param name="options">The options.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        private async ValueTask<Bucket> ClassifyArgumentAsync<T>(
            IProducerAsyncSegmentationStrategy strategy,
            EventSourceOptions options,
            string operation,
            string argumentName,
            T producedData)
        {
            Bucket segments = Bucket.Empty;

            var seg = await strategy.TryClassifyAsync(
                segments,
                operation,
                argumentName,
                producedData,
                options);

            #region Validation

            if (seg == null)
            {
                return segments;
            }

            #endregion // Validation

            return seg;
        }

        #endregion // ClassifyArgumentAsync

        #region InterceptAsync

        /// <summary>
        /// Call interceptors and store their intercepted data
        /// (which will be use by the consumer's interceptors).
        /// </summary>
        /// <param name="interceptors">The interceptors.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="interceptorsData">The interceptors data.</param>
        /// <returns></returns>
        private async Task<Bucket> InterceptAsync(
                                        IEnumerable<IProducerAsyncInterceptor> interceptors,
                                        Metadata metadata,
                                        Bucket payload,
                                        Bucket interceptorsData)
        {
            foreach (IProducerAsyncInterceptor interceptor in interceptors)
            {
                ReadOnlyMemory<byte> interceptorData = await interceptor.InterceptAsync(metadata, payload);
                interceptorsData = interceptorsData.Add(
                                        interceptor.InterceptorName,
                                        interceptorData);
            }

            return interceptorsData;
        }

        #endregion // InterceptAsync

        #region SendAsync

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="version">The version.</param>
        /// <param name="classifyAdaptors">The classify strategy adaptors.</param>
        /// <returns></returns>
        /// <remarks>
        /// MUST BE PROTECTED, called from the generated code
        /// </remarks>
        protected ValueTask<EventKeys> SendAsync(
            string operation,
            int version,
            params Func<IProducerPlan, Bucket, ValueTask<Bucket>>[] classifyAdaptors)
        {
            string id = Guid.NewGuid().ToString();

            var payload = Bucket.Empty;
            var interceptorsData = Bucket.Empty;
            return SendAsync(
                        _plan,
                        id,
                        payload,
                        interceptorsData,
                        operation,
                        version,
                        classifyAdaptors);
        }

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="interceptorsData">The interceptors data.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="version">The version.</param>
        /// <param name="classifyAdaptors">The classify strategy adaptors.</param>
        /// <returns></returns>
        private async ValueTask<EventKeys> SendAsync(
            IProducerPlan plan,
            string id,
            Bucket payload,
            Bucket interceptorsData,
            string operation,
            int version,
            Func<IProducerPlan, Bucket, ValueTask<Bucket>>[] classifyAdaptors)
        {
            Metadata metadata = new Metadata
            {
                MessageId = id,
                Environment = plan.Environment,
                Uri = plan.Uri,
                Operation = operation,
                Version = version
            };

            foreach (var classify in classifyAdaptors)
            {
                payload = await classify(plan, payload);
            }

            interceptorsData = await InterceptAsync(
                                            plan.Interceptors,
                                            metadata,
                                            payload,
                                            interceptorsData);

            var storageTypes = metadata.StorageTypes;
            if (!payload.IsEmpty)
                storageTypes = storageTypes | EventBucketCategories.Segments;
            if (!interceptorsData.IsEmpty)
                storageTypes = storageTypes | EventBucketCategories.Interceptions;
            if (storageTypes != metadata.StorageTypes)
                metadata = metadata with { StorageTypes = storageTypes };

            var announcement = new Announcement
            {
                Metadata = metadata,
                Segments = payload,
                InterceptorsData = interceptorsData
            };

            if (plan.ForwardPlans.Count == 0) // merged
            {
                var strategies = plan.StorageStrategies;
                var ch = plan.Channel;
                EventKey k = await ch.SendAsync(plan, announcement);
                return new EventKeys(k);
            }

            var keys = plan.ForwardPlans.Select(async forward =>
            {   // merged scenario
                EventKeys k = await SendAsync(
                             forward,
                             id,
                             payload,
                             interceptorsData,
                             operation,
                             version,
                             classifyAdaptors);
                return k;
            });
            var res = await Task.WhenAll(keys);
            return new EventKeys(res.SelectMany(m => m));
        }

        #endregion // SendAsync
    }
}
