using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uIP.Lib.Script;
using uIP.Lib.Service;

namespace uIP.Lib
{
    public partial class FrmScriptEditor : Form
    {
        private const string MarkNodeAsDraggable = "draggable";
        private bool IsDispose { get; set; } = false;
        private string MainThreadRunScriptName { get; set; } = "";
        public FrmScriptEditor()
        {
            InitializeComponent();

            // try to reload plugins info
            ReloadPlugins( ResourceManager.Get( ResourceManager.PluginServiceName ) as UPluginAssemblyService );
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            IsDispose = true;

            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        private static void AddTypesDesc( TreeNode root, UDataCarrierTypeDescription[] types )
        {
            if ( root == null || types == null ) return;
            foreach ( var t in types )
            {
                root.Nodes.Add( String.Format( "{0}: {1}", t.Tp.FullName, t.Desc ) );
            }
        }

        private void ReloadPlugins( UPluginAssemblyService ps )
        {
            if ( ps == null ) return;
            // clear the whole nodes
            treeView_methodsOfPlugin.Nodes.Clear();

            if ( ps.PluginAssemblies == null || ps.PluginAssemblies.Count <= 0 ) return;
            foreach ( var a in ps.PluginAssemblies )
            {
                TreeNode rootN = treeView_methodsOfPlugin.Nodes.Add( a.NameOfCSharpDefClass );
                rootN.Tag = a;
                foreach ( var m in a.UserQueriedMethodList )
                {
                    TreeNode methodN = rootN.Nodes.Add( m.MethodName );
                    methodN.Tag = MarkNodeAsDraggable;

                    //
                    // add each type description
                    //
                    // immutable parameter
                    TreeNode immu = methodN.Nodes.Add( "Immutable paramter types" );
                    AddTypesDesc( immu, m.ImmutableParamTypeDesc );
                    // variable parameter
                    TreeNode varia = methodN.Nodes.Add( "Variable parameter types" );
                    AddTypesDesc( varia, m.VariableParamTypeDesc );
                    // add prev required
                    TreeNode prev = methodN.Nodes.Add( "Prev input requirement types" );
                    AddTypesDesc( prev, m.PrevPropagationParamTypeDesc );
                    // add propagation
                    TreeNode propag = methodN.Nodes.Add( "Propagation types" );
                    AddTypesDesc( propag, m.RetPropagationParamTypeDesc );
                    // add results
                    TreeNode result = methodN.Nodes.Add( "Result types" );
                    AddTypesDesc( result, m.RetResultTypeDesc );
                }
            }
        }

        internal void ReloadScripts()
        {
            treeView_scripts.Nodes.Clear();
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) || ss.Scripts == null ) return;

            foreach ( var s in ss.Scripts )
            {
                TreeNode nod = treeView_scripts.Nodes.Add( s.NameOfId );
                nod.Tag = new ScriptTreeNodeRelatedInfo( "", null, null );
                if (s.MacroSet == null || s.MacroSet.Count <= 0) continue;
                foreach ( var m in s.MacroSet )
                {
                    string txt = $"{m.MethodName}->{m.OwnerOfPluginClass?.NameOfCSharpDefClass}";
                    TreeNode sub = nod.Nodes.Add( txt );
                    sub.BackColor = m.ConfigDone ? Color.LightSteelBlue : DefaultBackColor;
                    sub.Tag = new ScriptTreeNodeRelatedInfo( m.MethodName, m.OwnerOfPluginClass, m );
                }
            }
        }

