using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Results {
    public class ApiConstantsResult {
        public ApiGenreModel[] Genres { get; set; }
        public ApiGenreStyleModel[] Styles { get; set; }
        public ApiGenreCategoryModel[] Categories { get; set; }


        public static void BuildGenrePaths( ApiConstantsResult result ) {

            var styles = result.Styles.ToDictionary( s => s.ID );
            var categories = result.Categories.ToDictionary( c => c.ID );

            foreach ( var genre in result.Genres ) {

                var category = categories[ genre.CategoryID ];
                var style = styles[ genre.StyleID ];

                if ( String.Equals( category.Name, style.Name, StringComparison.OrdinalIgnoreCase ) || String.Equals( style.Name, genre.Name, StringComparison.OrdinalIgnoreCase ) )
                    style = null;

                genre.Path = category.Name + " > " + ( style != null ? style.Name + " > " : null ) + genre.Name;
            }
        }
    }
}
