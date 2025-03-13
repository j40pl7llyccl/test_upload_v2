using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

namespace uIP.LibBase.Utility
{
    public class PluginedAssembly
    {
        public string _strFileFullPath = null;
        public string _strFileName = null;
        public Assembly _AssemInstance = null;

        public PluginedAssembly() { }
        public static PluginedAssembly Copy(PluginedAssembly c)
        {
            if ( c == null ) return new PluginedAssembly();

            PluginedAssembly r = new PluginedAssembly();
            r._strFileFullPath = c._strFileFullPath;
            r._strFileName = c._strFileName;
            r._AssemInstance = c._AssemInstance;
            return r;
        }
    }

    public class PluginAssemblies
    {
        private List<PluginedAssembly> _EverLoadedAssemblies = new List<PluginedAssembly>();
        internal List<PluginedAssembly> LoadedAssemblies { get { return _EverLoadedAssemblies; } }
        private List<string> _PathsAddToVarPATH = new List<string>();

        public Assembly[] Assemblies {
            get {
                if (_EverLoadedAssemblies == null || _EverLoadedAssemblies.Count <= 0) {
                    return new Assembly[ 0 ];
                }

                List<Assembly> ret = new List<Assembly>();
                for(int i = 0; i < _EverLoadedAssemblies.Count; i++ ) {
                    ret.Add( _EverLoadedAssemblies[ i ]._AssemInstance );
                }
                return ret.ToArray();
            }
        }

        public PluginAssemblies()
        {
            CommonUtilities.AddResolveCurrDomainAssembly();
        }

        private PluginedAssembly CheckFileFromEverLoaded(string filePath, bool bFullPath = true)
        {
            if ( String.IsNullOrEmpty( filePath ) )
                return null;

            string lc = bFullPath ? filePath.ToLower() : Path.GetFileNameWithoutExtension(filePath).ToLower();

            for(int i = 0 ; i < _EverLoadedAssemblies.Count ; i++ )
            {
                if ( bFullPath && lc == _EverLoadedAssemblies[ i ]._strFileFullPath.ToLower() )
                    return _EverLoadedAssemblies[ i ];
                else if ( lc == _EverLoadedAssemblies[ i ]._strFileName.ToLower() )
                    return _EverLoadedAssemblies[ i ];
            }

            return null;
        }

        private static void AddEnvVarPath( List<string> lst, string path )
        {
            if ( String.IsNullOrEmpty( path ) || !Directory.Exists( path ) )
                return;

            bool exist = false;
            for ( int i = 0; i < lst.Count; i++ ) {
                if ( lst[ i ].ToLower() == path.ToLower() ) {
                    exist = true; break;
                }
            }

            if ( !exist ) {
                Environment.SetEnvironmentVariable( "PATH", Environment.GetEnvironmentVariable( "PATH" ) + ";" + path );
                lst.Add( path );
            }
        }

        public PluginedAssembly[] Search( string dirFullPath )
        {
            if ( !Directory.Exists( dirFullPath ) )
                return null;

            string[] files = Directory.GetFiles( dirFullPath, "*.dll", SearchOption.TopDirectoryOnly );
            if ( files == null || files.Length <= 0 )
                return null;

            List<PluginedAssembly> ret = new List<PluginedAssembly>();
            for ( int i = 0; i < files.Length; i++ )
            {
                PluginedAssembly plugined = CheckFileFromEverLoaded( files[ i ] );
                // not found in list, so add
                if ( plugined == null )
                {
                    bool bOk = true;

                    plugined = new PluginedAssembly();
                    plugined._strFileFullPath = String.Copy( files[ i ] );
                    plugined._strFileName = Path.GetFileNameWithoutExtension( files[ i ] );

                    AddEnvVarPath( _PathsAddToVarPATH, Path.GetDirectoryName( files[ i ] ) );

                    try { plugined._AssemInstance = Assembly.LoadFile( files[ i ] ); }
                    catch { bOk = false; }
                    if ( !bOk )
                        continue;
                    _EverLoadedAssemblies.Add( plugined );
                }
                
                ret.Add( PluginedAssembly.Copy( plugined ) );
            }

            return ret.Count <= 0 ? null : ret.ToArray();
        }

