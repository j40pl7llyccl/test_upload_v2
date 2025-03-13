using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace uIP.Lib.Utility
{
    public static class FileEncryptUtility
    {
        private const int KSZ = 1024;
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

        public static unsafe bool ENC( string srcPath, string dstPath )
        {
            if ( string.IsNullOrEmpty( srcPath ) || !File.Exists( srcPath ) )
                return false;

            // source path cannot equ dst path
            if ( srcPath.ToLower() == dstPath.ToLower() || string.IsNullOrEmpty( dstPath ) )
                return false;

            // prepare key
            Random rand = new Random( Environment.TickCount );
            TEncHeader hdr = TEncHeader.Create();
            int structSz = Marshal.SizeOf( typeof( TEncHeader ) );
            UInt32 srcSz = Convert.ToUInt32( new FileInfo( srcPath ).Length );

            // fill header info
            hdr._nHdrSize = Convert.ToUInt32( structSz );
            hdr._nFileLength = srcSz;
            for ( int i = 0; i < hdr._u32Reserved.Length; i++ )
                hdr._u32Reserved[ i ] = Convert.ToUInt32( rand.Next() );

            // get key
            byte[] key = GenKey( ref hdr );
            int ksz = KSZ;
            byte[] read1KBuff = new byte[ ksz ];

            using ( var fs = File.Open(dstPath, FileMode.Create))
            {
                // reserve header
                fs.Seek( structSz, SeekOrigin.Begin );

                // open encrypt
                RC2 rc2Alg = RC2.Create();
                var csEnec = new CryptoStream( fs, rc2Alg.CreateEncryptor( key, key ), CryptoStreamMode.Write );

                // read from source and write to enc
                long remain = srcSz;
                using ( var sfs = File.OpenRead( srcPath ) )
                {
                    for( ; ; )
                    {
                        var got = sfs.Read( read1KBuff, 0, ksz );
                        csEnec.Write( read1KBuff, 0, ksz );

                        remain -= got;
                        if ( remain <= 0 )
                            break;
                    }
                }
                csEnec.Close();
                csEnec.Dispose();
            }

            // calc data crc
            using ( var fs = File.OpenRead( dstPath ) )
            {
                // offset header
                fs.Seek( structSz, SeekOrigin.Begin );
                // calc crc
                hdr._u32DataCRC = ( uint )CRC32.Compute( fs );
            }

            // fill header other info
            hdr._nCipherLength = Convert.ToUInt32( ( new FileInfo( dstPath ) ).Length - structSz );

            // calc header crc
            byte[] tmpBuff = new byte[ structSz - Marshal.SizeOf( typeof( UInt32 ) ) ]; // discard header crc
            IntPtr heapMem = Marshal.AllocHGlobal( structSz );
            if ( heapMem == IntPtr.Zero )
                return false;
            Marshal.StructureToPtr( hdr, heapMem, false );
            Marshal.Copy( heapMem, tmpBuff, 0, tmpBuff.Length );
            hdr._u32HdrCRC = ( uint )CRC32.Compute( tmpBuff );

            // write header
            tmpBuff = new byte[ structSz ];
            Marshal.StructureToPtr( hdr, heapMem, false );
            Marshal.Copy( heapMem, tmpBuff, 0, tmpBuff.Length );

            using ( var fs = File.Open( dstPath, FileMode.Open, FileAccess.ReadWrite ) )
            {
                fs.Write(tmpBuff, 0, tmpBuff.Length);
            }

            Marshal.FreeHGlobal( heapMem );
            return true;
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
            byte[] tmpBuff = new byte[ structSz - Marshal.SizeOf( typeof( UInt32 ) ) ]; // discard header crc
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

        private static bool Check( Stream stream, long total, out TEncHeader hdr )
        {
            hdr = TEncHeader.Create();
            if ( stream == null )
                return false;

            //
            // read
            //
            int structSz = Marshal.SizeOf( typeof( TEncHeader ) );
            byte[] fileHdr = new byte[ structSz ];
            stream.Read( fileHdr, 0, fileHdr.Length );

            IntPtr pTmpBuff = Marshal.AllocHGlobal( structSz );
            if ( pTmpBuff == IntPtr.Zero )
                return false;

            // Copy from read buffer into structure
            Marshal.Copy( fileHdr, 0, pTmpBuff, structSz );
            // Read from heap mem
            hdr = ( TEncHeader )Marshal.PtrToStructure( pTmpBuff, typeof( TEncHeader ) );
            Marshal.FreeHGlobal( pTmpBuff );

            //
            // check
            //
            // header size
            if ( hdr._nHdrSize != structSz )
                return false;
            // Calc hdr CRC32
            UInt32 calcHdrCRC = ( UInt32 )CRC32.Compute( fileHdr, 0, structSz - Marshal.SizeOf( typeof( UInt32 ) ) );
            if ( calcHdrCRC != hdr._u32HdrCRC )
                return false;
            // Check data size
            if ( hdr._nCipherLength != Convert.ToUInt32( total - structSz ) )
                return false;
            // calc data CRC32
            UInt32 dataCrc = ( UInt32 )CRC32.Compute( stream );
            return hdr._u32DataCRC == dataCrc;
        }

        public static bool Check(Stream stream, long total)
        {
            return Check( stream, total, out var dummy );
        }
        public static bool Check(string filepath)
        {
            if (!File.Exists( filepath )) return false;
            var fsz = new FileInfo( filepath ).Length;
            try
            {
                var status = false;
                using(var fs = File.Open(filepath, FileMode.Open, FileAccess.Read))
                {
                    status = Check( fs, fsz );
                }
                return status;
            }
            catch { return false; }
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

        public static bool DEC( string srcPath, Stream dstS )
        {
            if ( string.IsNullOrEmpty( srcPath ) || !File.Exists( srcPath ) )
                return false;
            if ( dstS == null ) return false;
            try
            {
                long srcLength = new FileInfo( srcPath ).Length;
                var status = false;

                // check header and get it
                TEncHeader hdr;
                using ( var rs = File.Open( srcPath, FileMode.Open, FileAccess.Read ) )
                {
                    status = Check( rs, srcLength, out hdr );
                }
                if ( !status )
                    return false;

                // calc key
                byte[] key = GenKey( ref hdr );
                RC2 rc2Alg = RC2.Create();

                // descypt
                int hdrsz = Marshal.SizeOf( typeof( TEncHeader ) );
                int alloc = KSZ;
                byte[] rd = new byte[ alloc ];

                using ( var rs = File.Open( srcPath, FileMode.Open, FileAccess.Read ) )
                {
                    rs.Seek( hdrsz, SeekOrigin.Begin );

                    long remain = hdr._nFileLength;
                    CryptoStream csDec = new CryptoStream( rs, rc2Alg.CreateDecryptor( key, key ), CryptoStreamMode.Read );
                    for (; ; )
                    {
                        int got = csDec.Read( rd, 0, rd.Length );
                        int cpsz = got;
                        if ( cpsz > remain )
                            cpsz = ( int )remain;
                        dstS.Write( rd, 0, cpsz );
                        remain -= cpsz;
                        if ( got < rd.Length || remain <= 0 )
                            break;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DEC(string srcPath, string dstPath)
        {
            var status = true;
            // descypt
            using ( var ws = File.Open( dstPath, FileMode.Create, FileAccess.ReadWrite ) )
            {
                status = DEC( srcPath, ws );
            }

            return status;
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
