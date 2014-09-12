using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api.Loggers {
    public class MultiLogger : IApiLogger {
        public MultiLogger( params IApiLogger[] loggers ) {
            mLoggers = loggers;
        }
        private IApiLogger[] mLoggers;

        public void NewLine() {
            foreach ( var logger in mLoggers )
                logger.NewLine();
        }
        public void Log( string format, params object[] args ) {
            foreach ( var logger in mLoggers )
                logger.Log( format, args );
        }
        public void Divider() {
            foreach ( var logger in mLoggers )
                logger.Divider();
        }
        public void Header( string title ) {
            foreach ( var logger in mLoggers )
                logger.Header( title );
        }
    }
}
