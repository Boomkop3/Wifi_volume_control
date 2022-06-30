#nullable disable

using System.Xml;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        Font drawFont;
        SolidBrush drawBrush;
        Graphics graphics;
        public Form1()
        {
            InitializeComponent();
            bitmap = new Bitmap(32, 32);
            drawFont = new Font("Consolas", 18, FontStyle.Bold);
            drawBrush = new SolidBrush(Color.Black);
            graphics = Graphics.FromImage(bitmap);
        }

        private async Task<string> getVolume()
        {
            var client = new HttpClient();

            // Get the response.
            HttpResponseMessage response = await client.GetAsync(
                "http://192.168.0.150/goform/formMainZone_MainZoneXml.xml"
            );
            string data;
            HttpContent responseContent = response.Content;
            using (var reader = new StreamReader(await responseContent.ReadAsStreamAsync())) {
                data = await reader.ReadToEndAsync();
            }

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(data);
            XmlNode node;
            node = xml.FirstChild;
            while (node!.Name != "item") {
                node = node.NextSibling;
            }
            node = node.FirstChild;
            while (node!.Name != "MasterVolume") {
                node = node.NextSibling;
            }

            return $"{((int)Math.Abs(float.Parse(node.InnerText)))}";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NotifyIcon trayIcon = new NotifyIcon();
            trayIcon.Text = "Volume";
            trayIcon.Icon = GetIcon("");

            trayIcon.Visible = true;

            Task.Run(async () => {
                int max_delay = 500;
                int min_delay = 200;
                int delay = max_delay;
                string prev = "69";
                while (true) {
                    await Task.Delay(delay);
                    string volume;
                    try {
                        volume = await getVolume();
                    } catch {
                        delay = max_delay;
                        continue;
                    }
                    if (prev != volume) {
                        prev = volume;
                        trayIcon.Icon = GetIcon(volume);
                        delay = min_delay;
                    } else {
                        delay += 10;
                        if (delay > max_delay) delay = max_delay;
                    }
                }
            });
        }

        public Icon GetIcon(string text)
        {
            graphics.Clear(Color.Transparent);
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            graphics.DrawString(text, drawFont, drawBrush, 1, 2);

            Icon createdIcon = Icon.FromHandle(bitmap.GetHicon());

            return createdIcon;
        }

        private void Form1_Paint(object sender, PaintEventArgs e) {
            this.Hide();
        }
    }
}
