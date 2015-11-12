using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DTViewer
{
    [Serializable]
    public sealed class ViewerSettings
    {
        public string LastDataSettingsFileName { get; set; }

        public string LastClosedPositionFileName { get; set; }

        public static ViewerSettings LoadFromFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            ViewerSettings settings;

            var serializer = new XmlSerializer(typeof(ViewerSettings));

            using (var reader = new StreamReader(file))
            {
                settings = (ViewerSettings)serializer.Deserialize(reader);
            }

            return settings;
        }

        public void SaveToFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                throw new ArgumentNullException();
            }

            var serializer = new XmlSerializer(typeof(ViewerSettings));

            using (var writer = new StreamWriter(file))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}
