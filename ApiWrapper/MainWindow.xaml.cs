using ApiClient;
using ApiClient.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

using static ApiClient.Model.ListResponseModel;

namespace ApiWrapper
{
    public partial class MainWindow : Window
    {
        /* TODOs
         * !1. Get IP address of client
         * !2. Get IP address of DNS record
         * 3. Visual representation if button needs to be clicked or not
         * 4. Button which deletes old DNS entry and adds new DNS entry
         * 5. Option for manually selecting which is the old DNS entry
         * 6. Deactivate button if no need to update
         * 7. Functionallity to press the button even though it's deactivated
         * 8. Implement a normed way to store and access secrets
         */

        private IPAddress _clientIp;
        private IPAddress _dnsIp;
        private bool addressesMatch;

        #region User secrets
        private readonly static string[] _secrets = File.ReadAllLines(@"C:\Users\Emmi\Source\Repos\DreamHostApi\.secrets");
        private readonly static string _apiKey = _secrets[0];
        private readonly static string _domain = _secrets[1];
        private readonly static string _entryType = _secrets[2];
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _clientIp = await GetClientIpAsync();

            try
            {
                _dnsIp = await GetDnsIpAsync();
            }
            catch (Exception ex) when (ex.Message == "stub")
            {
                _ = MessageBox.Show(this, $"The specified DNS entry could not be found.", "Entry not found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            addressesMatch = _clientIp == _dnsIp;
        }

        private static async Task<IPAddress> GetClientIpAsync()
        {
            using WebClient client = new();
            Stream stream = await client.OpenReadTaskAsync(new Uri("http://ipinfo.io/ip"));
            using StreamReader ipStream = new(stream);
            return IPAddress.Parse(ipStream.ReadToEnd().Trim());
        }

        private static async Task<IPAddress> GetDnsIpAsync()
        {
            DnsApiProcessor api = new(_apiKey);
            ListResponseModel responseModel = await api.ListRecords(); // TODO implement a way to check for command errors
            List<ResponseDataModel> editableEntries = responseModel.Data.Where(entry => entry.Editable == 1 && entry.Record == _domain && entry.Type == _entryType).ToList();

            if (editableEntries.Count == 1)
                return IPAddress.Parse(editableEntries[0].Value);
            else
                throw new Exception("stub"); // TODO throw more specific exception
        }
    }
}
