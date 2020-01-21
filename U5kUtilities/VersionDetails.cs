using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Utilities
{
    public class VersionDetails
    {
        public class VersionDataFileItem
        {
            public string Path { get; set; }
            public string Date { get; set; }
            public string Size { get; set; }
            public string MD5 { get; set; }
        }
        public class VersionDataComponent
        {
            public string Name { get; set; }
            public List<VersionDataFileItem> Files = new List<VersionDataFileItem>();
        }
        public class VersionData
        {
            public string Server { get; set; }
            public string Version { get; set; }
            public string Fecha { get; set; }
            public List<VersionDataComponent> Components = new List<VersionDataComponent>();
        }

        VersionData version;
        public VersionDetails(string filepath)
        {
            version = JsonConvert.DeserializeObject<VersionData>(File.ReadAllText(@filepath));
            version.Server = System.Environment.MachineName;
            foreach (VersionDataComponent component in version.Components)
            {
                foreach (VersionDataFileItem fileitem in component.Files)
                {
                    if (File.Exists(fileitem.Path))
                    {
                        FileInfo fi = new FileInfo(fileitem.Path);
                        fileitem.Date = fi.LastWriteTime.ToShortDateString();
                        fileitem.Size = fi.Length.ToString();
                        fileitem.MD5 = EncryptionHelper.FileMd5Hash(fileitem.Path);
                    }
                    else
                    {
                        fileitem.Date = fileitem.Size = fileitem.MD5 = "File not found";
                    }
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(version);
        }
    }
}
