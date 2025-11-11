using System;
using System.Collections.Generic;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "ServiceData", menuName = "Game/ServiceData")]
    public class ServiceData : ScriptableObject
    {
        public List<ServiceRecord> services;
        
        /// <summary>
        /// Find the ServiceData entry for the given ServiceType.
        /// Returns null if not found.
        /// </summary>
        public ServiceRecord GetService(ServiceType type)
        {
            if (services == null || services.Count == 0)
                return null;

            return services.Find(s => s.serviceType == type);
        }

        /// <summary>
        /// Check if a service of given type exists.
        /// </summary>
        public bool HasService(ServiceType type)
        {
            return GetService(type) != null;
        }
    }

    [Serializable]
    public class ServiceRecord
    {
        public ServiceType serviceType; // e.g. "Download Image"
        public int price;          // e.g. 500
        public CurrencyType currencyType = CurrencyType.COIN;
    }
}