using System;
using System.Collections.Generic;

namespace Containers
{
    public class ControlledService
    {
        public int Id { get; set; }

        public String Key { get; set; }

        public String Name { get; set; }

        public String ServiceName { get; set; }

        public String Alias { get; set; }

        public String Path { get; set; }

        public String RemoteKey { get; set; }

        public List<String> LogFiles { get; set; }

        public ControlledService()
        {
        }

        public ControlledService(int id, string key, string name, string serviceName, string aliasName, string path)
        {
            Id = id;
            Key = key;
            Name = name;
            ServiceName = serviceName;
            Alias = aliasName;
            Path = path;
            LogFiles = new List<string>();
        }

        public ControlledService(
            int id,
            string key,
            string name,
            string serviceName,
            string aliasName,
            string path,
            string remoteKey)
        {
            Id = id;
            Key = key;
            Name = name;
            ServiceName = serviceName;
            Alias = aliasName;
            Path = path;
            RemoteKey = remoteKey;
            LogFiles = new List<string>();
        }

        public ControlledService(
            int id,
            string key,
            string name,
            string serviceName,
            string aliasName,
            string path,
            string remoteKey,
            List<String> logFiles)
        {
            Id = id;
            Key = key;
            Name = name;
            ServiceName = serviceName;
            Alias = aliasName;
            Path = path;
            RemoteKey = remoteKey;
            LogFiles = logFiles;
        }

        public override string ToString()
        {
            return string.Format(
                                 "Key: {0}, Name: {1}, ServiceName: {2}, Alias: {3}, Path: {4}, RemoteKey: {5}, LogFiles: {6}",
                                 Key,
                                 Name,
                                 ServiceName,
                                 Alias,
                                 Path,
                                 RemoteKey,
                                 EnumEx.GetStringFromArray(LogFiles));
        }
    }
}
