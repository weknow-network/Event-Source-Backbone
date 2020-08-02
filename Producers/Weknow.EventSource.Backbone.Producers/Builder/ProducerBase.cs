using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

using Weknow.EventSource.Backbone.Building;

using Bucket = System.Collections.Immutable.ImmutableDictionary<string, System.ReadOnlyMemory<byte>>;

// Flow Build ->
//      CodeGenerator -> 
//      ProducerBase.SendAsync(callInfo) ->
//                  .SendAsync(id, payload, interceptorsData, callInfo) -> 
//                  .SendAsync(plan, id, payload, interceptorsData, callInfo) ->
//                  .ClassifyAsync(plan, callInfo, payload) ->
//                      @foreach SegmentationStrategies
//                          @foreach call parameter
//                              DoSegmentationAsync(strategy, parameter) ->
//                              -- gen code
//                              DoSegmentationAsync(strategy, options, parameter) ->


namespace Weknow.EventSource.Backbone
{
    /// <summary>
    /// Handle the producing pipeline
    /// </summary>
    public abstract class ProducerBase
    {
        private readonly ProducerPlan _plan;

        #region Ctor

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="plan">The plan.</param>
        public ProducerBase(ProducerPlan plan)
        {
            _plan = plan;
        }

        #endregion // Ctor

        protected abstract ValueTask<Bucket> DoSegmentationAsync(
            IProducerAsyncSegmentationStrategy strategy,
            IEventSourceOptions options,
            ParamData parameter); // only use for the generics call formation

        #region ClassifyAsync

        /// <summary>
        /// Classify the operation payload from method arguments.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="callInfo">The call information.</param>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        private async ValueTask<Bucket> ClassifyAsync(
                                            ProducerPlan plan,
                                            CallInfo callInfo,
                                            Bucket payload)
        {
            foreach (var strategy in plan.SegmentationStrategies)
            {
                foreach (ParamData parameter in callInfo.Parameters)
                {
                    Bucket newSerments = await DoSegmentationAsync(strategy, plan.Options, parameter);

                    #region Validation

                    if (newSerments == Bucket.Empty)
                        continue;

                    #endregion // Validation

                    payload = payload.AddRange(newSerments);
                }
            }

            return payload;
        }

        #endregion // ClassifyAsync

        #region ClassifyArgumentAsync

        /// <summary>
        /// Prepare data of single argument in an operation
        /// for sending.
        /// By classifies the data into segments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strategy">The strategy.</param>
        /// <param name="options">The options.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="producedData">The produced data.</param>
        /// <returns></returns>
        protected async ValueTask<Bucket> ClassifyArgumentAsync<T>(
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
                // TODO: Log warning $"{nameof(strategy.TryClassifyAsync)} don't expect to return null value");
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
        /// <param name="callInfo">The call information.</param>
        /// <returns></returns>
        protected ValueTask SendAsync(
            CallInfo callInfo)
        {
            string id = Guid.NewGuid().ToString();

            var payload = Bucket.Empty;
            var interceptorsData = Bucket.Empty;
            return SendAsync(
                        _plan,
                        id,
                        payload, 
                        interceptorsData,
                        callInfo);
        }

        /// <summary>
        /// Sends the produced data via the channel.
        /// </summary>
        /// <param name="plan">The plan.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="payload">The payload.</param>
        /// <param name="interceptorsData">The interceptors data.</param>
        /// <param name="callInfo">The call information.</param>
        /// <returns></returns>
        protected async ValueTask SendAsync(
            ProducerPlan plan,
            string id,
            Bucket payload,
            Bucket interceptorsData,
            CallInfo callInfo)
        {
            Metadata metadata = new Metadata(
                                        id, 
                                        plan.Partition, 
                                        plan.Shard, 
                                        callInfo.Operation); 

            payload = await ClassifyAsync(plan, callInfo, payload);

            interceptorsData = await InterceptAsync(
                                            plan.Interceptors, 
                                            metadata, 
                                            payload, 
                                            interceptorsData);

            var announcement = new Announcement(
                metadata,
                payload,
                interceptorsData);

            if (plan.Forwards.Count == 0) // merged
            {
                await plan.Channel.SendAsync(announcement);
                return;
            }

            foreach (var forward in plan.Forwards)
            {   // merged scenario
                await SendAsync(
                            forward.Plan,
                            id,
                            payload,
                            interceptorsData,
                            callInfo);
            }
        }

        #endregion // SendAsync

        protected class CallInfo
        {
            public CallInfo(
                string operation,
                ParamData[] parameters)
            {
                Operation = operation;
                Parameters = parameters;
            }

            public string Operation { get; }
            public ParamData[] Parameters { get; }
        }

        protected class ParamData
        {
            public ParamData(
                        string parametersName,
                        Type parameterType,
                        object parametersValue)
            {
                ParametersName = parametersName;
                ParameterType = parameterType;
                ParametersValue = parametersValue;
            }

            public string ParametersName { get; }
            public Type ParameterType { get; }
            public object ParametersValue { get; }
        }
    }
}
