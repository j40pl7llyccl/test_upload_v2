using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;

namespace uIP.LibBase.Utility
{
    public static class FileEncryptUtility
    {
		public static bool IsCtrl { get; set; } = false;
		public static bool IsCtrlEnabled { get; set; } = false;
        [StructLayout(LayoutKind.Sequential)]
        internal struct TEncHeader
        {
            internal UInt32 _nHdrSize;
            internal UInt32 _nCipherLength;
            internal UInt32 _nFileLength;
            internal UInt32 _nReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            internal UInt32[] _u32Reserved;
            internal UInt32 _u32DataCRC;
            internal UInt32 _u32HdrCRC;

            internal static TEncHeader Create()
            {
                IntPtr pMem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TEncHeader)));
                if (pMem == IntPtr.Zero)
                    return new TEncHeader();
                TEncHeader ret = (TEncHeader)Marshal.PtrToStructure(pMem, typeof(TEncHeader));
                Marshal.FreeHGlobal(pMem);

                ret._nHdrSize = (uint)Marshal.SizeOf(typeof(TEncHeader));
                ret._nCipherLength = (uint)0;
                ret._nFileLength = 0;
                ret._nReserved = 0;
                for (int i = 0; i < ret._u32Reserved.Length; i++)
                    ret._u32Reserved[i] = (uint)0;
                ret._u32DataCRC = (uint)0;
                ret._u32HdrCRC = (uint)0;