        public bool Add( string filePath )
        {
            if ( !File.Exists( filePath ) )
                return false;

            PluginedAssembly plugined = CheckFileFromEverLoaded( filePath );
            if ( plugined == null )
            {
                bool bOk = true;

                plugined = new PluginedAssembly();
                plugined._strFileFullPath = String.Copy( filePath );
                plugined._strFileName = Path.GetFileNameWithoutExtension( filePath );

                AddEnvVarPath( _PathsAddToVarPATH, Path.GetDirectoryName( filePath ) );

                try { plugined._AssemInstance = Assembly.LoadFile( filePath ); }
                catch { bOk = false; }
                if ( !bOk ) return false;

                _EverLoadedAssemblies.Add( plugined );
            }

            return true;
        }


        public Type[] QueryFromType<BaseT>()
        {
            Dictionary<Type, PluginedAssembly> got = QueryByPluginedAssemblies<BaseT>( _EverLoadedAssemblies.ToArray() );
            if ( got == null )
                return null;

            List<Type> ret = new List<Type>();
            foreach(KeyValuePair<Type, PluginedAssembly>kv in got )
            {
                ret.Add( kv.Key );
            }
            return ret.Count <= 0 ? null : ret.ToArray();
        }

        public Type[] QueryAllTypes( string dllName )
        {
            if ( String.IsNullOrEmpty( dllName ) ) return null;

            PluginedAssembly plugined = CheckFileFromEverLoaded( dllName, false );
            if ( plugined == null )
                return null;

            return plugined._AssemInstance.GetTypes();
        }

        private static Dictionary<Type, PluginedAssembly> QueryByPluginedAssemblies(PluginedAssembly[] assemblies, Type tp)
        {
            if ( assemblies == null )
                return null;

            if ( tp == null ) return null;

            Dictionary<Type, PluginedAssembly> ret = new Dictionary<Type, PluginedAssembly>();
            for ( int i = 0 ; i < assemblies.Length ; i++ )
            {
                if ( assemblies[ i ]._AssemInstance == null )
                    continue;

                Type[] tps = assemblies[ i ]._AssemInstance.GetTypes();
                if ( tps == null || tps.Length <= 0 )
                    continue;

                for ( int j = 0 ; j < tps.Length ; j++ )
                {
                    if ( tps[ j ] == null )
                        continue;
                    if ( tps[ j ] == tp )
                    {
                        if ( ret.ContainsKey( tp ) )
                            ret[ tp ] = PluginedAssembly.Copy( assemblies[ i ] );
                        else
                            ret.Add( tp, PluginedAssembly.Copy( assemblies[ i ] ) );
                    }
                    else if ( tps[ j ].IsClass && tp.IsClass && tps[ j ].IsSubclassOf( tp ) )
                    {
                        if ( ret.ContainsKey( tp ) )
                            ret[ tps[ j ] ] = PluginedAssembly.Copy( assemblies[ i ] );
                        else
                            ret.Add( tps[ j ], PluginedAssembly.Copy( assemblies[ i ] ) );
                    }
                }
            }

            return ret;
        }
        public static Dictionary<Type, PluginedAssembly> QueryByPluginedAssemblies<T>( PluginedAssembly[] assemblies )
        {
            return QueryByPluginedAssemblies( assemblies, typeof( T ) );
        }

        public static Type GetTypeByFullName(string typeFullName)
        {
            if ( String.IsNullOrEmpty( typeFullName ) )
                return null;

            Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
            if ( all == null )
                return null;

            for(int i = 0 ; i < all.Length ; i++ )
            {
                if ( all[ i ] == null ) continue;
                Type ret = all[ i ].GetType( typeFullName, false, true );
                if ( ret != null )
                    return ret;
            }

            return null;
        }

        public Dictionary<Type, PluginedAssembly> QueryByTypeName( string baseTpFullNm )
        {
            if ( String.IsNullOrEmpty( baseTpFullNm ) )
                return null;

            Type bt = GetTypeByFullName( baseTpFullNm );
            if ( bt == null )
                return null;

            return QueryByPluginedAssemblies( _EverLoadedAssemblies.ToArray(), bt );
        }

        public static Assembly GetCurrentDomainAssemblyByTypeFullName(string name, out Type gotType )
        {
            gotType = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if ( assemblies == null ) return null;

            foreach(Assembly a in assemblies)
            {
                if ( a == null ) continue;
                if ( (gotType = a.GetType( name )) != null )
                    return a;
            }

            gotType = null;
            return null;
        }
    }
}
