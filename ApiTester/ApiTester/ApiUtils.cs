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
    public class ApiResponse<T> : ApiResponse {
        public new T Data { get; set; }
    }

    public class ApiResponse {
        public dynamic Data { get; set; }
        public String ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }

        public int StatusCode { get; set; }
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

    public class ApiUtils {
        public static HttpWebRequest Get( ApiToken token, string url ) {
            HttpWebRequest request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = "GET";
            request.Headers.Add( "Authorization", token.ToString() );
            request.PreAuthenticate = true;
            return request;
        }

        public static HttpWebRequest Post( ApiToken token, string url ) {
            HttpWebRequest request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = "POST";
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
                response.ErrorMessage = ex.Message;

                var httpResponse = ( HttpWebResponse )ex.Response;
                response.StatusCode = ( int )httpResponse.StatusCode;

                using( StreamReader sr = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    var raw = sr.ReadToEnd();
                    response.ErrorDetails = raw;
                }

                logger.Log( "Status:\t\t{0} {1}", response.StatusCode, httpResponse.StatusDescription );
                logger.Log( "ErrorResponse:\t{0}", response.ErrorDetails );
            }
            catch( Exception ex ) {
                response.ErrorMessage = ex.Message;
                logger.Log( "ERROR:\t{0}", ex.Message );
            }
            return response;
        }

        public static HttpWebRequest WriteXml( HttpWebRequest request, XmlDocument data ) {
            request.ContentType = "application/xml";
            var body = request.GetRequestStream();
            using( var w = XmlWriter.Create( body ) ) {
                data.WriteTo( w );
            }
            return request;
        }

        public static HttpWebRequest WriteJson( HttpWebRequest request, object data ) {
            request.ContentType = "application/json";
            var body = request.GetRequestStream();
            using( var tw = new StreamWriter( body ) ) {
                using( var w = new JsonTextWriter( tw ) ) {
                    var json = JsonConvert.SerializeObject( data );
                    w.WriteRaw( json );
                }
            }
            return request;
        }
    }
}
