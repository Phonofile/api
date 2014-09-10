using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ApiTester {
    public class ApiResponse<T> {
        public T Data { get; set; }

        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public String ErrorMessage { get; set; }
        public ApiException Exception { get; set; }

        public string Content { get; set; }

        public bool Ok { get { return StatusCode == 200; } }
    }

    public class ApiException {
        public String Message { get; set; }
        public String ExceptionMessage { get; set; }
        public String StackTrace { get; set; }
        public ApiException InnerException { get; set; }

        public static string GetErrorMessage( ApiException ex ) {
            var e = ex;

            if( e == null )
                return null;

            while( e.InnerException != null )
                e = e.InnerException;

            return e.ExceptionMessage;
        }
    }

    public interface IApiLogger {
        void Log( string format, params object[] args );
        void NewLine();
    }

    public class ConsoleLogger : IApiLogger {
        public void NewLine() {
            Console.WriteLine();
        }
        public void Log( string format, params object[] args ) {
            Console.WriteLine( format, args );
        }
    }

    public class VsOutputLogger : IApiLogger {
        public void NewLine() {
            Debug.WriteLine( "" );
        }

        public void Log( string format, params object[] args ) {
            Debug.WriteLine( format, args );
        }
    }

    public class MultiLogger : IApiLogger {
        public MultiLogger( params IApiLogger[] loggers ) {
            mLoggers = loggers;
        }
        private IApiLogger[] mLoggers;

        public void NewLine() {
            foreach( var logger in mLoggers )
                logger.NewLine();
        }

        public void Log( string format, params object[] args ) {
            foreach( var logger in mLoggers )
                logger.Log( format, args );
        }
    }

    public class ExceptionHelper {

        public static string GetErrorMessage( Exception ex ) {
            Exception e = GetInner( ex );

            if( e == null )
                return "An unknown exception ocurred";
            else
                return e.Message;
        }

        public static Exception GetInner( Exception ex ) {
            Exception e = ex;

            if( e == null )
                return null;

            while( e.InnerException != null )
                e = e.InnerException;

            return e;
        }
    }

    public class ApiUtils {
        public const string JSON = "application/json; charset=utf-8";
        public const string XML = "application/xml; charset=utf-8";

        public static HttpWebRequest Get( ApiToken token, string url ) {
            return CreateRequest( "GET", url, token );
        }

        public static HttpWebRequest Post( ApiToken token, string url ) {
            return CreateRequest( "POST", url, token );
        }

        public static HttpWebRequest Put( ApiToken token, string url ) {
            return CreateRequest( "PUT", url, token );
        }

        public static HttpWebRequest Delete( ApiToken token, string url ) {
            return CreateRequest( "DELETE", url, token );
        }

        private static HttpWebRequest CreateRequest( string method, string url, ApiToken token ) {
            HttpWebRequest request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = method;
            request.Headers.Add( "Authorization", token.ToString() );
            request.PreAuthenticate = true;

            return request;
        }

        public static ApiResponse<XmlDocument> ReadXml( HttpWebRequest request, IApiLogger logger ) {
            request.Accept = "application/xml";
            return ReadResponse<XmlDocument>( request, body => {
                var xml = new XmlDocument();
                xml.LoadXml( body );
                return xml;
            }, logger );
        }

        public static ApiResponse<T> ReadJson<T>( HttpWebRequest request, IApiLogger logger ) {
            request.Accept = "application/json";
            return ReadResponse<T>( request, body => JsonConvert.DeserializeObject<T>( body ), logger );
        }

        public static ApiResponse<T> ReadResponse<T>( HttpWebRequest request, Func<string, T> read, IApiLogger logger ) {
            var response = new ApiResponse<T>();
            try {
                logger.Log( request.Method + ":\t\t" + request.RequestUri );

                var httpResponse = ( HttpWebResponse )request.GetResponse();

                using( StreamReader sr = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    var raw = sr.ReadToEnd();
                    response.Data = read( raw );
                }

                httpResponse.Close();
                response.StatusCode = ( int )httpResponse.StatusCode;

                logger.Log( "Status:\t\t{0}", ( int )response.StatusCode + " " + httpResponse.StatusDescription );
            }
            catch( WebException ex ) {
                ReadErrorResponse<T>( response, ex );

                logger.Log( "Status:\t\t{0} {1}", response.StatusCode, response.StatusDescription );
                logger.Log( "Error:\t\t{0}", response.ErrorMessage );
            }
            catch( Exception ex ) {
                response.ErrorMessage = ex.Message;
                logger.Log( "ERROR:\t{0}", ex.Message );
            }
            return response;
        }

        private static void ReadErrorResponse<T>( ApiResponse<T> response, WebException webEx ) {

            try {
                response.ErrorMessage = ExceptionHelper.GetErrorMessage( webEx );
                response.StatusCode = 500;
                response.StatusDescription = "ERROR";

                var httpResponse = ( HttpWebResponse )webEx.Response;
                if( httpResponse != null ) {
                    response.StatusCode = ( int )httpResponse.StatusCode;
                    response.StatusDescription = httpResponse.StatusDescription;

                    using( StreamReader sr = new StreamReader( httpResponse.GetResponseStream() ) ) {
                        response.Content = sr.ReadToEnd();

                        if( httpResponse.ContentType == JSON ) {
                            response.Exception = JsonConvert.DeserializeObject<ApiException>( response.Content );

                            if( response.Exception != null )
                                response.ErrorMessage = ApiException.GetErrorMessage( response.Exception );
                        }
                    }
                }
            }
            catch( Exception ex ) {
                response.StatusCode = 500;
                response.ErrorMessage = ExceptionHelper.GetErrorMessage( ex );
            }
        }

        public static HttpWebRequest WriteXml( HttpWebRequest request, XmlDocument data ) {
            request.ContentType = XML;
            var body = request.GetRequestStream();
            using( var w = XmlWriter.Create( body /*,new XmlWriterSettings { Encoding = Encoding.UTF8 } */ ) ) {
                data.WriteTo( w );
            }
            return request;
        }

        public static HttpWebRequest WriteRaw( HttpWebRequest request, string data ) {
            if( data.Trim().StartsWith( "<" ) )
                request.ContentType = XML;
            else
                request.ContentType = JSON;

            var body = request.GetRequestStream();
            using( var w = new StreamWriter( body, Encoding.UTF8 ) ) {
                w.Write( data );
            }
            return request;
        }

        public static HttpWebRequest WriteJson( HttpWebRequest request, object data ) {
            request.ContentType = JSON;
            var body = request.GetRequestStream();
            using( var tw = new StreamWriter( body, Encoding.UTF8 ) ) {
                using( var w = new JsonTextWriter( tw ) ) {
                    var json = JsonConvert.SerializeObject( data );
                    w.WriteRaw( json );
                }
            }
            return request;
        }
    }
}
