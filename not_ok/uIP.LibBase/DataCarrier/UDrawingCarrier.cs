using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;

using uIP.LibBase.MarshalWinSDK;

namespace uIP.LibBase.DataCarrier
{

    #region Point Type
    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierPoints2d : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public tUDrawingCarrierRGB _Color = new tUDrawingCarrierRGB();
        // drawing data
        public List<tUDrawingCarrier2DCoor> _Coordinate = new List<tUDrawingCarrier2DCoor>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierPoints2d() { }
        public void AddPt( Int32 x, Int32 y ) { _Coordinate.Add( new tUDrawingCarrier2DCoor( x, y ) ); }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _Color = new tUDrawingCarrierRGB();
            // drawing data
            _Coordinate = new List<tUDrawingCarrier2DCoor>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }
    }
    #endregion

    #region Intersection Type

    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrier2dIntersection : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public eUDrawingCarrierIntersectionType _Att1 = eUDrawingCarrierIntersectionType.Perpendicular;
        public tUDrawingCarrierRGB _Color = new tUDrawingCarrierRGB();
        public Int32 _nLength = 5;
        // drawing data
        public List<tUDrawingCarrier2DCoor> _Coordinate = new List<tUDrawingCarrier2DCoor>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrier2dIntersection() { }
        public void AddPt( Int32 x, Int32 y ) { _Coordinate.Add( new tUDrawingCarrier2DCoor( x, y ) ); }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _Att1 = eUDrawingCarrierIntersectionType.Perpendicular;
            _Color = new tUDrawingCarrierRGB();
            _nLength = 5;
            // drawing data
            _Coordinate = new List<tUDrawingCarrier2DCoor>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }
    }
    #endregion

    #region Line Type
    public class UDrawingCarrierLine2d
    {
        public tUDrawingCarrier2DCoor _EP1 = new tUDrawingCarrier2DCoor();
        public tUDrawingCarrier2DCoor _EP2 = new tUDrawingCarrier2DCoor();
        public UDrawingCarrierLine2d() { }
        public UDrawingCarrierLine2d( Int32 x1, Int32 y1, Int32 x2, Int32 y2 )
        {
            _EP1.X = x1;
            _EP1.Y = y1;
            _EP2.X = x2;
            _EP2.Y = y2;
        }
    }

    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierLines2d : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public tUDrawingCarrierRGB _Color = new tUDrawingCarrierRGB();
        public Int32 _nWidth = 1;
        // drawing data
        public List<UDrawingCarrierLine2d> _Coordinate = new List<UDrawingCarrierLine2d>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierLines2d() { }
        public void AddLine( Int32 x1, Int32 y1, Int32 x2, Int32 y2 ) { _Coordinate.Add( new UDrawingCarrierLine2d( x1, y1, x2, y2 ) ); }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _Color = new tUDrawingCarrierRGB();
            _nWidth = 1;
            // drawing data
            _Coordinate = new List<UDrawingCarrierLine2d>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }
    }
    #endregion

    #region Ellipse Type
    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierEllipse2d : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public eUDrawingCarrierShapeFillType _DrawType = eUDrawingCarrierShapeFillType.Border;
        public tUDrawingCarrierRGB _BorderColor = new tUDrawingCarrierRGB();
        public Int32 _BorderWidth = 1;
        public tUDrawingCarrierRGB _FillColor = new tUDrawingCarrierRGB();
        // drawing data
        public List<tUDrawingCarrierRect> _Coordinate = new List<tUDrawingCarrierRect>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierEllipse2d() { }
        public void AddRect( tUDrawingCarrierRect rect ) { _Coordinate.Add( new tUDrawingCarrierRect( rect.Left, rect.Top, rect.Right, rect.Bottom ) ); }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _DrawType = eUDrawingCarrierShapeFillType.Border;
            _BorderColor = new tUDrawingCarrierRGB();
            _BorderWidth = 1;
            _FillColor = new tUDrawingCarrierRGB();
            // drawing data
            _Coordinate = new List<tUDrawingCarrierRect>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }
    }
    #endregion

    #region Rectangle Type
    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierRect2d : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public eUDrawingCarrierShapeFillType _DrawType = eUDrawingCarrierShapeFillType.Border;
        public tUDrawingCarrierRGB _BorderColor = new tUDrawingCarrierRGB();
        public Int32 _BorderWidth = 1;
        public tUDrawingCarrierRGB _FillColor = new tUDrawingCarrierRGB();
        // drawing data
        public List<tUDrawingCarrierRect> _Coordinate = new List<tUDrawingCarrierRect>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierRect2d() { }
        public void AddRect( tUDrawingCarrierRect rect ) { _Coordinate.Add( new tUDrawingCarrierRect( rect.Left, rect.Top, rect.Right, rect.Bottom ) ); }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _DrawType = eUDrawingCarrierShapeFillType.Border;
            _BorderColor = new tUDrawingCarrierRGB();
            _BorderWidth = 1;
            _FillColor = new tUDrawingCarrierRGB();
            // drawing data
            _Coordinate = new List<tUDrawingCarrierRect>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region Polygon Type
    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierPolygon2d : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public eUDrawingCarrierShapeFillType _DrawType = eUDrawingCarrierShapeFillType.Border;
        public tUDrawingCarrierRGB _BorderColor = new tUDrawingCarrierRGB();
        public Int32 _BorderWidth = 1;
        public tUDrawingCarrierRGB _FillColor = new tUDrawingCarrierRGB();
        // drawing data
        public List<tUDrawingCarrier2DCoor[]> _Coordinate = new List<tUDrawingCarrier2DCoor[]>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierPolygon2d() { }
        public void AddPolygonPts( tUDrawingCarrier2DCoor[] pts ) { if ( pts != null && pts.Length > 0 ) { _Coordinate.Add( pts ); } }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _DrawType = eUDrawingCarrierShapeFillType.Border;
            _BorderColor = new tUDrawingCarrierRGB();
            _BorderWidth = 1;
            _FillColor = new tUDrawingCarrierRGB();
            // drawing data
            _Coordinate = new List<tUDrawingCarrier2DCoor[]>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region BufferSpots Type
    public class UDrawingCarrierPixBuff
    {
        public Int32 X = 0;
        public Int32 Y = 0;

        public byte[] Buff = null;
        public Int32 Width = 0;
        public Int32 Height = 0;

        public UDrawingCarrierPixBuff() { }
        public UDrawingCarrierPixBuff( Int32 x, Int32 y, byte[] buff, Int32 w, Int32 h ) { X = x; Y = y; Buff = buff; Width = w; Height = h; }
        public void Set( Int32 x, Int32 y, byte val )
        {
            if ( x < 0 || y < 0 ) return;
            if ( Width <= 0 || Height <= 0 || Buff == null || Buff.Length <= 0 )
                return;

            long pos = ( long ) y * ( long ) Width + ( long ) x;
            if ( pos >= Buff.LongLength )
                return;

            Buff[ pos ] = val;
        }
    }
    public class UDarawingCarrierPixColor
    {
        public byte Val = 0;
        public tUDrawingCarrierRGB Color = new tUDrawingCarrierRGB();

        public UDarawingCarrierPixColor() { }
        public UDarawingCarrierPixColor( byte val, tUDrawingCarrierRGB col ) { Val = val; Color = col; }
    }

    /// <summary>
    /// _bUsingOffset -> true, all coordinates will add the base position _RefPos.
    /// </summary>
    public class UDrawingCarrierPixBuffers : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public List<UDarawingCarrierPixColor> _PixColorList = new List<UDarawingCarrierPixColor>();
        // drawing data
        public List<UDrawingCarrierPixBuff> _Buffers = new List<UDrawingCarrierPixBuff>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierPixBuffers() { }
        public void AddPixColor( byte val, byte r, byte g, byte b )
        {
            _PixColorList.Add( new UDarawingCarrierPixColor( val, new tUDrawingCarrierRGB( r, g, b ) ) );
        }
        public void AddBuffer( Int32 x, Int32 y, byte[] buff, Int32 w, int h )
        {
            if ( buff == null || buff.Length <= 0 || w <= 0 || h <= 0 )
                return;
            _Buffers.Add( new UDrawingCarrierPixBuff( x, y, buff, w, h ) );
        }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _PixColorList = new List<UDarawingCarrierPixColor>();
            // drawing data
            _Buffers = new List<UDrawingCarrierPixBuff>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region HBITMAP Type
    public class UDrawingCarrierHBitmap
    {
        public Int32 X = 0;
        public Int32 Y = 0;

        public bool bHandle = false;
        public IntPtr hBitmap = IntPtr.Zero;

        public byte[] Buffer = null;
        public Int32 Width = 0;
        public Int32 Height = 0;
        public Int32 Format = 0;

        public UDrawingCarrierHBitmap() { }
        public UDrawingCarrierHBitmap( Int32 x, Int32 y, IntPtr h, bool hand ) { X = x; Y = y; hBitmap = h; bHandle = hand; }
    }

    public class UDrawingCarrierHBitmaps : IDisposable, IUDataCarrierXmlIO
    {
        private bool _bDisposing = false;
        private bool _bDisposed = false;
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        // drawing data
        public List<UDrawingCarrierHBitmap> _HBITMAPs = new List<UDrawingCarrierHBitmap>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierHBitmaps() { }
        public void Add( Int32 x, Int32 y, IntPtr h, bool hand )
        {
            if ( _bDisposing || _bDisposed ) return;
            if ( h == IntPtr.Zero ) return;
            _HBITMAPs.Add( new UDrawingCarrierHBitmap( x, y, h, hand ) );
        }
        public void Dispose()
        {
            if ( _bDisposing || _bDisposed ) return;
            _bDisposing = true;

            for ( int i = 0 ; i < _HBITMAPs.Count ; i++ )
            {
                if ( _HBITMAPs[ i ] == null ) continue;
                if ( _HBITMAPs[ i ].bHandle && _HBITMAPs[ i ].hBitmap != IntPtr.Zero )
                {
                    CommonWinSdkFunctions.CloseHandle( _HBITMAPs[ i ].hBitmap );
                    _HBITMAPs[ i ].hBitmap = IntPtr.Zero;
                }
            }
            _HBITMAPs.Clear();

            _bDisposed = true;
            _bDisposing = false;
        }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            // drawing data
            if ( _HBITMAPs != null )
            {
                for ( int i = 0 ; i < _HBITMAPs.Count ; i++ )
                {
                    if ( _HBITMAPs[ i ] == null ) continue;
                    if ( _HBITMAPs[ i ].bHandle )
                        CommonWinSdkFunctions.CloseHandle( _HBITMAPs[ i ].hBitmap );
                    _HBITMAPs[ i ].hBitmap = IntPtr.Zero;
                }
                _HBITMAPs.Clear();
            }
            else
                _HBITMAPs = new List<UDrawingCarrierHBitmap>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region Bitmap Type
    public class UDrawingCarrierBitmap
    {
        public Int32 X = 0;
        public Int32 Y = 0;

        public bool bHandle = false;
        public Bitmap Bmp = null;

        public UDrawingCarrierBitmap() { }
        public UDrawingCarrierBitmap( Int32 x, Int32 y, Bitmap bmp, bool hand ) { X = x; Y = y; Bmp = bmp; bHandle = hand; }
    }

    public class UDrawingCarrierBitmaps : IDisposable, IUDataCarrierXmlIO
    {
        private bool _bDisposing = false;
        private bool _bDisposed = false;
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        // drawing data
        public List<UDrawingCarrierBitmap> _BmpList = new List<UDrawingCarrierBitmap>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierBitmaps() { }
        public void AddBmp( Int32 x, Int32 y, Bitmap bmp, bool hand )
        {
            if ( _bDisposing || _bDisposed ) return;
            if ( bmp == null ) return;

            _BmpList.Add( new UDrawingCarrierBitmap( x, y, bmp, hand ) );
        }
        public void Dispose()
        {
            if ( _bDisposing || _bDisposed ) return;
            _bDisposing = true;

            for ( int i = 0 ; i < _BmpList.Count ; i++ )
            {
                if ( _BmpList[ i ] == null ) continue;
                if ( _BmpList[ i ].bHandle && _BmpList[ i ].Bmp != null )
                {
                    _BmpList[ i ].Bmp.Dispose();
                    _BmpList[ i ].Bmp = null;
                }
            }
            _BmpList.Clear();

            _bDisposed = true;
            _bDisposing = false;
        }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            // drawing data
            if ( _BmpList != null )
            {
                for ( int i = 0 ; i < _BmpList.Count ; i++ )
                {
                    if ( _BmpList[ i ] == null ) continue;
                    if ( _BmpList[ i ].bHandle )
                        _BmpList[ i ].Bmp.Dispose();
                    _BmpList[ i ].Bmp = null;
                }
                _BmpList.Clear();
            }
            else
                _BmpList = new List<UDrawingCarrierBitmap>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region Image Type
    public class UDrawingCarrierImage
    {
        public Int32 X = 0;
        public Int32 Y = 0;
        public IntPtr Buff1 = IntPtr.Zero;
        public byte[] Buff2 = null;
        public Int32 Width = 0;
        public Int32 Height = 0;
        public Int32 Line = 0;
        public Int32 Format = 0;
        public object Context = null;
        public fpUDrawingCarrierPaintImageBuffFree fpHandleContext = null;

        public UDrawingCarrierImage() { }
        public UDrawingCarrierImage( Int32 x, Int32 y, IntPtr buff, Int32 w, Int32 h, Int32 l, Int32 f, object contxt, fpUDrawingCarrierPaintImageBuffFree fp )
        {
            X = x; Y = y;
            Buff1 = buff; Width = w; Height = h; Line = l; Format = f;
            Context = contxt;
            fpHandleContext = fp;
        }
        public UDrawingCarrierImage( Int32 x, Int32 y, byte[] buff, Int32 w, Int32 h, Int32 l, Int32 f, object contxt, fpUDrawingCarrierPaintImageBuffFree fp )
        {
            X = x; Y = y;
            Buff2 = buff; Width = w; Height = h; Format = f;
            Context = contxt;
            fpHandleContext = fp;
        }
    }

    public class UDrawingCarrierImages : IDisposable, IUDataCarrierXmlIO
    {
        private bool _bDisposing = false;
        private bool _bDisposed = false;
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        // drawing data
        public List<UDrawingCarrierImage> _Images = new List<UDrawingCarrierImage>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierImages() { }
        public void AddImage( Int32 x, Int32 y, IntPtr buf, Int32 w, Int32 h, Int32 f, Int32 l, object contxt, fpUDrawingCarrierPaintImageBuffFree fp )
        {
            if ( buf == IntPtr.Zero || w <= 0 || h <= 0 || f == 0 )
                return;

            _Images.Add( new UDrawingCarrierImage( x, y, buf, w, h, l, f, contxt, fp ) );
        }
        public void AddImage( Int32 x, Int32 y, byte[] buf, Int32 w, Int32 h, Int32 f, Int32 l, object contxt, fpUDrawingCarrierPaintImageBuffFree fp )
        {
            if ( buf == null || buf.Length <= 0 || w <= 0 || h <= 0 || f <= 0 )
                return;

            _Images.Add( new UDrawingCarrierImage( x, y, buf, w, h, l, f, contxt, fp ) );
        }
        public void Dispose()
        {
            if ( _bDisposing || _bDisposed ) return;
            _bDisposing = true;

            for ( int i = 0 ; i < _Images.Count ; i++ )
            {
                if ( _Images[ i ] == null ) continue;
                if ( _Images[ i ].fpHandleContext != null )
                {
                    _Images[ i ].fpHandleContext( _Images[ i ].Context );
                }
                _Images[ i ] = null;
            }
            _Images.Clear();

            _bDisposed = true;
            _bDisposing = false;
        }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            // drawing data
            if ( _Images != null )
            {
                for ( int i = 0 ; i < _Images.Count ; i++ )
                {
                    if ( _Images[ i ] == null ) continue;
                    if ( _Images[ i ].fpHandleContext != null && _Images[ i ].Context != null )
                        _Images[ i ].fpHandleContext( _Images[ i ].Context );
                    _Images[ i ].Context = null;
                    _Images[ i ].fpHandleContext = null;
                }
                _Images.Clear();
            }
            else
                _Images = new List<UDrawingCarrierImage>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    #region Text Type
    public class UDrawingCarrierText
    {
        public Int32 X = 0;
        public Int32 Y = 0;
        public string Txt = null;

        public UDrawingCarrierText() { }
        public UDrawingCarrierText( Int32 x, Int32 y, string str )
        {
            X = x; Y = y; Txt = str;
        }
    }

    public class UDrawingCarrierTexts : IUDataCarrierXmlIO
    {
        // offset info
        public bool _bUsingOffset = false;
        public tUDrawingCarrier2DCoor _RefPos = new tUDrawingCarrier2DCoor();
        // drawing attribute
        public string _FontName = null;
        public double _FontSize = 9.0;
        public tUDrawingCarrierRGB _FontColor = new tUDrawingCarrierRGB();
        public bool _bFontBold = false;
        public bool _bFontItalic = false;
        public bool _bFontUnderline = false;
        // drawing data
        public List<UDrawingCarrierText> _Strings = new List<UDrawingCarrierText>();
        // other information
        public string _strExtraInfo = null;

        public UDrawingCarrierTexts() { }
        public void Add( Int32 x, Int32 y, string str )
        {
            if ( String.IsNullOrEmpty( str ) ) return;
            _Strings.Add( new UDrawingCarrierText( x, y, str ) );
        }

        public void WriteDatCarrXml( XmlTextWriter tw )
        {
            UDrawingCarrierUtilities.WriteOutXml( this, tw );
        }
        public void ReadDatCarrXml( XmlNode nod )
        {
            // offset info
            _bUsingOffset = false;
            _RefPos = new tUDrawingCarrier2DCoor();
            // drawing attribute
            _FontName = null;
            _FontSize = 9.0;
            _FontColor = new tUDrawingCarrierRGB();
            _bFontBold = false;
            _bFontItalic = false;
            _bFontUnderline = false;
            // drawing data
            _Strings = new List<UDrawingCarrierText>();
            // other information
            _strExtraInfo = null;

            UDrawingCarrierUtilities.ReadInXml( this, nod );
        }

    }
    #endregion

    public class UDrawingCarrier
    {
        public eUDrawingCarrierType _Type = eUDrawingCarrierType.NA;
        public object _Data = null;
        public fpUDataCarrierXMLWriter _fpWrite = null;
        public fpUDataCarrierXMLReader _fpRead = null;

        public UDrawingCarrier() { }
        public UDrawingCarrier( eUDrawingCarrierType tp, object dat, fpUDataCarrierXMLWriter fpW, fpUDataCarrierXMLReader fpR )
        {
            _Type = tp; _Data = dat;
            _fpWrite = fpW;
            _fpRead = fpR;
        }
    }

    public class UDrawingCarriers : IDisposable
    {
        private static string _strBGSizWEleName = "ImageResSizW";
        private static string _strBGSizHEleName = "ImageResSizH";
        private static string _strDrawingCarrDataSecEleName = "DrawCarrDatSec";
        private static string _strDrawingCarrTypeSecEleName = "DrawCarrTypSec";

        private bool _bDisposing = false;
        private bool _bDisposed = false;

        // backgroung size
        private Int32 _nBackgroundSizeW = 0;
        private Int32 _nBackgroundSizeH = 0;

        // All drawing information
        private List<UDrawingCarrier> _listKeep = new List<UDrawingCarrier>();

        public Int32 BackgroundW { get { return _nBackgroundSizeW; } set { _nBackgroundSizeW = value; } }
        public Int32 BackgroundH { get { return _nBackgroundSizeH; } set { _nBackgroundSizeH = value; } }
        public List<UDrawingCarrier> DrawingData { get { return _listKeep; } }

        public UDrawingCarriers() { }
        public UDrawingCarriers( Int32 sizeW, Int32 sizeH ) { _nBackgroundSizeW = sizeW; _nBackgroundSizeH = sizeH; }

        public void Dispose()
        {
            if ( _bDisposing || _bDisposed ) return;
            _bDisposing = true;

            for ( int i = 0 ; i < _listKeep.Count ; i++ )
            {
                if ( _listKeep[ i ] == null ) continue;

                IDisposable disp = _listKeep[ i ]._Data as IDisposable;
                if ( disp != null ) disp.Dispose();
                _listKeep[ i ] = null;
            }
            _listKeep.Clear();

            _bDisposed = true;
            _bDisposing = false;
        }

        public bool Add<T>(T dat)
        {
            Type tp = typeof( T );
            if ( tp.IsValueType ) return false;
            if ( tp == typeof( UDrawingCarrierPoints2d ) ) return Add( dat as UDrawingCarrierPoints2d );
            else if ( tp == typeof( UDrawingCarrier2dIntersection ) ) return Add( dat as UDrawingCarrier2dIntersection );
            else if ( tp == typeof( UDrawingCarrierLines2d ) ) return Add( dat as UDrawingCarrierLines2d );
            else if ( tp == typeof( UDrawingCarrierEllipse2d ) ) return Add( dat as UDrawingCarrierEllipse2d );
            else if ( tp == typeof( UDrawingCarrierRect2d ) ) return Add( dat as UDrawingCarrierRect2d );
            else if ( tp == typeof( UDrawingCarrierPolygon2d ) ) return Add( dat as UDrawingCarrierPolygon2d );
            else if ( tp == typeof( UDrawingCarrierPixBuffers ) ) return Add( dat as UDrawingCarrierPixBuffers );
            else if ( tp == typeof( UDrawingCarrierHBitmaps ) ) return Add( dat as UDrawingCarrierHBitmaps );
            else if ( tp == typeof( UDrawingCarrierBitmaps ) ) return Add( dat as UDrawingCarrierBitmaps );
            else if ( tp == typeof( UDrawingCarrierImages ) ) return Add( dat as UDrawingCarrierImages );
            else if ( tp == typeof( UDrawingCarrierTexts ) ) return Add( dat as UDrawingCarrierTexts );

            return false;
        }

        public bool Add( UDrawingCarrierPoints2d dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Point, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrier2dIntersection dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Intersection, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierLines2d dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Line, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierEllipse2d dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Ellipse, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierRect2d dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Rectangle, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierPolygon2d dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Polygon, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierPixBuffers dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.BufferSpots, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierHBitmaps dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.HBITMAP, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierBitmaps dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Bitmap, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierImages dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Image, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public bool Add( UDrawingCarrierTexts dat )
        {
            if ( dat == null ) return false;
            _listKeep.Add( new UDrawingCarrier( eUDrawingCarrierType.Text, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml ) );
            return true;
        }

        public void Write( XmlTextWriter tw )
        {
            if ( tw == null ) return;

            // background size
            tw.WriteElementString( _strBGSizWEleName, _nBackgroundSizeW.ToString() );
            tw.WriteElementString( _strBGSizHEleName, _nBackgroundSizeH.ToString() );

            // write each data
            for ( int i = 0 ; i < _listKeep.Count ; i++ )
            {
                if ( _listKeep[ i ] == null ) continue;
                tw.WriteStartElement( _strDrawingCarrDataSecEleName );

                tw.WriteElementString( _strDrawingCarrTypeSecEleName, _listKeep[ i ]._Type.ToString() );
                tw.WriteStartElement( _listKeep[ i ]._Type.ToString() );

                if ( _listKeep[ i ]._fpWrite != null )
                    _listKeep[ i ]._fpWrite( tw );

                tw.WriteEndElement();
                tw.WriteEndElement();
            }
        }

        public void Read( XmlNode nodP )
        {
            if ( nodP == null ) return;

            for ( int i = 0 ; i < _listKeep.Count ; i++ )
            {
                if ( _listKeep[ i ] == null ) continue;
                IDisposable disp = _listKeep[ i ]._Data as IDisposable;
                if ( disp != null )
                    disp.Dispose();
            }
            _listKeep.Clear();

            XmlNode nod = null;

            nod = nodP.SelectSingleNode( _strBGSizWEleName );
            if ( nod != null ) UDrawingCarrierUtilities.StringToInt32( nod.InnerText, ref _nBackgroundSizeW, _nBackgroundSizeW );
            nod = nodP.SelectSingleNode( _strBGSizHEleName );
            if ( nod != null ) UDrawingCarrierUtilities.StringToInt32( nod.InnerText, ref _nBackgroundSizeH, _nBackgroundSizeH );

            XmlNodeList nodl = nodP.SelectNodes( _strDrawingCarrDataSecEleName );
            if ( nodl == null || nodl.Count <= 0 ) return;

            for ( int i = 0 ; i < nodl.Count ; i++ )
            {
                if ( nodl[ i ] == null ) continue;
                nod = nodl[ i ].SelectSingleNode( _strDrawingCarrTypeSecEleName );
                if ( nod == null || !Enum.IsDefined( typeof( eUDrawingCarrierType ), nod.InnerText ) )
                    continue;
                eUDrawingCarrierType tp = ( eUDrawingCarrierType ) Enum.Parse( typeof( eUDrawingCarrierType ), nod.InnerText );
                nod = nodl[ i ].SelectSingleNode( tp.ToString() );
                if ( nod == null ) continue;

                UDrawingCarrier item = null;
                if ( tp == eUDrawingCarrierType.Point )
                {
                    UDrawingCarrierPoints2d dat = new UDrawingCarrierPoints2d();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Intersection )
                {
                    UDrawingCarrier2dIntersection dat = new UDrawingCarrier2dIntersection();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Line )
                {
                    UDrawingCarrierLines2d dat = new UDrawingCarrierLines2d();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Rectangle )
                {
                    UDrawingCarrierRect2d dat = new UDrawingCarrierRect2d();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Ellipse )
                {
                    UDrawingCarrierEllipse2d dat = new UDrawingCarrierEllipse2d();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Polygon )
                {
                    UDrawingCarrierPolygon2d dat = new UDrawingCarrierPolygon2d();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.BufferSpots )
                {
                    UDrawingCarrierPixBuffers dat = new UDrawingCarrierPixBuffers();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.HBITMAP )
                {
                    UDrawingCarrierHBitmaps dat = new UDrawingCarrierHBitmaps();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Bitmap )
                {
                    UDrawingCarrierBitmaps dat = new UDrawingCarrierBitmaps();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Image )
                {
                    UDrawingCarrierImages dat = new UDrawingCarrierImages();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else if ( tp == eUDrawingCarrierType.Text )
                {
                    UDrawingCarrierTexts dat = new UDrawingCarrierTexts();
                    dat.ReadDatCarrXml( nod );
                    item = new UDrawingCarrier( tp, dat, dat.WriteDatCarrXml, dat.ReadDatCarrXml );
                }
                else
                    Console.WriteLine( "[UDrawingCarriers::Read] get unhandleable type = {0}.", tp.ToString() );

                if ( item != null )
                    _listKeep.Add( item );
            }
        }

    }
}
