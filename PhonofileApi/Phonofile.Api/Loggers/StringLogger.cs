using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Loggers {
    public class StringLogger : IApiLogger {
        private StringBuilder mLog = new StringBuilder();

        public void Divider() {
            mLog.AppendLine( "-----------------------------------------------------------------" );
        }

        public void Header( string title ) {
            NewLine();
            NewLine();
            mLog.AppendLine( "*****************************************************************" );
            mLog.Append( "** " );
            Log( title );
            mLog.AppendLine( "*****************************************************************" );
        }

        public void Log( string format, params object[] args ) {
            mLog.AppendFormat( format, args );
        }

        public void NewLine() {
            mLog.AppendLine();
        }

        public override string ToString() {
            return mLog.ToString();
        }
    }
}
