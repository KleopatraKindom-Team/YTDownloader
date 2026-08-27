using System;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using CliWrap;
using CliWrap.EventStream;

namespace YtDownloader
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                if (File.Exists("app.ico"))
                {
                    this.Icon = new BitmapImage(new Uri(Path.GetFullPath("app.ico")));
                }
            }
            catch 
            {
            }

            string downloadsPath = "";
            try
            {
                downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), 
                    "Downloads"
                );

                if (!Directory.Exists(downloadsPath))
                {
                    downloadsPath = AppDomain.CurrentDomain.BaseDirectory;
                }
            }
            catch
            {
                downloadsPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            FolderPathTextBox.Text = downloadsPath;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите папку для сохранения видео",
                InitialDirectory = FolderPathTextBox.Text
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = dialog.FolderName;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string url = UrlTextBox.Text.Trim();
            string outputFolder = FolderPathTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                StatusLabel.Text = "Введите ссылку на видео.";
                return;
            }

            if (!Directory.Exists(outputFolder))
            {
                StatusLabel.Text = "Указанная папка не существует.";
                return;
            }

            if (!File.Exists("yt-dlp.exe") || !File.Exists("ffmpeg.exe"))
            {
                MessageBox.Show(
                    "Не найдены yt-dlp.exe или ffmpeg.exe в рабочей папке!", 
                    "Ошибка", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                return;
            }

            SetUIState(isDownloading: true);
            StatusLabel.Text = "Подготовка и получение названия...";

            try
            {
                string outputTemplate = Path.Combine(outputFolder, "%(title)s.%(ext)s");

                var cmd = Cli.Wrap("yt-dlp.exe")
                    .WithArguments(new[]
                    {
                        "-f", "bv*+ba/b",
                        "--merge-output-format", "mp4",
                        "-o", outputTemplate,
                        url
                    });

                await foreach (var cmdEvent in cmd.ListenAsync())
                {
                    if (cmdEvent is StandardOutputCommandEvent stdOut)
                    {
                        string line = stdOut.Text;

                        if (line.Contains("[download]") && line.Contains("%"))
                        {
                            var match = Regex.Match(line, @"(\d+(?:\.\d+)?)%");
                            if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double percent))
                            {
                                DownloadProgressBar.IsIndeterminate = false;
                                DownloadProgressBar.Value = percent;
                                StatusLabel.Text = $"Скачивание: {percent:F1}%";
                            }
                        }
                        else if (line.Contains("[Merger]"))
                        {
                            DownloadProgressBar.IsIndeterminate = true;
                            StatusLabel.Text = "Склеивание видео и аудио (FFmpeg)...";
                        }
                    }
                }

                StatusLabel.Text = "Загрузка завершена!";
                MessageBox.Show($"Видео успешно сохранено в папку:\n{outputFolder}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Ошибка скачивания.";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetUIState(isDownloading: false);
            }
        }

        private void SetUIState(bool isDownloading)
        {
            DownloadButton.IsEnabled = !isDownloading;
            UrlTextBox.IsEnabled = !isDownloading;
            BrowseButton.IsEnabled = !isDownloading;
            DownloadProgressBar.Visibility = isDownloading ? Visibility.Visible : Visibility.Collapsed;
            DownloadProgressBar.IsIndeterminate = isDownloading;
            if (isDownloading) DownloadProgressBar.Value = 0;
        }
    }
}
