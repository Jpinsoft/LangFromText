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
        Dictionary<string, SmartData<TStorage>> storageDictionary = new Dictionary<string, SmartData<TStorage>>();
        private string storageFile;

        public string KeyName { get; private set; }
        private bool wasChanged = false;

        public void InitStorage(string storageKeyName, string storageFolder)
        {
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
            if (searchKeyPredicate == null)
                return storageDictionary.Values.ToList();

            return storageDictionary.Values.Where(searchKeyPredicate).ToList();
        }

        public SmartData<TStorage> GetSmartData(string dataKey, string contextKey = null)
        {
            string dictKey = GetDictKey(dataKey, contextKey);

            if (!storageDictionary.ContainsKey(dictKey))
                return null;

            return storageDictionary[dictKey];
        }

        // TODO: zabranit menenia Storage mimo tejto metody SetSmartData!! Napr. cez Proxy objekty
        public void SetSmartData(TStorage data, string dataKey, string contextKey = null)
        {
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


        public bool DeleteSmartData(SmartData<TStorage> data)
        {
            if (data == null)
                throw new ArgumentNullException($"{nameof(data)}");

            return DeleteSmartData(data.Key, data.ContextKey);
        }

        public bool DeleteSmartData(string dataKey, string contextKey = null)
        {
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

        private string GetDictKey(string dataKey, string contextKey)
        {
            return $"{contextKey}-{dataKey}".ToLower();
        }
    }
}
