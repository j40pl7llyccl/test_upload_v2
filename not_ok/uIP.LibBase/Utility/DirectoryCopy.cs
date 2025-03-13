using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace uIP.LibBase.Utility
{
    public static class DirCpyUtility
    {
        public static bool Copy(string dst, string src)
        {
            if (String.IsNullOrEmpty(src) || !Directory.Exists(src))
                return false;
            if (String.IsNullOrEmpty(dst))
                return false;
            if (!CheckDirAvailable(dst, src))
                return false;

            bool bCreateDst = false;
            try
            {
                if (!Directory.Exists(dst))
                {
                    try
                    {
                        DirectoryInfo dirInfo = Directory.CreateDirectory(dst);
                        if (dirInfo == null)
                            bCreateDst = false;
                        else
                            bCreateDst = dirInfo.Exists;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                    bCreateDst = true;
            }
            catch
            {
                bCreateDst = false;
            }
            if (!bCreateDst)
                return false;

            return (RC(dst, src));
        }

        private static bool CheckDirAvailable(string dst, string src)
        {
            if (String.IsNullOrEmpty(dst) || String.IsNullOrEmpty(src))
                return false;

            string dst1 = dst.Replace('/', '\\');
            string src1 = src.Replace('/', '\\');

            dst1 = dst1.ToLower();
            src1 = src1.ToLower();

            // remove end symbol
            dst1 = dst1[dst1.Length - 1] == '\\' ? dst1.Substring(0, dst1.Length - 1) : dst1;
            src1 = src1[src1.Length - 1] == '\\' ? src1.Substring(0, src1.Length - 1) : src1;

            // src cannot equ to dst
            if (dst1 == src1)
                return false;


            // set check
            int index = dst1.IndexOf(src1, 0, StringComparison.Ordinal);
            if (index == 0)
            {
                // dst as subset of src
                if ((src1.Length + 1) <= dst1.Length && dst1[src1.Length] == '\\')
                    return false;
            }
            index = src1.IndexOf(dst1, 0, StringComparison.Ordinal);
            if (index == 0)
            {
                // src as subset of src
                if ((dst1.Length + 1) <= src1.Length && src1[dst1.Length] == '\\')
                    return false;
            }

            return true;
        }

        private static bool RC(string dst, string src)
        {
            if (String.IsNullOrEmpty(dst) || String.IsNullOrEmpty(src))
                return false;
            if (!Directory.Exists(src) || !Directory.Exists(dst))
                return false;

            // Copy current files in src dir to dst dir
            string[] src_files = Directory.GetFiles(src);
            if (src_files != null && src_files.Length > 0)
            {
                for (int i = 0; i < src_files.Length; i++)
                {
                    try
                    {
                        File.Copy(src_files[i], String.Format(@"{0}\{1}", dst, Path.GetFileName(src_files[i])));
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Recursive copying
            string[] src_subdir = Directory.GetDirectories(src);
            if (src_subdir != null && src_subdir.Length > 0)
            {
                for (int i = 0; i < src_subdir.Length; i++)
                {
                    // Create new one
                    string pathnew = String.Format(@"{0}\{1}", dst, Path.GetFileName(src_subdir[i]));
                    if (!Directory.Exists(pathnew))
                    {
                        try
                        {
                            Directory.CreateDirectory(pathnew);
                        }
                        catch
                        {
                            return false;
                        }
                    }

                    // call again
                    if (!RC(pathnew, src_subdir[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
