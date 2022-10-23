using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordReplacer.Common
{
    public static class Helper
    {
        public static string SanitizeFileName(string file)
        {
            var charList = new List<char>
            {
                '\\',
                '/',
                '|',
                ':',
                '*',
                '?',
                '"',
                '<',
                '>'
            };

            foreach (var c in charList)
            {
                file = file.Replace(c, '_');
            }

            return file;
        }
    }
}
