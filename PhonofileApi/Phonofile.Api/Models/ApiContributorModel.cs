
namespace Phonofile.Api.Models {
    public class ApiContributorModel : IHasDescriptors {
        public long ID { get; set; }
        public long ArtistID { get; set; }
        public string Name { get; set; }
        public bool PublicDomain { get; set; }
        public string Type { get; set; }
        public long? DefaultPublisherID { get; set; }

        public ApiDescriptorModel[] Descriptors { get; set; }
    }

    public class ApiPublisherModel : ApiContributorModel {
    }
}
