namespace uIP.Lib
{
    partial class FrmScriptEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeView_methodsOfPlugin = new System.Windows.Forms.TreeView();
            this.contextMenuStrip_pluginsRightClk = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.popupPluginClassSetupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label_plugins = new System.Windows.Forms.Label();
            this.label_scriptEdit = new System.Windows.Forms.Label();
            this.treeView_scripts = new System.Windows.Forms.TreeView();
            this.contextMenuStrip_scriptRightCkl = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox_scriptOP = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem_newScript = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_deleteScript = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_scriptToRun = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_runWithoutFreePropagation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_withFreeMacroProgation = new System.Windows.Forms.ToolStripMenuItem();
            this.useMainThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_scriptRename = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_scriptEnableLog = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripTextBox_macroOP = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripMenuItem_setupMacroImmuParam = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_setupMacroVarParam = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_setupMacroInnerFunc = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_removeMacro = new System.Windows.Forms.ToolStripMenuItem();
            this.button_saveAllScripts = new System.Windows.Forms.Button();
            this.button_reloadScript = new System.Windows.Forms.Button();
            this.button_saveOneScript = new System.Windows.Forms.Button();
            this.button_savePluginClass = new System.Windows.Forms.Button();
            this.button_reloadPluginClassSettings = new System.Windows.Forms.Button();
            this.button_runtimeLoadPlugin = new System.Windows.Forms.Button();
            this.button_classPopupConfig = new System.Windows.Forms.Button();
            this.button_infiniteRunSelect = new System.Windows.Forms.Button();
            this.comboBox_infiniteRunList = new System.Windows.Forms.ComboBox();
            this.button_stopRunInfinite = new System.Windows.Forms.Button();
            this.toolStripMenuItem_accordingTypeToRun = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_pluginsRightClk.SuspendLayout();
            this.contextMenuStrip_scriptRightCkl.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView_methodsOfPlugin
            // 
            this.treeView_methodsOfPlugin.AllowDrop = true;
            this.treeView_methodsOfPlugin.ContextMenuStrip = this.contextMenuStrip_pluginsRightClk;
            this.treeView_methodsOfPlugin.Location = new System.Drawing.Point(25, 51);
            this.treeView_methodsOfPlugin.Name = "treeView_methodsOfPlugin";
            this.treeView_methodsOfPlugin.Size = new System.Drawing.Size(771, 233);
            this.treeView_methodsOfPlugin.TabIndex = 0;
            this.treeView_methodsOfPlugin.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_methodsOfPlugin_ItemDrag);
            // 
            // contextMenuStrip_pluginsRightClk
            // 
            this.contextMenuStrip_pluginsRightClk.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popupPluginClassSetupToolStripMenuItem});
            this.contextMenuStrip_pluginsRightClk.Name = "contextMenuStrip_pluginsRightClk";
            this.contextMenuStrip_pluginsRightClk.Size = new System.Drawing.Size(218, 26);
            // 
            // popupPluginClassSetupToolStripMenuItem
            // 
            this.popupPluginClassSetupToolStripMenuItem.Name = "popupPluginClassSetupToolStripMenuItem";
            this.popupPluginClassSetupToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.popupPluginClassSetupToolStripMenuItem.Text = "Popup Plugin Class Setup";
            this.popupPluginClassSetupToolStripMenuItem.Click += new System.EventHandler(this.popupPluginClassSetupToolStripMenuItem_Click);
            // 
            // label_plugins
            // 
            this.label_plugins.AutoSize = true;
            this.label_plugins.Location = new System.Drawing.Point(33, 30);
            this.label_plugins.Name = "label_plugins";
            this.label_plugins.Size = new System.Drawing.Size(123, 18);
            this.label_plugins.TabIndex = 1;
            this.label_plugins.Text = "Methods of Plugin";
            // 
            // label_scriptEdit
            // 
            this.label_scriptEdit.AutoSize = true;
            this.label_scriptEdit.Location = new System.Drawing.Point(33, 309);
            this.label_scriptEdit.Name = "label_scriptEdit";
            this.label_scriptEdit.Size = new System.Drawing.Size(50, 18);
            this.label_scriptEdit.TabIndex = 2;
            this.label_scriptEdit.Text = "Scripts";
            // 
            // treeView_scripts
            // 
            this.treeView_scripts.AllowDrop = true;
            this.treeView_scripts.ContextMenuStrip = this.contextMenuStrip_scriptRightCkl;
            this.treeView_scripts.Location = new System.Drawing.Point(25, 330);
            this.treeView_scripts.Name = "treeView_scripts";
            this.treeView_scripts.Size = new System.Drawing.Size(771, 226);
            this.treeView_scripts.TabIndex = 3;
            this.treeView_scripts.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeView_scripts_ItemDrag);
            this.treeView_scripts.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView_scripts_NodeMouseClick);
            this.treeView_scripts.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeView_scripts_DragDrop);
            this.treeView_scripts.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeView_scripts_DragEnter);
            this.treeView_scripts.DragOver += new System.Windows.Forms.DragEventHandler(this.treeView_scripts_DragOver);
            this.treeView_scripts.DragLeave += new System.EventHandler(this.treeView_scripts_DragLeave);
            // 
            // contextMenuStrip_scriptRightCkl
            // 
            this.contextMenuStrip_scriptRightCkl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator2,
            this.toolStripTextBox_scriptOP,
            this.toolStripMenuItem_newScript,
            this.toolStripMenuItem_deleteScript,
            this.toolStripMenuItem_scriptToRun,
            this.toolStripMenuItem_scriptRename,
            this.toolStripMenuItem_scriptEnableLog,
            this.toolStripSeparator1,
            this.toolStripTextBox_macroOP,
            this.toolStripMenuItem_setupMacroImmuParam,
            this.toolStripMenuItem_setupMacroVarParam,
            this.toolStripMenuItem_setupMacroInnerFunc,
            this.toolStripMenuItem_removeMacro});
            this.contextMenuStrip_scriptRightCkl.Name = "contextMenuStrip_scriptRightCkl";
            this.contextMenuStrip_scriptRightCkl.Size = new System.Drawing.Size(181, 286);
            this.contextMenuStrip_scriptRightCkl.Opened += new System.EventHandler(this.contextMenuStrip_scriptRightCkl_Opened);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripTextBox_scriptOP
            // 
            this.toolStripTextBox_scriptOP.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.toolStripTextBox_scriptOP.Name = "toolStripTextBox_scriptOP";
            this.toolStripTextBox_scriptOP.ReadOnly = true;
            this.toolStripTextBox_scriptOP.Size = new System.Drawing.Size(100, 23);
            this.toolStripTextBox_scriptOP.Text = "Script OP";
            // 
            // toolStripMenuItem_newScript
            // 
            this.toolStripMenuItem_newScript.Name = "toolStripMenuItem_newScript";
            this.toolStripMenuItem_newScript.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_newScript.Text = "New";
            this.toolStripMenuItem_newScript.Click += new System.EventHandler(this.toolStripMenuItem_newScript_Click);
            // 
            // toolStripMenuItem_deleteScript
            // 
            this.toolStripMenuItem_deleteScript.Name = "toolStripMenuItem_deleteScript";
            this.toolStripMenuItem_deleteScript.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_deleteScript.Text = "Delete";
            this.toolStripMenuItem_deleteScript.Click += new System.EventHandler(this.toolStripMenuItem_deleteScript_Click);
            // 
            // toolStripMenuItem_scriptToRun
            // 
            this.toolStripMenuItem_scriptToRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_runWithoutFreePropagation,
            this.toolStripMenuItem_withFreeMacroProgation,
            this.useMainThreadToolStripMenuItem,
            this.toolStripMenuItem_accordingTypeToRun});
            this.toolStripMenuItem_scriptToRun.Name = "toolStripMenuItem_scriptToRun";
            this.toolStripMenuItem_scriptToRun.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_scriptToRun.Text = "Run";
            // 
            // toolStripMenuItem_runWithoutFreePropagation
            // 
            this.toolStripMenuItem_runWithoutFreePropagation.Name = "toolStripMenuItem_runWithoutFreePropagation";
            this.toolStripMenuItem_runWithoutFreePropagation.Size = new System.Drawing.Size(258, 22);
            this.toolStripMenuItem_runWithoutFreePropagation.Text = "Without free macro propagation";
            this.toolStripMenuItem_runWithoutFreePropagation.Click += new System.EventHandler(this.toolStripMenuItem_runWithoutFreePropagation_Click);
            // 
            // toolStripMenuItem_withFreeMacroProgation
            // 
            this.toolStripMenuItem_withFreeMacroProgation.Name = "toolStripMenuItem_withFreeMacroProgation";
            this.toolStripMenuItem_withFreeMacroProgation.Size = new System.Drawing.Size(258, 22);
            this.toolStripMenuItem_withFreeMacroProgation.Text = "Free macro propagation";
            this.toolStripMenuItem_withFreeMacroProgation.Click += new System.EventHandler(this.toolStripMenuItem_withFreeMacroProgation_Click);
            // 
            // useMainThreadToolStripMenuItem
            // 
            this.useMainThreadToolStripMenuItem.Name = "useMainThreadToolStripMenuItem";
            this.useMainThreadToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.useMainThreadToolStripMenuItem.Text = "Use Main Thread";
            this.useMainThreadToolStripMenuItem.Click += new System.EventHandler(this.useMainThreadToolStripMenuItem_Click);
            // 
            // toolStripMenuItem_scriptRename
            // 
            this.toolStripMenuItem_scriptRename.Name = "toolStripMenuItem_scriptRename";
            this.toolStripMenuItem_scriptRename.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_scriptRename.Text = "Rename";
            this.toolStripMenuItem_scriptRename.Click += new System.EventHandler(this.toolStripMenuItem_scriptRename_Click);
            // 
            // toolStripMenuItem_scriptEnableLog
            // 
            this.toolStripMenuItem_scriptEnableLog.Name = "toolStripMenuItem_scriptEnableLog";
            this.toolStripMenuItem_scriptEnableLog.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_scriptEnableLog.Text = "Enable Log";
            this.toolStripMenuItem_scriptEnableLog.Click += new System.EventHandler(this.toolStripMenuItem_scriptEnableLog_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripTextBox_macroOP
            // 
            this.toolStripTextBox_macroOP.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.toolStripTextBox_macroOP.Name = "toolStripTextBox_macroOP";
            this.toolStripTextBox_macroOP.ReadOnly = true;
            this.toolStripTextBox_macroOP.Size = new System.Drawing.Size(100, 23);
            this.toolStripTextBox_macroOP.Text = "Macro OP";
            // 
            // toolStripMenuItem_setupMacroImmuParam
            // 
            this.toolStripMenuItem_setupMacroImmuParam.Name = "toolStripMenuItem_setupMacroImmuParam";
            this.toolStripMenuItem_setupMacroImmuParam.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_setupMacroImmuParam.Text = "Setup Immutable";
            this.toolStripMenuItem_setupMacroImmuParam.Click += new System.EventHandler(this.toolStripMenuItem_setupMacroImmuParam_Click);
            // 
            // toolStripMenuItem_setupMacroVarParam
            // 
            this.toolStripMenuItem_setupMacroVarParam.Name = "toolStripMenuItem_setupMacroVarParam";
            this.toolStripMenuItem_setupMacroVarParam.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_setupMacroVarParam.Text = "Setup Variable";
            this.toolStripMenuItem_setupMacroVarParam.Click += new System.EventHandler(this.toolStripMenuItem_setupMacroVarParam_Click);
            // 
            // toolStripMenuItem_setupMacroInnerFunc
            // 
            this.toolStripMenuItem_setupMacroInnerFunc.Name = "toolStripMenuItem_setupMacroInnerFunc";
            this.toolStripMenuItem_setupMacroInnerFunc.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_setupMacroInnerFunc.Text = "Setup Function";
            this.toolStripMenuItem_setupMacroInnerFunc.Click += new System.EventHandler(this.toolStripMenuItem_setupMacroInnerFunc_Click);
            // 
            // toolStripMenuItem_removeMacro
            // 
            this.toolStripMenuItem_removeMacro.Name = "toolStripMenuItem_removeMacro";
            this.toolStripMenuItem_removeMacro.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_removeMacro.Text = "Remove";
            this.toolStripMenuItem_removeMacro.Click += new System.EventHandler(this.toolStripMenuItem_removeMacro_Click);
            // 
            // button_saveAllScripts
            // 
            this.button_saveAllScripts.Location = new System.Drawing.Point(802, 330);
            this.button_saveAllScripts.Name = "button_saveAllScripts";
            this.button_saveAllScripts.Size = new System.Drawing.Size(121, 62);
            this.button_saveAllScripts.TabIndex = 4;
            this.button_saveAllScripts.Text = "Save All";
            this.button_saveAllScripts.UseVisualStyleBackColor = true;
            this.button_saveAllScripts.Click += new System.EventHandler(this.button_saveAllScripts_Click);
            // 
            // button_reloadScript
            // 
            this.button_reloadScript.Location = new System.Drawing.Point(929, 330);
            this.button_reloadScript.Name = "button_reloadScript";
            this.button_reloadScript.Size = new System.Drawing.Size(121, 62);
            this.button_reloadScript.TabIndex = 5;
            this.button_reloadScript.Text = "Reload";
            this.button_reloadScript.UseVisualStyleBackColor = true;
            this.button_reloadScript.Click += new System.EventHandler(this.button_reloadScript_Click);
            // 
            // button_saveOneScript
            // 
            this.button_saveOneScript.Location = new System.Drawing.Point(802, 411);
            this.button_saveOneScript.Name = "button_saveOneScript";
            this.button_saveOneScript.Size = new System.Drawing.Size(121, 62);
            this.button_saveOneScript.TabIndex = 6;
            this.button_saveOneScript.Text = "Save Selection";
            this.button_saveOneScript.UseVisualStyleBackColor = true;
            this.button_saveOneScript.Click += new System.EventHandler(this.button_saveOneScript_Click);
            // 
            // button_savePluginClass
            // 
            this.button_savePluginClass.Location = new System.Drawing.Point(802, 51);
            this.button_savePluginClass.Name = "button_savePluginClass";
            this.button_savePluginClass.Size = new System.Drawing.Size(121, 62);
            this.button_savePluginClass.TabIndex = 4;
            this.button_savePluginClass.Text = "Save Plugins Settings";
            this.button_savePluginClass.UseVisualStyleBackColor = true;
            this.button_savePluginClass.Click += new System.EventHandler(this.button_savePluginClass_Click);
            // 
            // button_reloadPluginClassSettings
            // 
            this.button_reloadPluginClassSettings.Location = new System.Drawing.Point(929, 51);
            this.button_reloadPluginClassSettings.Name = "button_reloadPluginClassSettings";
            this.button_reloadPluginClassSettings.Size = new System.Drawing.Size(121, 62);
            this.button_reloadPluginClassSettings.TabIndex = 4;
            this.button_reloadPluginClassSettings.Text = "Reload Plugins Settings";
            this.button_reloadPluginClassSettings.UseVisualStyleBackColor = true;
            this.button_reloadPluginClassSettings.Click += new System.EventHandler(this.button_reloadPluginClassSettings_Click);
            // 
            // button_runtimeLoadPlugin
            // 
            this.button_runtimeLoadPlugin.Location = new System.Drawing.Point(802, 119);
            this.button_runtimeLoadPlugin.Name = "button_runtimeLoadPlugin";
            this.button_runtimeLoadPlugin.Size = new System.Drawing.Size(121, 62);
            this.button_runtimeLoadPlugin.TabIndex = 4;
            this.button_runtimeLoadPlugin.Text = "Load Plugin Assembly";
            this.button_runtimeLoadPlugin.UseVisualStyleBackColor = true;
            this.button_runtimeLoadPlugin.Click += new System.EventHandler(this.button_runtimeLoadPlugin_Click);
            // 
            // button_classPopupConfig
            // 
            this.button_classPopupConfig.Location = new System.Drawing.Point(929, 119);
            this.button_classPopupConfig.Name = "button_classPopupConfig";
            this.button_classPopupConfig.Size = new System.Drawing.Size(121, 62);
            this.button_classPopupConfig.TabIndex = 7;
            this.button_classPopupConfig.Text = "Popup Class Config";
            this.button_classPopupConfig.UseVisualStyleBackColor = true;
            this.button_classPopupConfig.Click += new System.EventHandler(this.button_classPopupConfig_Click);
            // 
            // button_infiniteRunSelect
            // 
            this.button_infiniteRunSelect.Location = new System.Drawing.Point(802, 494);
            this.button_infiniteRunSelect.Name = "button_infiniteRunSelect";
            this.button_infiniteRunSelect.Size = new System.Drawing.Size(121, 62);
            this.button_infiniteRunSelect.TabIndex = 8;
            this.button_infiniteRunSelect.Text = "Infinite Run Select";
            this.button_infiniteRunSelect.UseVisualStyleBackColor = true;
            this.button_infiniteRunSelect.Click += new System.EventHandler(this.button_infiniteRunSelect_Click);
            // 
            // comboBox_infiniteRunList
            // 
            this.comboBox_infiniteRunList.FormattingEnabled = true;
            this.comboBox_infiniteRunList.Location = new System.Drawing.Point(929, 530);
            this.comboBox_infiniteRunList.Name = "comboBox_infiniteRunList";
            this.comboBox_infiniteRunList.Size = new System.Drawing.Size(121, 26);
            this.comboBox_infiniteRunList.TabIndex = 9;
            // 
            // button_stopRunInfinite
            // 
            this.button_stopRunInfinite.Location = new System.Drawing.Point(929, 494);
            this.button_stopRunInfinite.Name = "button_stopRunInfinite";
            this.button_stopRunInfinite.Size = new System.Drawing.Size(121, 30);
            this.button_stopRunInfinite.TabIndex = 10;
            this.button_stopRunInfinite.Text = "Stop Infinite";
            this.button_stopRunInfinite.UseVisualStyleBackColor = true;
            this.button_stopRunInfinite.Click += new System.EventHandler(this.button_stopRunInfinite_Click);
            // 
            // toolStripMenuItem_accordingTypeToRun
            // 
            this.toolStripMenuItem_accordingTypeToRun.Name = "toolStripMenuItem_accordingTypeToRun";
            this.toolStripMenuItem_accordingTypeToRun.Size = new System.Drawing.Size(258, 22);
            this.toolStripMenuItem_accordingTypeToRun.Text = "According type to run";
            this.toolStripMenuItem_accordingTypeToRun.Click += new System.EventHandler(this.toolStripMenuItem_accordingTypeToRun_Click);
            // 
            // FrmScriptEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1075, 582);
            this.Controls.Add(this.button_stopRunInfinite);
            this.Controls.Add(this.comboBox_infiniteRunList);
            this.Controls.Add(this.button_infiniteRunSelect);
            this.Controls.Add(this.button_classPopupConfig);
            this.Controls.Add(this.button_saveOneScript);
            this.Controls.Add(this.button_reloadScript);
            this.Controls.Add(this.button_runtimeLoadPlugin);
            this.Controls.Add(this.button_reloadPluginClassSettings);
            this.Controls.Add(this.button_savePluginClass);
            this.Controls.Add(this.button_saveAllScripts);
            this.Controls.Add(this.treeView_scripts);
            this.Controls.Add(this.label_scriptEdit);
            this.Controls.Add(this.label_plugins);
            this.Controls.Add(this.treeView_methodsOfPlugin);
            this.Font = new System.Drawing.Font("Tahoma", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.Name = "FrmScriptEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Script";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmScriptEditor_FormClosing);
            this.contextMenuStrip_pluginsRightClk.ResumeLayout(false);
            this.contextMenuStrip_scriptRightCkl.ResumeLayout(false);
            this.contextMenuStrip_scriptRightCkl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView_methodsOfPlugin;
        private System.Windows.Forms.Label label_plugins;
        private System.Windows.Forms.Label label_scriptEdit;
        private System.Windows.Forms.TreeView treeView_scripts;
        private System.Windows.Forms.Button button_saveAllScripts;
        private System.Windows.Forms.Button button_reloadScript;
        private System.Windows.Forms.Button button_saveOneScript;
        private System.Windows.Forms.Button button_savePluginClass;
        private System.Windows.Forms.Button button_reloadPluginClassSettings;
        private System.Windows.Forms.Button button_runtimeLoadPlugin;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_pluginsRightClk;
        private System.Windows.Forms.ToolStripMenuItem popupPluginClassSetupToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_scriptRightCkl;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_newScript;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_deleteScript;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_scriptOP;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox_macroOP;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_setupMacroImmuParam;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_setupMacroVarParam;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_setupMacroInnerFunc;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_removeMacro;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_scriptToRun;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_scriptRename;
        private System.Windows.Forms.Button button_classPopupConfig;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_runWithoutFreePropagation;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_withFreeMacroProgation;
        private System.Windows.Forms.Button button_infiniteRunSelect;
        private System.Windows.Forms.ComboBox comboBox_infiniteRunList;
        private System.Windows.Forms.Button button_stopRunInfinite;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_scriptEnableLog;
        private System.Windows.Forms.ToolStripMenuItem useMainThreadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_accordingTypeToRun;
    }
}