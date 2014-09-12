using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Phonofile.Api {
    public interface IApiResource {
        long OwnerID { get; }
        long CompanyID { get; }

        string Type { get; }
        string FileName { get; }
        string ContentType { get; }

        Stream GetStream();
    }
}
