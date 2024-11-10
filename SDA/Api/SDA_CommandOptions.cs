using System.Collections.Generic;

namespace SIPS.Framework.SDA.Api
{
    public class SDA_CommandOptions
    {
        public int CommandTimeout { get; set; }
        public int BatchSize { get; set; }
        public int BacthSizeDefault { get => 50000; }
        public bool UseBatch { get; set; }
        readonly Dictionary<string, object> _customeOptions;
        public SDA_CommandOptions()
        {
            _customeOptions = new Dictionary<string, object>();
        }

        // add method to set custom options
        public void SetCustomOption(string key, object value)
        {
            _customeOptions[key] = value;
        }

        // add method to get custom options
        public object GetCustomOption(string key)
        {
            return _customeOptions[key];
        }

        // add method to get all custom options
        public IReadOnlyDictionary<string, object> GetCustomOptions()
        {
            return _customeOptions;
        }

        // add method to remove custom option
        public void RemoveCustomOption(string key)
        {
            _customeOptions.Remove(key);
        }

        public void ClearCustomOptions()
        {
            _customeOptions.Clear();
        }

        // add method to check if custom option exists
        public bool ContainsCustomOption(string key)
        {
            return _customeOptions.ContainsKey(key);
        }

        // add method to add multiple custom options
        public void AddCustomOptions(Dictionary<string, object> options)
        {
            foreach (var option in options)
            {
                _customeOptions[option.Key] = option.Value;
            }
        }

    }

}
