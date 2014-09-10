using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ApiTester {
    public class ApiToken {
        [JsonProperty( "access_token" )]
        public String AccessToken { get; set; }
        [JsonProperty( "token_type" )]
        public String TokenType { get; set; }
        [JsonProperty( "expires_in" )]
        public int ExpiresIn { get; set; }
        [JsonProperty( "userName" )]
        public String UserName { get; set; }
        [JsonProperty( ".issued" )]
        public DateTime Issued { get; set; }
        [JsonProperty( ".expires" )]
        public DateTime Expires { get; set; }

        public override string ToString() {
            return TokenType + " " + AccessToken;
        }
    }

    public class ApiClient {
        public ApiClient( String baseUrl, IApiLogger logger = null ) {
            BaseUrl = baseUrl;
            Logger = logger ?? new VsOutputLogger();
        }

        public ApiToken Token { get; set; }
        public String BaseUrl { get; set; }
        public bool IsAuthenticated { get { return Token != null; } }
        public IApiLogger Logger { get; set; }

        public ApiResponse<ApiToken> Authenticate( String email, String password ) {
            Log( "AUTHENTICATE" );
            var url = BaseUrl + "/token";
            var credentials = String.Format( "grant_type=password&username={0}&password={1}", email, password );

            var request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var body = request.GetRequestStream();
            using ( var w = new StreamWriter( body ) ) {
                w.Write( credentials );
            }

            var response = ApiUtils.ReadJson<ApiToken>( request, Logger );
            if ( response.Data != null )
                Token = response.Data;

            return response;
        }

        public ApiResponse<T> GetConstants<T>() {
            Log( "GetConstants" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/constants" );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetReleaseXml( long id ) {
            Log( "GetReleaseXml" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return ApiUtils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetReleaseJson<T>( long id ) {
            Log( "GetReleaseJson" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetDraftXml( long id ) {
            Log( "GetDraftXml" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return ApiUtils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetDraftJson<T>( long id ) {
            Log( "GetDraftJson" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        /// <summary>
        /// Updates an existing release draft.
        /// </summary>
        /// <typeparam name="T">Draft data type</typeparam>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="doc">Draft update data</param>
        /// <returns>ApiResponse</returns>
        public ApiResponse<TResult> UpdateDraft<T, TResult>( T doc ) {
            Log( "UpdateDraft" );

            var request = ApiUtils.Post( EnsureToken(), BaseUrl + "/draft" );

            if ( doc is XmlDocument )
                ApiUtils.WriteXml( request, doc as XmlDocument );
            else
                ApiUtils.WriteJson( request, doc );

            return ApiUtils.ReadJson<TResult>( request, Logger );
        }

        private ApiToken EnsureToken() {
            if ( Token == null )
                throw new InvalidOperationException( "Failed to execute API method. Missing required token. Please authenticate first." );
            return Token;
        }

        private void Log( String val, bool header = true ) {

            if ( header ) {
                Logger.NewLine();
                Logger.Log( "--------------------------------------------------------------" );
            }
            Logger.Log( val );
            if ( header )
                Logger.Log( "--------------------------------------------------------------" );
        }
    }
}
