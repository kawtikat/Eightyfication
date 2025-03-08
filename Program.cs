using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BatteryMonitor
{
    public partial class MainForm : Form
    {
        private bool hasNotified = false;
        private double lastBatteryLevel = 0;
        private System.Windows.Forms.Timer updateTimer;
        private ProgressBar batteryProgressBar;
        private Label batteryLevelLabel;
        private Label batteryStatusLabel;
        private Label powerSourceLabel;
        private Label notificationStatusLabel;
        private GroupBox batteryInfoGroup;
        private Button settingsButton;
        private NotifyIcon trayIcon;
        private CheckBox minimizeToTrayCheckBox;

        public MainForm()
        {
            InitializeComponent();
            InitializeTimer();
            UpdateBatteryInfo();
        }

        private void InitializeComponent()
        {
            this.Text = "Eightyfication";
            this.Size = new Size(300, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = SystemIcons.Shield;

            batteryInfoGroup = new GroupBox
            {
                Text = "Battery Information",
                Location = new Point(20, 20),
                Size = new Size(250, 120)
            };
            this.Controls.Add(batteryInfoGroup);

            batteryProgressBar = new ProgressBar
            {
                Location = new Point(20, 25),
                Size = new Size(200, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };
            batteryInfoGroup.Controls.Add(batteryProgressBar);

            batteryLevelLabel = new Label
            {
                Location = new Point(20, 50),
                Size = new Size(200, 20),
                Text = "Battery Level: 0%"
            };
            batteryInfoGroup.Controls.Add(batteryLevelLabel);

            batteryStatusLabel = new Label
            {
                Location = new Point(20, 70),
                Size = new Size(200, 20),
                Text = "Battery Status: Unknown"
            };
            batteryInfoGroup.Controls.Add(batteryStatusLabel);

            powerSourceLabel = new Label
            {
                Location = new Point(20, 90),
                Size = new Size(200, 20),
                Text = "Power Source: Unknown"
            };
            batteryInfoGroup.Controls.Add(powerSourceLabel);
            
            notificationStatusLabel = new Label
            {
                Location = new Point(20, 150),
                Size = new Size(150, 20),
                Text = "Notification at 80%"
            };
            this.Controls.Add(notificationStatusLabel);

            minimizeToTrayCheckBox = new CheckBox
            {
                Text = "Minimize to tray",
                Location = new Point(20, 170),
                Size = new Size(100, 20),
                Checked = false
            };
            this.Controls.Add(minimizeToTrayCheckBox);

            settingsButton = new Button
            {
                Text = "Settings",
                Location = new Point(190, 155),
                Size = new Size(80, 25)
            };
            settingsButton.Click += SettingsButton_Click;
            this.Controls.Add(settingsButton);

            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Visible = true,
                Text = "Battery Monitor"
            };
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            ContextMenuStrip trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Open", null, TrayIcon_DoubleClick);
            trayMenu.Items.Add("Exit", null, (s, e) => Application.Exit());
            trayIcon.ContextMenuStrip = trayMenu;

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && minimizeToTrayCheckBox.Checked)
            {
                e.Cancel = true; 
                this.Hide();
                trayIcon.ShowBalloonTip(2000, "Battery Monitor", 
                    "Application is still running in the system tray", ToolTipIcon.Info);
            }
            else
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
        }
        
        private void InitializeTimer()
        {
            updateTimer = new System.Windows.Forms.Timer
            {
                Interval = 10000
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateBatteryInfo();
        }

        private void UpdateBatteryInfo()
        {
            try
            {
                PowerStatus status = SystemInformation.PowerStatus;
                int batteryLevel = (int)(status.BatteryLifePercent * 100);
                string batteryStatus = GetBatteryStatus(status.BatteryChargeStatus);
                string powerSource = (status.PowerLineStatus == PowerLineStatus.Online ? "AC Power" : "Battery");

                batteryProgressBar.Value = batteryLevel;
                batteryLevelLabel.Text = $"Battery Level: {batteryLevel}%";
                batteryStatusLabel.Text = $"Battery Status: {batteryStatus}";
                powerSourceLabel.Text = $"Power Source: {powerSource}";
                
                trayIcon.Text = $"Battery - {batteryLevel}%";

                if (batteryLevel == 80 && lastBatteryLevel < 80 && !hasNotified)
                {
                    ShowNotification("Battery Alert", $"Battery level has reached {batteryLevel}%");
                    hasNotified = true;
                }
                else if (batteryLevel < 50)
                {
                    hasNotified = false;
                }

                lastBatteryLevel = batteryLevel;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating battery information: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetBatteryStatus(BatteryChargeStatus status)
        {
            if (status.HasFlag(BatteryChargeStatus.Charging))
                return "Charging";
            else if (status.HasFlag(BatteryChargeStatus.Critical))
                return "Critical";
            else if (status.HasFlag(BatteryChargeStatus.Low))
                return "Low";
            else if (status.HasFlag(BatteryChargeStatus.High))
                return "High";
            else
                return "Normal";
        }

        private void ShowNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Settings functionality will be implemented in a future version.", 
                "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}