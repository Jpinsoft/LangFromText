using System;
using System.Collections.Generic;
using System.Text;

namespace Jpinsoft.LangTainer.ContainerStorage.Types
{
    public interface ISmartStorage<TStorage>
    {
        /// <summary>
        /// Jedinecny nazov kontajnera
        /// </summary>
        string KeyName { get; }

        void InitStorage(string storageKeyName, string connString);

        /// <summary>
        /// Prehlada vsetky kluce pomocou searchKeyPredicate. SearchPredicate je povinny!!
        /// </summary>
        List<SmartData<TStorage>> SearchSmartData(Func<SmartData<TStorage>, bool> searchKeyPredicate = null);

        /// <summary>
        /// Ak dany kluc neexistuje, vrati NULL
        /// </summary>
        SmartData<TStorage> GetSmartData(string key, string contextKey = null);

        /// <summary>
        /// Ak udaj s danym klucom uz existuje, vykona UPDATE, inak CREATE.
        /// </summary>
        void SetSmartData(TStorage data, string dataKey, string contextKey = null);


        /// <summary>
        /// Ak najde data s danym klucom v data a uspesne vykona odstranenie vrati TRUE. Inak FALSE.
        /// </summary>        
        bool DeleteSmartData(SmartData<TStorage> data);

        /// <summary>
        /// Ak najde data s danym klucom a uspesne vykona odstranenie vrati TRUE. Inak FALSE.
        /// </summary>
        bool DeleteSmartData(string dataKey, string contextKey = null);

        /// <summary>
        /// Ulozi zmeny, pokial nebol ziadne data v ulozisku zmenene, nevykona ziadnu akciu.
        /// Vrati True ak vykonal ulozenie
        /// </summary>
        bool SaveChanges();
    }
}
