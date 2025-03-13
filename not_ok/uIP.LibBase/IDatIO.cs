using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.LibBase
{
    public interface IDatIO
    {
        string UniqueName { get; }

        string IOFileName { get; }
        bool CanWrite { get; }
        bool WriteDat( string folderPath, string fileName );
        bool CanRead { get; }
        bool ReadDat( string folderPath, string fileName );

        bool PopupGUI();
        Control GetGUI();
    }

    public interface ISettingIO
    {
        bool Add( IDatIO inst );
        void Remove( IDatIO inst );
        void Remove( string name );
        bool Write( string filePath );
        bool Read( string filePath );

        bool Config( IDatIO inst );
        bool Config( string name );
        Control GetConfig( IDatIO inst );
        Control GetConfig( string name );
    }
}
