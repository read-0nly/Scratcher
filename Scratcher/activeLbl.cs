using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;

namespace Scratcher
{
    class activeLbl : System.Windows.Forms.Label
    {
        public System.Xml.XmlNode xml;
        public Boolean firstRun = true;
        public System.Diagnostics.PerformanceCounter pc;
        public List<EventLogQuery> eventsQuery = new List<EventLogQuery>();
        public List<EventLogReader> eventsReader = new List<EventLogReader>();
    }
}
