using System;
using System.Runtime.InteropServices;

namespace uIP.LibBase.DataCarrier
{
    public enum eUDrawingCarrierType : int
    {
        NA = 0,
        Point,
        Intersection,
        Line,
        Ellipse,
        Rectangle,
        Polygon,
        BufferSpots,
        HBITMAP,
        Bitmap,
        Image,
        Text,
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct tUDrawingCarrier2DCoor
    {
        public Int32 X;
        public Int32 Y;

        public tUDrawingCarrier2DCoor( Int32 x, Int32 y ) { X = x; Y = y; }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct tUDrawingCarrierRGB
    {
        public byte B;
        public byte G;
        public byte R;

        public tUDrawingCarrierRGB( byte r, byte g, byte b ) { B = b; G = g; R = r; }
    }

    [StructLayout( LayoutKind.Sequential )]
    public struct tUDrawingCarrierRect
    {
        public Int32 Left;
        public Int32 Top;
        public Int32 Right;
        public Int32 Bottom;

        public tUDrawingCarrierRect( Int32 l, Int32 t, Int32 r, Int32 b ) { Left = l; Top = t; Right = r; Bottom = b; }
    }

    public enum eUDrawingCarrierShapeFillType : int
    {
        Border = 0,
        Fill,
        Both,
    }

    public enum eUDrawingCarrierIntersectionType : int
    {
        Perpendicular = 0,
        Cross,
    }

    public delegate void fpUDrawingCarrierPaintImageBuffFree( object context );

}
