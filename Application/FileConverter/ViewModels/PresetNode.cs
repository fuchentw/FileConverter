// <copyright file="PresetNode.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter.ViewModels
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;

    using CommunityToolkit.Mvvm.ComponentModel;

    public abstract class AbstractTreeNode : ObservableObject, IDataErrorInfo
    {
        private PresetFolderNode parent;

        protected AbstractTreeNode(PresetFolderNode parent)
        {
            this.Parent = parent;
        }

#if DEBUG
        protected AbstractTreeNode()
        {
        }
#endif

        public PresetFolderNode Parent
        {
            get => this.parent;
            set
            {
                this.parent = value;
                this.OnPropertyChanged();
            }
        }

        public abstract string Name
        {
            get;
            set;
        }

        public string this[string columnName] => this.Validate(columnName);

        public bool HasError => !string.IsNullOrEmpty(this.Error);

        public virtual string Error
        {
            get
            {
                if (this.Parent == null)
                {
                    // This is the root folder. Don't check rules on this specific folder.
                    return string.Empty;
                }

                string errorString = this.Validate("Name");
                if (!string.IsNullOrEmpty(errorString))
                {
                    return errorString;
                }

                errorString = this.Validate("OutputFileNameTemplate");
                if (!string.IsNullOrEmpty(errorString))
                {
                    return errorString;
                }

                return string.Empty;
            }
        }

        protected virtual string Validate(string propertyName)
        {
            // Return error message if there is an error, else return empty or null string.
            switch (propertyName)
            {
                case "Name":
                {
                    if (string.IsNullOrEmpty(this.Name))
                    {
                        return "The preset name can't be empty.";
                    }

                    if (this.Name.Contains(";"))
                    {
                        return "The preset name can't contains the character ';'.";
                    }

                    if (this.Name.Contains("/"))
                    {
                        return "The preset name can't contains the character '/'.";
                    }

                    if (this.Parent != null)
                    {
                        int count = this.Parent.Children.Count(node => node.Name == this.Name);
                        if (count > 1)
                        {
                            return "The preset name is already used.";
                        }
                    }
                }

                break;
            }

            return string.Empty;
        }
    }

    public class PresetNode : AbstractTreeNode
    {
        public PresetNode(ConversionPreset preset, PresetFolderNode parent) : base(parent)
        {
            this.Preset = preset;
        }

#if DEBUG
        public PresetNode() : base()
        {
        }
#endif

        public ConversionPreset Preset
        {
            get;
#if DEBUG
            set;
#endif
        }

        public override string Name
        {
            get => this.Preset.ShortName;

            set
            {
                this.Preset.ShortName = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.HasError));
            }
        }

        public string OutputFileNameTemplate
        {
            get => this.Preset.OutputFileNameTemplate;

            set
            {
                this.Preset.OutputFileNameTemplate = value;
                this.OnPropertyChanged();
            }
        }

        public override string Error
        {
            get
            {
                string baseError = base.Error;
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                if (this.Preset != null && this.Preset.OutputType == OutputType.Custom)
                {
                    string err = this.Validate("CustomExecutablePath");
                    if (!string.IsNullOrEmpty(err)) return err;

                    err = this.Validate("CustomOutputExtension");
                    if (!string.IsNullOrEmpty(err)) return err;

                    err = this.Validate("CustomArgumentsTemplate");
                    if (!string.IsNullOrEmpty(err)) return err;
                }

                return string.Empty;
            }
        }

        public string CustomExecutablePath
        {
            get
            {
                if (this.Preset == null)
                {
                    return string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(this.Preset.CustomExecutablePath))
                {
                    return this.Preset.CustomExecutablePath;
                }

                if (this.Preset.OutputType != OutputType.Custom)
                {
                    return this.Preset.DefaultExecutablePath;
                }

                return string.Empty;
            }
            set
            {
                if (this.Preset != null)
                {
                    if (string.IsNullOrWhiteSpace(value) ||
                        (this.Preset.OutputType != OutputType.Custom && string.Equals(value.Trim(), this.Preset.DefaultExecutablePath, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        this.Preset.CustomExecutablePath = null;
                    }
                    else
                    {
                        this.Preset.CustomExecutablePath = value.Trim();
                    }
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.HasError));
                    this.OnPropertyChanged(nameof(this.IsCustomExecutablePathCustomized));
                }
            }
        }

        public bool IsCustomExecutablePathCustomized
        {
            get
            {
                if (this.Preset == null || this.Preset.OutputType == OutputType.Custom)
                {
                    return false;
                }
                return !string.IsNullOrWhiteSpace(this.Preset.CustomExecutablePath) &&
                       !string.Equals(this.Preset.CustomExecutablePath, this.Preset.DefaultExecutablePath, System.StringComparison.OrdinalIgnoreCase);
            }
        }

        public string CustomArgumentsTemplate
        {
            get => this.Preset?.CustomArgumentsTemplate;
            set
            {
                if (this.Preset != null)
                {
                    this.Preset.CustomArgumentsTemplate = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.HasError));
                }
            }
        }

        public string CustomOutputExtension
        {
            get => this.Preset?.CustomOutputExtension;
            set
            {
                if (this.Preset != null)
                {
                    this.Preset.CustomOutputExtension = value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.HasError));
                }
            }
        }

        protected override string Validate(string propertyName)
        {
            string error = base.Validate(propertyName);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            // Return error message if there is an error, else return empty or null string.
            switch (propertyName)
            {
                case "CustomExecutablePath":
                    if (this.Preset != null)
                    {
                        if (this.Preset.OutputType == OutputType.Custom)
                        {
                            if (string.IsNullOrWhiteSpace(this.CustomExecutablePath))
                            {
                                return "Please specify the custom executable path.";
                            }

                            string expanded = System.Environment.ExpandEnvironmentVariables(this.CustomExecutablePath);
                            if (!System.IO.File.Exists(expanded))
                            {
                                return "The specified executable file does not exist.";
                            }
                        }
                        else if (this.IsCustomExecutablePathCustomized)
                        {
                            string expanded = System.Environment.ExpandEnvironmentVariables(this.CustomExecutablePath);
                            if (!System.IO.File.Exists(expanded) && !System.IO.Directory.Exists(expanded))
                            {
                                return "The specified executable file does not exist.";
                            }
                        }
                    }
                    break;

                case "CustomOutputExtension":
                    if (this.Preset != null && this.Preset.OutputType == OutputType.Custom)
                    {
                        if (string.IsNullOrWhiteSpace(this.Preset.CustomOutputExtension))
                        {
                            return "Please specify the custom output extension.";
                        }
                    }
                    break;

                case "CustomArgumentsTemplate":
                    if (this.Preset != null && this.Preset.OutputType == OutputType.Custom)
                    {
                        if (string.IsNullOrWhiteSpace(this.Preset.CustomArgumentsTemplate))
                        {
                            return "The arguments template cannot be empty.";
                        }

                        if (!this.Preset.CustomArgumentsTemplate.Contains("{input}") || !this.Preset.CustomArgumentsTemplate.Contains("{output}"))
                        {
                            return "The arguments template must contain both {input} and {output}.";
                        }
                    }
                    break;

                case "OutputFileNameTemplate":
                    {
                        string sampleOutputFilePath = this.Preset.GenerateOutputFilePath(FileConverter.Properties.Resources.OutputFileNameTemplateSample, 1, 3);
                        if (string.IsNullOrEmpty(sampleOutputFilePath))
                        {
                            return "The output filename template must produce a non empty result.";
                        }

                        if (!PathHelpers.IsPathValid(sampleOutputFilePath))
                        {
                            // Diagnostic to feedback purpose.
                            // Drive letter.
                            if (!PathHelpers.IsPathDriveLetterValid(sampleOutputFilePath))
                            {
                                return "The output filename template must define a root (for example c:\\, use (p) to use the input file path).";
                            }

                            // File name.
                            string filename = PathHelpers.GetFileName(sampleOutputFilePath);
                            if (filename == null)
                            {
                                return "The output file name must not be empty (use (f) to use the name of the input file).";
                            }

                            char[] invalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
                            for (int index = 0; index < invalidFileNameChars.Length; index++)
                            {
                                if (filename.Contains(invalidFileNameChars[index]))
                                {
                                    return "The output file name must not contains the character '" + invalidFileNameChars[index] + "'.";
                                }
                            }

                            // Directory names.
                            string path = sampleOutputFilePath.Substring(3, sampleOutputFilePath.Length - 3 - filename.Length);
                            char[] invalidPathChars = System.IO.Path.GetInvalidPathChars();
                            for (int index = 0; index < invalidPathChars.Length; index++)
                            {
                                if (string.IsNullOrEmpty(path))
                                {
                                    return "The output directory name must not be empty (use (d0), (d1), ... to use the name of the parent directories of the input file).";
                                }

                                if (path.Contains(invalidPathChars[index]))
                                {
                                    return "The output directory name must not contains the character '" + invalidPathChars[index] + "'.";
                                }
                            }

                            string[] directories = path.Split('\\');
                            for (int index = 0; index < directories.Length; ++index)
                            {
                                string directoryName = directories[index];
                                if (string.IsNullOrEmpty(directoryName))
                                {
                                    return "The output directory name must not be empty (use (d0), (d1), ... to use the name of the parent directories of the input file).";
                                }
                            }

                            return "The output filename template is invalid";
                        }
                    }

                    break;
            }

            return string.Empty;
        }
    }

    public class PresetFolderNode : AbstractTreeNode
    {
        private string name;
        private ObservableCollection<AbstractTreeNode> children = new ObservableCollection<AbstractTreeNode>();

        public PresetFolderNode(string name, PresetFolderNode parent) : base(parent)
        {
            this.Name = name;
        }

#if DEBUG
        public PresetFolderNode() : base()
        {
        }
#endif

        public override string Name
        {
            get => this.name;

            set
            {
                this.name = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.HasError));
            }
        }

        public ObservableCollection<AbstractTreeNode> Children
        {
            get => this.children;

            set
            {
                this.children = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsNodeInHierarchy(AbstractTreeNode node, bool recurse)
        {
            Diagnostics.Debug.Assert(node != null, "node != null");
            foreach (ObservableObject child in this.Children)
            {
                if (child == node)
                {
                    return true;
                }

                if (recurse && child is PresetFolderNode subFolder)
                {
                    if (subFolder.IsNodeInHierarchy(node, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
