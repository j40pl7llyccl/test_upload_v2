using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace uIP.LibBase
{
    #region Drawing Data Definitions
    [StructLayout( LayoutKind.Sequential )]
    public struct TRGB
    {
        public byte B;
        public byte G;
        public byte R;

        public static string ToDeclare()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( "[StructLayout( LayoutKind.Sequential )]\n" );
            sb.Append( "public struct TRGB\n" );
            sb.Append( "{\n" );
            sb.Append( "    public byte B;\n" );
            sb.Append( "    public byte G;\n" );
            sb.Append( "    public byte R;\n" );
            sb.Append( "}\n" );

            return sb.ToString();
        }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct TPOINT
    {
        public Int32 x;
        public Int32 y;
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct TRECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TRECT( int left, int top, int right, int bottom )
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TRECT( Rectangle rect )
        {
            Left = rect.Left;
            Top = rect.Top;
            Right = rect.Left + rect.Width;
            Bottom = rect.Top + rect.Height;
        }

        public static string ToDeclare()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( "[StructLayout( LayoutKind.Sequential )]\n" );
            sb.Append( "public struct TRECT\n" );
            sb.Append( "{\n" );
            sb.Append( "    public int Left;\n" );
            sb.Append( "    public int Top;\n" );
            sb.Append( "    public int Right;\n" );
            sb.Append( "    public int Bottom;\n" );
            sb.Append( "}\n" );

            return sb.ToString();
        }
    }
    #endregion

}