using Jpinsoft.LangTainer.ContainerStorage.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Jpinsoft.LangTainer.ContainerStorage
{
    public class JsonFileSmartStorage<TStorage> : ISmartStorage<TStorage>
    {
        private Dictionary<string, SmartData<TStorage>> storageDictionary = new Dictionary<string, SmartData<TStorage>>();
        private string storageFile;

        public string KeyName { get; private set; }

        private bool wasChanged = false;

        private void CheckInit()
        {
            if (string.IsNullOrEmpty(KeyName) || string.IsNullOrEmpty(storageFile))
                throw new Exception($"JsonFileSmartStorage was not initialized. You must call {nameof(InitStorage)} first.");
        }

        private string GetDictKey(string dataKey, string contextKey)
        {
            return $"{contextKey}-{dataKey}".ToLower();
        }

        public void InitStorage(string storageKeyName, string storageFolder)
        {
            if (string.IsNullOrEmpty(storageKeyName) || string.IsNullOrEmpty(storageFolder))
                throw new ArgumentException($"Param '{nameof(storageKeyName)}' or '{nameof(storageFolder)}' param is empty.", nameof(storageKeyName));

            if (!Directory.Exists(storageFolder))
                throw new ArgumentException($"Folder '{storageFolder}' does not exists", nameof(storageFolder));

            this.KeyName = storageKeyName;
            this.storageFile = Path.Combine(storageFolder, storageKeyName + ".json");

            storageDictionary.Clear();

            if (File.Exists(storageFile))
            {
                using (StreamReader sr = new StreamReader(storageFile))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        JsonSerializer serializer = new JsonSerializer();

                        SmartData<TStorage>[] data = serializer.Deserialize<SmartData<TStorage>[]>(reader);

                        foreach (SmartData<TStorage> item in data)
                            storageDictionary.Add(GetDictKey(item.Key, item.ContextKey), item);
                    }
                }
            }
        }

        public List<SmartData<TStorage>> SearchSmartData(Func<SmartData<TStorage>, bool> searchKeyPredicate = null)
        {
            CheckInit();

            if (searchKeyPredicate == null)
                return storageDictionary.Values.ToList();

            return storageDictionary.Values.Where(searchKeyPredicate).ToList();
        }

        public SmartData<TStorage> GetSmartData(string dataKey, string contextKey = null)
        {
            CheckInit();

            string dictKey = GetDictKey(dataKey, contextKey);

            if (!storageDictionary.ContainsKey(dictKey))
                return null;

            return storageDictionary[dictKey];
        }

        // TODO: zabranit menenia Storage mimo tejto metody SetSmartData!! Napr. cez Proxy objekty
        public void SetSmartData(TStorage data, string dataKey, string contextKey = null)
        {
            CheckInit();

            if (string.IsNullOrEmpty(dataKey) || data == null)
                throw new ArgumentNullException($"{nameof(dataKey)},{nameof(data)}");

            string dictKey = GetDictKey(dataKey, contextKey);

            if (!storageDictionary.ContainsKey(dictKey))  // ADD    
            {
                storageDictionary.Add(dictKey, new SmartData<TStorage>
                {
                    Key = dataKey,
                    ContextKey = contextKey,
                    Created = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    DataObject = data
                });
            }
            else // UPDATE
            {
                storageDictionary[dictKey].LastUpdate = DateTime.Now;
                storageDictionary[dictKey].DataObject = data;
            }

            wasChanged = true; // Skusit aj pre jednotlive zaznamy SmartData
        }

        public bool DeleteSmartData(string dataKey, string contextKey = null)
        {
            CheckInit();

            if (string.IsNullOrEmpty(dataKey))
                throw new ArgumentNullException($"{nameof(dataKey)}");

            string dictKey = GetDictKey(dataKey, contextKey);

            if (storageDictionary.Remove(dictKey))
            {
                wasChanged = true;
                return true;
            }

            return false;
        }

        public bool SaveChanges()
        {
            CheckInit();

            if (wasChanged)
            {
                using (StreamWriter sw = new StreamWriter(storageFile))
                {
                    using (JsonTextWriter writer = new JsonTextWriter(sw))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Serialize(writer, storageDictionary.Select(kvp => kvp.Value));
                    }
                }

                wasChanged = false;
                return true;
            }

            return false;
        }

        public void ResetStorage()
        {
            CheckInit();

            try { File.Delete(storageFile); }
            catch { }

            storageDictionary.Clear();
            wasChanged = true;
        }
    }
}