                return ret;
            }
        }

        private static unsafe byte[] GenKey(ref TEncHeader hdr)
        {
            byte[] ret = new byte[ 8 ];

            uint val1 = hdr._u32Reserved[ 2 ] | hdr._u32Reserved[ 0 ] ^ ( ~hdr._nHdrSize );
            uint val2 = hdr._u32Reserved[ 3 ] ^ hdr._u32Reserved[ 1 ] ^ ( ~hdr._nHdrSize );

            fixed ( byte* p = ret )
            {
                byte* p1 = ( byte* ) &val1;
                byte* p2 = ( byte* ) &val2;
                int cnt = 0;

                for ( int i = 0; i < 4; i++, cnt++ )
                {
                    p[ cnt ] = *p2++;
                }
                for ( int i = 0; i < 4; i++, cnt++ )
                {
                    p[ cnt ] = *p1++;
                }
            }

            return ret;
        }

        public static unsafe bool ENC( string path )
        {
            if ( String.IsNullOrEmpty( path ) || !File.Exists( path ) )
                return false;
			if ( IsCtrl && !IsCtrlEnabled )
				return false;

            IntPtr heapMem = IntPtr.Zero;

            // Read all data
            byte[] fileData = File.ReadAllBytes( path );
            if ( fileData == null || fileData.Length <= 0 )
                return false;

            Random rand = new Random( Environment.TickCount );
            TEncHeader hdr = TEncHeader.Create();
            int structSz = Marshal.SizeOf( typeof( TEncHeader ) );

            // fill header info
            hdr._nHdrSize = Convert.ToUInt32( structSz );
            hdr._nFileLength = Convert.ToUInt32( fileData.Length );
            for ( int i = 0; i < hdr._u32Reserved.Length; i++ )
                hdr._u32Reserved[ i ] = Convert.ToUInt32( rand.Next() );

            // Encrypt data
            byte[] key = GenKey( ref hdr );
            MemoryStream msEnecrypt = new MemoryStream();
            RC2 rc2Alg = RC2.Create();
            CryptoStream csEnec = new CryptoStream( msEnecrypt, rc2Alg.CreateEncryptor( key, key ), CryptoStreamMode.Write );
            csEnec.Write( fileData, 0, fileData.Length );
            csEnec.FlushFinalBlock();
            byte[] ciphertext = msEnecrypt.ToArray();

            // Close streams
            msEnecrypt.Close();
            csEnec.Close();

            // Calc Data CRC32
            hdr._u32DataCRC = ( uint ) CRC32.Compute( ciphertext, 0, ciphertext.Length );
            // Calc Hdr CRC32
            byte[] tmpBuff = new byte[ structSz - Marshal.SizeOf( typeof( UInt32 ) ) ];
            // Set data size
            hdr._nCipherLength = Convert.ToUInt32( ciphertext.Length );

            heapMem = Marshal.AllocHGlobal( structSz );
            if ( heapMem == IntPtr.Zero )
                return false;
            Marshal.StructureToPtr( hdr, heapMem, false );
            Marshal.Copy( heapMem, tmpBuff, 0, tmpBuff.Length );
            hdr._u32HdrCRC = ( uint ) CRC32.Compute( tmpBuff );

            // Copy to writen buffer
            byte[] wtBuff = new byte[ structSz + ciphertext.Length ];
            Marshal.StructureToPtr( hdr, heapMem, false );
            Marshal.Copy( heapMem, wtBuff, 0, structSz );
            fixed ( byte* pCipher = ciphertext )
            {
                void* pSrc = ( void* ) pCipher;
                Marshal.Copy( new IntPtr( pSrc ), wtBuff, structSz, ciphertext.Length );
            }

            Marshal.FreeHGlobal( heapMem );

            // Write to file
            File.WriteAllBytes( path, wtBuff );
            return true;
        }

        public static bool CheckFile(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return false;
			if ( IsCtrl && !IsCtrlEnabled )
				return false;

            TEncHeader hdr = TEncHeader.Create();
            int structSz = Marshal.SizeOf(typeof(TEncHeader));
            byte[] fileData = File.ReadAllBytes(path);
            if (fileData == null || fileData.Length < structSz)
                return false;

            IntPtr pTmpBuff = Marshal.AllocHGlobal(structSz);
            if (pTmpBuff == IntPtr.Zero)
                return false;

            // Copy from read buffer
            Marshal.Copy(fileData, 0, pTmpBuff, structSz);
            // Read from heap mem
            hdr = (TEncHeader)Marshal.PtrToStructure(pTmpBuff, typeof(TEncHeader));
            Marshal.FreeHGlobal(pTmpBuff);
            pTmpBuff = IntPtr.Zero;
            if (hdr._nHdrSize != structSz)
                return false;
            // Calc hdr CRC32
            UInt32 calcHdrCRC = (UInt32)CRC32.Compute(fileData, 0, structSz - Marshal.SizeOf(typeof(UInt32)));
            if (calcHdrCRC != hdr._u32HdrCRC)
                return false;
            // Check data size
            if (hdr._nCipherLength != Convert.ToUInt32(fileData.Length - structSz))
                return false;
            // Calc data CRC32
            byte[] ciphertxt = new byte[hdr._nCipherLength];
            for (int i = 0; i < ciphertxt.Length; i++)
                ciphertxt[i] = fileData[structSz + i];
            UInt32 calcDatCRC = (UInt32)CRC32.Compute(ciphertxt);
            if (calcDatCRC != hdr._u32DataCRC)
                return false;

            return true;
        }

        public static bool DEC( string path )
        {
            if ( String.IsNullOrEmpty( path ) || !File.Exists( path ) )
                return false;
			if ( IsCtrl && !IsCtrlEnabled )
				return false;

            TEncHeader hdr = TEncHeader.Create();
            int structSz = Marshal.SizeOf( typeof( TEncHeader ) );
            byte[] fileData = File.ReadAllBytes( path );
            if ( fileData == null || fileData.Length < structSz )
                return false;

            IntPtr pTmpBuff = Marshal.AllocHGlobal( structSz );
            if ( pTmpBuff == IntPtr.Zero )
                return false;

            // Copy from read buffer
            Marshal.Copy( fileData, 0, pTmpBuff, structSz );
            // Read from heap mem
            hdr = ( TEncHeader ) Marshal.PtrToStructure( pTmpBuff, typeof( TEncHeader ) );
            Marshal.FreeHGlobal( pTmpBuff );
            pTmpBuff = IntPtr.Zero;
            if ( hdr._nHdrSize != structSz )
                return false;
            // Calc hdr CRC32
            UInt32 calcHdrCRC = ( UInt32 ) CRC32.Compute( fileData, 0, structSz - Marshal.SizeOf( typeof( UInt32 ) ) );
            if ( calcHdrCRC != hdr._u32HdrCRC )
                return false;
            // Check data size
            if ( hdr._nCipherLength != Convert.ToUInt32( fileData.Length - structSz ) )
                return false;
            // Calc data CRC32
            byte[] ciphertxt = new byte[ hdr._nCipherLength ];
            for ( int i = 0; i < ciphertxt.Length; i++ )
                ciphertxt[ i ] = fileData[ structSz + i ];
            UInt32 calcDatCRC = ( UInt32 ) CRC32.Compute( ciphertxt );
            if ( calcDatCRC != hdr._u32DataCRC )
                return false;

            // Calc key
            byte[] key = GenKey( ref hdr );
            RC2 rc2Alg = RC2.Create();
            MemoryStream msDecrypt = new MemoryStream( fileData, structSz, Convert.ToInt32( hdr._nCipherLength ) );
            CryptoStream csDec = new CryptoStream( msDecrypt, rc2Alg.CreateDecryptor( key, key ), CryptoStreamMode.Read );
            byte[] plaintext = new byte[ hdr._nCipherLength ];
            csDec.Read( plaintext, 0, plaintext.Length );

            byte[] wrPlaintxt = new byte[ hdr._nFileLength ];
            for ( int i = 0; i < wrPlaintxt.Length; i++ )
                wrPlaintxt[ i ] = plaintext[ i ];

            File.WriteAllBytes( path, wrPlaintxt );
            msDecrypt.Dispose();
            return true;
        }

    }
}
