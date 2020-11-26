namespace ApiClient.Model
{
    public class ErrorResponseModel : DnsCommandResponseModel
    {
        /// <summary>
        /// Reason name
        /// </summary>
        public new string Data { get; init; }
    }
}