        private void frmScriptEditor_FormClosing( object sender, FormClosingEventArgs e )
        {
            if ( e.CloseReason == CloseReason.UserClosing )
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        #region Script edit drag-drop

        private void treeView_methodsOfPlugin_ItemDrag( object sender, ItemDragEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
            {
                if ( e.Item is TreeNode node && node?.Tag as string == MarkNodeAsDraggable )
                {
                    Console.WriteLine($"ItemDrag: {node.Text}");
                    DoDragDrop( node, DragDropEffects.Move );
                }
            }
        }

        #region Script tree view drag-drop

        internal enum NodeTpToAdd
        {
            First,
            Normal,
            Last
        }

        internal class ScriptTreeNodeRelatedInfo
        {
            internal string MethodNameInPluginAssembly = "";
            internal UMacroMethodProviderPlugin PluginInstance = null;
            internal UMacro MacroOfScript = null;

            internal ScriptTreeNodeRelatedInfo( string methodNm, UMacroMethodProviderPlugin plugin,
                UMacro createdMacro )
            {
                MethodNameInPluginAssembly = methodNm;
                PluginInstance = plugin;
                MacroOfScript = createdMacro;
            }
        }

        private TreeNode prevScriptTreeNode = null;

        private void treeView_scripts_DragEnter( object sender, DragEventArgs e )
        {
            e.Effect = e.AllowedEffect;
        }

        private static void ConfigBakColorOfTreeNode( TreeNode n )
        {
            if ( n == null ) return;
            n.BackColor = n.Tag is ScriptTreeNodeRelatedInfo
                ? ( n.Tag as ScriptTreeNodeRelatedInfo )?.MacroOfScript?.ConfigDone ?? true
                    ? DefaultBackColor
                    : Color.LightSteelBlue
                : Color.LightSteelBlue;
        }

        private void treeView_scripts_DragOver( object sender, DragEventArgs e )
        {
            Point pt = treeView_scripts.PointToClient( new Point( e.X, e.Y ) );
            TreeNode cn = treeView_scripts.GetNodeAt( pt );
            if ( prevScriptTreeNode != cn && prevScriptTreeNode != null )
            {
                ConfigBakColorOfTreeNode( prevScriptTreeNode );
            }
            if ( cn != null )
            {
                cn.BackColor = SystemColors.MenuHighlight;
            }
            prevScriptTreeNode = cn;
        }


        private static string GenDiffString( UDataCarrierTypeDescription[] prev, UDataCarrierTypeDescription[] next, bool markPrev )
        {
            var sb = new StringBuilder();
            int pl = prev?.Length ?? 0;
            int nl = next?.Length ?? 0;
            int maxL = pl > nl ? pl : nl;
            sb.Append( "Confirm to accept or not following type difference:\n" );
            for ( int i = 0; i < maxL; i++ )
            {
                sb.AppendFormat( "{2}Nod: {0} <-> {3}Nod: {1}\n",
                    prev == null || i >= prev.Length ? "-" : prev[ i ].Tp.FullName,
                    next == null || i >= next.Length ? "-" : next[ i ].Tp.FullName, 
                    markPrev ? "*" : "",
                    markPrev ? "" : "*" );
            }

            return sb.ToString();
        }

        private bool CheckToAdd( NodeTpToAdd tp, string methodName, UMacroMethodProviderPlugin belongAssembly,
            TreeNode relativeNodePrev = null, TreeNode relativeNodeNext = null )
        {
            // invalid: cannot find macro by method name
            UMacro m = belongAssembly.QueryOpenedMethod( methodName );
            UMacro nextM = (relativeNodeNext?.Tag as ScriptTreeNodeRelatedInfo )?.MacroOfScript ?? null;
            UMacro prevM = ( relativeNodePrev?.Tag as ScriptTreeNodeRelatedInfo )?.MacroOfScript ?? null;
            if ( m == null ) return false;

            bool cmpWithNext = false;
            bool cmpWithPrev = false;
            switch ( tp )
            {
                case NodeTpToAdd.First:
                    if ( relativeNodeNext != null )
                    {
                        if ( nextM == null ) return false;
                        cmpWithNext = true;
                    }

                    if ( m.PrevPropagationParamTypeDesc != null && m.PrevPropagationParamTypeDesc.Length != 0 )
                    {
                        if ( MessageBox.Show( @"Accept 1st macro with prev propagation not empty?", @"Confirm",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1 ) != DialogResult.OK )
                            return false;
                    }
                    break;
                case NodeTpToAdd.Last:
                    if ( prevM == null ) return false;
                    cmpWithPrev = true;
                    break;
                case NodeTpToAdd.Normal:
                    if ( prevM == null || nextM == null ) return false;
                    cmpWithPrev = cmpWithNext = true;
                    break;
            }

            if ( cmpWithNext )
            {
                if ( !UDataCarrierTypeDescription.CmpPrefect( m.RetPropagationParamTypeDesc,
                    nextM.PrevPropagationParamTypeDesc ) )
                {
                    if ( MessageBox.Show(
                        GenDiffString( m.RetPropagationParamTypeDesc, nextM.PrevPropagationParamTypeDesc, true ),
                        @"Confirm Difference", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1 ) != DialogResult.OK )
                        return false;
                }
            }

            if ( cmpWithPrev )
            {
                if ( !UDataCarrierTypeDescription.CmpPrefect( prevM.RetPropagationParamTypeDesc,
                    m.PrevPropagationParamTypeDesc ) )
                {
                    if ( MessageBox.Show(
                        GenDiffString( prevM.RetPropagationParamTypeDesc, m.PrevPropagationParamTypeDesc, false ),
                        @"Confirm Difference", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question, MessageBoxDefaultButton.Button1 ) != DialogResult.OK )
                        return false;
                }
            }

            return true;
        }

        //private static void BindingNodeToMacro( TreeNode nod, UMacro createdMacro)
        //{
        //    if ( nod == null || createdMacro == null || createdMacro.OwnerOfScript == null ) return;
        //    nod.Tag = new ScriptTreeNodeRelatedInfo( createdMacro.MethodName, createdMacro.OwnerOfPluginClass,
        //        createdMacro );
        //    nod.BackColor = createdMacro.ConfigDone ? Color.Transparent : Color.LightSteelBlue;
        //}

        private static bool BindingNodeToMacro( TreeNode nod, string scriptToAddMacro, UMacroMethodProviderPlugin plugin, string method, bool isAppend = true, int pos = 0 )
        {
            if ( nod == null || plugin == null || plugin.QueryOpenedMethod( method ) == null ) return false;

            // get script
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return false;
            UScript script = ss.GetScript( scriptToAddMacro );
            if ( script == null ) return false;

            // create process
            // 1. call to gen immutable parameter
            UDataCarrier[] immuParam = plugin.SetupMacroImmutableOnes( method );
            // 2. call to gen variable parameter
            bool dummyB = false;
            UDataCarrier[] variParam = plugin.SetupMacroVariables( method, script.MacroSet, ref dummyB );
            // 3. create macro
            UMacro macro = plugin.CreateMacroInstance( UDataCarrier.MakeOneItemArray( method ), immuParam, variParam );
            if ( macro == null ) return false;
            // 4. add/ insert to script
            if ( isAppend ) script.Add( macro );
            else script.InsertBeforeIndex( macro, pos );

            // append info to node
            nod.BackColor = Color.LightSteelBlue;
            nod.Tag = new ScriptTreeNodeRelatedInfo( method, plugin, macro );
            return true;
        }

        private void treeView_scripts_DragDrop( object sender, DragEventArgs e )
        {
            // Remark: drag-drop using add

            Point pt = treeView_scripts.PointToClient( new Point( e.X, e.Y ) );
            TreeNode trgN = treeView_scripts.GetNodeAt( pt );
            TreeNode srcN = ( TreeNode ) e.Data.GetData( typeof( TreeNode ) );
            UMacroMethodProviderPlugin belongAssembly = srcN.Parent?.Tag as UMacroMethodProviderPlugin ?? null;
            //ScriptTreeNodeRelatedInfo srcNInfo = srcN?.Tag as ScriptTreeNodeRelatedInfo ?? null; // use to check from same
            if ( trgN != null && srcN != null )
            {
                bool srcIsScriptNode = srcN.Tag is ScriptTreeNodeRelatedInfo;


                // drag from plugin
                if ( srcN.Parent != null )
                {
                    string txt = $"{srcN.Text}->{srcN.Parent.Text}";
                    TreeNode newN = null;
                    int pos = 0;
                    bool doAppend = true;
                    if ( trgN.Level == 0 )
                    {
                        if ( trgN.Nodes.Count <= 0 )
                        {
                            if (!srcIsScriptNode) // from plugin tree view drag
                            {
                                if ( CheckToAdd( NodeTpToAdd.First, srcN.Text, belongAssembly ) )
                                    newN = trgN.Nodes.Add( txt ); // insert to 1st without any
                            }
                        }
                        else
                        {
                            if (!srcIsScriptNode ) // new macro
                            {
                                if ( CheckToAdd( NodeTpToAdd.First, srcN.Text, belongAssembly, null, trgN.Nodes[ 0 ] ) )
                                    newN = trgN.Nodes.Insert( 0, txt ); // insert to 1st and need to check
                            }
                            else
                            {
                                // in same script, move position
                                if ( trgN == srcN.Parent && trgN.Nodes.Count > 1 && srcN.Index != 0 )
                                {
                                    if ( MessageBox.Show(
                                             $@"Will move '{srcN.Text}' to index 0 position?",
                                             @"Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) ==
                                         DialogResult.OK )
                                    {
                                        trgN.Nodes.RemoveAt( srcN.Index );
                                        trgN.Nodes.Insert( 0, srcN );
                                    }
                                }
                            }
                        }

                        if ( !trgN.IsExpanded ) trgN.Expand();
                    }
                    else
                    {
                        int insertIndex = trgN.Index + 1;
                        if ( insertIndex >= trgN.Parent.Nodes.Count )
                        {
                            if (!srcIsScriptNode ) // new macro
                            {
                                if ( CheckToAdd( NodeTpToAdd.Last, srcN.Text, belongAssembly, trgN ) )
                                    newN = trgN.Parent.Nodes.Add( txt ); // add to last
                            }
                            else
                            {
                                if ( trgN.Parent == srcN.Parent && srcN.Parent.LastNode != srcN )
                                {
                                    if ( MessageBox.Show(
                                             $@"Will move '{srcN.Text}' to end position?",
                                             @"Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) ==
                                         DialogResult.OK )
                                    {
                                        srcN.Parent.Nodes.Remove( srcN );
                                        trgN.Parent.Nodes.Add( srcN );
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!srcIsScriptNode ) // new macro
                            {
                                if ( CheckToAdd( NodeTpToAdd.Normal, srcN.Text, belongAssembly, trgN, trgN.NextNode ) )
                                {
                                    newN = trgN.Parent.Nodes.Insert( insertIndex, txt ); // insert between
                                    doAppend = false;
                                    pos = insertIndex;
                                }
                            }
                            else
                            {
                                if ( trgN.Parent == srcN.Parent && trgN.Parent.Nodes[ insertIndex ] != srcN )
                                {
                                    TreeNode posN = trgN.Parent.Nodes[ insertIndex ];
                                    if ( MessageBox.Show(
                                             $@"Will move '{srcN.Text}' after {posN.Text} position?",
                                             @"Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) ==
                                         DialogResult.OK )
                                    {
                                        srcN.Parent.Nodes.Remove( srcN );
                                        posN.Parent.Nodes.Insert( posN.Index, srcN );
                                    }
                                }
                            }
                        }
                    }

                    if ( newN != null )
                    {
                        // binding node with macro info
                        if ( !BindingNodeToMacro( newN, newN.Parent.Text, belongAssembly, srcN.Text, doAppend, pos ) )
                            newN.Remove(); // with error remove
                    }
                }
            }

            if ( prevScriptTreeNode != null )
            {
                ConfigBakColorOfTreeNode( prevScriptTreeNode );
                prevScriptTreeNode = null;
            }
        }

        private void treeView_scripts_DragLeave( object sender, EventArgs e )
        {
            if ( prevScriptTreeNode != null )
            {
                ConfigBakColorOfTreeNode( prevScriptTreeNode );
                prevScriptTreeNode = null;
            }
        }

        private void treeView_scripts_ItemDrag( object sender, ItemDragEventArgs e )
        {
            if ( e.Button == MouseButtons.Left )
            {
                if ( e.Item is TreeNode n && n.Level != 0 )
                    DoDragDrop( e.Item, DragDropEffects.Move );
            }
        }

        #endregion

        #endregion

        private void popupPluginClassSetupToolStripMenuItem_Click( object sender, EventArgs e )
        {
            TreeNode nod = treeView_methodsOfPlugin.SelectedNode;
            if ( nod == null || nod.Level != 0 )
            {
                MessageBox.Show( @"Please select a plugin instance to config of root node(Level 0)!" );
                return;
            }
            // popup class setup
            ( nod.Tag as UMacroMethodProviderPlugin )?.SetClassControl( UDataCarrier.MakeOne( UMacroMethodProviderPlugin.PredefClassIoctl_ParamGUI ), null );
        }

        #region Script New/ Delete

        private void toolStripMenuItem_newScript_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;

            FrmEditScriptName dlg = new FrmEditScriptName();
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( ss.GetScript( dlg.ScriptName ) != null )
                    MessageBox.Show( $@"Script: {dlg.ScriptName} already existed!" );
                else
                {
                    if ( ss.NewScript( dlg.ScriptName ) )
                    {
                        TreeNode n = treeView_scripts.Nodes.Add( dlg.ScriptName );
                        n.Tag = new ScriptTreeNodeRelatedInfo( "", null, null ); // just mark as script node
                    }
                }
            }
            dlg.Dispose();
        }

        private void toolStripMenuItem_deleteScript_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;

            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;
            if ( MessageBox.Show( $@"Confirm to delete script: {n.Text} ?", @"Confirm",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                return;
            ss.DeleteScript( n.Text );
            n.Remove();
        }

        private void toolStripMenuItem_scriptRename_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;

            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;
            FormScriptRename dlg = new FormScriptRename();
            dlg.OriName = n.Text;
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( ss.Rename( dlg.OriName, dlg.NewName ) )
                    n.Text = dlg.NewName;
            }
            dlg.Dispose();
        }

        private void contextMenuStrip_scriptRightCkl_Opened( object sender, EventArgs e )
        {
            ContextMenuStrip cms = sender as ContextMenuStrip;
            if ( cms == null )
                return;
            toolStripMenuItem_scriptEnableLog.Text = "Enable Log";
            if ( !ResourceManager.Get<UScriptService>( ResourceManager.ScriptService, null, out var ss ) )
                return;

            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;

            var script = ss.GetScript( n.Text );
            if ( script != null )
            {
                toolStripMenuItem_scriptEnableLog.Text = script.EnableLogOut ? "Disable Log" : "Enable Log";
            }
        }

        private void toolStripMenuItem_scriptEnableLog_Click( object sender, EventArgs e )
        {
            ToolStripMenuItem tsmi = sender as ToolStripMenuItem;
            if ( tsmi == null )
                return;
            if ( !ResourceManager.Get<UScriptService>( ResourceManager.ScriptService, null, out var ss ) )
                return;

            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;

            var script = ss.GetScript( n.Text );
            if ( script != null )
            {
                script.EnableLogOut = !script.EnableLogOut;
            }
        }

        #endregion

        #region Macro OP

        private void treeView_scripts_NodeMouseClick( object sender, TreeNodeMouseClickEventArgs e )
        {
            toolStripTextBox_macroOP.Enabled = e.Node.Level != 0;
            toolStripMenuItem_setupMacroImmuParam.Enabled = e.Node.Level != 0;
            toolStripMenuItem_setupMacroVarParam.Enabled = e.Node.Level != 0;
            toolStripMenuItem_setupMacroInnerFunc.Enabled = e.Node.Level != 0;
            toolStripMenuItem_removeMacro.Enabled = e.Node.Level != 0;
        }

        private void toolStripMenuItem_setupMacroImmuParam_Click( object sender, EventArgs e )
        {
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level == 0 ) return;

            ScriptTreeNodeRelatedInfo info = ( n.Tag as ScriptTreeNodeRelatedInfo );
            if ( info == null || info.PluginInstance == null || info.MacroOfScript == null ) return;
            info.PluginInstance.SetMacroControl( info.MacroOfScript,
                UDataCarrier.MakeOne( UMacroMethodProviderPlugin.PredefMacroIoctl_SetupImmParam ), null );
        }

        private void toolStripMenuItem_setupMacroVarParam_Click( object sender, EventArgs e )
        {
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level == 0 ) return;

            ScriptTreeNodeRelatedInfo info = ( n.Tag as ScriptTreeNodeRelatedInfo );
            if ( info == null || info.PluginInstance == null || info.MacroOfScript == null ) return;
            info.PluginInstance.SetMacroControl( info.MacroOfScript,
                UDataCarrier.MakeOne( UMacroMethodProviderPlugin.PredefMacroIoctl_SetupVarParam ), null );
        }

        private void toolStripMenuItem_setupMacroInnerFunc_Click( object sender, EventArgs e )
        {
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level == 0 ) return;

            ScriptTreeNodeRelatedInfo info = ( n.Tag as ScriptTreeNodeRelatedInfo );
            if ( info == null || info.PluginInstance == null || info.MacroOfScript == null ) return;
            info.PluginInstance.SetMacroControl( info.MacroOfScript,
                UDataCarrier.MakeOne( UMacroMethodProviderPlugin.PredefMacroIoctl_SetupMacro ), null );
        }

