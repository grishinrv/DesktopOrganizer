namespace BackupConfigManager
{
    public sealed class BackupManagerApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;

        public BackupManagerApplicationContext()
        {
            var menus = new ContextMenuStrip();
            menus.Items.Add("Exit", null, Exit);
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("book.ico"),
                ContextMenuStrip = menus,
                Visible = true
            };
        }

        private void Exit(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
