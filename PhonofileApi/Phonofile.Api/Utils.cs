using Newtonsoft.Json;
using Phonofile.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Phonofile.Api {

    public static class Utils {
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
            if ( method == null )
                throw new ArgumentNullException( "method" );
            if ( url == null )
                throw new ArgumentNullException( "url" );
            if ( token == null )
                throw new ArgumentNullException( "token" );

            HttpWebRequest request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = method;
            request.Headers.Add( "Authorization", token.ToString() );
            request.PreAuthenticate = true;

            return request;
        }

        public static ApiResponse<XmlDocument> ReadXml( HttpWebRequest request, IApiLogger logger ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );

            request.Accept = "application/xml";
            return ReadResponse<XmlDocument>( request, body => {
                var xml = new XmlDocument();
                xml.LoadXml( body );
                return xml;
            }, logger );
        }

        public static ApiResponse<T> ReadJson<T>( HttpWebRequest request, IApiLogger logger ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );

            request.Accept = "application/json";
            return ReadResponse<T>( request, body => JsonConvert.DeserializeObject<T>( body ), logger );
        }

        public static ApiResponse<T> ReadResponse<T>( HttpWebRequest request, Func<string, T> read, IApiLogger logger ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( read == null )
                throw new ArgumentNullException( "read" );
            if ( logger == null )
                throw new ArgumentNullException( "logger" );

            var response = new ApiResponse<T>();
            try {
                logger.Log( request.Method + ":\t\t" + request.RequestUri );

                var httpResponse = ( HttpWebResponse )request.GetResponse();

                using ( StreamReader sr = new StreamReader( httpResponse.GetResponseStream() ) ) {
                    var raw = sr.ReadToEnd();
                    response.Data = read( raw );
                }

                httpResponse.Close();
                response.StatusCode = ( int )httpResponse.StatusCode;

                logger.Log( "Status:\t\t{0}", ( int )response.StatusCode + " " + httpResponse.StatusDescription );
            } catch ( WebException ex ) {
                ReadErrorResponse<T>( response, ex );

                logger.Log( "Status:\t\t{0} {1}", response.StatusCode, response.StatusDescription );
                logger.Log( "Error:\t\t{0}", response.ErrorMessage );
            } catch ( Exception ex ) {
                response.ErrorMessage = ex.Message;
                logger.Log( "ERROR:\t{0}", ex.Message );
            }
            return response;
        }

        private static void ReadErrorResponse<T>( ApiResponse<T> response, WebException webEx ) {
            if ( response == null )
                throw new ArgumentNullException( "response" );
            if ( webEx == null )
                throw new ArgumentNullException( "webEx" );

            try {
                response.ErrorMessage = ExceptionHelper.GetErrorMessage( webEx );
                response.StatusCode = 500;
                response.StatusDescription = "ERROR";

                var httpResponse = ( HttpWebResponse )webEx.Response;
                if ( httpResponse != null ) {
                    response.StatusCode = ( int )httpResponse.StatusCode;
                    response.StatusDescription = httpResponse.StatusDescription;

                    using ( StreamReader sr = new StreamReader( httpResponse.GetResponseStream() ) ) {
                        response.Content = sr.ReadToEnd();

                        if ( httpResponse.ContentType == JSON ) {
                            response.Exception = JsonConvert.DeserializeObject<ApiException>( response.Content );

                            if ( response.Exception != null )
                                response.ErrorMessage = ApiException.GetErrorMessage( response.Exception );
                        }
                    }
                }
            } catch ( Exception ex ) {
                response.StatusCode = 500;
                response.ErrorMessage = ExceptionHelper.GetErrorMessage( ex );
            }
        }

        public static HttpWebRequest WriteXml( HttpWebRequest request, XmlDocument data ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( data == null )
                throw new ArgumentNullException( "data" );

            request.ContentType = XML;
            var body = request.GetRequestStream();
            using ( var w = XmlWriter.Create( body /*,new XmlWriterSettings { Encoding = Encoding.UTF8 } */ ) ) {
                data.WriteTo( w );
            }
            return request;
        }

        public static HttpWebRequest WriteRaw( HttpWebRequest request, string data ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( data == null )
                throw new ArgumentNullException( "data" );

            if ( data.Trim().StartsWith( "<" ) )
                request.ContentType = XML;
            else
                request.ContentType = JSON;

            var body = request.GetRequestStream();
            using ( var w = new StreamWriter( body, Encoding.UTF8 ) ) {
                w.Write( data );
            }
            return request;
        }

        public static HttpWebRequest WriteJson( HttpWebRequest request, object data ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( data == null )
                throw new ArgumentNullException( "data" );

            request.ContentType = JSON;
            var body = request.GetRequestStream();
            using ( var tw = new StreamWriter( body, Encoding.UTF8 ) ) {
                using ( var w = new JsonTextWriter( tw ) ) {
                    var json = JsonConvert.SerializeObject( data );
                    w.WriteRaw( json );
                }
            }
            return request;
        }

        public static HttpWebRequest WriteMultipart( HttpWebRequest request, IApiResource resource ) {
            var multipart = new Multipart( request );

            using ( var body = request.GetRequestStream() ) {
                multipart.WritePart( body, "id", resource.OwnerID.ToString() );
                //multipart.WritePart( body, "companyId", resource.CompanyID );
                multipart.WritePart( body, "type", resource.Type );

                multipart.WriteFilePart( body, "file", resource.FileName, resource.ContentType, resource.GetStream() );

                multipart.WriteTrailer( body );
            }
            return request;
        }

        public static HttpWebRequest WriteForm( HttpWebRequest request, object values ) {
            request.ContentType = "application/x-www-form-urlencoded";
            var formValues = new System.Web.Routing.RouteValueDictionary( values );

            string formData = "&";
            foreach ( var nameValue in formValues ) {
                string encodedValue = null;

                if ( nameValue.Value != null )
                    encodedValue = System.Web.HttpUtility.UrlEncode( nameValue.Value.ToString() );

                formData += ( nameValue.Key + "=" + encodedValue + "&" );
            }

            var body = request.GetRequestStream();
            using ( var w = new StreamWriter( body, Encoding.UTF8 ) ) {
                w.Write( formData.TrimEnd( '&' ) );
            }
            return request;
        }

        public static void Write<T>( HttpWebRequest request, T doc ) {
            if ( request == null )
                throw new ArgumentNullException( "request" );
            if ( doc == null )
                throw new ArgumentNullException( "doc" );

            if ( doc is XmlDocument )
                WriteXml( request, doc as XmlDocument );
            else if ( doc is string )
                WriteRaw( request, doc.ToString() );
            else
                WriteJson( request, doc );
        }
    }

    public class ExceptionHelper {

        public static string GetErrorMessage( Exception ex ) {
            Exception e = GetInner( ex );

            if ( e == null )
                return "An unknown exception ocurred";
            else
                return e.Message;
        }

        public static Exception GetInner( Exception ex ) {
            Exception e = ex;

            if ( e == null )
                return null;

            while ( e.InnerException != null )
                e = e.InnerException;

            return e;
        }
    }

    public class Multipart {
        private const string FormdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
        private const string HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

        protected byte[] BoundaryBytes { get; set; }
        protected byte[] BoundaryEndBytes { get; set; }

        public Multipart( HttpWebRequest request ) {
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString( "x" );
            BoundaryBytes = System.Text.Encoding.ASCII.GetBytes( "\r\n--" + boundary + "\r\n" );
            BoundaryEndBytes = System.Text.Encoding.ASCII.GetBytes( "\r\n--" + boundary + "--\r\n" );

            request.ContentType = "multipart/form-data; boundary=" + boundary;
        }

        public void WritePart( Stream rs, string name, string data ) {
            WriteBoundary( rs );

            // write data
            string formitem = string.Format( FormdataTemplate, name, data );
            byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes( formitem );
            rs.Write( formitembytes, 0, formitembytes.Length );
        }

        public void WriteFilePart( Stream rs, string paramName, string fileName, string contentType, Stream fileStream ) {
            WriteBoundary( rs );

            string header = string.Format( HeaderTemplate, paramName, fileName, contentType );
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes( header );
            rs.Write( headerbytes, 0, headerbytes.Length );

            byte[] buffer = new byte[ 4096 ];
            int bytesRead = 0;
            while ( ( bytesRead = fileStream.Read( buffer, 0, buffer.Length ) ) != 0 ) {
                rs.Write( buffer, 0, bytesRead );
            }
            fileStream.Close();
        }

        public void WriteBoundary( Stream rs ) {
            rs.Write( BoundaryBytes, 0, BoundaryBytes.Length );
        }

        public void WriteTrailer( Stream rs ) {
            rs.Write( BoundaryEndBytes, 0, BoundaryEndBytes.Length );
        }
    }
}
