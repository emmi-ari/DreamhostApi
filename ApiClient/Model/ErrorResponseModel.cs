namespace ApiClient.Model
{
    public class ErrorResponseModel : DnsCommandResponseModel
    {
        /// <summary>
        /// Reason name
        /// </summary>
        public string Data { get; init; }
    }
}
