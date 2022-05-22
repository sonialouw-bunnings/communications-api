using Bunnings.Common.WebApi.Models.Settings;

namespace Bunnings.Common.WebApi.Models
{
    /// <summary>
    /// Common API Application Settings that every API should have.
    /// </summary>
    public interface IApplicationSettings
    {
        /// <summary>
        /// API / Application Name. Used in logging, swagger docs and various other places.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// API Base URL
        /// </summary>
        public string BaseUrl { get; set; }

        public bool IsUsingAbsoluteLinks => true;

        public bool IsUsingHttps => true;

        /// <summary>
        /// OAuth2 URI for token and authorization endpoints
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// OAuth2: Only accept JWT tokens that are to be used with this API (Audience is in the token audience collection)
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// OAuth2: api secret used to communicate with the introspection endpoint to load user information.
        /// </summary>
        public string ApiSecret => string.Empty;

        /// <summary>
        /// Path to the role map
        /// </summary>
        public string RoleMap => string.Empty;

        /// <summary>
        /// API Default version if no version is provided on controller.
        /// </summary>
        public Version Version { get; set; }
    }
}
