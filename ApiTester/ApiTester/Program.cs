using ApiTester;
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
    class Program {

        static void Main( string[] args ) {
            //var apiBaseUrl = "https://login.phonofile.com/api";
            var apiBaseUrl = "http://localhost/api";

            var logger = new MultiLogger(new ConsoleLogger(), new VsOutputLogger());

            var client = new ApiClient( apiBaseUrl, logger );
            client.Authenticate( "phonofile-api-test@phonofile.com", "phonofileapitest2014" );

            if ( client.IsAuthenticated ) {

                //Examples.CreateNewRelease( client );
                //Examples.CreateNewReleaseXml( client );
                //Examples.UpdateReleaseDraft( client, 439862 );    
                Examples.UpdateRelease( client, 433886 );
            }

            Console.WriteLine();
            Console.WriteLine( "Press any key to exit" );
            Console.ReadKey();
        }
    }
}
