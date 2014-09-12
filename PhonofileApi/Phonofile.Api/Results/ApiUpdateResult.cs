
namespace Phonofile.Api.Results {
    public class ApiUpdateResult {
        public Change[] Changes { get; set; }

        public class Change {
            public string SubjectTitle { get; set; }
            public string SubjectId { get; set; }
            public string SubjectType { get; set; }

            public string FieldName { get; set; }
            public string OriginalValue { get; set; }
            public string NewValue { get; set; }

            public override string ToString() {
                return SubjectType + "." + FieldName + ": " + OriginalValue + " -> " + NewValue;
            }
        }
    }
}
