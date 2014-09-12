using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonofile.Api.Models {
    public class ApiServiceLinkModel {
        public long ItemID { get; set; }

        public String WebUri { get; set; }
        public String ClientUri { get; set; }

        public String VendorID { get; set; }
        public String ParentVendorID { get; set; }

        public String ServiceName { get; set; }
        public long ServiceID { get; set; }

        public String Type { get; set; }
    }
}
