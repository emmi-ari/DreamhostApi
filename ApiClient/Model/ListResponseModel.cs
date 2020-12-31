using System.Collections.Generic;

namespace ApiClient.Model
{
    public class ListResponseModel : DnsCommandResponseModel
    {
        /// <summary>
        /// An array of DNS entries
        /// </summary>
        public List<ResponseDataModel> Data { get; set; }

        public class ResponseDataModel
        {
            /// <summary>
            /// Indicates if the record is modifiable or not
            /// </summary>
            public int Editable { get; set; }

            /// <summary>
            /// The name of the record
            /// </summary>
            public string Record { get; set; }

            /// <summary>
            /// The <see cref="RecordType"/> of the DNS entry
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// The value of the record
            /// </summary>
            public string Value { get; set; }
        }
    }
}