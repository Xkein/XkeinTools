using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xkein.Helpers
{
    public static class FileSystemHelpers
    {
        public static void PrepareDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (dir != string.Empty)
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
