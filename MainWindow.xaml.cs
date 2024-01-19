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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Renci.SshNet;
using System.IO;
using Renci.SshNet.Common;

namespace SFTP_File_Sender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Replace these variables with your specific details
        private string hostname = "your_hostname";
        private string username = "your_username";
        private string password = "your_password";
        private string privateKeyPath = "path_to_your_private_key_file";
        private int port = 22;
        private string localPath = "your_localPath";
        private string remotePath = "your_remotePath";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get user-entered server details
                string hostname = HostnameTextBox.Text;
                string username = UsernameTextBox.Text;
                int port = int.TryParse(PortTextBox.Text, out int parsedPort) ? parsedPort : 22; // Default to 22 if parsing fails
                string localPath = LocalPathTextBox.Text;
                string remotePath = RemotePathTextBox.Text;

                // Get yesterday's date in the format "yyyyMMdd"
                string previousDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

                // Get a list of files for yesterday in the local path
                string[] relevantFiles = Directory.GetFiles(localPath, $"UBTxn{previousDate}*.txt");

                if (relevantFiles.Length == 0)
                {
                    MessageBox.Show($"No relevant files found for {previousDate}.");
                    return;
                }

                using (var client = new SftpClient(hostname, port, username, new PrivateKeyFile("path_to_your_private_key_file")))
                {
                    client.Connect();

                    foreach (string file in relevantFiles)
                    {
                        // Get the filename without the full path
                        string filename = System.IO.Path.GetFileName(file);

                        try
                        {
                            // Upload the file to the absolute remote path
                            using (var fileStream = new FileStream(file, FileMode.Open))
                            {
                                client.UploadFile(fileStream, System.IO.Path.Combine(remotePath, filename), true);
                            }

                            MessageBox.Show($"File '{filename}' uploaded successfully.");
                        }
                        catch (SftpPermissionDeniedException pdex)
                        {
                            // Handle permission denied exception
                            MessageBox.Show($"Permission denied while uploading '{filename}': {pdex.Message}");
                            // You can choose to log the error or take other appropriate actions
                        }
                        catch (Exception ex)
                        {
                            // Handle other exceptions
                            MessageBox.Show($"Error while uploading '{filename}': {ex.Message}");
                            // Add more detailed error handling if needed
                        }
                    }

                    client.Disconnect();
                }

                MessageBox.Show("All relevant files uploaded successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                // Add more detailed error handling if needed
            }
        }

        private void HostnameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
