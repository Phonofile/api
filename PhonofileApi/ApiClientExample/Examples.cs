using Phonofile.Api;
using Phonofile.Api.Models;
using Phonofile.Api.Results;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ApiClientExample {
    public static class Examples {

        /// <summary>
        /// Creates a new release using regular a regular object. 
        /// The object is then serialized to JSON and sent to server.
        /// The result is a new release id
        /// </summary>
        public static void CreateNewRelease( ApiClient client ) {

            var response = client.CreateDraft( new {
                title = "Test release",
                displayArtist = "Tim Cook",
                labelID = 6091
            } );

            if ( response.Ok ) {
                var releaseId = response.Data.ID;

                client.Logger.Log( "New release created ID: {0}", releaseId );
            }
        }

        /// <summary>
        /// Creates a new release using XML. 
        /// The result is a new release id
        /// </summary>
        public static void CreateNewReleaseXml( ApiClient client ) {
            // This could easily be a XmlDocument, but we are using plain string for clearity.
            // NB! The order of elements matter! They need to be in a alphabetical order.
            // If not elements may be skipped!
            var xml = @"
                <release xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>
                    <genreID>1</genreID>
                    <labelName>Test Label 4</labelName>
                    <title>Test test</title>
                    <tracks>
                        <track>
                            <title>Test track 1</title>
                        </track>
                    </tracks>
                    <upc>123456789</upc>
                </release>";

            var response = client.CreateDraft( xml );

            if ( response.Ok ) {
                var releaseId = response.Data.ID;

                client.Logger.Log( "New release created ID: {0}", releaseId );
            }
        }

        /// <summary>
        /// Creates a new release using XML. 
        /// The result is a new release id
        /// </summary>
        public static void CreateNewReleaseFromFile( ApiClient client ) {
            var doc = new XmlDocument();
            doc.Load( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "new-release.xml" ) );

            var response = client.CreateDraft( doc );

            if ( response.Ok ) {
                var releaseId = response.Data.ID;

                client.Logger.Log( "New release created ID: {0}", releaseId );
            }
        }


        /// <summary>
        /// Get all genres.
        /// </summary>
        public static void GetGenres( ApiClient client ) {
            var response = client.GetConstants();
            if ( response.Ok ) {
                var genres = response.Data.Genres;

                // Populates the genre models with a genre path
                ApiConstantsResult.BuildGenrePaths( response.Data );

                // Lets sort the genres by path ( looks prettier )
                var ordered = genres.OrderBy( g => g.Path );

                client.Logger.NewLine();
                client.Logger.Log( "GENRES" );
                foreach ( var genre in ordered )
                    client.Logger.Log( "{0}\t {1}", genre.ID, genre.Path );
            }
        }

        /// <summary>
        /// Get all external links for a given release.
        /// </summary>
        public static void GetReleaseLinks( ApiClient client ) {
            var response = client.GetReleaseLinks( 433886 );
            if ( response.Ok ) {
                var links = response.Data;

                client.Logger.NewLine();
                client.Logger.Log( "LINKS" );
                client.Logger.Divider();
                client.Logger.Log( "ItemID\t| Type\t\t| Service\t| Link" );
                client.Logger.Divider();
                foreach ( var link in links ) {
                    var type = link.Type == "1" ? "Release" : "Track";

                    client.Logger.Log( "{0}\t| {1}\t\t| {2}\t| {3}", link.ItemID, type, link.ServiceName, link.WebUri );
                }
            }
        }

        /// <summary>
        /// Get all contributors.
        /// Great for syncing with your own system
        /// </summary>
        public static void GetContributors( ApiClient client ) {
            var response = client.GetContributors();
            if ( response.Ok ) {
                var contributors = response.Data;

                client.Logger.NewLine();
                client.Logger.Log( "CONTRIBUTORS" );
                foreach ( var contributor in response.Data.Contributors )
                    client.Logger.Log( "{0}: {1}", contributor.ID, contributor.Name );

                client.Logger.NewLine();
                client.Logger.Log( "PUBLISHERS" );
                foreach ( var contributor in response.Data.Publishers )
                    client.Logger.Log( "{0}: {1}", contributor.ID, contributor.Name );
            }
        }

        /// <summary>
        /// Get or create a new contributor.
        /// By using the GetOrCreateContributor you will always get the existing contributor if it already is created.
        /// If there is no contributor with the given name, you will get a new contributor reference.
        /// </summary>
        public static void GetOrCreateContributor( ApiClient client ) {
            // Calling this method twice with the same name will return the same contributor
            var response1 = client.GetOrCreateContributor( "New artist" );
            var response2 = client.GetOrCreateContributor( "New artist" );

            if ( response1.Ok && response2.Ok ) {
                var contributor1 = response1.Data;
                var contributor2 = response2.Data;

                // contributor1.id == contributor2.id -> true                

                client.Logger.NewLine();
                client.Logger.Log( "{0}: {1}", contributor1.ID, contributor1.Name );
                client.Logger.Log( "{0}: {1}", contributor2.ID, contributor2.Name );
            }
        }

        /// <summary>
        /// Update an existing contributor
        /// </summary>
        public static void UpdateContributor( ApiClient client ) {
            var response = client.GetOrCreateContributor( "New artist" );

            if ( response.Ok ) {
                var contributor = response.Data;

                contributor.SetDescriptor( "en", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." );
                contributor.SetDescriptor( "nb", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." );

                var updateResponse = client.UpdateContributor( contributor );

                if ( updateResponse.Ok ) {
                    client.Logger.Log( "Contributor {0} updated", contributor.ID );
                }
            }
        }

        /// <summary>
        /// Upload a contributor image
        /// </summary>
        public static void UploadContributorImage( ApiClient client ) {
            var response = client.GetOrCreateContributor( "New artist" );
            if ( response.Ok ) {
                var contributor = response.Data;

                var image = ApiResource.ContributorImage( contributor.ID, @"c:\test\artist.jpg" );

                var uploadResponse = client.UploadResource( image );

                if ( uploadResponse.Ok ) {

                    client.Logger.Log( "Uploaded image successfully for contributor: {0}", contributor.ID );
                }
            }
        }

        /// <summary>
        /// Upload a draft cover
        /// </summary>
        public static void UploadDraftCoverImage( ApiClient client ) {
            var image = ApiResource.CoverImage( 439862, @"c:\test\cover.jpg" );

            var uploadResponse = client.UploadResource( image );

            if ( uploadResponse.Ok ) {

                client.Logger.Log( "Uploaded image successfully for draft: {0}", 439862 );
            }

        }

        /// <summary>
        /// Get account information.
        /// </summary>
        public static void GetAccount( ApiClient client ) {
            var response = client.GetAccount();
            if ( response.Ok ) {
                var account = response.Data;

                client.Logger.NewLine();
                client.Logger.Log( "COMPANIES" );
                client.Logger.Divider();
                client.Logger.Log( "ID\t| Name" );
                client.Logger.Divider();
                foreach ( var company in account.Companies )
                    client.Logger.Log( "{0}\t| {1}", company.ID, company.Name );

                client.Logger.NewLine();
                client.Logger.Log( "LABELS" );
                client.Logger.Divider();
                client.Logger.Log( "CompID\t| ID\t| Name" );
                client.Logger.Divider();
                foreach ( var label in account.Labels )
                    client.Logger.Log( "{0}\t| {1}\t| {2}", label.CompanyID, label.ID, label.Name );
            }
        }

        /// <summary>
        /// Updates an existing release draft.
        /// </summary>
        public static void UpdateReleaseDraft( ApiClient client, long id ) {
            var draftResponse = client.GetDraftXml( id );
            if ( draftResponse.Ok ) {
                var draft = draftResponse.Data;

                var title = draft.DocumentElement.SelectSingleNode( "title" );
                title.InnerText = "New title";

                var updateResponse = client.UpdateDraft( draft );

                if ( updateResponse.Ok ) {
                    client.Logger.Log( "Release draft {0} - updated", id );
                }
            }
        }

        /// <summary>
        /// Updates an existing release.
        /// </summary>
        public static void UpdateRelease( ApiClient client, long id ) {
            var getResponse = client.GetRelease<dynamic>( id );
            if ( getResponse.Ok ) {
                var existingRelease = getResponse.Data;
                existingRelease.title = "New release title";

                var updateResponse = client.UpdateRelease( existingRelease );

                if ( updateResponse.Ok ) {
                    client.Logger.NewLine();
                    client.Logger.Log( "Release {0} updated", id );
                    foreach ( var change in updateResponse.Data.Changes )
                        client.Logger.Log( "{0}.{1}:\t\t\"{2}\"\t\t->\t\"{3}\"",
                            change.SubjectType, change.FieldName, change.OriginalValue, change.NewValue );
                }
            }
        }
    }
}
