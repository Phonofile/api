using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ApiTester {
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

            if( response.Ok ) {
                var releaseId = response.Data.ID;

                client.Logger.Log( "New release created - ID: {0}", releaseId );
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

            if( response.Ok ) {
                var releaseId = response.Data.ID;

                client.Logger.Log( "New release created - ID: {0}", releaseId );
            }
        }

        /// <summary>
        /// Get all contributors.
        /// </summary>
        public static void GetContributors( ApiClient client ) {
            var response = client.GetContributors(1217);
            if( response.Ok ) {
                var contributors = response.Data;

                foreach( var contributor in response.Data.Contributors )
                    client.Logger.Log( "{0}: {1}", contributor.ID, contributor.Name );
            }
        }

        /// <summary>
        /// Updates an existing release draft.
        /// </summary>
        public static void UpdateReleaseDraft( ApiClient client, long id ) {
            var draftResponse = client.GetDraftXml( id );
            if( draftResponse.Ok ) {
                var draft = draftResponse.Data;

                var title = draft.DocumentElement.SelectSingleNode( "title" );
                title.InnerText = "New title";

                var updateResponse = client.UpdateDraft( draft );

                if( updateResponse.Ok ) {
                    client.Logger.Log( "Release draft {0} - updated", id );
                }
            }
        }

        /// <summary>
        /// Updates an existing release.
        /// </summary>
        public static void UpdateRelease( ApiClient client, long id ) {
            var getResponse = client.GetRelease<dynamic>( id );
            if( getResponse.Ok ) {
                var existingRelease = getResponse.Data;
                existingRelease.title = "New release title";

                var updateResponse = client.UpdateRelease( existingRelease );

                if( updateResponse.Ok ) {
                    client.Logger.NewLine();
                    client.Logger.Log( "Release {0} updated", id );
                    foreach( var change in updateResponse.Data.Changes )
                        client.Logger.Log( "{0}.{1}:\t\t\"{2}\"\t\t->\t\"{3}\"",
                            change.SubjectType, change.FieldName, change.OriginalValue, change.NewValue );
                }
            }
        }
    }
}
