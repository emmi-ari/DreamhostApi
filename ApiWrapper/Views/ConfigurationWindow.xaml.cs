using ApiClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ApiWrapper
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private readonly AppSettings _settings = new();

        #region User entered values
        private string _apiKey;
        private string _domainName;
        private string _recordType;
        #endregion

        public ConfigurationWindow(Window owner)
        {
            InitializeComponent();

            Owner = owner;

            RecordType[] recordTypes = Enum.GetValues<RecordType>();
            foreach (RecordType recordType in recordTypes)
            {
                recordTypeComboBox.Items.Add(recordType);
            }
        }

        private void SettingsValue_TextChanged(object sender, EventArgs e)
        {
            switch (sender)
            {
                case TextBox textBox:
                    if (textBox.Name == apiTextBox.Name)
                        _apiKey = textBox.Text;
                    else if (textBox.Name == domainTextBox.Name)
                        _domainName = domainTextBox.Text;
                    break;

                case ComboBox comboBox:
                    _recordType = (string)comboBox.SelectedItem;
                    break;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) =>
            Close();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.apiKey = _apiKey;
            _settings.domain = _domainName;
            _settings.recordType = _recordType;

            DialogResult = !string.IsNullOrEmpty(_apiKey) || !string.IsNullOrEmpty(_domainName) || !string.IsNullOrEmpty(_recordType);
        }
    }
}
