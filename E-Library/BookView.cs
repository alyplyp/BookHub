using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace E_Library
{
    public partial class BookView : Form
    {
        private const string ApiUrl = "https://www.dbooks.org/api/recent";
        private static readonly HttpClient httpClient = new HttpClient();

        public BookView()
        {
            InitializeComponent();
            LoadBooksAsync();
        }

        private async void LoadBooksAsync()
        {
            try
            {
                var books = await GetBooksAsync();
                PopulateFlowLayout(books);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<BookData>> GetBooksAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetStringAsync(ApiUrl);
                Console.WriteLine($"API Response: {response}");

                var apiResult = JsonConvert.DeserializeObject<ApiResult>(response);

                return apiResult.Books;
            }
        }


        private void PopulateFlowLayout(List<BookData> books)
        {
            flowLayoutPanel.Invoke((MethodInvoker)delegate {
                flowLayoutPanel.Controls.Clear();
            });

            foreach (var book in books)
            {
                var bookControl = CreateBookControl(book);
                flowLayoutPanel.Invoke((MethodInvoker)delegate {
                    flowLayoutPanel.Controls.Add(bookControl);
                });
            }
        }

        private UserControl CreateBookControl(BookData book)
        {
            var bookControl = new UserControl();
            var flowLayoutPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            var pictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(150, 150),
                Image = Properties.Resources.PlaceholderImage, // Default image
                Cursor = Cursors.Hand // Set cursor to indicate clickable
            };

            var titleLabel = new Label
            {
                Text = book.Title,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, pictureBox.Bottom) // Place the title right below the image
            };

            // Handle click event to open the URL in a browser
            pictureBox.Click += (sender, e) =>
            {
                OpenUrlInBrowser(book.Url);
            };

            try
            {
                using (var httpClient = new HttpClient())
                using (var stream = httpClient.GetStreamAsync(book.Image).Result)
                {
                    pictureBox.Image = Image.FromStream(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }

            flowLayoutPanel.Controls.Add(pictureBox);
            flowLayoutPanel.Controls.Add(titleLabel);

            bookControl.Controls.Add(flowLayoutPanel);

            return bookControl;
        }

        private void OpenUrlInBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening URL: {ex.Message}");
            }
        }
    }

    public class ApiResult
    {
        public string Status { get; set; }
        public int Total { get; set; }
        public List<BookData> Books { get; set; }
    }
}
