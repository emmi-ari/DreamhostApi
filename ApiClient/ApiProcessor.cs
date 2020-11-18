using ApiClient.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiClient
{
    /// <summary>
    /// All available DNS commands
    /// </summary>
    public enum DnsCommand
    {
        ListRecord,
        AddRecord,
        RemoveRecord
    }

    /// <summary>
    /// Editable record types in DreamHost
    /// </summary>
    public enum RecordType
    {
        A,
        AAAA,
        CNAME,
        NAPTR,
        NS,
        PTR,
        SRV,
        TXT
    }

    /// <summary>
    /// Lists or modifies DreamHost DNS records using DreamHost's API
    /// </summary>
    /// <exception cref="HttpRequestException">Thrown when the HTTP status code is not a success code</exception>
    public class ApiProcessor
    {
        private readonly string _apiKey;

        private readonly Uri _baseUri = new Uri("https://api.dreamhost.com");
        public ApiProcessor(string apiKey)
        {
            _apiKey = apiKey;
        }

        /// <summary>
        /// Adds a DNS record
        /// </summary>
        /// <param name="recordName">The name of the record</param>
        /// <param name="recordType">The record type (e.g. A, AAAA, MX, TXT)</param>
        /// <param name="value">The value of the DNS record to add</param>
        /// <returns>A response model with the executed command and a success status</returns>
        /// <example>
        /// <code>
        /// {
        ///     ApiProcessor api = new ApiProcessor("6SHU5P2HLDAYECUM");
        ///     api.AddRecord(recordName: "Mail subdomain", recordType: RecordType.A, value: "mail.example.com");
        /// }
        /// </code>
        /// </example>
        public async Task<ModifyResponseModel> AddRecord(string recordName, RecordType recordType, string value) =>
            await ExecuteCommand(DnsCommand.AddRecord, recordName, recordType.ToString(), value);

        /// <summary>
        /// Removes a DNS record
        /// </summary>
        /// <param name="recordName">The name of the record which should be removed</param>
        /// <param name="recordType">The type of the record which should be removed</param>
        /// <param name="value">The value of the record which should be removed</param>
        /// <returns>A response model with the executed command and a success status</returns>
        /// <example>
        /// <code>
        /// {
        ///     ApiProcessor api = new ApiProcessor("6SHU5P2HLDAYECUM");
        ///     api.RemoveRecord(recordName: "Docs section", recordType: RecordType.AAAA, value: "doc.example.com");
        /// }
        /// </code>
        /// </example>
        public async Task<ModifyResponseModel> RemoveRecord(string recordName, RecordType recordType, string value) =>
            await ExecuteCommand(DnsCommand.RemoveRecord, recordName, recordType.ToString(), value);

        /// <summary>
        /// Lists the records
        /// </summary>
        /// <returns>A response model with all necessary informations that gets returned from the API</returns>
        public async Task<ListResponseModel> ListRecords() =>
            await ExecuteCommand(DnsCommand.ListRecord);

        private async Task<dynamic> ExecuteCommand(DnsCommand command, params string[] parameters)
        {
            Client client = new Client();
            Uri uri = GetUri(command, parameters);
            using HttpResponseMessage responseMessage = await client.ApiClient.GetAsync(uri);

            if (responseMessage.IsSuccessStatusCode)
            {
                string response = await responseMessage.Content.ReadAsStringAsync();
                switch (command)
                {
                    case DnsCommand.ListRecord:
                        return JsonConvert.DeserializeObject<ListResponseModel>(response);

                    case DnsCommand.AddRecord:
                    case DnsCommand.RemoveRecord:
                        return JsonConvert.DeserializeObject<ModifyResponseModel>(response);

                    default:
                        return response;
                }
            }
            else
            {
                throw new HttpRequestException(responseMessage.ReasonPhrase);
            }
        }

        private Uri GetUri(DnsCommand command, params string[] parameters)
        {
            if (command != DnsCommand.ListRecord && parameters.Length != 3)
                throw new ArgumentException(nameof(parameters), $"{nameof(parameters)} needs the record name, record type and the record value, when {command} is set to 'RemoveRecord' or 'AddRecord'.");

            Guid guid = Guid.NewGuid();

            string record = string.Empty;
            string type = string.Empty;
            string value = string.Empty;
            string comment = string.Empty;

            switch (command)
            {
                case DnsCommand.AddRecord:
                case DnsCommand.RemoveRecord:
                    record = parameters[0];
                    type = parameters[1];
                    value = parameters[2];
                    comment = $"[{DateTime.UnixEpoch.Millisecond}] {command} {guid}";
                    break;
            }
            

            string relativeUrl = command switch
            {
                DnsCommand.ListRecord => $"?key={_apiKey}&cmd=dns-list_records&unique_id={guid}&format=json",
                DnsCommand.AddRecord => $"?key={_apiKey}&cmd=dns-add_record&record={record}&type={type}&value={value}&comment={comment}&unique_id={guid}&format=json",
                DnsCommand.RemoveRecord => $"?key={_apiKey}&cmd=dns-remove_record&record={record}&type={type}&value={value}&unique_id={guid}&format=json",
                _ => throw new ArgumentException("Unable to handle parameter", nameof(command)),
            };

            return new Uri(_baseUri, relativeUrl);
        }
    }
}