        private void toolStripMenuItem_removeMacro_Click( object sender, EventArgs e )
        {
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level == 0 ) return;

            if ( MessageBox.Show( $@"Will remove macro({n.Text}) from script({n.Parent.Text}) ?",
                @"Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) != DialogResult.OK )
                return;

            if ( !( n.Tag is ScriptTreeNodeRelatedInfo info ) )
            {
                MessageBox.Show( "Cannot remove macro with invalid type!" );
                return;
            }
            if (!ResourceManager.Get<UScriptService>(ResourceManager.ScriptService, null, out var ss) || ss == null)
            {
                MessageBox.Show( "Cannot get script service instance to delete!" );
                return;
            }

            var script = ss.GetScript( n.Parent.Text );
            if (script == null)
            {
                MessageBox.Show( $"Cannot find script({n.Parent?.Text??""})" );
                return;
            }

            if (!script.RemoveFrom(info.MacroOfScript))
            {
                MessageBox.Show( $"Cannot remove macro({info.MacroOfScript.MethodName}) from script({script.NameOfId})" );
                return;
            }

            n.Remove();

        }

        #endregion

        private void button_savePluginClass_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.PluginServiceName ) is UPluginAssemblyService ps ) ) return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = @"zip file|*.zip";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    string msg = null;
                    if ( !ps.SavePluginClassSettings( dlg.FileName, ref msg ) )
                        MessageBox.Show( $@"Write plugins setting to file fail: {msg}" );
                }
            }
            dlg.Dispose();
        }

        private void button_reloadPluginClassSettings_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.PluginServiceName ) is UPluginAssemblyService ps ) ) return;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = @"zip file|*.zip";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    string msg = null;
                    if ( !ps.LoadPluginClassSettings( dlg.FileName, ref msg ) )
                        MessageBox.Show( $@"Read plugins setting from file fail: {msg}" );
                }
            }
            dlg.Dispose();

        }

        private void button_runtimeLoadPlugin_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.PluginServiceName ) is UPluginAssemblyService ps ) ) return;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = @"assembly file|*.dll";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    if ( ps.LoadAssembly( dlg.FileName ) )
                        ReloadPlugins( ps );
                }
            }
            dlg.Dispose();

        }

        private void button_saveAllScripts_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = @"zip file|*.zip";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    string msg = null;
                    if ( !ss.WriteAllScriptsSettings( dlg.FileName, ref msg ) )
                        MessageBox.Show( $@"Write scripts setting to file fail: {msg}" );
                }
            }
            dlg.Dispose();

        }

        private void button_reloadScript_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = @"zip file|*.zip";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    string msg = null;
                    if ( !ss.ReadSettings( dlg.FileName, ref msg ) )
                        MessageBox.Show( $@"Read scripts setting from file fail: {msg}" );
                    else ReloadScripts();
                }
            }
            dlg.Dispose();

        }

        private void button_saveOneScript_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = @"zip file|*.zip";
            if ( dlg.ShowDialog() == DialogResult.OK )
            {
                if ( !String.IsNullOrEmpty( dlg.FileName ) )
                {
                    string msg = null;
                    if ( !ss.WriteScriptSettings( n.Text, dlg.FileName, ref msg ) )
                        MessageBox.Show( $@"Write script setting to file fail: {msg}" );
                }
            }
            dlg.Dispose();

        }

        private void button_classPopupConfig_Click( object sender, EventArgs e )
        {
            ( ResourceManager.Get( ResourceManager.PluginServiceName ) as UPluginAssemblyService )?.PopupGUI();
        }

        private void WithFreeMacroPropation_ScriptDoneCall( object context, ScriptExecReturnCode retCode, List<UMacroProduceCarrierResult> results, List<UMacroProduceCarrierDrawingResult> drawResults )
        {
            if ( IsDispose || !ResourceManager.SystemAvaliable )
                return;

            if ( !ResourceManager.Get<UScriptService>( ResourceManager.ScriptService, null, out var ss ) )
                return;

            var script = ss.GetScript( context as string );
            if ( script == null )
                return;

            if ( retCode != ScriptExecReturnCode.OK )
            {
                if ( InvokeRequired )
                    Invoke( new Action( () => MessageBox.Show( $"Exec {context as string} fail in [{script.OnErrorIndex}:{script.OnErrorMethod}]:\n{script.StatusMessage}" ) ) );
                else
                    MessageBox.Show( $"Exec {context as string} fail in [{script.OnErrorIndex}:{script.OnErrorMethod}]:\n{script.StatusMessage}" );
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( $"Exec {context as string} time:" );
                foreach(var m in script.MacroSet)
                {
                    sb.AppendLine( $"   [{m.MethodName}]" );
                    sb.AppendLine( $"      Time: {m.ExecTime:0.00} ms" );
                }

                ULibAgent.Singleton.LogNormal?.Invoke( sb.ToString() );
            }
        }

        private void toolStripMenuItem_withFreeMacroProgation_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptRunnerFactory ) is UScriptRunnerFactory f ) ||
                 !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;
            f.NewRunner0( n.Text, WithFreeMacroPropation_ScriptDoneCall, ss.GetScript( n.Text ) );
        }

        private void toolStripMenuItem_runWithoutFreePropagation_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptRunnerFactory ) is UScriptRunnerFactory f ) ||
                 !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ) return;
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;
            f.NewRunner0( null, null, ss.GetScript( n.Text ), null, false );
        }

        private void useMainThreadToolStripMenuItem_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) ||
                 !( ResourceManager.Get(ResourceManager.LibAgent) is ULibAgent libA ) )
                return;
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 ) return;

            // use main thread to run
            var s = ss.GetScript( n.Text );
            if (s.AbilityJumpMacro )
            {
                MainThreadRunScriptName = string.Copy( n.Text );

                //s.EnableGotoFunc = true;
                var retCode = s.Running( false );
                //s.EnableGotoFunc = false;

                MainThreadRunScriptName = "";
                libA.LogNormal?.Invoke( $"Main exec {n.Text}: result code={retCode}" );
            }
        }

        private static void RemoveNotExistingRunningId(ComboBox c)
        {
            if ( !ResourceManager.Get<UScriptRunnerFactory>( ResourceManager.ScriptRunnerFactory, null, out var srf ) )
                return;
            List<object> toDel = new List<object>();
            foreach(var i in c.Items)
            {
                if ( srf.GetRunner( i as string ) == null )
                    toDel.Add( i );
            }
            foreach(var i in toDel)
                c.Items.Remove( i );
            c.Text = "";
        }

        private void Infinite_ScriptDoneCall( object context )
        {
            if ( IsDispose || !ResourceManager.SystemAvaliable )
                return;
            if ( !(context is string scriptName) || string.IsNullOrEmpty(scriptName) )
                return;

            if ( ResourceManager.Get<UScriptService>( ResourceManager.ScriptService, null, out var ss ) )
            {
                var script = ss.GetScript( scriptName );
                StringBuilder sb = new StringBuilder();
                if ( script != null )
                {
                    sb.AppendLine( $"Exec {context as string} Time:" );
                    foreach ( var m in script.MacroSet )
                    {
                        // about 14-us for a run into NG doing simple things
                        sb.AppendLine( $"   [{m.MethodName}]" );
                        sb.AppendLine( $"      Cur: {m.ExecTime:0.00000} ms" );
                        if (m.MinExecTime != double.MaxValue)
                            sb.AppendLine( $"      Min: {m.MinExecTime:0.00} ms" );
                        sb.AppendLine( $"      Avg: {m.AvgExecTime:0.00} ms" );
                        if (m.MaxExecTime != double.MinValue)
                            sb.AppendLine( $"      Max: {m.MaxExecTime:0.00} ms" );
                    }
                }

                var dispS = sb.ToString();
                ULibAgent.Singleton.LogNormal?.Invoke( dispS );

                if ( InvokeRequired )
                    Invoke( new Action( () =>
                    {
                        RemoveNotExistingRunningId( comboBox_infiniteRunList );
                        MessageBox.Show( dispS );
                    } ) );
                else
                {
                    RemoveNotExistingRunningId( comboBox_infiniteRunList );
                    MessageBox.Show( dispS );
                }
            }
        }


        private void button_infiniteRunSelect_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptRunnerFactory ) is UScriptRunnerFactory f ) ||
                 !( ResourceManager.Get( ResourceManager.ScriptService ) is UScriptService ss ) )
            {
                MessageBox.Show( "Error: Reource not ready!" );
                return;
            }
            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 )
            {
                MessageBox.Show( "Error: no script in selecting!" );
                return;
            }
            if ( MessageBox.Show( $"Confirm script({n.Text}) doing infinite run?", "Confirm", MessageBoxButtons.OKCancel ) != DialogResult.OK )
                return;

            var bOnErrorEnd = MessageBox.Show( "Enable on error end exec.?", "Question", MessageBoxButtons.OKCancel ) == DialogResult.OK;

            var id = f.NewRunner0( string.Copy(n.Text), null, ss.GetScript( n.Text ), nExecTimes: UScriptRunnerFactory.InfiniteExecCount, bEnableOnErrorEnd: bOnErrorEnd, CallbackLoopEnd: new Action<object>( Infinite_ScriptDoneCall ), bEnableEvalTime: true );
            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show( $"Error: scrip({n.Text}) do infinite run fail!" );
                return;
            }

            comboBox_infiniteRunList.Items.Add( id );
        }

        private void CancelRunning( Object context )
        {            
            Invoke( new System.Action( () =>
            {
                comboBox_infiniteRunList.Items.Remove( context );
                comboBox_infiniteRunList.Text = "";
                comboBox_infiniteRunList.SelectedIndex = -1;
            } ) );
        }

        private void button_stopRunInfinite_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptRunnerFactory ) is UScriptRunnerFactory f ) )
            {
                MessageBox.Show( "Error: Reource not ready!" );
                return;
            }
            if (comboBox_infiniteRunList.SelectedIndex < 0)
            {
                MessageBox.Show( "No item to be stop!" );
                return;
            }

            var item = comboBox_infiniteRunList.Items[ comboBox_infiniteRunList.SelectedIndex ];
            f.Cancel( item.ToString(), new HandleCancelScriptCallback( CancelRunning ), item );
        }

        private static void JustRunDone( object context, ScriptExecReturnCode retCode, UDataCarrier[] results )
        {
            if ( !ResourceManager.SystemAvaliable )
                return;

            ResourceManager.InvokeMainThread( new Func<object, object>( ctx => {
                MessageBox.Show( $"{ctx.ToString()} exec done!" );
                return null;
            } ), context );
        }

        private void toolStripMenuItem_accordingTypeToRun_Click( object sender, EventArgs e )
        {
            if ( !( ResourceManager.Get( ResourceManager.ScriptRunnerFactory ) is UScriptRunnerFactory f ) )
                return;

            TreeNode n = treeView_scripts.SelectedNode;
            if ( n == null || n.Level != 0 )
                return;

            f.RunScript( n.Text, JustRunDone, n.Text, out var idOrMsg );
        }
    }
}
