using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace G2Cy.WpfHost.ViewModels
{
    internal class MainWindowViewModel : BindableBase, IDisposable
    {
        private readonly PluginController _pluginController;
        private readonly ErrorHandlingService _errorHandlingService;

        public MainWindowViewModel(ErrorHandlingService errorHandlingService, PluginController pluginController)
        {
            _errorHandlingService = errorHandlingService;
            _pluginController = pluginController;

            LoadPluginCommand = new DelegateCommand<PluginCatalogEntry>(LoadPlugin);
            CloseTabCommand = new DelegateCommand<Plugin>(CloseTab);

            _pluginController.LoadCatalogAcync()
                .ContinueWith(
                    unusedTask => { AvailablePlugins = _pluginController.AvailablePlugins; });
        }

        private IEnumerable<PluginCatalogEntry> _availablePlugins;
        public IEnumerable<PluginCatalogEntry> AvailablePlugins
        {
            get { return _availablePlugins; }
            set { _availablePlugins = value; RaisePropertyChanged("AvailablePlugins"); }
        }

        public ObservableCollection<Plugin> LoadedPlugins { get { return _pluginController.LoadedPlugins; } }

        private Plugin _selectedPlugin;
        public Plugin SelectedPlugin
        {
            get { return _selectedPlugin; }
            set { _selectedPlugin = value; RaisePropertyChanged("SelectedPlugin"); }
        }

        public ICommand LoadPluginCommand { get; private set; }
        public ICommand CloseTabCommand { get; private set; }

        public void Dispose()
        {
            _pluginController.Dispose();
        }

        public bool CanClose()
        {
            var unsavedItems = _pluginController.GetUnsavedItems();
            if (unsavedItems.Count == 0) return true;

            var sb = new StringBuilder();

            sb.AppendLine("The following items are not saved:");
            foreach (var data in unsavedItems)
            {
                sb.AppendLine("\t" + data.Key.Title + ":");
                foreach (var item in data.Value)
                {
                    sb.AppendLine("\t\t" + item);
                }
            }

            sb.AppendLine();
            sb.Append("Are you sure you want to close the application and lose this data?");
            return _errorHandlingService.Confirm(sb.ToString());
        }

#if CSHARP_45
        private async void LoadPlugin(PluginCatalogEntry catalogEntry)
        {
            try
            {
                var plugin = await _pluginController.LoadPluginAsync(catalogEntry);
                SelectedPlugin = plugin;
            }
            catch (Exception ex)
            {
                _errorHandlingService.ShowError("Error loading plugin", ex);
            }
        }
#else
        private void LoadPlugin(PluginCatalogEntry catalogEntry)
        {

            var task = _pluginController.LoadPluginAsync(catalogEntry);
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            task.ContinueWith(t =>
            {
                try
                {
                    SelectedPlugin = t.Result;
                }
                catch (Exception ex)
                {
                    _errorHandlingService.ShowError("Error loading plugin", ex);
                }
            }, scheduler);
        }
#endif

        private void CloseTab(Plugin plugin)
        {
            if (!CanClose(plugin)) return;

            bool changeSelection = (plugin == SelectedPlugin);
            int selectedIndex = LoadedPlugins.IndexOf(plugin);

            _pluginController.RemovePlugin(plugin);

            if (changeSelection)
            {
                int count = LoadedPlugins.Count;

                if (count == 0)
                {
                    SelectedPlugin = null;
                }
                else
                {
                    if (selectedIndex >= count) selectedIndex = count - 1;
                    SelectedPlugin = LoadedPlugins[selectedIndex];
                }
            }
        }

        private bool CanClose(Plugin plugin)
        {
            var unsavedItems = _pluginController.GetUnsavedItems(plugin);
            if (unsavedItems == null || unsavedItems.Length == 0) return true;

            var message = "The following items are not saved:\r\n" +
                String.Join("\r\n", unsavedItems) + "\r\n\r\n" +
                "Are you sure you want to close " + plugin.Title + "?";

            return _errorHandlingService.Confirm(message);
        }
    }
}
