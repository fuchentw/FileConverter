// <copyright file="ConversionJob_CustomCli.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverter.ConversionJobs
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Debug = FileConverter.Diagnostics.Debug;

    public class ConversionJob_CustomCli : ConversionJob
    {
        private Process exeProcess;

        public ConversionJob_CustomCli() : base()
        {
        }

        public ConversionJob_CustomCli(ConversionPreset conversionPreset, string inputFilePath) : base(conversionPreset, inputFilePath)
        {
        }

        protected override bool IsCancelable() => this.State == ConversionState.InProgress;

        protected override void Initialize()
        {
            base.Initialize();

            if (this.ConversionPreset == null)
            {
                throw new Exception("The conversion preset must be valid.");
            }

            string executablePath = this.ConversionPreset.CustomExecutablePath;
            if (string.IsNullOrEmpty(executablePath))
            {
                this.ConversionFailed("Custom executable path is not specified.");
                return;
            }

            executablePath = Environment.ExpandEnvironmentVariables(executablePath);

            if (!Path.IsPathRooted(executablePath))
            {
                string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(applicationDirectory))
                {
                    string candidatePath = Path.Combine(applicationDirectory, executablePath);
                    if (File.Exists(candidatePath))
                    {
                        executablePath = candidatePath;
                    }
                }
            }

            if (!File.Exists(executablePath))
            {
                this.ConversionFailed($"Can't find custom executable ({executablePath}).");
                Debug.LogError($"Can't find custom executable ({executablePath}).");
                return;
            }
        }

        protected override void Convert()
        {
            if (this.ConversionPreset == null)
            {
                throw new Exception("The conversion preset must be valid.");
            }

            string executablePath = Environment.ExpandEnvironmentVariables(this.ConversionPreset.CustomExecutablePath);
            if (!Path.IsPathRooted(executablePath))
            {
                string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(applicationDirectory))
                {
                    string candidatePath = Path.Combine(applicationDirectory, executablePath);
                    if (File.Exists(candidatePath))
                    {
                        executablePath = candidatePath;
                    }
                }
            }

            if (!File.Exists(executablePath))
            {
                this.ConversionFailed($"Can't find custom executable ({executablePath}).");
                return;
            }

            string template = this.ConversionPreset.CustomArgumentsTemplate;
            if (string.IsNullOrEmpty(template))
            {
                template = "\"{input}\" -o \"{output}\"";
            }

            string inputDirectory = Path.GetDirectoryName(this.InputFilePath) ?? string.Empty;
            string inputFileNameNoExtension = Path.GetFileNameWithoutExtension(this.InputFilePath);
            string outputDirectory = Path.GetDirectoryName(this.OutputFilePath) ?? string.Empty;
            string outputFileNameNoExtension = Path.GetFileNameWithoutExtension(this.OutputFilePath);

            string arguments = template
                .Replace("{input}", this.InputFilePath)
                .Replace("{output}", this.OutputFilePath)
                .Replace("{inputDir}", inputDirectory)
                .Replace("{inputFileName}", inputFileNameNoExtension)
                .Replace("{outputDir}", outputDirectory)
                .Replace("{outputFileName}", outputFileNameNoExtension);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executablePath, arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = !string.IsNullOrEmpty(inputDirectory) && Directory.Exists(inputDirectory) ? inputDirectory : AppDomain.CurrentDomain.BaseDirectory
            };

            this.UserState = Properties.Resources.ConversionStateConversion;
            Debug.Log($"Execute custom command: {processStartInfo.FileName} {processStartInfo.Arguments}");

            try
            {
                using (this.exeProcess = Process.Start(processStartInfo))
                {
                    StringBuilder outputBuilder = new StringBuilder();
                    StringBuilder errorBuilder = new StringBuilder();

                    this.exeProcess.OutputDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            outputBuilder.AppendLine(args.Data);
                            Debug.Log($"[CLI Output] {args.Data}");
                        }
                    };

                    this.exeProcess.ErrorDataReceived += (sender, args) =>
                    {
                        if (args.Data != null)
                        {
                            errorBuilder.AppendLine(args.Data);
                            Debug.Log($"[CLI Error] {args.Data}");
                        }
                    };

                    this.exeProcess.BeginOutputReadLine();
                    this.exeProcess.BeginErrorReadLine();

                    while (!this.exeProcess.WaitForExit(100))
                    {
                        if (this.CancelIsRequested && !this.exeProcess.HasExited)
                        {
                            try
                            {
                                this.exeProcess.Kill();
                            }
                            catch
                            {
                            }

                            break;
                        }
                    }

                    if (this.CancelIsRequested)
                    {
                        this.ConversionFailed(Properties.Resources.ErrorCanceled);
                        return;
                    }

                    if (this.exeProcess.ExitCode != 0)
                    {
                        string errorMessage = errorBuilder.ToString().Trim();
                        if (string.IsNullOrEmpty(errorMessage))
                        {
                            errorMessage = outputBuilder.ToString().Trim();
                        }

                        if (string.IsNullOrEmpty(errorMessage))
                        {
                            errorMessage = $"Process exited with code {this.exeProcess.ExitCode}";
                        }

                        this.ConversionFailed(errorMessage);
                    }
                }
            }
            catch (Exception exception)
            {
                this.ConversionFailed(exception.Message);
                throw;
            }
            finally
            {
                this.exeProcess = null;
            }
        }

        public override void Cancel()
        {
            base.Cancel();

            try
            {
                if (this.exeProcess != null && !this.exeProcess.HasExited)
                {
                    this.exeProcess.Kill();
                }
            }
            catch
            {
            }
        }
    }
}
