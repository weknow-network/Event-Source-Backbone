using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using NJsonSchema.Generation;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Weknow core extensions for ASP.NET Core
    /// </summary>
    public static class OpenAPIExtensions
    {
        private const string TERMS_OF_SERVICE = "Free (MIT)";
        private const string DESC = "";

        private static readonly NamingStrategy _namingStrategy = new CamelCaseNamingStrategy();

        #region AddOpenAPIWeknow

        /// <summary>
        /// Adds the weknow open API configuration.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="version">The version.</param>
        /// <param name="title">The title.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// version
        /// or
        /// title
        /// </exception>
        public static IServiceCollection AddOpenAPIWeknow(
            this IServiceCollection services,
            string version,
            string title,
            string? description = null)
        {
            #region Validations

            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));
            if (string.IsNullOrEmpty(title))
                throw new ArgumentNullException(nameof(title));

            #endregion // Validations

            #region services.Configure<ApiBehaviorOptions>(...)

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // disable the module validation in order to enable to send in-process data
                options.SuppressModelStateInvalidFilter = true;
            });

            #endregion // services.Configure<ApiBehaviorOptions>(...)

            #region services.AddOpenApiDocument(...)

            services.AddOpenApiDocument(cfg =>
            {
                cfg.GenerateExamples = true;
                cfg.GenerateEnumMappingDescription = true;
                cfg.IgnoreObsoleteProperties = true;
                cfg.PostProcess = document =>
                {
                    document.Info.Version = version;
                    document.Info.Title = title;
                    document.Info.Description = description ?? DESC;
                    document.Info.TermsOfService = TERMS_OF_SERVICE;
                    //document.Info.License = new NSwag.OpenApiLicense
                    //{
                    //    Name = "Private library",
                    //    Url = "https://example.com/license"
                    //};
                };
                var setting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                };
                setting.Converters.Add(new StringEnumConverter(_namingStrategy, true));
                cfg.SerializerSettings = setting;
                cfg.DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull;
            });

            #endregion // services.AddOpenApiDocument(...)

            return services;
        }

        #endregion // AddOpenAPIWeknow

        #region UseOpenAPIWeknow

        /// <summary>
        /// Use the weknow open API configuration.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseOpenAPIWeknow(this IApplicationBuilder app)
        {
            app.UseOpenApi(cfg =>
            {
                cfg.DocumentName = "Event Source";

            });
            app.UseSwaggerUi3(cfg =>
            {

            });

            return app;
        }

        #endregion // UseOpenAPIWeknow
    }
}
