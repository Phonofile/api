using Phonofile.Api.Models;
using Phonofile.Api.Results;
using System;
using System.IO;
using System.Net;
using System.Xml;

namespace Phonofile.Api {

    public class ApiClient {
        public ApiClient( string baseUrl, IApiLogger logger ) {
            if ( baseUrl == null )
                throw new ArgumentNullException( "baseUrl" );
            if ( logger == null )
                throw new ArgumentNullException( "logger" );

            BaseUrl = baseUrl;
            Logger = logger;
        }

        public ApiToken Token { get; set; }
        public string BaseUrl { get; set; }
        public bool IsAuthenticated { get { return Token != null; } }
        public IApiLogger Logger { get; set; }

        public ApiResponse<ApiToken> Authenticate( string email, string password ) {
            Logger.Header( "Authenticate" );
            if ( email == null )
                throw new ArgumentNullException( "email" );
            if ( password == null )
                throw new ArgumentNullException( "password" );

            var url = BaseUrl + "/token";
            var credentials = string.Format( "grant_type=password&username={0}&password={1}", email, password );

            var request = ( HttpWebRequest )HttpWebRequest.Create( url );
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var body = request.GetRequestStream();
            using ( var w = new StreamWriter( body ) ) {
                w.Write( credentials );
            }

            var response = Utils.ReadJson<ApiToken>( request, Logger );
            if ( response.Data != null )
                Token = response.Data;

            return response;
        }

        public ApiResponse<ApiConstantsResult> GetConstants() {
            Logger.Header( "GetConstants" );
            var request = Utils.Get( EnsureToken(), BaseUrl + "/constants" );

            return Utils.ReadJson<ApiConstantsResult>( request, Logger );
        }

        public ApiResponse<ApiContributorsResult> GetContributors( long companyId = 0 ) {
            Logger.Header( "GetContributors" );
            if ( companyId == 0 )
                companyId = EnsureToken().PrimaryCompanyID;
            if ( companyId == 0 )
                throw new ArgumentException( "companyId", "Please provide a valid company id." );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/contributorlookup/" + companyId );

            return Utils.ReadJson<ApiContributorsResult>( request, Logger );
        }

        public ApiResponse<ApiContributorModel> GetOrCreateContributor( String name, long companyId = 0 ) {
            Logger.Header( "GetOrCreateContributor" );
            if ( String.IsNullOrWhiteSpace( name ) )
                throw new ArgumentNullException( "name" );

            if ( companyId == 0 )
                companyId = EnsureToken().PrimaryCompanyID;

            if ( companyId == 0 )
                throw new ArgumentException( "companyId", "Please provide a valid company id." );

            var request = Utils.Put( EnsureToken(), BaseUrl + "/contributor" );

            Utils.WriteForm( request, new { name = name, companyId = companyId } );

            return Utils.ReadJson<ApiContributorModel>( request, Logger );
        }

        public ApiResponse<ApiAccountModel> GetAccount() {
            Logger.Header( "GetAccount" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/me" );

            return Utils.ReadJson<ApiAccountModel>( request, Logger );
        }

        /// <summary>
        /// Gets all external links for a given release
        /// </summary>
        public ApiResponse<ApiServiceLinkModel[]> GetReleaseLinks( long id ) {
            Logger.Header( "GetReleaseServiceLinks" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/release/" + id + "/links" );

            return Utils.ReadJson<ApiServiceLinkModel[]>( request, Logger );
        }

        public ApiResponse<T> GetRelease<T>( long id ) {
            Logger.Header( "GetRelease" );
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id", id, "Invalid release id" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return Utils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetReleaseXml( long id ) {
            Logger.Header( "GetReleaseXml" );
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id", id, "Invalid release id" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return Utils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetReleaseJson<T>( long id ) {
            Logger.Header( "GetReleaseJson" );
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id", id, "Invalid release id" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/release/" + id );

            return Utils.ReadJson<T>( request, Logger );
        }

        public ApiResponse<XmlDocument> GetDraftXml( long id ) {
            Logger.Header( "GetDraftXml" );
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id", id, "Invalid release id" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return Utils.ReadXml( request, Logger );
        }

        public ApiResponse<T> GetDraftJson<T>( long id ) {
            Logger.Header( "GetDraftJson" );
            if ( id <= 0 )
                throw new ArgumentOutOfRangeException( "id", id, "Invalid release id" );

            var request = Utils.Get( EnsureToken(), BaseUrl + "/draft/" + id );

            return Utils.ReadJson<T>( request, Logger );
        }

        /// <summary>
        /// Creates a new release draft.
        /// </summary>
        /// <typeparam name="T">Draft data type</typeparam>
        /// <param name="doc">Draft creation data</param>
        public ApiResponse<ApiCreateResult> CreateDraft<T>( T doc ) {
            Logger.Header( "CreateDraft" );
            if ( doc == null )
                throw new ArgumentNullException( "doc" );

            var request = Utils.Put( EnsureToken(), BaseUrl + "/draft" );

            Utils.Write<T>( request, doc );

            return Utils.ReadJson<ApiCreateResult>( request, Logger );
        }

        /// <summary>
        /// Updates an existing contributor.
        /// </summary>
        /// <typeparam name="T">Contributor data type</typeparam>
        /// <param name="doc">Contributor update data</param>
        public ApiResponse<ApiUpdateResult> UpdateContributor<T>( T doc ) {
            Logger.Header( "UpdateContributor" );
            if ( doc == null )
                throw new ArgumentNullException( "doc" );

            var request = Utils.Post( EnsureToken(), BaseUrl + "/contributor" );

            Utils.Write<T>( request, doc );

            return Utils.ReadJson<ApiUpdateResult>( request, Logger );
        }

        /// <summary>
        /// Uploads a new resource ( eg. image or booklet ).
        /// </summary>               
        public ApiResponse<ApiUploadResult[]> UploadResource( IApiResource file ) {
            Logger.Header( "UploadResource" );
            if ( file == null )
                throw new ArgumentNullException( "file" );

            var request = Utils.Put( EnsureToken(), BaseUrl + "/resource" );

            Utils.WriteMultipart( request, file );

            return Utils.ReadJson<ApiUploadResult[]>( request, Logger );
        }

        /// <summary>
        /// Updates an existing release draft.
        /// </summary>
        /// <typeparam name="T">Draft data type</typeparam>
        /// <param name="doc">Draft update data</param>
        public ApiResponse<ApiUpdateResult> UpdateDraft<T>( T doc ) {
            Logger.Header( "UpdateDraft" );
            if ( doc == null )
                throw new ArgumentNullException( "doc" );

            var request = Utils.Post( EnsureToken(), BaseUrl + "/draft" );

            Utils.Write<T>( request, doc );

            return Utils.ReadJson<ApiUpdateResult>( request, Logger );
        }

        /// <summary>
        /// Updates an existing release.
        /// </summary>
        /// <typeparam name="T">Release data type</typeparam>
        /// <param name="doc">Release update data</param>
        public ApiResponse<ApiUpdateResult> UpdateRelease<T>( T doc ) {
            Logger.Header( "UpdateRelease" );
            if ( doc == null )
                throw new ArgumentNullException( "doc" );


            var request = Utils.Post( EnsureToken(), BaseUrl + "/release" );

            Utils.Write<T>( request, doc );

            return Utils.ReadJson<ApiUpdateResult>( request, Logger );
        }

        private ApiToken EnsureToken() {
            if ( Token == null )
                throw new InvalidOperationException( "Failed to execute API method. Missing required token. Please authenticate first." );
            return Token;
        }
    }
}
