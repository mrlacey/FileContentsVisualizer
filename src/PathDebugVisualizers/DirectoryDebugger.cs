using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.DebuggerVisualizers;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



[assembly: DebuggerVisualizer(
    typeof(PathDebugVisualizers.DirectoryDebugger),
    typeof(VisualizerObjectSource),
    Target = typeof(string),
    Description = "Open if directory")]

namespace PathDebugVisualizers
{
    public class DirectoryDebugger : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var str = objectProvider.GetObject().ToString();

            if (File.Exists(str))
            {
                Open(Path.GetDirectoryName(str));
            }
            else if (Directory.Exists(str))
            {
                Open(str);
            }
            else
            {
                var errMsg = $"Not a recognized path: {str}";

               // var dte = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

               // dte.StatusBar.Text = errMsg;

                MessageBox.Show(errMsg, "Open if Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Open(string path)
        {
            var myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo.FileName = path;
            myProcess.Start();
        }
    }
}
