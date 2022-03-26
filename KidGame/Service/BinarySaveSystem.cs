using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KidGame.Service
{
    public static class BinarySaveSystem 
    {

        private static readonly string FilePath = Application.persistentDataPath + "/serv3.dta";

        public static void Save(SubsData saveData)
        {

            FileStream dataStream = new FileStream(FilePath, FileMode.Create);

            BinaryFormatter converter = new BinaryFormatter();
            converter.Serialize(dataStream, saveData);

            dataStream.Close();
        }

        public static SubsData Load()
        {
            if (File.Exists(FilePath))
            {
                // File exists 
                FileStream dataStream = new FileStream(FilePath, FileMode.Open);

                BinaryFormatter converter = new BinaryFormatter();
                SubsData saveData = converter.Deserialize(dataStream) as SubsData;

                dataStream.Close();
                return saveData;
            }
            else
            {
                // File does not exist
                Debug.Log("Save file not found in " + FilePath);
                return new SubsData();
            }
        }
    }
}