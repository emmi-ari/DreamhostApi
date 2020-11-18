using ApiClient;
using ApiClient.Model;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

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
         */

        private string _clientIp;
        private string _dnsIp;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region _clientIp
            using WebClient client = new WebClient();
            Stream stream = await client.OpenReadTaskAsync(new Uri("http://icanhazip.com"));
            using StreamReader ipStream = new StreamReader(stream);
            _clientIp = ipStream.ReadToEnd().Trim();
            #endregion

            #region _dnsIp
            ApiProcessor api = new ApiProcessor(string.Empty);
            ListResponseModel responseModel = await api.ListRecords();
            responseModel.Data = responseModel.Data.Where(entry => entry.Editable == 1).ToList();
            _dnsIp = responseModel.Data[0].Value;
            #endregion
        }
    }
}
