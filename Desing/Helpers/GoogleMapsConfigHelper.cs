using System;
using System.Configuration;

namespace Desing.Helpers
{
    /// <summary>
    /// Resolves Google Maps settings from environment variable or Web.config
    /// (including optional merge file Web.GoogleMaps.config — see Web.GoogleMaps.config.example).
    /// </summary>
    public static class GoogleMapsConfigHelper
    {
        private const string ApiKeySetting = "GoogleMaps:ApiKey";
        private const string LanguageSetting = "GoogleMaps:Language";
        private const string RegionSetting = "GoogleMaps:Region";
        private const string EnvApiKey = "GOOGLE_MAPS_API_KEY";

        public static string GetApiKey()
        {
            var fromEnv = Environment.GetEnvironmentVariable(EnvApiKey);
            if (!string.IsNullOrWhiteSpace(fromEnv))
            {
                return fromEnv.Trim();
            }

            return ConfigurationManager.AppSettings[ApiKeySetting] ?? string.Empty;
        }

        public static string GetLanguage()
        {
            return ConfigurationManager.AppSettings[LanguageSetting] ?? "es";
        }

        public static string GetRegion()
        {
            return ConfigurationManager.AppSettings[RegionSetting] ?? "ES";
        }

        public static bool IsApiKeyConfigured()
        {
            return !string.IsNullOrWhiteSpace(GetApiKey());
        }
    }
}
