
namespace Phonofile.Api.Models {
    public class ApiEntityModel {
        public long ID { get; set; }
    }

    public class ApiCompanyModel : ApiEntityModel {
        public string Name { get; set; }
    }

    public class ApiLabelModel : ApiEntityModel {
        public string Name { get; set; }
        public long CompanyID { get; set; }
    }

    public class ApiAccountModel : ApiEntityModel {

        public ApiCompanyModel[] Companies { get; set; }
        public ApiLabelModel[] Labels { get; set; }
    }
}
