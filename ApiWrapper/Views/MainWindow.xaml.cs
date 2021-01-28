using ApiClient;
using ApiClient.Model;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

#pragma warning disable CA1822
namespace ApiWrapper
{
    public partial class MainWindow : Window
    {
        /* TODOs
         *  !1. Get IP address of client
         *  !2. Get IP address of DNS record
         *  !3. Visual representation if button needs to be clicked or not
         *  !4. Button which deletes old DNS entry and adds new DNS entry
         *   5. Option for manually selecting which is the old DNS entry
         *  !6. Deactivate button if no need to update
         *   7. Functionallity to press the button even though it's deactivated
         *   8. Implement a normed way to store and access secrets (check for a existing secret file => first run => prompt for secrets)
         *   9. Add menu for user secrets
         *  10. Add localization 
         *  11. Actually validate the settings
         */

        private IPAddress _clientIp;
        private IPAddress _dnsIp;

        private bool _addressesMatch;

        private readonly ListResponseModel _editableEntries = new ListResponseModel();

        private bool _shiftDown = false;
        private bool _initialButtonState;

        #region User secrets
        private readonly string _apiKey;
        private readonly string _domain;
        private readonly string _recordType;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            string[] secrets = File.ReadAllLines(@"C:\Users\Emmi\Source\Repos\DreamHostApi\.secrets"); // legacy
            _apiKey = secrets[0];                                                                           // legacy
            _domain = secrets[1];                                                                           // legacy
            _recordType = secrets[2];                                                                       // legacy
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SettingsValidator())
                LaunchSettingsWindow();

            await UpdateStatus();
        }

        private static bool SettingsValidator()
        {
            AppSettings settings = new();
            bool appUnconfigured = settings.apiKey == string.Empty
                                || settings.domain == string.Empty
                                || settings.recordType == string.Empty;

            return appUnconfigured;
        }

        private void LaunchSettingsWindow()
        {
            ConfigurationWindow window = new(this);
            window.ShowDialog();
            if (!SettingsValidator())
            {
                // TODO show message 
            }
        }

        private async Task UpdateStatus()
        {
            _clientIp = await GetClientIpAsync();
            //_clientIp = IPAddress.Parse("255.255.255.255");

            try
            {
                _dnsIp = await GetDnsIpAsync();
            }
            catch (Exception ex) when (ex.Message == "stub")
            {
                _ = MessageBox.Show(this, $"The specified DNS entry could not be found.", "Entry not found", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            _addressesMatch = _clientIp.Equals(_dnsIp);

            ownIpLabel.Content = _clientIp.ToString();
            dnsIpLabel.Content = _dnsIp.ToString();

            if (_addressesMatch)
            {
                syncNeededLabel.Content = "No";
                syncNeededLabel.Foreground = new SolidColorBrush(Color.FromRgb(24, 200, 24));
                syncIpsButton.IsEnabled = false;
            }
            else
            {
                syncNeededLabel.Content = "Yes";
                syncNeededLabel.Foreground = new SolidColorBrush(Color.FromRgb(200, 24, 24));
                syncIpsButton.IsEnabled = true;
            }

            _initialButtonState = syncIpsButton.IsEnabled;
        }

        private async Task<IPAddress> GetClientIpAsync()
        {
            using WebClient client = new();
            Stream stream = await client.OpenReadTaskAsync(new Uri("http://ipinfo.io/ip"));
            using StreamReader ipStream = new(stream);
            return IPAddress.Parse(ipStream.ReadToEnd().Trim());
        }

        private async Task<IPAddress> GetDnsIpAsync()
        {
            DnsApiProcessor api = new(_apiKey);
            ListResponseModel responseModel = await api.ListRecords();

            if (responseModel.Reason != null)
                throw new Exception("stub"); // TODO throw more specific exception

            _editableEntries.Data = responseModel.Data.Where(entry => entry.Editable == 1 && entry.Record == _domain && entry.Type == _recordType).ToList();

            if (_editableEntries.Data.Count == 1)
                return IPAddress.Parse(_editableEntries.Data[0].Value);
            else
                throw new Exception("stub"); // TODO throw more specific exception
        }

        private async void SyncIpsButton_Click(object sender, RoutedEventArgs e)
        {
            DnsApiProcessor api = new(_apiKey);
            string recordName = _editableEntries.Data[0].Record;
            _ = Enum.TryParse(_editableEntries.Data[0].Type, out RecordType recordType);
            string recordValue = _editableEntries.Data[0].Value;

            RemoveOldRecord(api, recordName, recordType, recordValue);
            AddNew(api, recordName, recordType);
            await UpdateStatus();

            void RemoveOldRecord(DnsApiProcessor api, string recordName, RecordType recordType, string recordValue)
            {
                Task<ModifyResponseModel> removeRecordTask = Task.Run(async () => await api.RemoveRecord(recordName, recordType, recordValue));
                if (removeRecordTask.Result.Result != "success")
                {
                    MessageBoxResult result = MessageBox.Show(owner: this,
                        messageBoxText: $"Error while removing old entry{Environment.NewLine}" +
                                        $"    Status code: {removeRecordTask.Result.Result}{Environment.NewLine}" +
                                        $"    Reason: {removeRecordTask.Result.Reason}{Environment.NewLine}" +
                                        $"Do you want to retry?",
                        caption: $"{this.Title} - Remove old DNS record",
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Error,
                        defaultResult: MessageBoxResult.No);

                    if (result == MessageBoxResult.Yes)
                        RemoveOldRecord(api, recordName, recordType, recordValue);
                    else
                        _ = MessageBox.Show(owner: this,
                        messageBoxText: $"No modifications where made",
                        caption: $"{this.Title} - Remove old DNS record",
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Exclamation);

                }
            }
            
            void AddNew(DnsApiProcessor api, string recordName, RecordType recordType)
            {
                Task<ModifyResponseModel> addRecordTask = Task.Run(async () => await api.AddRecord(recordName, recordType, _clientIp.ToString()));
                if (addRecordTask.Result.Result != "success")
                {
                    MessageBoxResult result = MessageBox.Show(owner: this,
                        messageBoxText: $"Error while adding new entry{Environment.NewLine}" +
                                        $"    Status code: {addRecordTask.Result.Result}{Environment.NewLine}" +
                                        $"    Reason: {addRecordTask.Result.Reason}{Environment.NewLine}" +
                                        $"Do you want to retry? (recommended)",
                        caption: $"{this.Title} - Add new DNS record",
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Error,
                        defaultResult: MessageBoxResult.Yes);

                    if (result == MessageBoxResult.Yes)
                        AddNew(api, recordName, recordType);
                    else
                        MessageBox.Show(owner: this,
                        messageBoxText: $"The old record for \"{recordName}\" couldn't be updated. There should now be no DNS records for this record for the type {recordType}. Manual action required to add the record.",
                        caption: $"{this.Title} - Add new DNS record",
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Error,
                        defaultResult: MessageBoxResult.Yes);
                }
                else
                {
                    _ = MessageBox.Show(owner: this,
                        messageBoxText: $"The DNS entry for {_domain} (Type \"{_recordType}\") was successfully updated from {_dnsIp} to {_clientIp}",
                        caption: $"{this.Title} - Successfully updated entry",
                        button: MessageBoxButton.OK,
                        icon: MessageBoxImage.Information);
                }
            }
        }

        private void SyncIpsButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (!syncIpsButton.IsEnabled && _shiftDown)
                syncIpsButton.IsEnabled = true;
        }
    }
}
#pragma warning restore CA1822
