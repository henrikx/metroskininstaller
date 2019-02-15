using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metro_Skin_Installer
{
    public class Extra
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string Path { get; set; }
        public List<string> linkedExtrasIds { get; set; }
        public string Category { get; set; }
        public string previewPicturePath { get; set; }
        public bool scheduledInstall { get; set; }
    }
}
