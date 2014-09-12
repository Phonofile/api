using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Phonofile.Api {
    public class ApiResource : IApiResource {
        public long OwnerID { get; set; }
        public long CompanyID { get; set; }

        public string Type { get; set; }
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public System.IO.Stream GetStream() {
            return new FileStream( FileName, FileMode.Open, FileAccess.Read );
        }

        public static ApiResource ContributorImage( long contributorId, string fileName ) {
            return new ApiResource {
                Type = "artistImage",
                OwnerID = contributorId,
                FileName = fileName,
                ContentType = "image/jpg"
            };
        }

        public static ApiResource CoverImage( long releaseId, string fileName ) {
            return new ApiResource {
                Type = "coverImage",
                OwnerID = releaseId,
                FileName = fileName,
                ContentType = "image/jpg"
            };
        }
    }
}
