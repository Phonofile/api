
namespace Phonofile.Api {
    public class ApiException {
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public ApiException InnerException { get; set; }

        public static string GetErrorMessage( ApiException ex ) {
            var e = ex;

            if ( e == null )
                return null;

            while ( e.InnerException != null )
                e = e.InnerException;

            return e.ExceptionMessage;
        }
    }
}
