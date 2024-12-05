using Shared;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace BackupConfigManager
{
    public sealed class BackupManagerApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly ProjectsService _projectsService = new ProjectsService();
        private readonly ContextMenuStrip _menu;

        public BackupManagerApplicationContext()
        {
            _menu = new ContextMenuStrip();
            _menu.Items.Add("Add project file", null, AddProjectFile);
            _menu.Items.Add(new ToolStripSeparator());
            if (AddProjectFilesMenuItems(_menu).GetAwaiter().GetResult())
            {
                _menu.Items.Add(new ToolStripSeparator());
            }
            _menu.Items.Add("Exit application", null, Exit);
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon("book.ico"),
                ContextMenuStrip = _menu,
                Visible = true
            };
        }

        private void Exit(object? sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            Application.Exit();
        }

        private async void AddProjectFile(object? sender, EventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var file = dialog.FileName;
                    if (!string.IsNullOrEmpty(file))
                    {
                        await AddProjectFileToMenu(file, false);
                    }
                }
            }
            catch (Exception ex)
            {
                // ignore
            }
        }

        private async Task AddProjectFileToMenu(string file, bool skipIfListedCheck)
        {
            var projects = await _projectsService.Read();
            bool fileAdded = projects.Projects.Add(file);
            if (fileAdded)
            {
                projects.ActiveProject = file;
                await _projectsService.Save(projects);
            }

            if (fileAdded || skipIfListedCheck)
            {
                _menu.Items.Add(file, null, AddProjectFile);
            }
        }

        private async Task<bool> AddProjectFilesMenuItems(ContextMenuStrip menu)
        {
            try{
                var projects = await _projectsService.Read();
                var result = projects.Projects.Any();

                foreach (var project in projects.Projects)
                {

                }
            }
            catch (Exception ex)
            {
                // ignore
                return false;
            }
        }

        private async void SetAsCurrentProject(string file)
        {

        }

        private async void RemoveProject(string file)
        {

        }
    }
}
