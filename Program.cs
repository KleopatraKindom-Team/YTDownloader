using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using YoutubeExplode;

namespace YTDownloader
{
    internal static class Program
    {
        static string saveFolder = "";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();


            Form form = new Form();

            form.Text = "YT Downloader";
            form.Icon = new Icon("app.ico");
            form.Size = new Size(720,420);
            form.StartPosition = FormStartPosition.CenterScreen;
            form.BackColor = Color.FromArgb(25,25,25);



            Label title = new Label();

            title.Text = "YouTube Downloader";

            title.ForeColor = Color.White;

            title.Font =
            new Font("Segoe UI",22,FontStyle.Bold);

            title.Location =
            new Point(30,25);

            title.AutoSize = true;




            TextBox url = new TextBox();

            url.Location =
            new Point(30,90);

            url.Size =
            new Size(640,35);

            url.BackColor =
            Color.FromArgb(45,45,45);

            url.ForeColor =
            Color.White;

            url.PlaceholderText =
            "Ссылка YouTube";





            ComboBox quality = new ComboBox();

            quality.Location =
            new Point(30,150);

            quality.Size =
            new Size(150,35);

            quality.Items.Add("360p");
            quality.Items.Add("720p");
            quality.Items.Add("1080p");

            quality.SelectedIndex = 1;




            Button folder = new Button();

            folder.Text =
            "📁 Папка";

            folder.Location =
            new Point(220,145);

            folder.Size =
            new Size(120,45);





            Button download = new Button();

            download.Text =
            "⬇ Скачать";

            download.Location =
            new Point(380,145);

            download.Size =
            new Size(130,45);





            foreach(Button b in new Button[]
            {
                folder,
                download
            })
            {
                b.ForeColor =
                Color.White;

                b.BackColor =
                Color.FromArgb(60,60,60);

                b.FlatStyle =
                FlatStyle.Flat;

                b.Font =
                new Font("Segoe UI",10,FontStyle.Bold);
            }




            ProgressBar progress =
            new ProgressBar();

            progress.Location =
            new Point(30,240);

            progress.Size =
            new Size(640,25);

            progress.Minimum=0;
            progress.Maximum=100;




            Label status = new Label();

            status.Text =
            "Готово";

            status.ForeColor =
            Color.White;

            status.Location =
            new Point(30,290);

            status.AutoSize=true;






            folder.Click += (s,e)=>
            {
                FolderBrowserDialog f =
                new FolderBrowserDialog();


                if(f.ShowDialog()==DialogResult.OK)
                {
                    saveFolder =
                    f.SelectedPath;

                    status.Text =
                    saveFolder;
                }
            };








            download.Click += async (s,e)=>
            {

                try
                {

                    if(saveFolder=="")
                    {
                        MessageBox.Show(
                        "Выберите папку");

                        return;
                    }


                    YoutubeClient yt =
                    new YoutubeClient();


                    status.Text =
                    "Получение данных...";



                    var video =
                    await yt.Videos.GetAsync(url.Text);



                    var manifest =
                    await yt.Videos.Streams
                    .GetManifestAsync(video.Id);



                    int q = 720;


                    if(quality.Text=="360p")
                        q=360;


                    if(quality.Text=="1080p")
                        q=1080;




                    var stream =
                    manifest.GetMuxedStreams()
                    .Where(x =>
                    x.VideoQuality.MaxHeight<=q)
                    .OrderByDescending(
                    x=>x.VideoQuality.MaxHeight)
                    .First();



                    string file =
                    Path.Combine(
                    saveFolder,
                    video.Title+".mp4");





                    var prog =
                    new Progress<double>(p =>
                    {

                        int percent =
                        (int)(p*100);


                        progress.Value =
                        percent;


                        status.Text =
                        "Скачивание: "
                        +percent+"%";

                    });





                    await yt.Videos.Streams
                    .DownloadAsync(
                    stream,
                    file,
                    prog);



                    status.Text =
                    "Готово ✔";

                }


                catch(Exception ex)
                {
                    MessageBox.Show(
                    ex.Message);
                }

            };






            form.Controls.Add(title);
            form.Controls.Add(url);
            form.Controls.Add(quality);
            form.Controls.Add(folder);
            form.Controls.Add(download);
            form.Controls.Add(progress);
            form.Controls.Add(status);


            Application.Run(form);
        }
    }
}