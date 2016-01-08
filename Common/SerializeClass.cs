using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Model;

namespace Common
{
    public class SerializeClass
    {
        public static void SerializeObject(object obj)
        {
            try
            {
                FileStream fs = new FileStream("../../Data/Servers.Dat", FileMode.Create);
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, obj);
                fs.Close();
            }
            catch (Exception)
            { }
        }

        public static List<RemoteServers> DeSerializeServers()
        {
            try
            {
                FileStream fs1 = new FileStream("../../Data/Servers.Dat", FileMode.Open);
                BinaryFormatter bf1 = new BinaryFormatter();
                List<RemoteServers> listRemoteServers = (List<RemoteServers>)bf1.Deserialize(fs1);
                fs1.Close();
                return listRemoteServers;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}