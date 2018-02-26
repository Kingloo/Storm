using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Storm.Common;

namespace Storm.DataAccess
{
    public class TxtRepo
    {
        private readonly FileInfo urlsFile = null;
        private readonly char commentCharacter = Char.Parse("#");

        public TxtRepo(FileInfo urlsFile)
            : this(urlsFile, Char.Parse("#"))
        {
            this.urlsFile = urlsFile ?? throw new ArgumentNullException(nameof(urlsFile));
        }

        public TxtRepo(FileInfo urlsFile, char commentCharacter)
        {
            this.commentCharacter = commentCharacter;
        }
        
        public async Task<string[]> LoadAsync()
        {
            var lines = new List<string>();

            FileStream fsAsync = null;

            try
            {
                fsAsync = new FileStream(
                    urlsFile.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    4096,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

                using (StreamReader sr = new StreamReader(fsAsync))
                {
                    fsAsync = null;

                    string line = string.Empty;

                    while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        if (line.StartsWith(commentCharacter.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            continue;
                        }

                        lines.Add(line);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                Log.LogException(ex);

                return Array.Empty<string>();
            }
            finally
            {
                fsAsync?.Dispose();
            }

            return lines.ToArray();
        }

        public void OpenFile()
        {
            try
            {
                Process.Start("notepad.exe", urlsFile.FullName);
            }
            catch (FileNotFoundException ex)
            {
                Log.LogException(ex);

                Process.Start(urlsFile.FullName);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(GetType().ToString());
            sb.AppendLine(urlsFile?.FullName ?? "urlsFile is null");
            
            sb.Append("comment character: ");
            sb.AppendLine(commentCharacter.ToString());

            return sb.ToString();
        }
    }
}
