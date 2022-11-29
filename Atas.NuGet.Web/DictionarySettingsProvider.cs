using System;
using System.Collections.Generic;
using NuGet.Server.Core.Infrastructure;

namespace Atas.NuGet.Web
{
	class DictionarySettingsProvider : ISettingsProvider
	{
		private readonly Dictionary<string, object> _settings;

		public DictionarySettingsProvider(Dictionary<string, object> settings)
		{
			_settings = settings;
		}

		public bool GetBoolSetting(string key, bool defaultValue)
		{
			return _settings.ContainsKey(key) ? Convert.ToBoolean(_settings[key]) : defaultValue;
		}

		public int GetIntSetting(string key, int defaultValue)
		{
			return _settings.ContainsKey(key) ? Convert.ToInt32(_settings[key]) : defaultValue;
		}

		public string GetStringSetting(string key, string defaultValue)
		{
			return _settings.ContainsKey(key) ? Convert.ToString(_settings[key]) : defaultValue;
		}
	}
}