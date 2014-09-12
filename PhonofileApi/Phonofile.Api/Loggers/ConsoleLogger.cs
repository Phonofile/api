using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Loggers {
    public class ConsoleLogger : IApiLogger {
        public void NewLine() {
            Console.WriteLine();
        }
        public void Log( string format, params object[] args ) {
            Console.WriteLine( format, args );
        }
        public void Divider() {
            Console.WriteLine( "-----------------------------------------------------------------" );
        }
        public void Header( string title ) {
            NewLine();
            NewLine();
            Console.WriteLine( "*****************************************************************" );
            Console.Write( "** " );
            Log( title );
            Console.WriteLine( "*****************************************************************" );
        }
    }
}
