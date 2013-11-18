using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SimpleDictionary.Utility
{
    internal static class DeepCopy
    {
        /// <summary>
        /// Makes a deep copy of the specified Object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToCopy">Object to make a deep copy of.</param>
        /// <returns>Deep copy of the Object</returns>
        public static T Make<T>(T objectToCopy) where T : class
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, objectToCopy);
                ms.Position = 0;
                return (T) bf.Deserialize(ms);
            }
        }
    }
}