namespace ApiClient.Model
{
    public class ModifyResponseModel : DnsCommandResponseModel
    {
        /// <summary>
        /// Describes the performed action
        /// </summary>
        public string Data { get; init; }
    }
}
