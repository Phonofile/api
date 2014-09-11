using Phonofile.Api;
using Phonofile.Api.Loggers;
using System;

namespace ApiClientExample {
    class Program {

        static void Main( string[] args ) {
            //var apiBaseUrl = "https://login.phonofile.com/api";
            var apiBaseUrl = "http://localhost/api";
            //var apiBaseUrl = "https://staging.phonofile.com/api";

            var logger = new MultiLogger( new ConsoleLogger(), new VsOutputLogger() );

            var client = new ApiClient( apiBaseUrl, logger );
            client.Authenticate( "phonofile-api-test@phonofile.com", "phonofileapitest2014" );

            if ( client.IsAuthenticated ) {

                //Examples.GetAccount( client );
                //Examples.GetGenres( client );
                //Examples.CreateNewRelease( client );
                //Examples.CreateNewReleaseXml( client );
                Examples.CreateNewReleaseFromFile( client );
                //Examples.UpdateReleaseDraft( client, 439862 );    
                //Examples.UpdateRelease( client, 433886 );
                //Examples.UpdateContributor( client );
                //Examples.GetContributors( client );                
                //Examples.GetOrCreateContributor( client );
                //Examples.GetReleaseLinks( client );
                //Examples.UploadContributorImage( client );
            }

            Console.WriteLine();
            Console.WriteLine( "Press any key to exit" );
            Console.ReadKey();
        }
    }
}
