using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace uIP.LibBase.Utility
{
    public static class GuiUtilities
    {
        public static void AddMenuStripItemBySearchText( string pathDesc, MenuStrip menu, out ToolStripMenuItem createdItem, object tag = null, EventHandler onclk = null )
        {
            createdItem = null;
            if ( String.IsNullOrEmpty( pathDesc ) )
                return;

            string[] paths = pathDesc.Split( '/', '\\' );

            if ( paths == null || paths.Length <= 0 )
                return;
            if ( String.IsNullOrEmpty( paths[ 0 ] ) )
                return;

            for ( int i = 0; i < paths.Length; i++ ) {
                if ( String.IsNullOrEmpty( paths[ i ] ) )
                    return;
            }

            string l1 = paths[ 0 ].ToLower();
            ToolStripItem L0 = null;

            for ( int i = 0; i < menu.Items.Count; i++ ) {
                // no case cmp
                if ( menu.Items[ i ].Text.ToLower() == l1 ) {
                    L0 = menu.Items[ i ];
                    break;
                }
            }

            if ( L0 == null ) {
                ToolStripMenuItem mi = new ToolStripMenuItem();
                mi.Name = String.Format( "{0}_{1}", typeof( ToolStripMenuItem ).Name, paths[ 0 ] );
                mi.Text = paths[ 0 ];
                if ( tag != null ) mi.Tag = tag;
                menu.Items.Add( mi );
                L0 = mi;
                if ( paths.Length == 1 ) {
                    if ( onclk != null ) mi.Click += onclk;
                    createdItem = mi;
                }
            }
            if ( !( L0 is ToolStripMenuItem ) )
                return;

            ToolStripMenuItem curItem = L0 as ToolStripMenuItem;
            for ( int i = 1; i < paths.Length; i++ ) {
                if ( curItem == null ) return;
                string curItemNm = paths[ i ].ToLower();
                for ( int j = 0; j < curItem.DropDownItems.Count; j++ ) {
                    if ( curItemNm == curItem.DropDownItems[ j ].Text.ToLower() ) {
                        curItem = curItem.DropDownItems[ j ] as ToolStripMenuItem;
                        continue;
                    }
                }
                ToolStripMenuItem itm = new ToolStripMenuItem();
                itm.Name = String.Format( "{0}_{1}", typeof( ToolStripMenuItem ).Name, paths[ i ] );
                itm.Text = paths[ i ];
                curItem.DropDownItems.Add( itm );
                curItem = itm; // for next search
                if ( i == ( paths.Length - 1 ) ) {
                    if ( onclk != null ) itm.Click += onclk;
                    createdItem = itm;
                }
            }
        }

        /// <summary>
        /// Put a control onto one. Not suggest do this after system startup especially GUI ACL working.
        /// Cause, the TabPage may temperally move out from the TabControl.
        /// </summary>
        /// <param name="paths">Given a control's Name path to put a control</param>
        /// <param name="curLayer">indicate the current index, begin at 0</param>
        /// <param name="curCtrl">indicate current control, begin from a root control</param>
        /// <param name="ctrlToPut">a control will be put</param>
        /// <param name="extraName">when target control is TabControl, this is given Text string of a new TabPage</param>
        /// <param name="bLoc">a path is successfully targeted</param>
        /// <param name="bSucc">a control is put a control</param>
        public static void PutControlBySearchCompoName( string[] paths, int curLayer, Control curCtrl, Control ctrlToPut, string extraName, out bool bLoc, out bool bSucc )
        {
            bLoc = bSucc = false;
            if ( curLayer == ( paths.Length - 1 ) ) {
                bLoc = true;
                if ( curCtrl == null ) return;

                if ( curCtrl is Form || curCtrl is GroupBox || curCtrl is Panel ) {
                    curCtrl.Controls.Add( ctrlToPut );
                    bSucc = true;
                } else if ( curCtrl is TabControl && !String.IsNullOrEmpty( extraName ) ) {
                    TabControl tc = curCtrl as TabControl;
                    string nm = String.Format( "{0}_{1}", typeof( TabPage ).Name, extraName );
                    TabPage tp = null;
                    for ( int i = 0; i < tc.TabPages.Count; i++ ) {
                        if ( tc.TabPages[ i ].Name == nm ) {
                            tp = tc.TabPages[ i ]; break;
                        }
                    }
                    if ( tp == null ) {
                        tp = new TabPage( extraName );
                        tp.Name = String.Format( "{0}_{1}", typeof( TabPage ).Name, extraName );
                        tp.Tag = extraName;
                        tc.TabPages.Add( tp );
                        tp.Controls.Add( ctrlToPut );
                    } else {
                        tp.Controls.Add( ctrlToPut );
                    }
                    bSucc = true;
                }
                if ( curCtrl is Panel ) {
                    Panel p = curCtrl as Panel;
                    p.AutoScroll = true;
                }
                return;
            }

            if ( curCtrl.Name.ToLower() == paths[ curLayer ].ToLower() ) {
                if ( curCtrl.Controls != null ) {
                    for ( int i = 0; i < curCtrl.Controls.Count; i++ ) {
                        bool bLoced, bSucced;
                        PutControlBySearchCompoName( paths, curLayer + 1, curCtrl.Controls[ i ], ctrlToPut, extraName, out bLoced, out bSucced );
                        if ( bLoced ) {
                            bLoc = bLoced;
                            bSucc = bSucced;
                            return;
                        }
                    }
                }
            }
        }
    }
}
