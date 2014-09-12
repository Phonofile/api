using Phonofile.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Phonofile.Api {
    public interface IHasDescriptors {
        ApiDescriptorModel[] Descriptors { get; set; }
    }
}
