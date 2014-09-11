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

    public class ApiActionResult {
        public long ID { get; set; }
    }

    public class ApiUpdateResult {
        public Change[] Changes { get; set; }

        public class Change {
            public String SubjectTitle { get; set; }
            public String SubjectId { get; set; }
            public String SubjectType { get; set; }

            public String FieldName { get; set; }
            public String OriginalValue { get; set; }
            public String NewValue { get; set; }

            public override string ToString() {
                return SubjectType + "." + FieldName + ": " + OriginalValue + " -> " + NewValue;
            }
        }
    }

    public class ApiContributor {
        public long ID { get; set; }
        public long ArtistID { get; set; }
        public String Name { get; set; }
        public bool PublicDomain { get; set; }
    }

    public class ApiPublisher : ApiContributor {
    }

    public class ApiContributorsResult {
        public ApiContributor[] Contributors { get; set; }
        public ApiPublisher[] Publishers { get; set; }
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
            LogHeader( "AUTHENTICATE" );
            var url = BaseUrl + "/token";
            var credentials = String.Format( "grant_type=password&username={0}&password={1}", email, password );

            var request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var body = request.GetRequestStream();
            using( var w = new StreamWriter( body ) ) {
                w.Write( credentials );
            }

            var response = ApiUtils.ReadJson<ApiToken>( request, Logger );
            if( response.Data != null )
                Token = response.Data;

            return response;
        }

        public ApiResponse<T> GetConstants<T>() {
            LogHeader( "GetConstants" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/constants" );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<ApiContributorsResult> GetContributors( long companyId ) {
            LogHeader( "GetContributors" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/contributorlookup/" + companyId );

            return ApiUtils.ReadJson<ApiContributorsResult>( request, Logger );
        }

        public ApiResponse<T> GetRelease<T>( long id ) {
            LogHeader( "GetRelease" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetReleaseXml( long id ) {
            LogHeader( "GetReleaseXml" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return ApiUtils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetReleaseJson<T>( long id ) {
            LogHeader( "GetReleaseJson" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetDraftXml( long id ) {
            LogHeader( "GetDraftXml" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return ApiUtils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetDraftJson<T>( long id ) {
            LogHeader( "GetDraftJson" );
            var request = ApiUtils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return ApiUtils.ReadJson<T>( request, Logger );
        }

        /// <summary>
        /// Creates a new release draft.
        /// </summary>
        /// <typeparam name="T">Draft data type</typeparam>
        /// <param name="doc">Draft creation data</param>
        public ApiResponse<ApiActionResult> CreateDraft<T>( T doc ) {
            LogHeader( "CreateDraft" );

            var request = ApiUtils.Put( EnsureToken(), BaseUrl + "/draft" );

            WriteRequestContent<T>( request, doc );

            return ApiUtils.ReadJson<ApiActionResult>( request, Logger );
        }

        private static void WriteRequestContent<T>( HttpWebRequest request, T doc ) {
            if( doc is XmlDocument )
                ApiUtils.WriteXml( request, doc as XmlDocument );
            else if( doc is string )
                ApiUtils.WriteRaw( request, doc.ToString() );
            else
                ApiUtils.WriteJson( request, doc );
        }

        /// <summary>
        /// Updates an existing release draft.
        /// </summary>
        /// <typeparam name="T">Draft data type</typeparam>
        /// <param name="doc">Draft update data</param>
        public ApiResponse<ApiActionResult> UpdateDraft<T>( T doc ) {
            LogHeader( "UpdateDraft" );

            var request = ApiUtils.Post( EnsureToken(), BaseUrl + "/draft" );

            WriteRequestContent<T>( request, doc );

            return ApiUtils.ReadJson<ApiActionResult>( request, Logger );
        }

        /// <summary>
        /// Updates an existing release.
        /// </summary>
        /// <typeparam name="T">Release data type</typeparam>
        /// <param name="doc">Release update data</param>
        public ApiResponse<ApiUpdateResult> UpdateRelease<T>( T doc ) {
            LogHeader( "UpdateRelease" );

            var request = ApiUtils.Post( EnsureToken(), BaseUrl + "/release" );

            WriteRequestContent<T>( request, doc );

            return ApiUtils.ReadJson<ApiUpdateResult>( request, Logger );
        }

        private ApiToken EnsureToken() {
            if( Token == null )
                throw new InvalidOperationException( "Failed to execute API method. Missing required token. Please authenticate first." );
            return Token;
        }

        private void LogHeader( String val ) {
            Logger.NewLine();
            Logger.Log( "--------------------------------------------------------------" );
            Logger.Log( val );
            Logger.Log( "--------------------------------------------------------------" );
        }
    }
}
