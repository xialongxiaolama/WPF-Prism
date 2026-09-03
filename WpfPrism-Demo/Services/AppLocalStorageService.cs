using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WpfPrism_Demo.Services
{
    /// <summary>
    /// 简易封装Json文件的本地存储, 类似浏览器中的localstorage
    /// </summary>
    public class AppLocalStorageService
    {
        private readonly string _filePath;
        private Dictionary<string, string> _storage = new ();

        public AppLocalStorageService(string filePath)
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, filePath);
            Load();
        }

        /// <summary>
        /// 加载本地Json文件
        /// </summary>
        private void Load()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _storage = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
            }
        }
        /// <summary>
        /// 写入磁盘
        /// </summary>
        private void Save()
        {
            var json = JsonSerializer.Serialize(_storage, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        // 设置，对标localStorage.setItem
        public void SetItem(string key, string value)
        {
            _storage[key] = value;
            Save();
        }

        //读取
        public string? GetItem(string key)
        {
            return _storage.TryGetValue(key, out var v) ? v : null;
        }

        //删除
        public void RemoveItem(string key)
        {
            _storage.Remove(key);
            Save();
        }

        //清空
        public void Clear()
        {
            _storage.Clear();
            Save();
        }

        //存对象
        public void SetObject<T>(string key, T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            SetItem(key, json);
        }

        public T? GetObject<T>(string key)
        {
            var str = GetItem(key);
            if (str == null) return default;
            return JsonSerializer.Deserialize<T>(str);
        }
    }
}
