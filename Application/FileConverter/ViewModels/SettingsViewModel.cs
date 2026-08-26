// <copyright file="SettingsViewModel.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter.ViewModels
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Xml;

    using Microsoft.Win32;
    
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using CommunityToolkit.Mvvm.Input;

    using FileConverter.Annotations;
    using FileConverter.Services;
    using FileConverter.Views;

    /// <summary>
    /// This class contains properties that the settings View can data bind to.
    /// </summary>
    public class SettingsViewModel : ObservableRecipient, IDataErrorInfo
    {
        private InputExtensionCategory[] inputCategories;
        private PresetFolderNode presetsRootFolder;
        private PresetFolderNode selectedFolder;
        private PresetNode selectedPreset;
        private Settings settings;
        private bool displaySeeChangeLogLink = true;

        private RelayCommand<string> openUrlCommand;
        private RelayCommand getChangeLogContentCommand;
        private RelayCommand createFolderCommand;
        private RelayCommand newPresetCommand;
        private RelayCommand duplicatePresetCommand;
        private RelayCommand importPresetCommand;
        private RelayCommand exportPresetCommand;
        private RelayCommand removePresetCommand;
        private RelayCommand saveCommand;
        private RelayCommand<CancelEventArgs> closeCommand;
        private RelayCommand browseCustomExecutableCommand;
        private RelayCommand resetCustomExecutableCommand;
        private RelayCommand<string> insertArgumentTagCommand;
        private RelayCommand<string> applyCustomQuickTemplateCommand;
        private RelayCommand addCustomInputExtensionCommand;
        private RelayCommand<string> removeCustomInputExtensionCommand;
        private RelayCommand openPresetsFolderCommand;
        private RelayCommand openSettingsFolderCommand;
        private string newCustomInputExtension;

        private ListCollectionView outputTypes;
        private CultureInfo[] supportedCultures;
        private Helpers.HardwareAccelerationMode[] hardwareAccelerationModes = { Helpers.HardwareAccelerationMode.Off, Helpers.HardwareAccelerationMode.CUDA, Helpers.HardwareAccelerationMode.AMF };

        public event Action OnPresetCreated;
        public event Action OnFolderCreated;

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel()
        {
            this.getChangeLogContentCommand = new RelayCommand(this.DownloadChangeLogAction);
            this.openUrlCommand = new RelayCommand<string>((url) => Process.Start(url));
            this.createFolderCommand = new RelayCommand(this.CreateFolder);
            this.newPresetCommand = new RelayCommand(() => this.AddNewPreset(false));
            this.duplicatePresetCommand = new RelayCommand(() => this.AddNewPreset(true), this.CanDuplicateSelectedPreset);
            this.importPresetCommand = new RelayCommand(this.ImportPreset);
            this.exportPresetCommand = new RelayCommand(this.ExportSelectedPreset, this.CanExportSelectedPreset);
            this.removePresetCommand = new RelayCommand(this.RemoveSelectedPreset, this.CanRemoveSelectedPreset);
            this.saveCommand = new RelayCommand(this.SaveSettings, this.CanSaveSettings);
            this.closeCommand = new RelayCommand<CancelEventArgs>(this.CloseSettings);
            this.browseCustomExecutableCommand = new RelayCommand(this.BrowseCustomExecutable);
            this.resetCustomExecutableCommand = new RelayCommand(this.ResetCustomExecutable);
            this.insertArgumentTagCommand = new RelayCommand<string>(this.InsertArgumentTag);
            this.applyCustomQuickTemplateCommand = new RelayCommand<string>(this.ApplyCustomQuickTemplate);
            this.addCustomInputExtensionCommand = new RelayCommand(this.AddCustomInputExtensionAction);
            this.removeCustomInputExtensionCommand = new RelayCommand<string>(this.RemoveCustomInputExtensionAction);
            this.openPresetsFolderCommand = new RelayCommand(this.OpenPresetsFolderAction);
            this.openSettingsFolderCommand = new RelayCommand(this.OpenSettingsFolderAction);

            ISettingsService settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            this.Settings = settingsService.Settings;

            List<OutputTypeViewModel> outputTypeViewModels = new List<OutputTypeViewModel>();
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Ogg));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Mp3));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Aac));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Flac));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Wav));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Mkv));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Mp4));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Ogv));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Webm));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Avi));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Png));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Jpg));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Webp));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Ico));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Gif));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Pdf));
            outputTypeViewModels.Add(new OutputTypeViewModel(OutputType.Custom));
            this.outputTypes = new ListCollectionView(outputTypeViewModels);
            this.outputTypes.GroupDescriptions.Add(new PropertyGroupDescription("Category"));

            this.SupportedCultures = Helpers.GetSupportedCultures().ToArray();

            this.InitializeCompatibleInputExtensions();
            this.InitializePresetFolders();
        }

        public IEnumerable<InputExtensionCategory> InputCategories
        {
            get
            {
                if (this.inputCategories == null)
                {
                    yield break;
                }

                for (int index = 0; index < this.inputCategories.Length; index++)
                {
                    InputExtensionCategory category = this.inputCategories[index];
                    if (this.SelectedPreset == null || Helpers.IsOutputTypeCompatibleWithCategory(this.SelectedPreset.Preset.OutputType, category.Name))
                    {
                        yield return category;
                    }
                }
            }
        }
        
        public InputPostConversionAction[] InputPostConversionActions => new[]
                                                                             {
                                                                                 InputPostConversionAction.None,
                                                                                 InputPostConversionAction.MoveInArchiveFolder,
                                                                                 InputPostConversionAction.Delete,
                                                                             };

        public PresetFolderNode PresetsRootFolder
        {
            get => this.presetsRootFolder;

            set
            {
                this.presetsRootFolder = value;
                this.OnPropertyChanged();
            }
        }

        public AbstractTreeNode SelectedItem
        {
            get
            {
                if (this.SelectedFolder != null)
                {
                    return this.SelectedFolder;
                }

                return this.SelectedPreset;
            }

            set
            {
                if (value is PresetNode preset)
                {
                    this.SelectedPreset = preset;
                    this.SelectedFolder = null;
                }
                else if (value is PresetFolderNode folder)
                {
                    this.SelectedFolder = folder;
                    this.SelectedPreset = null;
                }
                else
                {
                    this.SelectedPreset = null;
                    this.SelectedFolder = null;
                }

                this.OnPropertyChanged();
            }
        }

        public PresetFolderNode SelectedFolder
        {
            get => this.selectedFolder;

            set
            {
                this.selectedFolder = value;

                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SelectedItem));
                this.removePresetCommand?.NotifyCanExecuteChanged();
                this.exportPresetCommand?.NotifyCanExecuteChanged();
                this.duplicatePresetCommand?.NotifyCanExecuteChanged();
            }
        }

        public PresetNode SelectedPreset
        {
            get => this.selectedPreset;

            set
            {
                if (this.selectedPreset != null)
                {
                    this.selectedPreset.Preset.PropertyChanged -= this.SelectedPresetPropertyChanged;
                }

                this.selectedPreset = value;

                if (this.selectedPreset != null)
                {
                    this.selectedPreset.Preset.PropertyChanged += this.SelectedPresetPropertyChanged;
                }

                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SelectedItem));
                this.OnPropertyChanged(nameof(this.InputCategories));
                this.removePresetCommand?.NotifyCanExecuteChanged();
                this.exportPresetCommand?.NotifyCanExecuteChanged();
                this.duplicatePresetCommand?.NotifyCanExecuteChanged();
            }
        }

        public Settings Settings
        {
            get => this.settings;

            set
            {
                this.settings = value;
                this.OnPropertyChanged();
            }
        }

        public CultureInfo[] SupportedCultures
        {
            get => this.supportedCultures;
            set
            {
                this.supportedCultures = value;
                this.OnPropertyChanged();
            }
        }

        public Helpers.HardwareAccelerationMode[] HardwareAccelerationModes
        {
            get => this.hardwareAccelerationModes;
            set
            {
                this.hardwareAccelerationModes = value;
                this.OnPropertyChanged();
            }
        }

        public ListCollectionView OutputTypes
        {
            get => this.outputTypes;
            set
            {
                this.outputTypes = value;
                this.OnPropertyChanged();
            }
        }
        
        public bool DisplaySeeChangeLogLink
        {
            get
            {
                return this.displaySeeChangeLogLink;
            }

            private set
            {
                this.displaySeeChangeLogLink = value;

                this.OnPropertyChanged();
            }
        }
        
        public ICommand GetChangeLogContentCommand => this.getChangeLogContentCommand;

        public ICommand OpenUrlCommand => this.openUrlCommand;

        public ICommand CreateFolderCommand => this.createFolderCommand;

        public ICommand AddNewPresetCommand => this.newPresetCommand;

        public ICommand DuplicatePresetCommand => this.duplicatePresetCommand;

        public ICommand ImportPresetCommand => this.importPresetCommand;

        public ICommand ExportPresetCommand => this.exportPresetCommand;

        public ICommand RemoveSelectedPresetCommand => this.removePresetCommand;

        public ICommand SaveCommand => this.saveCommand;

        public ICommand CloseCommand => this.closeCommand;

        public ICommand BrowseCustomExecutableCommand => this.browseCustomExecutableCommand;

        public ICommand ResetCustomExecutableCommand => this.resetCustomExecutableCommand;

        public ICommand InsertArgumentTagCommand => this.insertArgumentTagCommand;

        public ICommand ApplyCustomQuickTemplateCommand => this.applyCustomQuickTemplateCommand;

        public ICommand AddCustomInputExtensionCommand => this.addCustomInputExtensionCommand;

        public ICommand RemoveCustomInputExtensionCommand => this.removeCustomInputExtensionCommand;

        public ICommand OpenPresetsFolderCommand => this.openPresetsFolderCommand;

        public ICommand OpenSettingsFolderCommand => this.openSettingsFolderCommand;

        public string NewCustomInputExtension
        {
            get => this.newCustomInputExtension;
            set => this.SetProperty(ref this.newCustomInputExtension, value);
        }

        public TreeViewSelectionBehavior.IsChildOfPredicate PresetsHierarchyPredicate => (object nodeA, object nodeB) =>
            {
                if (nodeA is PresetNode)
                {
                    return false;
                }

                PresetFolderNode parentFolder = nodeA as PresetFolderNode;
                Diagnostics.Debug.Assert(parentFolder != null, "Node should be a preset folder.");

                return parentFolder.IsNodeInHierarchy(nodeB as AbstractTreeNode, true);
            };

        public string Error
        {
            get
            {
                string nodeError = this.CheckErrorRecursively(this.presetsRootFolder);
                if (!string.IsNullOrEmpty(nodeError))
                {
                    return nodeError;
                }

                return string.Empty;
            }
        }

        public string this[string columnName] => this.Error;

        [NotNull]
        public string ImportDirectoryPath
        {
            get
            {
                string path = FileConverter.Registry.GetValue(FileConverter.Registry.Keys.ImportInitialFolder, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                if (!Directory.Exists(path))
                {
                    path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                return path;
            }

            set
            {
                if (!Directory.Exists(value))
                {
                    return;
                }

                FileConverter.Registry.SetValue(FileConverter.Registry.Keys.ImportInitialFolder, value);
            }
        }

        private string CheckErrorRecursively(AbstractTreeNode node)
        {
            string nodeError = node.Error;
            if (!string.IsNullOrEmpty(nodeError))
            {
                return nodeError;
            }

            if (node is PresetFolderNode folder)
            {
                foreach (AbstractTreeNode child in folder.Children)
                {
                    nodeError = this.CheckErrorRecursively(child);
                    if (!string.IsNullOrEmpty(nodeError))
                    {
                        return nodeError;
                    }
                }
            }

            return string.Empty;
        }

        private void SelectedPresetPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == "OutputType")
            {
                this.OnPropertyChanged(nameof(this.InputCategories));
            }

            this.saveCommand.NotifyCanExecuteChanged();
        }

        private void NodePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            this.saveCommand.NotifyCanExecuteChanged();
        }

        private void DownloadChangeLogAction()
        {
            IUpgradeService upgradeService = Ioc.Default.GetRequiredService<IUpgradeService>();
            upgradeService.DownloadChangeLog();
            this.DisplaySeeChangeLogLink = false;
        }

        private void InitializeCompatibleInputExtensions()
        {
            List<InputExtensionCategory> categories = new List<InputExtensionCategory>();
            for (int index = 0; index < Helpers.CompatibleInputExtensions.Length; index++)
            {
                string compatibleInputExtension = Helpers.CompatibleInputExtensions[index];
                string extensionCategory = Helpers.GetExtensionCategory(compatibleInputExtension);
                InputExtensionCategory category = categories.Find(match => match.Name == extensionCategory);
                if (category == null)
                {
                    category = new InputExtensionCategory(extensionCategory);
                    categories.Add(category);
                }

                category.AddExtension(compatibleInputExtension);
            }

            // Load user-defined custom input extensions into "Custom" category
            if (this.Settings.CustomInputExtensions != null && this.Settings.CustomInputExtensions.Count > 0)
            {
                InputExtensionCategory customCategory = categories.Find(match => match.Name == Helpers.InputCategoryNames.Custom);
                if (customCategory == null)
                {
                    customCategory = new InputExtensionCategory(Helpers.InputCategoryNames.Custom);
                    categories.Add(customCategory);
                }

                foreach (string customExt in this.Settings.CustomInputExtensions)
                {
                    customCategory.AddExtension(customExt, true);
                }
            }

            this.inputCategories = categories.ToArray();
            this.OnPropertyChanged(nameof(this.InputCategories));
        }

        private void InitializePresetFolders()
        {
            this.presetsRootFolder = new PresetFolderNode(null, null);
            foreach (ConversionPreset preset in this.Settings.ConversionPresets)
            {
                PresetFolderNode parent = this.presetsRootFolder;
                foreach (string folderName in preset.ParentFoldersNames)
                {
                    PresetFolderNode subFolder = parent.Children.FirstOrDefault(match => match is PresetFolderNode && ((PresetFolderNode)match).Name == folderName) as PresetFolderNode;
                    if (subFolder == null)
                    {
                        subFolder = this.CreateFolderNode(folderName, parent);
                    }

                    parent = subFolder;
                }

                this.CreatePresetNode(preset, parent);
            }

            this.OnPropertyChanged(nameof(this.PresetsRootFolder));
        }

        private void ComputePresetsParentFoldersNamesAndFillSettings(AbstractTreeNode node, List<string> folderNamesCache)
        {
            if (node is PresetFolderNode folder)
            {
                if (!string.IsNullOrEmpty(folder.Name))
                {
                    folderNamesCache.Add(folder.Name);
                }

                foreach (var child in folder.Children)
                {
                    this.ComputePresetsParentFoldersNamesAndFillSettings(child, folderNamesCache);
                }

                if (!string.IsNullOrEmpty(folder.Name))
                {
                    folderNamesCache.RemoveAt(folderNamesCache.Count - 1);
                }
            }
            else if (node is PresetNode preset)
            {
                preset.Preset.ParentFoldersNames = folderNamesCache.ToArray();
                this.settings.ConversionPresets.Add(preset.Preset);
            }
        }

        private void CloseSettings(CancelEventArgs args)
        {
            ISettingsService settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            settingsService.RevertSettings();

            INavigationService navigationService = Ioc.Default.GetRequiredService<INavigationService>();
            navigationService.Close(Pages.Settings, args != null);
        }

        private bool CanSaveSettings()
        {
            return string.IsNullOrEmpty(this.Error);
        }

        private void SaveSettings()
        {
            // Compute parent folder names.
            this.settings.ConversionPresets.Clear();
            this.ComputePresetsParentFoldersNamesAndFillSettings(this.presetsRootFolder, new List<string>());
            
            // Save changes.
            ISettingsService settingsService = Ioc.Default.GetRequiredService<ISettingsService>();
            settingsService.SaveSettings();

            INavigationService navigationService = Ioc.Default.GetRequiredService<INavigationService>();
            navigationService.Close(Pages.Settings, false);
        }

        private void CreateFolder()
        {
            PresetFolderNode parent;
            if (this.SelectedFolder != null)
            {
                parent = this.SelectedFolder;
            }
            else if (this.SelectedItem != null)
            {
                parent = this.SelectedItem.Parent;
            }
            else
            {
                parent = this.presetsRootFolder;
            }

            int insertIndex = parent.Children.IndexOf(this.SelectedItem) + 1;
            if (insertIndex < 0)
            {
                insertIndex = parent.Children.Count;
            }

            // Generate a unique folder name.
            string folderName = Properties.Resources.DefaultFolderName;
            int index = 1;
            while (parent.Children.Any(match => match is PresetFolderNode folder && folder.Name == folderName))
            {
                index++;
                folderName = $"{Properties.Resources.DefaultFolderName} ({index})";
            }

            PresetFolderNode newFolder = new PresetFolderNode(folderName, parent);

            parent.Children.Insert(insertIndex, newFolder);

            newFolder.PropertyChanged += this.NodePropertyChanged;

            this.SelectedItem = newFolder;

            this.saveCommand.NotifyCanExecuteChanged();

            this.OnFolderCreated();
        }

        private bool CanDuplicateSelectedPreset()
        {
            return this.SelectedPreset != null;
        }

        private void AddNewPreset(bool duplicate)
        {
            PresetFolderNode parent;
            if (this.SelectedFolder != null)
            {
                parent = this.SelectedFolder;
            }
            else if (this.SelectedItem != null)
            {
                parent = this.SelectedItem.Parent;
            }
            else
            {
                parent = this.presetsRootFolder;
            }

            int insertIndex = parent.Children.IndexOf(this.SelectedItem) + 1;
            if (insertIndex < 0)
            {
                insertIndex = parent.Children.Count;
            }

            // Generate a unique preset name.
            string presetName = Properties.Resources.DefaultPresetName;
            int index = 1;
            while (parent.Children.Any(match => match is PresetNode folder && folder.Preset.ShortName == presetName))
            {
                index++;
                presetName = $"{Properties.Resources.DefaultPresetName} ({index})";
            }

            // Create preset by copying the selected one.
            ConversionPreset newPreset = null;
            if (this.SelectedPreset != null && duplicate)
            {
                newPreset = new ConversionPreset(presetName, this.SelectedPreset.Preset);
            }
            else
            {
                newPreset = new ConversionPreset(presetName, OutputType.Mkv, new string[0]);
            }

            PresetNode node = new PresetNode(newPreset, parent);

            parent.Children.Insert(insertIndex, node);

            node.PropertyChanged += this.NodePropertyChanged;

            this.SelectedItem = node;

            this.OnPresetCreated.Invoke();

            this.removePresetCommand.NotifyCanExecuteChanged();
            this.saveCommand.NotifyCanExecuteChanged();
        }

        private void ImportPreset()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = Properties.Resources.ImportPresets,
                Filter = "Preset file (*.xml)|*.xml",
                InitialDirectory = this.ImportDirectoryPath,
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (!File.Exists(openFileDialog.FileName))
                {
                    Diagnostics.Debug.LogError("File does not exist.");
                    return;
                }

                string directoryPath = Path.GetDirectoryName(openFileDialog.FileName);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    this.ImportDirectoryPath = directoryPath;
                }

                List<ConversionPreset> presetsToImport = new List<ConversionPreset>();

                try
                {
                    // Detect root element of the XML file to deserialize accurately
                    string rootElementName = null;
                    using (XmlReader reader = XmlReader.Create(openFileDialog.FileName))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                rootElementName = reader.LocalName;
                                break;
                            }
                        }
                    }

                    if (string.Equals(rootElementName, "ConversionPreset", StringComparison.OrdinalIgnoreCase))
                    {
                        // Single preset XML
                        XmlHelpers.LoadFromFile<ConversionPreset>(rootElementName, openFileDialog.FileName, out ConversionPreset singlePreset);
                        if (singlePreset != null)
                        {
                            presetsToImport.Add(singlePreset);
                        }
                    }
                    else if (string.Equals(rootElementName, "Settings", StringComparison.OrdinalIgnoreCase))
                    {
                        // Full Settings XML
                        XmlHelpers.LoadFromFile<Settings>("Settings", openFileDialog.FileName, out Settings importedSettings);
                        if (importedSettings?.ConversionPresets != null)
                        {
                            presetsToImport.AddRange(importedSettings.ConversionPresets);
                        }
                    }
                    else
                    {
                        // Multiple presets wrapped in <Presets> or other root
                        string rootToUse = string.IsNullOrEmpty(rootElementName) ? "Presets" : rootElementName;
                        XmlHelpers.LoadFromFile<List<ConversionPreset>>(rootToUse, openFileDialog.FileName, out presetsToImport);
                    }
                }
                catch (Exception ex)
                {
                    Diagnostics.Debug.LogError($"Failed to import presets: {ex}");
                    MessageBox.Show(
                        $"Failed to import preset:\n{ex.Message}",
                        "File Converter",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (presetsToImport == null || presetsToImport.Count == 0)
                {
                    MessageBox.Show(
                        "No valid conversion presets found in the selected file.",
                        "File Converter",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // Add imported presets to preset tree.
                bool itemSelected = false;
                foreach (ConversionPreset conversionPreset in presetsToImport)
                {
                    // Auto-register any custom/new input extensions into Custom category
                    if (conversionPreset.InputTypes != null)
                    {
                        foreach (string inputType in conversionPreset.InputTypes)
                        {
                            if (!Helpers.CompatibleInputExtensions.Contains(inputType, StringComparer.OrdinalIgnoreCase) &&
                                !this.Settings.CustomInputExtensions.Contains(inputType, StringComparer.OrdinalIgnoreCase))
                            {
                                this.Settings.CustomInputExtensions.Add(inputType);
                                InputExtensionCategory customCategory = this.inputCategories.FirstOrDefault(match => match.Name == Helpers.InputCategoryNames.Custom);
                                if (customCategory != null)
                                {
                                    customCategory.AddExtension(inputType, isCustom: true);
                                }
                            }
                        }
                    }

                    PresetFolderNode parent = this.PresetsRootFolder;
                    foreach (string folderName in conversionPreset.ParentFoldersNames)
                    {
                        PresetFolderNode folderNode = parent.Children.FirstOrDefault(match => match is PresetFolderNode && match.Name == folderName) as PresetFolderNode;
                        if (folderNode == null)
                        {
                            folderNode = this.CreateFolderNode(folderName, parent);

                            if (!itemSelected)
                            {
                                this.SelectedItem = folderNode;
                                itemSelected = true;
                            }
                        }

                        parent = folderNode;
                    }

                    PresetNode node = this.CreatePresetNode(conversionPreset, parent);
                    if (!itemSelected)
                    {
                        this.SelectedItem = node;
                        itemSelected = true;
                    }
                }
            }
        }

        private void ExportSelectedPreset()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Export selected preset or folder",
                Filter = "Preset file (*.xml)|*.xml",
                InitialDirectory = this.ImportDirectoryPath,
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    this.ImportDirectoryPath = directoryPath;
                }

                if (Path.GetExtension(filePath) != ".xml")
                {
                    filePath += ".xml";
                }

                this.settings.ConversionPresets.Clear();
                this.ComputePresetsParentFoldersNamesAndFillSettings(this.presetsRootFolder, new List<string>());

                List<ConversionPreset> presetsToExport = new List<ConversionPreset>();
                this.FillWithPresetsRecursively(this.SelectedItem, presetsToExport);

                XmlHelpers.SaveToFile("Presets", filePath, presetsToExport);
            }
        }

        private bool CanExportSelectedPreset()
        {
            return this.SelectedItem != null;
        }

        private void RemoveSelectedPreset()
        {
            this.SelectedItem.PropertyChanged -= this.NodePropertyChanged;

            this.SelectedItem.Parent.Children.Remove(this.SelectedItem);

            this.SelectedItem = null;

            this.removePresetCommand.NotifyCanExecuteChanged();
            this.saveCommand.NotifyCanExecuteChanged();
        }

        private bool CanRemoveSelectedPreset()
        {
            return this.SelectedItem != null;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.UnbindNode(this.presetsRootFolder);
        }

        private void UnbindNode(AbstractTreeNode node)
        {
            node.PropertyChanged -= this.NodePropertyChanged;

            if (node is PresetFolderNode folder)
            {
                foreach (AbstractTreeNode child in folder.Children)
                {
                    this.UnbindNode(child);
                }
            }
        }

        private void FillWithPresetsRecursively(AbstractTreeNode node, List<ConversionPreset> presets)
        {
            if (node is PresetNode presetNode)
            {
                presets.Add(presetNode.Preset);
            }
            else if (node is PresetFolderNode folder)
            {
                foreach (AbstractTreeNode childNode in folder.Children)
                {
                    this.FillWithPresetsRecursively(childNode, presets);
                }
            }
        }

        private PresetFolderNode CreateFolderNode(string folderName, PresetFolderNode parent)
        {
            PresetFolderNode subFolder = new PresetFolderNode(folderName, parent);
            parent.Children.Add(subFolder);

            subFolder.PropertyChanged += this.NodePropertyChanged;
            return subFolder;
        }

        private PresetNode CreatePresetNode(ConversionPreset preset, PresetFolderNode parent)
        {
            PresetNode presetNode = new PresetNode(preset, parent);
            parent.Children.Add(presetNode);

            presetNode.PropertyChanged += this.NodePropertyChanged;
            return presetNode;
        }

        private void BrowseCustomExecutable()
        {
            if (this.SelectedPreset?.Preset == null)
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = Properties.Resources.CustomExecutablePath,
                Filter = "Executable files (*.exe;*.bat;*.cmd)|*.exe;*.bat;*.cmd|All files (*.*)|*.*",
                CheckFileExists = true
            };

            string initialPath = this.SelectedPreset.CustomExecutablePath;
            if (!string.IsNullOrEmpty(initialPath) && File.Exists(initialPath))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(initialPath);
                dialog.FileName = Path.GetFileName(initialPath);
            }
            else
            {
                string defaultPath = this.SelectedPreset.Preset.DefaultExecutablePath;
                if (!string.IsNullOrEmpty(defaultPath) && File.Exists(defaultPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(defaultPath);
                    dialog.FileName = Path.GetFileName(defaultPath);
                }
            }

            if (dialog.ShowDialog() == true)
            {
                this.SelectedPreset.CustomExecutablePath = dialog.FileName;
            }
        }

        private void ResetCustomExecutable()
        {
            if (this.SelectedPreset != null)
            {
                this.SelectedPreset.CustomExecutablePath = string.Empty;
            }
        }

        private void InsertArgumentTag(string tag)
        {
            if (this.SelectedPreset?.Preset == null || string.IsNullOrEmpty(tag))
            {
                return;
            }

            string current = this.SelectedPreset.Preset.CustomArgumentsTemplate ?? string.Empty;
            if (string.IsNullOrWhiteSpace(current))
            {
                this.SelectedPreset.Preset.CustomArgumentsTemplate = tag;
            }
            else
            {
                this.SelectedPreset.Preset.CustomArgumentsTemplate = current + " " + tag;
            }
        }

        private void ApplyCustomQuickTemplate(string templateType)
        {
            if (this.SelectedPreset?.Preset == null)
            {
                return;
            }

            switch (templateType)
            {
                case "Pandoc":
                    this.SelectedPreset.Preset.CustomArgumentsTemplate = "\"{input}\" -o \"{output}\"";
                    this.SelectedPreset.Preset.CustomOutputExtension = "pdf";
                    break;

                case "Default":
                default:
                    this.SelectedPreset.Preset.CustomArgumentsTemplate = "\"{input}\" -o \"{output}\"";
                    if (string.IsNullOrEmpty(this.SelectedPreset.Preset.CustomOutputExtension))
                    {
                        this.SelectedPreset.Preset.CustomOutputExtension = "out";
                    }
                    break;
            }
        }

        private void AddCustomInputExtensionAction()
        {
            string ext = this.NewCustomInputExtension?.Trim().TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
            {
                return;
            }

            // Add to Settings.CustomInputExtensions
            if (!this.Settings.CustomInputExtensions.Contains(ext))
            {
                this.Settings.CustomInputExtensions.Add(ext);
            }

            // Find or create "Custom" category in inputCategories
            List<InputExtensionCategory> categories = new List<InputExtensionCategory>(this.inputCategories);
            InputExtensionCategory customCategory = categories.Find(match => match.Name == Helpers.InputCategoryNames.Custom);
            if (customCategory == null)
            {
                customCategory = new InputExtensionCategory(Helpers.InputCategoryNames.Custom);
                categories.Add(customCategory);
                this.inputCategories = categories.ToArray();
                this.OnPropertyChanged(nameof(this.InputCategories));
            }

            customCategory.AddExtension(ext, true);

            // Automatically check this extension for currently selected preset
            if (this.SelectedPreset != null)
            {
                this.SelectedPreset.Preset.AddInputType(ext);
                InputExtension item = customCategory.InputExtensions.FirstOrDefault(e => e.Name == ext);
                item?.OnCategoryChanged();
            }

            this.NewCustomInputExtension = string.Empty;
        }

        private void RemoveCustomInputExtensionAction(string ext)
        {
            if (string.IsNullOrEmpty(ext))
            {
                return;
            }

            // Remove from Settings.CustomInputExtensions
            this.Settings.CustomInputExtensions.Remove(ext);

            // Remove from all presets
            foreach (ConversionPreset preset in this.Settings.ConversionPresets)
            {
                preset.RemoveInputType(ext);
            }

            // Remove from "Custom" category
            InputExtensionCategory customCategory = this.inputCategories.FirstOrDefault(match => match.Name == Helpers.InputCategoryNames.Custom);
            if (customCategory != null)
            {
                customCategory.RemoveExtension(ext);
            }
        }

        private void OpenPresetsFolderAction()
        {
            try
            {
                string presetsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
                if (!Directory.Exists(presetsDir))
                {
                    Directory.CreateDirectory(presetsDir);
                }

                Process.Start("explorer.exe", presetsDir);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open presets folder: {ex.Message}");
            }
        }

        private void OpenSettingsFolderAction()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string defaultXml = Path.Combine(baseDir, "Settings.default.xml");

                if (File.Exists(defaultXml))
                {
                    Process.Start("explorer.exe", $"/select,\"{defaultXml}\"");
                }
                else
                {
                    Process.Start("explorer.exe", baseDir);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open settings folder: {ex.Message}");
            }
        }
    }
}
