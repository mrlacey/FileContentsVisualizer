using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace PathDebugVisualizers.VSIX
{
    // Load in a generic way but try and avoid causing any startup perf hit
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PathDebugVisualizersVSIXPackage.PackageGuidString)]
    public sealed class PathDebugVisualizersVSIXPackage : AsyncPackage
    {
        public const string PackageGuidString = "dc39b4e3-68d5-4c92-80b0-9a40e4b381a4";

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            //await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            await TaskScheduler.Default;

            InstallVisualizer("PathDebugVisualizers.dll");
        }

        // Rather than a separate installer. See also https://learn.microsoft.com/en-us/visualstudio/debugger/how-to-install-a-visualizer?view=vs-2022
        // Based on https://github.com/visualstudioextensibility/VSX-Samples/tree/master/VSIXDebuggerVisualizer and https://github.com/atkulp/StringAsFilenameDebuggerVisualizer/blob/master/Solution/Package/MainPackage.cs
        private void InstallVisualizer(string assemblyName)
        {
            try
            {
                base.Initialize();

                // The Visualizer dll is in the same folder than the package because its project is added as reference to this project,
                // so it is included inside the .vsix file. We only need to deploy it to the correct destination folder.
                var sourceFolderFullName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

                // Get the destination folder for visualizers
                var shell = base.GetService(typeof(SVsShell)) as IVsShell;
                shell.GetProperty((int)__VSSPROPID2.VSSPROPID_VisualStudioDir, out object documentsFolderFullNameObject);
                var documentsFolderFullName = documentsFolderFullNameObject.ToString();
                var destinationFolderFullName = Path.Combine(documentsFolderFullName, "Visualizers");

                var sourceFileFullName = Path.Combine(sourceFolderFullName, assemblyName);
                var destinationFileFullName = Path.Combine(destinationFolderFullName, assemblyName);

                CopyFileIfNewerVersion(sourceFileFullName, destinationFileFullName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void CopyFileIfNewerVersion(string sourceFileFullName, string destinationFileFullName)
        {
            bool copy = false;

            if (File.Exists(destinationFileFullName))
            {
                FileVersionInfo sourceFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(sourceFileFullName);
                FileVersionInfo destinationFileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(destinationFileFullName);

                if (sourceFileVersionInfo.FileMajorPart > destinationFileVersionInfo.FileMajorPart)
                {
                    copy = true;
                }
                else if (sourceFileVersionInfo.FileMajorPart == destinationFileVersionInfo.FileMajorPart
                     && sourceFileVersionInfo.FileMinorPart > destinationFileVersionInfo.FileMinorPart)
                {
                    copy = true;
                }
            }
            else
            {
                // First time
                copy = true;
            }

            if (copy)
            {
                File.Copy(sourceFileFullName, destinationFileFullName, true);
            }
        }
    }
}
