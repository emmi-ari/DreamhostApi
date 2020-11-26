namespace ApiClient.Model
{
    public abstract class DnsCommandResponseModel
    {
        public string Result { get; init; }

        public dynamic Data { get; init; }

        public string Reason { get; init; }
    }
}
