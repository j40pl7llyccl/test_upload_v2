using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace uIP.LibBase.Utility
{
    public class LogLevelList
    {
        internal enum OP
        {
            Less,
            More,
            Between
        }
        internal class Item
        {
            internal int Val_1;
            internal int Val_2;
            internal OP ValOp;
            internal bool MatchCondAcceptOrNot;

            internal Item( int val1, int val2, OP op, bool acceptMatchCond)
            {
                Val_1 = val1;
                Val_2 = val2;
                ValOp = op;
                MatchCondAcceptOrNot = acceptMatchCond;
            }

            internal bool Check( int v )
            {
                switch ( ValOp )
                {
                    case OP.Less: return MatchCondAcceptOrNot ? v <= Val_1 : !( v <= Val_1 );
                    case OP.More: return MatchCondAcceptOrNot ? v >= Val_1 : !( v >= Val_1 );
                    default: return MatchCondAcceptOrNot ? v >= Val_1 && v <= Val_2 : !( v >= Val_1 && v <= Val_2 );
                }
            }
        }
        private object m_sync = new object();
        private List<Item> m_conds = new List< Item >();

        public void AddLessThan( int v, bool accept = true )
        {
            Monitor.Enter( m_sync );
            try
            {
                m_conds.Add( new Item( v, 0, OP.Less, accept ) );
            } finally{ Monitor.Exit( m_sync ); }
        }

        public void AddMoreThan( int v, bool accept = true )
        {
            Monitor.Enter( m_sync );
            try
            {
                m_conds.Add( new Item( v, 0, OP.More, accept ) );
            } finally { Monitor.Exit( m_sync ); }
        }

        public void AddBetween( int v1, int v2, bool accept = true )
        {
            int max = v1 > v2 ? v1 : v2;
            int min = v1 < v2 ? v1 : v2;
            Monitor.Enter( m_sync );
            try
            {
                m_conds.Add( new Item( min, max, OP.Between, accept ) );
            } finally { Monitor.Exit( m_sync ); }

        }

        public bool Accept( int v )
        {
            if ( m_conds.Count <= 0 ) return true;
            foreach ( var c in m_conds )
            {
                if ( c.Check( v ) ) return true;
            }

            return false;
        }
    }
}
