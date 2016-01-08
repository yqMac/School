using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Model
{
    [Serializable()]
    public class BookMark
    {
        private string markLocation;

        public string MarkLocation
        {
            get { return markLocation; }
            set { markLocation = value; }
        }
        private string markUrl;

        public string MarkUrl
        {
            get { return markUrl; }
            set { markUrl = value; }
        }
    }
}
