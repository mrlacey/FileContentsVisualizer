using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.DebuggerVisualizers;

[assembly: DebuggerVisualizer(
    typeof(PathDebugVisualizers.FileDebugger),
    typeof(VisualizerObjectSource),
    Target = typeof(string),
    Description = "Open if file")]

namespace PathDebugVisualizers
{
    public class FileDebugger : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            var str = objectProvider.GetObject().ToString();

            if (File.Exists(str))
            {
                Open(str);
            }
            else
            {
                var dir = Path.GetDirectoryName(str);

                if (Directory.Exists(dir))
                {
                    Open(dir);
                }
                else
                {
                    MessageBox.Show($"Not a recognized file path: {str}", "Open if Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void Open(string path)
        {
            var myProcess = new Process();
            myProcess.StartInfo.FileName = path;
            myProcess.Start();
        }
    }
}
