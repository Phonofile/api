using Phonofile.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Results {
    public class ApiContributorsResult {
        public ApiContributorModel[] Contributors { get; set; }
        public ApiPublisherModel[] Publishers { get; set; }
    }
}
