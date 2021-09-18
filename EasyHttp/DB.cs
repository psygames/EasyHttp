using LitJson;
using System;
using System.Collections.Generic;
using System.IO;

namespace EasyHttp
{
    public class DB<T>
    {
        private Dictionary<string, T> data = new Dictionary<string, T>();

        public T Get(string key)
        {
            data.TryGetValue(key, out var val);
            return val;
        }

        public T Get(Predicate<T> predicate)
        {
            foreach (var value in data.Values)
            {
                if (predicate(value))
                {
                    return value;
                }
            }
            return default;
        }

        public void Set(string key, T val)
        {
            data[key] = val;
        }

        public void Load(string path)
        {
            var text = File.ReadAllText(path);
            data = JsonMapper.ToObject<Dictionary<string, T>>(text);
        }

        public void Save(string path)
        {
            var text = JsonMapper.ToJson(data);
            File.WriteAllText(path, text);
        }
    }
}
