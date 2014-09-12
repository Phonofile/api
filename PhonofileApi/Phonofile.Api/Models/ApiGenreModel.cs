using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonofile.Api.Results {
    public class ApiGenreModel {
        public long ID { get; set; }
        public String Name { get; set; }
        public String Path { get; set; }
        public long CategoryID { get; set; }
        public long StyleID { get; set; }
    }

    public class ApiGenreStyleModel {
        public long ID { get; set; }
        public String Name { get; set; }
    }

    public class ApiGenreCategoryModel {
        public long ID { get; set; }
        public String Name { get; set; }
    }
}
