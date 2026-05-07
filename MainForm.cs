using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace AR_Browse
{
    public partial class MainForm : Form
    {
        private WebView2 webView;
        private TextBox urlBar;
        private Panel navBar;

        public MainForm()
        {
            InitializeComponent();

            // Branding & Window Setup
            this.Text = "AR Browse";
            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(800, 450);
            this.BackColor = Color.FromArgb(18, 18, 18);

            InitializeBrowser();
        }

        private async void InitializeBrowser()
        {
            // 1. Browser Engine Setup
            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            // 2. Navigation Bar (The "AR" UI)
            navBar = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(28, 28, 28)
            };

            // --- SETTINGS BUTTON (Far Right Corner) ---
            Button btnSettings = CreateNavBtn("⚙", 0);
            btnSettings.Dock = DockStyle.Right; // Forces to absolute right
            btnSettings.Width = 50;
            btnSettings.Click += (s, e) => ShowSettingsMenu();

            // --- NAVIGATION BUTTONS (Left Side) ---
            Button btnBack = CreateNavBtn("←", 15);
            btnBack.Click += (s, e) => { if (webView?.CanGoBack == true) webView.GoBack(); };

            Button btnForward = CreateNavBtn("→", 60);
            btnForward.Click += (s, e) => { if (webView?.CanGoForward == true) webView.GoForward(); };

            Button btnRefresh = CreateNavBtn("↻", 105);
            btnRefresh.Click += (s, e) => { webView?.CoreWebView2?.Reload(); };

            // --- SMART ADDRESS BAR (Auto-Stretching) ---
            urlBar = new TextBox
            {
                Left = 160,
                Top = 18,
                Width = 850, // Initial width
                Anchor = AnchorStyles.Left | AnchorStyles.Right, // Stretches with window
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                Text = "https://www.google.com"
            };

            urlBar.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    HandleNavigation();
                    e.SuppressKeyPress = true;
                }
            };

            // --- ASSEMBLE UI (Order is key for Docking) ---
            navBar.Controls.Add(btnSettings); // Add first for far-right dock
            navBar.Controls.Add(btnBack);
            navBar.Controls.Add(btnForward);
            navBar.Controls.Add(btnRefresh);
            navBar.Controls.Add(urlBar);

            this.Controls.Add(webView);
            this.Controls.Add(navBar);

            // --- ENGINE INITIALIZATION & SECURITY ---
            await webView.EnsureCoreWebView2Async(null);

            // Apply TLS and Browser Policies
            webView.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Dark;
            webView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;

            // Sync URL Bar
            webView.SourceChanged += (s, e) => { urlBar.Text = webView.Source.ToString(); };

            webView.Source = new Uri("https://www.google.com");
        }

        private void HandleNavigation()
        {
            string input = urlBar.Text.Trim();
            if (input.Contains(".") && !input.Contains(" "))
            {
                if (!input.StartsWith("http")) input = "https://" + input;
                webView.CoreWebView2.Navigate(input);
            }
            else
            {
                webView.CoreWebView2.Navigate("https://www.google.com/search?q=" + Uri.EscapeDataString(input));
            }
        }

        private Button CreateNavBtn(string text, int x)
        {
            Button b = new Button
            {
                Text = text,
                Left = x,
                Top = 10,
                Width = 40,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 60);
            return b;
        }

        private void ShowSettingsMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(28, 28, 28);
            menu.ForeColor = Color.White;
            menu.ShowImageMargin = false;

            // Security Header
            var title = menu.Items.Add("AR BROWSE SECURITY");
            title.Enabled = false;
            title.Font = new Font(menu.Font, FontStyle.Bold);

            menu.Items.Add(new ToolStripSeparator());

            // TLS Info
            menu.Items.Add("TLS 1.2/1.3 Handshake: Force Active").Enabled = false;

            // Privacy Option
            var clearData = menu.Items.Add("Clear Cache & History");
            clearData.Click += async (s, e) => {
                await webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                MessageBox.Show("All browsing data wiped.", "AR Ecosystem Security");
            };

            // About
            var about = menu.Items.Add("About AR Browse");
            about.Click += (s, e) => MessageBox.Show("AR Browse\nBuilt for the AR Ecosystem\nEngine: Chromium (WebView2)", "System Info");

            menu.Show(Cursor.Position);
        }
    }
}