using Phonofile.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api {
    public static class ApiExtensions {
        public static void SetDescriptor( this IHasDescriptors c, String language, String value, String type = null ) {
            if ( c.Descriptors == null )
                c.Descriptors = new ApiDescriptorModel[ 0 ];

            var existing = c.Descriptors.FirstOrDefault( d => d.Type == type && d.Language == language );
            if ( existing == null ) {
                var newList = new List<ApiDescriptorModel>( c.Descriptors );
                newList.Add( new ApiDescriptorModel { Language = language, Type = type ?? "Description", Value = value } );
                c.Descriptors = newList.ToArray();
            } else
                existing.Value = value;
        }
    }
}
