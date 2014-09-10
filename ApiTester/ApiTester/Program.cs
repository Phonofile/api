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
            var client = new ApiClient( "https://login.phonofile.com/api", new ConsoleLogger() );

            client.Authenticate( "magnus-test-200@phonofile.com", "123456" );

            if( client.IsAuthenticated ) {

                var constsResponse = client.GetConstants<dynamic>();

                var draftResponse = client.GetDraftJson<dynamic>( 439236 );

                draftResponse.Data.title = "TESTER API UPDATE 2";
                draftResponse.Data.titleVersion = "Tim Cook";

                if( draftResponse.Data != null )
                    client.UpdateDraft( draftResponse.Data );
            }

            Console.WriteLine();
            Console.WriteLine( "Press any key to exit" );
            Console.ReadKey();
        }
    }
}
