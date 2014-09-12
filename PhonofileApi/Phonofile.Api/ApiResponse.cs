
namespace Phonofile.Api {
    public class ApiResponse<T> {
        public T Data { get; set; }

        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public string ErrorMessage { get; set; }
        public ApiException Exception { get; set; }

        public string Content { get; set; }

        public bool Ok { get { return StatusCode == 200; } }
    }
}
