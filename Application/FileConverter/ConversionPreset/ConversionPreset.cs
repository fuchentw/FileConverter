// <copyright file="ConversionPreset.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml.Serialization;
    
    using CommunityToolkit.Mvvm.ComponentModel;

    using FileConverter.Controls;

    [XmlRoot]
    [XmlType]
    public class ConversionPreset : ObservableObject, IXmlSerializable
    {
        private string shortName;

        private OutputType outputType;
        private List<string> inputTypes;
        private InputPostConversionAction inputPostConversionAction;
        private ConversionSettings settings = new ConversionSettings();
        private string outputFileNameTemplate;
        private string customExecutablePath;
        private string customArgumentsTemplate;
        private string customOutputExtension;

        public ConversionPreset()
        {
            this.FullName = Properties.Resources.DefaultPresetName;
        }

        public ConversionPreset(string shortName, OutputType outputType, params string[] inputTypes)
        {
            this.ShortName = shortName;
            this.OutputType = outputType;
            List<string> inputTypeList = new List<string>();
            inputTypeList.AddRange(inputTypes);
            this.InputTypes = inputTypeList;

            this.outputFileNameTemplate = "(p)(f)";
        }

        public ConversionPreset(string shortName, ConversionPreset source, params string[] additionalInputTypes)
        {
            this.ShortName = shortName;
            this.ParentFoldersNames = source.ParentFoldersNames;
            this.OutputType = source.outputType;
            this.customExecutablePath = source.customExecutablePath;
            this.customArgumentsTemplate = source.customArgumentsTemplate;
            this.customOutputExtension = source.customOutputExtension;
            List<string> inputTypeList = new List<string>();
            if (source.inputTypes != null)
            {
                inputTypeList.AddRange(source.inputTypes);
            }

            if (additionalInputTypes != null)
            {
                inputTypeList.AddRange(additionalInputTypes);
            }

            this.InputTypes = inputTypeList;

            this.outputFileNameTemplate = source.OutputFileNameTemplate;

            foreach (KeyValuePair<string, string> conversionSetting in source.settings)
            {
                this.SetSettingsValue(conversionSetting.Key, conversionSetting.Value);
            }
        }

        [XmlAttribute("Name")]
        public string FullName
        {
            get
            {
                string fullName = string.Empty;
                if (this.ParentFoldersNames != null)
                {
                    foreach (string folder in this.ParentFoldersNames)
                    {
                        fullName += $"{folder}/";
                    }
                }

                fullName += this.shortName;

                return fullName;
            }

            set
            {
                string[] folders = value.Split('/');
                if (folders.Length == 0)
                {
                    Diagnostics.Debug.Log("Invalid full name.");
                    this.ShortName = value;
                    return;
                }

                this.ShortName = folders[folders.Length - 1];
                Array.Resize(ref folders, folders.Length - 1);

                this.ParentFoldersNames = folders;
            }
        }

        [XmlIgnore]
        public string ShortName
        {
            get => this.shortName;

            set
            {
                this.shortName = value;
                this.OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public string[] ParentFoldersNames
        {
            get;
            set;
        }

        [XmlAttribute]
        public OutputType OutputType
        {
            get => this.outputType;

            set
            {
                this.outputType = value;
                this.InitializeDefaultSettings(this.outputType);
                this.OnPropertyChanged();
                this.CoerceInputTypes();
            }
        }

        [XmlAttribute]
        public bool IsDefaultSettings
        {
            get;
            set;
        }

        [XmlElement]
        public List<string> InputTypes
        {
            get
            {
                return this.inputTypes;
            }

            set
            {
                this.inputTypes = value;
                for (int index = 0; index < this.inputTypes.Count; index++)
                {
                    this.inputTypes[index] = this.inputTypes[index].ToLowerInvariant();
                }
                
                this.OnPropertyChanged();
            }
        }

        [XmlElement]
        public InputPostConversionAction InputPostConversionAction
        {
            get
            {
                return this.inputPostConversionAction;
            }

            set
            {
                this.inputPostConversionAction = value;
                this.OnPropertyChanged();
            }
        }

        [XmlElement("Settings")]
        public ConversionSetting[] XmlSerializableSettings
        {
            get
            {
                int index = 0;
                ConversionSetting[] settings = new ConversionSetting[this.settings.Count];
                foreach (KeyValuePair<string, string> keyValuePair in this.settings)
                {
                    settings[index] = new ConversionSetting(keyValuePair);
                    index++;
                }

                return settings;
            }

            set
            {
                if (value != null)
                {
                    for (int index = 0; index < value.Length; index++)
                    {
                        // Compatibility issues.
                        if (value[index].Key == "Bitrate")
                        {
                            this.SetSettingsValue(ConversionSettingKeys.AudioBitrate, value[index].Value);
                            continue;
                        }

                        if (value[index].Key == "Encoding")
                        {
                            this.SetSettingsValue(ConversionSettingKeys.AudioEncodingMode, value[index].Value);
                            continue;
                        }

                        // Load settings.
                        this.SetSettingsValue(value[index].Key, value[index].Value);
                    }
                }

                this.OnPropertyChanged(nameof(this.Settings));
            }
        }

        [XmlElement]
        public string OutputFileNameTemplate
        {
            get
            {
                return this.outputFileNameTemplate;
            }

            set
            {
                this.outputFileNameTemplate = value;
                this.OnPropertyChanged();
            }
        }

        [XmlElement]
        public string CustomExecutablePath
        {
            get => this.customExecutablePath;
            set
            {
                this.customExecutablePath = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CommandPreview));
                this.OnPropertyChanged(nameof(this.IsCustomExecutablePathValid));
            }
        }

        [XmlElement]
        public string CustomArgumentsTemplate
        {
            get => this.customArgumentsTemplate;
            set
            {
                this.customArgumentsTemplate = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CommandPreview));
            }
        }

        [XmlElement]
        public string CustomOutputExtension
        {
            get => this.customOutputExtension;
            set
            {
                this.customOutputExtension = value != null ? value.Trim().TrimStart('.') : null;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CommandPreview));
            }
        }

        public static string GetDefaultExecutablePath(OutputType outputType)
        {
            string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            switch (outputType)
            {
                case OutputType.Aac:
                case OutputType.Avi:
                case OutputType.Flac:
                case OutputType.Gif:
                case OutputType.Mkv:
                case OutputType.Mp3:
                case OutputType.Mp4:
                case OutputType.Ogg:
                case OutputType.Ogv:
                case OutputType.Wav:
                case OutputType.Webm:
                    return System.IO.Path.Combine(appDir, "ffmpeg.exe");

                case OutputType.Avif:
                case OutputType.Jpg:
                case OutputType.Pdf:
                case OutputType.Png:
                case OutputType.Webp:
                case OutputType.Ico:
                    return System.IO.Path.Combine(appDir, "gswin64c.exe");

                default:
                    return string.Empty;
            }
        }

        [XmlIgnore]
        public string DefaultExecutablePath => GetDefaultExecutablePath(this.OutputType);

        [XmlIgnore]
        public bool IsCustomExecutablePathValid
        {
            get
            {
                string targetPath = !string.IsNullOrWhiteSpace(this.customExecutablePath)
                    ? this.customExecutablePath
                    : (this.OutputType != OutputType.Custom ? this.DefaultExecutablePath : null);

                if (string.IsNullOrWhiteSpace(targetPath))
                {
                    return false;
                }
                string expanded = Environment.ExpandEnvironmentVariables(targetPath);
                return System.IO.File.Exists(expanded) || System.IO.Directory.Exists(expanded);
            }
        }

        [XmlIgnore]
        public string CommandPreview
        {
            get
            {
                string exe = !string.IsNullOrWhiteSpace(this.customExecutablePath)
                    ? $"\"{this.customExecutablePath}\""
                    : "\"{ExecutablePath}\"";

                string ext = !string.IsNullOrWhiteSpace(this.customOutputExtension)
                    ? this.customOutputExtension
                    : "out";

                string sampleInput = @"C:\Sample\Document.docx";
                string sampleOutput = $@"C:\Sample\Document.{ext}";
                string sampleInputDir = @"C:\Sample";
                string sampleOutputDir = @"C:\Sample";
                string sampleInputFileName = "Document";
                string sampleOutputFileName = "Document";

                string template = !string.IsNullOrWhiteSpace(this.customArgumentsTemplate)
                    ? this.customArgumentsTemplate
                    : "\"{input}\" -o \"{output}\"";

                string args = template
                    .Replace("{input}", sampleInput)
                    .Replace("{output}", sampleOutput)
                    .Replace("{inputDir}", sampleInputDir)
                    .Replace("{outputDir}", sampleOutputDir)
                    .Replace("{inputFileName}", sampleInputFileName)
                    .Replace("{outputFileName}", sampleOutputFileName);

                return $"> {exe} {args}";
            }
        }

        [XmlIgnore]
        public string ConversionArchiveFolderName
        {
            get
            {
                return Properties.Resources.ConversionArchives;
            }
        }

        [XmlIgnore]
        public IConversionSettings Settings
        {
            get => this.settings;

            set
            {
                if (value == null)
                {
                    return;
                }

                foreach (KeyValuePair<string, string> conversionSetting in value)
                {
                    if (!this.settings.ContainsKey(conversionSetting.Key))
                    {
                        this.settings.Add(conversionSetting.Key, conversionSetting.Value);
                    }
                    else
                    {
                        this.settings[conversionSetting.Key] = conversionSetting.Value;
                    }
                }

                this.OnPropertyChanged();
            }
        }

        public void Clean()
        {
            // Remove unrelevant settings.
            List<string> settingsToRemove = new List<string>();
            foreach (string settingsKey in this.settings.Keys)
            {
                if (!this.IsRelevantSetting(settingsKey))
                {
                    settingsToRemove.Add(settingsKey);
                }
            }

            for (int index = 0; index < settingsToRemove.Count; index++)
            {
                this.settings.Remove(settingsToRemove[index]);
            }
        }

        public void OnDeserializationComplete()
        {
            for (int index = 0; index < this.InputTypes.Count; index++)
            {
                this.InputTypes[index] = this.InputTypes[index].ToLowerInvariant();
            }

            this.CoerceInputTypes();
        }

        public void AddInputType(string inputType)
        {
            if (this.inputTypes.Contains(inputType))
            {
                return;
            }

            this.inputTypes.Add(inputType);
            this.OnPropertyChanged(nameof(this.InputTypes));
        }

        public void RemoveInputType(string inputType)
        {
            if (this.inputTypes.Remove(inputType))
            {
                this.OnPropertyChanged(nameof(this.InputTypes));
            }
        }

        public string GenerateOutputFilePath(string inputFilePath, int numberIndex, int numberMax)
        {
            string outputExtension = (this.OutputType == OutputType.Custom && !string.IsNullOrEmpty(this.CustomOutputExtension))
                ? this.CustomOutputExtension.TrimStart('.')
                : this.OutputType.ToString().ToLowerInvariant();

            return PathHelpers.GenerateFilePathFromTemplate(inputFilePath, outputExtension, this.OutputFileNameTemplate, numberIndex, numberMax);
        }

        public void SetSettingsValue(string settingsKey, string value)
        {
            if (string.IsNullOrEmpty(settingsKey))
            {
                throw new ArgumentNullException(nameof(settingsKey));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!this.IsRelevantSetting(settingsKey))
            {
                return;
            }

            if (!this.settings.ContainsKey(settingsKey))
            {
                this.settings.Add(settingsKey, value);
            }

            this.settings[settingsKey] = value;

            this.OnPropertyChanged(nameof(this.Settings));
        }

        public string GetSettingsValue(string settingsKey)
        {
            if (string.IsNullOrEmpty(settingsKey))
            {
                throw new ArgumentNullException(nameof(settingsKey));
            }

            if (this.settings.ContainsKey(settingsKey))
            {
                return this.settings[settingsKey];
            }

            return null;
        }

        public T GetSettingsValue<T>(string settingsKey)
        {
            string settingsValue = this.GetSettingsValue(settingsKey);

            if (settingsValue == null)
            {
                return default(T);
            }

            Type type = typeof(T);
            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, settingsValue);
            }

            return (T)Convert.ChangeType(settingsValue, type, NumberFormatInfo.InvariantInfo);
        }

        public bool IsRelevantSetting(string settingsKey)
        {
            return this.Settings.ContainsKey(settingsKey);
        }

        private void CoerceInputTypes()
        {
            if (this.inputTypes == null)
            {
                return;
            }

            for (int index = 0; index < this.inputTypes.Count; index++)
            {
                string inputType = this.inputTypes[index];
                string inputCategory = Helpers.GetExtensionCategory(inputType);
                if (!Helpers.IsOutputTypeCompatibleWithCategory(this.OutputType, inputCategory))
                {
                    this.RemoveInputType(inputType);
                    index--;
                }
            }
        }

        private void InitializeDefaultSettings(OutputType outputType)
        {
            this.settings.Clear();

            switch (outputType)
            {
                // Audio
                case OutputType.Aac:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "128");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioChannelCount, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Flac:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioChannelCount, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Ogg:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "160");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioChannelCount, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Mp3:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioEncodingMode, EncodingMode.Mp3VBR.ToString(), true);
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "190");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioChannelCount, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Wav:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioEncodingMode, EncodingMode.Wav16.ToString(), true);
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioChannelCount, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                // Video
                case OutputType.Avi:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableAudio, "True");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoQuality, "20");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "190");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Mkv:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableAudio, "True");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoQuality, "28");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoEncodingSpeed, "Medium");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "128");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Mp4:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableAudio, "True");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoQuality, "28");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoEncodingSpeed, "Medium");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "128");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Ogv:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableAudio, "True");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoQuality, "7");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "160");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                case OutputType.Webm:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableAudio, "True");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.AudioBitrate, "160");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoQuality, "40");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.EnableFFMPEGCustomCommand, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.FFMPEGCustomCommand, string.Empty);
                    break;

                // Images
                case OutputType.Avif:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageQuality, "50");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageClampSizePowerOf2, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageMaximumSize, "0");
                    break;

                case OutputType.Gif:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.VideoFramesPerSecond, "15");
                    break;

                case OutputType.Png:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageClampSizePowerOf2, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageMaximumSize, "0");
                    break;

                case OutputType.Jpg:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageQuality, "90");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageClampSizePowerOf2, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageMaximumSize, "0");
                    break;

                case OutputType.Webp:
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageQuality, "40");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageScale, "1");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageRotation, "0");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageClampSizePowerOf2, "False");
                    this.InitializeSettingsValue(ConversionPreset.ConversionSettingKeys.ImageMaximumSize, "0");
                    break;

                case OutputType.Ico:
                    break;

                // Documents
                case OutputType.Pdf:
                    break;

                case OutputType.Custom:
                    break;

                default:
                    throw new System.Exception("Missing default settings for type " + outputType);
            }

            this.OnPropertyChanged(nameof(this.Settings));
        }

        private void InitializeSettingsValue(string settingsKey, string value, bool force = false)
        {
            if (string.IsNullOrEmpty(settingsKey))
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!this.settings.ContainsKey(settingsKey))
            {
                this.settings.Add(settingsKey, value);
            }
            else if (force)
            {
                this.settings[settingsKey] = value;
            }
        }

        public struct ConversionSetting
        {
            public ConversionSetting(KeyValuePair<string, string> keyValuePair)
            {
                this.Key = keyValuePair.Key;
                this.Value = keyValuePair.Value;
            }

            [XmlAttribute]
            public string Key
            {
                get;
                set;
            }

            [XmlAttribute]
            public string Value
            {
                get;
                set;
            }
        }

        public struct ConversionSettingKeys
        {
            public const string AudioEncodingMode = "AudioEncodingMode";
            public const string AudioBitrate = "AudioBitrate";
            public const string AudioChannelCount = "AudioChannelCount";
            public const string ImageQuality = "ImageQuality";
            public const string ImageScale = "ImageScale";
            public const string ImageRotation = "ImageRotation";
            public const string ImageClampSizePowerOf2 = "ImageClampSizePowerOf2";
            public const string ImageMaximumSize = "ImageMaximumSize";
            public const string VideoQuality = "VideoQuality";
            public const string VideoEncodingSpeed = "VideoEncodingSpeed";
            public const string VideoScale = "VideoScale";
            public const string VideoRotation = "VideoRotation";
            public const string VideoFramesPerSecond = "VideoFramesPerSecond";
            public const string FFMPEGCustomCommand = "FFMPEGCustomCommand";

            public const string EnableAudio = "EnableAudio";
            public const string EnableVideo = "EnableVideo";
            public const string EnableFFMPEGCustomCommand = "EnableFFMPEGCustomCommand";
        }
    }
}
