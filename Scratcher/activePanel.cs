using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scratcher
{
    class activePanel : System.Windows.Forms.FlowLayoutPanel
    {
        public System.Xml.XmlNode xml;
        public List<activeLbl> dynalbl = new List<activeLbl>();
    }
}
