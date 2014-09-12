using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phonofile.Api {
    public interface IApiLogger {
        void Divider();
        void Header( string title );
        void Log( string format, params object[] args );
        void NewLine();
    }
}
