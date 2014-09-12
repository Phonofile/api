using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Loggers {

    public class VsOutputLogger : IApiLogger {
        public void NewLine() {
            Debug.WriteLine( "" );
        }

        public void Log( string format, params object[] args ) {
            Debug.WriteLine( format, args );
        }
        public void Divider() {
            Debug.WriteLine( "-----------------------------------------------------------------" );
        }
        public void Header( string title ) {
            NewLine();
            NewLine();
            Debug.WriteLine( "*****************************************************************" );
            Debug.Write( "** " );
            Log( title );
            Debug.WriteLine( "*****************************************************************" );
        }
    }
}
