//*****************************************************************************

// Copyright (C) 1999-2002, Microsoft Corporation.  All Rights Reserved.

//*****************************************************************************

Option Compare Text;

using Microsoft.VisualBasic;

using System;

using System.Reflection;

using System.Diagnostics;

using System.Collections;

using System.Drawing;

using System.Windows.Forms;

public class MainWindow : : Form;

    // The main user interface window.

    public const WINDOW_TITLE string = "XML Documentation Tool";

    public const SAVE_MESSAGE string = "Do you want to save changes to the current XML Documentation file?";

    // these constants are used to look up data in the registry

    public const APPNAME string = WINDOW_TITLE           ' the application name;

    public const HISTORYFILES string = "History Files"   ' the registry group for history files;

    private m_Doc MainDoc                        'The core engine instance.;

    private m_AsmFileName string                 'The filename of the assembly.;

    private m_XMLFileName string                 'The filename of the XML Documentation file.;

    private m_CopyNode MemberNode                'Store the node copied;

    private m_FindDialog FindDialog   'The Find dialog instance.;

#region " Windows Form Designer generated code";

    //Form overrides dispose to clean up the component list.

    protected override void Dispose(bool disposing) {

        base.Dispose(disposing);

        if (components != null) {

            components.Dispose();

        }

    }

    public MdiClient System.Windows.Forms.MdiClient;

    public TreeView XMLDocumentationTool.AssemblyTreeView;

    public ErrorWindow XMLDocumentationTool.ErrorWindow;

    private StatusPanelErrorInformation System.Windows.Forms.StatusBarPanel;

    private MenuEdit System.Windows.Forms.MenuItem;

    private MenuEditCut System.Windows.Forms.MenuItem;

    private MenuEditCopy System.Windows.Forms.MenuItem;

    private MenuEditPaste System.Windows.Forms.MenuItem;

    private MenuEditSplitter1 System.Windows.Forms.MenuItem;

    private MenuEditSplitter2 System.Windows.Forms.MenuItem;

    private MenuEditFind System.Windows.Forms.MenuItem;

    private MenuFileSaveAs System.Windows.Forms.MenuItem;

    private MenuFileExit System.Windows.Forms.MenuItem;

    private MenuFileOpenXML System.Windows.Forms.MenuItem;

    private MenuFileSave System.Windows.Forms.MenuItem;

    private MainSplitter System.Windows.Forms.Splitter;

    private MainMenu System.Windows.Forms.MainMenu;

    private MenuFile System.Windows.Forms.MenuItem;

    private MenuFileSplitter System.Windows.Forms.MenuItem;

    private MenuView System.Windows.Forms.MenuItem;

    private MenuViewExpandAll System.Windows.Forms.MenuItem;

    private MenuViewCollapseAll System.Windows.Forms.MenuItem;

    private MenuViewShowErrorWindow System.Windows.Forms.MenuItem;

    private MenuWindow System.Windows.Forms.MenuItem;

    private MenuWindowCascade System.Windows.Forms.MenuItem;

    private MenuWindowTile System.Windows.Forms.MenuItem;

    private MenuWindowArrangeIcons System.Windows.Forms.MenuItem;

    private MenuWindowCloseAll System.Windows.Forms.MenuItem;

    private MainStatusBar System.Windows.Forms.StatusBar;

    private StatusPanelAction System.Windows.Forms.StatusBarPanel;

    private TreeViewContextMenu System.Windows.Forms.ContextMenu;

    private TreeViewContextMenuOpen System.Windows.Forms.MenuItem;

    private MenuViewSplitter System.Windows.Forms.MenuItem;

    private ErrorPanelSplitter System.Windows.Forms.Splitter;

    private ImageList System.Windows.Forms.ImageList;

    private ErrorPanel System.Windows.Forms.Panel;

    private ErrorWindowTitle System.Windows.Forms.Label;

    private MenuEditDeleteAllXMLNodes System.Windows.Forms.MenuItem;

    private MenuViewVersionInfo System.Windows.Forms.MenuItem;

    private MenuFileOpen System.Windows.Forms.MenuItem;

    //Required by the Windows Form Designer

    private System.ComponentModel.IContainer components = null;

    //NOTE: The following procedure is required by the Windows Form Designer

    //It can be modified using the Windows Form Designer.  

    //Do ! modify it using the code editor.

    internal MenuEditDeleteError System.Windows.Forms.MenuItem;

    private MenuEditClearNode System.Windows.Forms.MenuItem;

    private TreeViewContextMenuClear System.Windows.Forms.MenuItem;

    private MenuHelpMenu System.Windows.Forms.MenuItem;

    internal MenuHelp System.Windows.Forms.MenuItem;

    internal MenuHelpAbout System.Windows.Forms.MenuItem;

    private void InitializeComponent() {

        this.components = new System.ComponentModel.Container();

        resources System.Resources.ResourceManager = new System.Resources.ResourceManager(typeof(MainWindow));

        this.MdiClient = new System.Windows.Forms.MdiClient();

        this.MainStatusBar = new System.Windows.Forms.StatusBar();

        this.StatusPanelAction = new System.Windows.Forms.StatusBarPanel();

        this.StatusPanelErrorInformation = new System.Windows.Forms.StatusBarPanel();

        this.MenuEdit = new System.Windows.Forms.MenuItem();

        this.MenuEditCut = new System.Windows.Forms.MenuItem();

        this.MenuEditCopy = new System.Windows.Forms.MenuItem();

        this.MenuEditPaste = new System.Windows.Forms.MenuItem();

        this.MenuEditSplitter1 = new System.Windows.Forms.MenuItem();

        this.MenuEditClearNode = new System.Windows.Forms.MenuItem();

        this.MenuEditDeleteAllXMLNodes = new System.Windows.Forms.MenuItem();

        this.MenuEditDeleteError = new System.Windows.Forms.MenuItem();

        this.MenuEditSplitter2 = new System.Windows.Forms.MenuItem();

        this.MenuEditFind = new System.Windows.Forms.MenuItem();

        this.TreeView = new XMLDocumentationTool.AssemblyTreeView();

        this.TreeViewContextMenu = new System.Windows.Forms.ContextMenu();

        this.TreeViewContextMenuOpen = new System.Windows.Forms.MenuItem();

        this.TreeViewContextMenuClear = new System.Windows.Forms.MenuItem();

        this.ImageList = new System.Windows.Forms.ImageList(this.components);

        this.MenuFileSaveAs = new System.Windows.Forms.MenuItem();

        this.MenuFileExit = new System.Windows.Forms.MenuItem();

        this.MenuFileOpenXML = new System.Windows.Forms.MenuItem();

        this.MenuFileSave = new System.Windows.Forms.MenuItem();

        this.MainSplitter = new System.Windows.Forms.Splitter();

        this.MainMenu = new System.Windows.Forms.MainMenu();

        this.MenuFile = new System.Windows.Forms.MenuItem();

        this.MenuFileOpen = new System.Windows.Forms.MenuItem();

        this.MenuFileSplitter = new System.Windows.Forms.MenuItem();

        this.MenuView = new System.Windows.Forms.MenuItem();

        this.MenuViewExpandAll = new System.Windows.Forms.MenuItem();

        this.MenuViewCollapseAll = new System.Windows.Forms.MenuItem();

        this.MenuViewSplitter = new System.Windows.Forms.MenuItem();

        this.MenuViewShowErrorWindow = new System.Windows.Forms.MenuItem();

        this.MenuViewVersionInfo = new System.Windows.Forms.MenuItem();

        this.MenuWindow = new System.Windows.Forms.MenuItem();

        this.MenuWindowCascade = new System.Windows.Forms.MenuItem();

        this.MenuWindowTile = new System.Windows.Forms.MenuItem();

        this.MenuWindowArrangeIcons = new System.Windows.Forms.MenuItem();

        this.MenuWindowCloseAll = new System.Windows.Forms.MenuItem();

        this.MenuHelpMenu = new System.Windows.Forms.MenuItem();

        this.MenuHelp = new System.Windows.Forms.MenuItem();

        this.MenuHelpAbout = new System.Windows.Forms.MenuItem();

        this.ErrorPanel = new System.Windows.Forms.Panel();

        this.ErrorWindow = new XMLDocumentationTool.ErrorWindow();

        this.ErrorPanelSplitter = new System.Windows.Forms.Splitter();

        this.ErrorWindowTitle = new System.Windows.Forms.Label();

        CType(this.StatusPanelAction, System.ComponentModel.ISupportInitialize).BeginInit();

        CType(this.StatusPanelErrorInformation, System.ComponentModel.ISupportInitialize).BeginInit();

        this.ErrorPanel.SuspendLayout();

        this.SuspendLayout();

        //

        //MdiClient

        //

        this.MdiClient.AccessibleDescription = CType(resources.GetObject("MdiClient.AccessibleDescription"), string);

        this.MdiClient.AccessibleName = CType(resources.GetObject("MdiClient.AccessibleName"), string);

        this.MdiClient.Anchor = CType(resources.GetObject("MdiClient.Anchor"), System.Windows.Forms.AnchorStyles);

        this.MdiClient.BackgroundImage = CType(resources.GetObject("MdiClient.BackgroundImage"), System.Drawing.Image);

        this.MdiClient.Dock = CType(resources.GetObject("MdiClient.Dock"), System.Windows.Forms.DockStyle);

        this.MdiClient.Enabled = CType(resources.GetObject("MdiClient.Enabled"), bool);

        this.MdiClient.Font = CType(resources.GetObject("MdiClient.Font"), System.Drawing.Font);

        this.MdiClient.ImeMode = CType(resources.GetObject("MdiClient.ImeMode"), System.Windows.Forms.ImeMode);

        this.MdiClient.Location = CType(resources.GetObject("MdiClient.Location"), System.Drawing.Point);

        this.MdiClient.Name = "MdiClient";

        this.MdiClient.RightToLeft = CType(resources.GetObject("MdiClient.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.MdiClient.Size = CType(resources.GetObject("MdiClient.Size"), System.Drawing.Size);

        this.MdiClient.TabIndex = CType(resources.GetObject("MdiClient.TabIndex"), int);

        this.MdiClient.Text = resources.GetString("MdiClient.Text");

        this.MdiClient.Visible = CType(resources.GetObject("MdiClient.Visible"), bool);

        //

        //MainStatusBar

        //

        this.MainStatusBar.AccessibleDescription = CType(resources.GetObject("MainStatusBar.AccessibleDescription"), string);

        this.MainStatusBar.AccessibleName = CType(resources.GetObject("MainStatusBar.AccessibleName"), string);

        this.MainStatusBar.Anchor = CType(resources.GetObject("MainStatusBar.Anchor"), System.Windows.Forms.AnchorStyles);

        this.MainStatusBar.BackgroundImage = CType(resources.GetObject("MainStatusBar.BackgroundImage"), System.Drawing.Image);

        this.MainStatusBar.Dock = CType(resources.GetObject("MainStatusBar.Dock"), System.Windows.Forms.DockStyle);

        this.MainStatusBar.Enabled = CType(resources.GetObject("MainStatusBar.Enabled"), bool);

        this.MainStatusBar.Font = CType(resources.GetObject("MainStatusBar.Font"), System.Drawing.Font);

        this.MainStatusBar.ImeMode = CType(resources.GetObject("MainStatusBar.ImeMode"), System.Windows.Forms.ImeMode);

        this.MainStatusBar.Location = CType(resources.GetObject("MainStatusBar.Location"), System.Drawing.Point);

        this.MainStatusBar.Name = "MainStatusBar";

        this.MainStatusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {this.StatusPanelAction, this.StatusPanelErrorInformation});

        this.MainStatusBar.RightToLeft = CType(resources.GetObject("MainStatusBar.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.MainStatusBar.ShowPanels = true;

        this.MainStatusBar.Size = CType(resources.GetObject("MainStatusBar.Size"), System.Drawing.Size);

        this.MainStatusBar.TabIndex = CType(resources.GetObject("MainStatusBar.TabIndex"), int);

        this.MainStatusBar.Text = resources.GetString("MainStatusBar.Text");

        this.MainStatusBar.Visible = CType(resources.GetObject("MainStatusBar.Visible"), bool);

        //

        //StatusPanelAction

        //

        this.StatusPanelAction.Alignment = CType(resources.GetObject("StatusPanelAction.Alignment"), System.Windows.Forms.HorizontalAlignment);

        this.StatusPanelAction.Icon = CType(resources.GetObject("StatusPanelAction.Icon"), System.Drawing.Icon);

        this.StatusPanelAction.MinWidth = CType(resources.GetObject("StatusPanelAction.MinWidth"), int);

        this.StatusPanelAction.Text = resources.GetString("StatusPanelAction.Text");

        this.StatusPanelAction.ToolTipText = resources.GetString("StatusPanelAction.ToolTipText");

        this.StatusPanelAction.Width = CType(resources.GetObject("StatusPanelAction.Width"), int);

        //

        //StatusPanelErrorInformation

        //

        this.StatusPanelErrorInformation.Alignment = CType(resources.GetObject("StatusPanelErrorInformation.Alignment"), System.Windows.Forms.HorizontalAlignment);

        this.StatusPanelErrorInformation.Icon = CType(resources.GetObject("StatusPanelErrorInformation.Icon"), System.Drawing.Icon);

        this.StatusPanelErrorInformation.MinWidth = CType(resources.GetObject("StatusPanelErrorInformation.MinWidth"), int);

        this.StatusPanelErrorInformation.Text = resources.GetString("StatusPanelErrorInformation.Text");

        this.StatusPanelErrorInformation.ToolTipText = resources.GetString("StatusPanelErrorInformation.ToolTipText");

        this.StatusPanelErrorInformation.Width = CType(resources.GetObject("StatusPanelErrorInformation.Width"), int);

        //

        //MenuEdit

        //

        this.MenuEdit.Enabled = CType(resources.GetObject("MenuEdit.Enabled"), bool);

        this.MenuEdit.Index = 1;

        this.MenuEdit.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuEditCut, this.MenuEditCopy, this.MenuEditPaste, this.MenuEditSplitter1, this.MenuEditClearNode, this.MenuEditDeleteAllXMLNodes, this.MenuEditDeleteError, this.MenuEditSplitter2, this.MenuEditFind});

        this.MenuEdit.Shortcut = CType(resources.GetObject("MenuEdit.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEdit.ShowShortcut = CType(resources.GetObject("MenuEdit.ShowShortcut"), bool);

        this.MenuEdit.Text = resources.GetString("MenuEdit.Text");

        this.MenuEdit.Visible = CType(resources.GetObject("MenuEdit.Visible"), bool);

        //

        //MenuEditCut

        //

        this.MenuEditCut.Enabled = CType(resources.GetObject("MenuEditCut.Enabled"), bool);

        this.MenuEditCut.Index = 0;

        this.MenuEditCut.Shortcut = CType(resources.GetObject("MenuEditCut.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditCut.ShowShortcut = CType(resources.GetObject("MenuEditCut.ShowShortcut"), bool);

        this.MenuEditCut.Text = resources.GetString("MenuEditCut.Text");

        this.MenuEditCut.Visible = CType(resources.GetObject("MenuEditCut.Visible"), bool);

        //

        //MenuEditCopy

        //

        this.MenuEditCopy.Enabled = CType(resources.GetObject("MenuEditCopy.Enabled"), bool);

        this.MenuEditCopy.Index = 1;

        this.MenuEditCopy.Shortcut = CType(resources.GetObject("MenuEditCopy.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditCopy.ShowShortcut = CType(resources.GetObject("MenuEditCopy.ShowShortcut"), bool);

        this.MenuEditCopy.Text = resources.GetString("MenuEditCopy.Text");

        this.MenuEditCopy.Visible = CType(resources.GetObject("MenuEditCopy.Visible"), bool);

        //

        //MenuEditPaste

        //

        this.MenuEditPaste.Enabled = CType(resources.GetObject("MenuEditPaste.Enabled"), bool);

        this.MenuEditPaste.Index = 2;

        this.MenuEditPaste.Shortcut = CType(resources.GetObject("MenuEditPaste.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditPaste.ShowShortcut = CType(resources.GetObject("MenuEditPaste.ShowShortcut"), bool);

        this.MenuEditPaste.Text = resources.GetString("MenuEditPaste.Text");

        this.MenuEditPaste.Visible = CType(resources.GetObject("MenuEditPaste.Visible"), bool);

        //

        //MenuEditSplitter1

        //

        this.MenuEditSplitter1.Enabled = CType(resources.GetObject("MenuEditSplitter1.Enabled"), bool);

        this.MenuEditSplitter1.Index = 3;

        this.MenuEditSplitter1.Shortcut = CType(resources.GetObject("MenuEditSplitter1.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditSplitter1.ShowShortcut = CType(resources.GetObject("MenuEditSplitter1.ShowShortcut"), bool);

        this.MenuEditSplitter1.Text = resources.GetString("MenuEditSplitter1.Text");

        this.MenuEditSplitter1.Visible = CType(resources.GetObject("MenuEditSplitter1.Visible"), bool);

        //

        //MenuEditClearNode

        //

        this.MenuEditClearNode.Enabled = CType(resources.GetObject("MenuEditClearNode.Enabled"), bool);

        this.MenuEditClearNode.Index = 4;

        this.MenuEditClearNode.Shortcut = CType(resources.GetObject("MenuEditClearNode.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditClearNode.ShowShortcut = CType(resources.GetObject("MenuEditClearNode.ShowShortcut"), bool);

        this.MenuEditClearNode.Text = resources.GetString("MenuEditClearNode.Text");

        this.MenuEditClearNode.Visible = CType(resources.GetObject("MenuEditClearNode.Visible"), bool);

        //

        //MenuEditDeleteAllXMLNodes

        //

        this.MenuEditDeleteAllXMLNodes.Enabled = CType(resources.GetObject("MenuEditDeleteAllXMLNodes.Enabled"), bool);

        this.MenuEditDeleteAllXMLNodes.Index = 5;

        this.MenuEditDeleteAllXMLNodes.Shortcut = CType(resources.GetObject("MenuEditDeleteAllXMLNodes.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditDeleteAllXMLNodes.ShowShortcut = CType(resources.GetObject("MenuEditDeleteAllXMLNodes.ShowShortcut"), bool);

        this.MenuEditDeleteAllXMLNodes.Text = resources.GetString("MenuEditDeleteAllXMLNodes.Text");

        this.MenuEditDeleteAllXMLNodes.Visible = CType(resources.GetObject("MenuEditDeleteAllXMLNodes.Visible"), bool);

        //

        //MenuEditDeleteError

        //

        this.MenuEditDeleteError.Enabled = CType(resources.GetObject("MenuEditDeleteError.Enabled"), bool);

        this.MenuEditDeleteError.Index = 6;

        this.MenuEditDeleteError.Shortcut = CType(resources.GetObject("MenuEditDeleteError.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditDeleteError.ShowShortcut = CType(resources.GetObject("MenuEditDeleteError.ShowShortcut"), bool);

        this.MenuEditDeleteError.Text = resources.GetString("MenuEditDeleteError.Text");

        this.MenuEditDeleteError.Visible = CType(resources.GetObject("MenuEditDeleteError.Visible"), bool);

        //

        //MenuEditSplitter2

        //

        this.MenuEditSplitter2.Enabled = CType(resources.GetObject("MenuEditSplitter2.Enabled"), bool);

        this.MenuEditSplitter2.Index = 7;

        this.MenuEditSplitter2.Shortcut = CType(resources.GetObject("MenuEditSplitter2.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditSplitter2.ShowShortcut = CType(resources.GetObject("MenuEditSplitter2.ShowShortcut"), bool);

        this.MenuEditSplitter2.Text = resources.GetString("MenuEditSplitter2.Text");

        this.MenuEditSplitter2.Visible = CType(resources.GetObject("MenuEditSplitter2.Visible"), bool);

        //

        //MenuEditFind

        //

        this.MenuEditFind.Enabled = CType(resources.GetObject("MenuEditFind.Enabled"), bool);

        this.MenuEditFind.Index = 8;

        this.MenuEditFind.Shortcut = CType(resources.GetObject("MenuEditFind.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuEditFind.ShowShortcut = CType(resources.GetObject("MenuEditFind.ShowShortcut"), bool);

        this.MenuEditFind.Text = resources.GetString("MenuEditFind.Text");

        this.MenuEditFind.Visible = CType(resources.GetObject("MenuEditFind.Visible"), bool);

        //

        //TreeView

        //

        this.TreeView.AccessibleDescription = CType(resources.GetObject("TreeView.AccessibleDescription"), string);

        this.TreeView.AccessibleName = CType(resources.GetObject("TreeView.AccessibleName"), string);

        this.TreeView.AllowDrop = true;

        this.TreeView.Anchor = CType(resources.GetObject("TreeView.Anchor"), System.Windows.Forms.AnchorStyles);

        this.TreeView.BackgroundImage = CType(resources.GetObject("TreeView.BackgroundImage"), System.Drawing.Image);

        this.TreeView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

        this.TreeView.ContextMenu = this.TreeViewContextMenu;

        this.TreeView.Dock = CType(resources.GetObject("TreeView.Dock"), System.Windows.Forms.DockStyle);

        this.TreeView.Enabled = CType(resources.GetObject("TreeView.Enabled"), bool);

        this.TreeView.Font = CType(resources.GetObject("TreeView.Font"), System.Drawing.Font);

        this.TreeView.HideSelection = false;

        this.TreeView.ImageIndex = CType(resources.GetObject("TreeView.ImageIndex"), int);

        this.TreeView.ImageList = this.ImageList;

        this.TreeView.ImeMode = CType(resources.GetObject("TreeView.ImeMode"), System.Windows.Forms.ImeMode);

        this.TreeView.Indent = CType(resources.GetObject("TreeView.Indent"), int);

        this.TreeView.ItemHeight = CType(resources.GetObject("TreeView.ItemHeight"), int);

        this.TreeView.Location = CType(resources.GetObject("TreeView.Location"), System.Drawing.Point);

        this.TreeView.Name = "TreeView";

        this.TreeView.RightToLeft = CType(resources.GetObject("TreeView.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.TreeView.SelectedImageIndex = CType(resources.GetObject("TreeView.SelectedImageIndex"), int);

        this.TreeView.Size = CType(resources.GetObject("TreeView.Size"), System.Drawing.Size);

        this.TreeView.Sorted = true;

        this.TreeView.TabIndex = CType(resources.GetObject("TreeView.TabIndex"), int);

        this.TreeView.Text = resources.GetString("TreeView.Text");

        this.TreeView.Visible = CType(resources.GetObject("TreeView.Visible"), bool);

        //

        //TreeViewContextMenu

        //

        this.TreeViewContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.TreeViewContextMenuOpen, this.TreeViewContextMenuClear});

        this.TreeViewContextMenu.RightToLeft = CType(resources.GetObject("TreeViewContextMenu.RightToLeft"), System.Windows.Forms.RightToLeft);

        //

        //TreeViewContextMenuOpen

        //

        this.TreeViewContextMenuOpen.Enabled = CType(resources.GetObject("TreeViewContextMenuOpen.Enabled"), bool);

        this.TreeViewContextMenuOpen.Index = 0;

        this.TreeViewContextMenuOpen.Shortcut = CType(resources.GetObject("TreeViewContextMenuOpen.Shortcut"), System.Windows.Forms.Shortcut);

        this.TreeViewContextMenuOpen.ShowShortcut = CType(resources.GetObject("TreeViewContextMenuOpen.ShowShortcut"), bool);

        this.TreeViewContextMenuOpen.Text = resources.GetString("TreeViewContextMenuOpen.Text");

        this.TreeViewContextMenuOpen.Visible = CType(resources.GetObject("TreeViewContextMenuOpen.Visible"), bool);

        //

        //TreeViewContextMenuClear

        //

        this.TreeViewContextMenuClear.Enabled = CType(resources.GetObject("TreeViewContextMenuClear.Enabled"), bool);

        this.TreeViewContextMenuClear.Index = 1;

        this.TreeViewContextMenuClear.Shortcut = CType(resources.GetObject("TreeViewContextMenuClear.Shortcut"), System.Windows.Forms.Shortcut);

        this.TreeViewContextMenuClear.ShowShortcut = CType(resources.GetObject("TreeViewContextMenuClear.ShowShortcut"), bool);

        this.TreeViewContextMenuClear.Text = resources.GetString("TreeViewContextMenuClear.Text");

        this.TreeViewContextMenuClear.Visible = CType(resources.GetObject("TreeViewContextMenuClear.Visible"), bool);

        //

        //ImageList

        //

        this.ImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;

        this.ImageList.ImageSize = CType(resources.GetObject("ImageList.ImageSize"), System.Drawing.Size);

        this.ImageList.TransparentColor = System.Drawing.Color.Lime;

        //

        //MenuFileSaveAs

        //

        this.MenuFileSaveAs.Enabled = CType(resources.GetObject("MenuFileSaveAs.Enabled"), bool);

        this.MenuFileSaveAs.Index = 3;

        this.MenuFileSaveAs.Shortcut = CType(resources.GetObject("MenuFileSaveAs.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileSaveAs.ShowShortcut = CType(resources.GetObject("MenuFileSaveAs.ShowShortcut"), bool);

        this.MenuFileSaveAs.Text = resources.GetString("MenuFileSaveAs.Text");

        this.MenuFileSaveAs.Visible = CType(resources.GetObject("MenuFileSaveAs.Visible"), bool);

        //

        //MenuFileExit

        //

        this.MenuFileExit.Enabled = CType(resources.GetObject("MenuFileExit.Enabled"), bool);

        this.MenuFileExit.Index = 5;

        this.MenuFileExit.Shortcut = CType(resources.GetObject("MenuFileExit.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileExit.ShowShortcut = CType(resources.GetObject("MenuFileExit.ShowShortcut"), bool);

        this.MenuFileExit.Text = resources.GetString("MenuFileExit.Text");

        this.MenuFileExit.Visible = CType(resources.GetObject("MenuFileExit.Visible"), bool);

        //

        //MenuFileOpenXML

        //

        this.MenuFileOpenXML.Enabled = CType(resources.GetObject("MenuFileOpenXML.Enabled"), bool);

        this.MenuFileOpenXML.Index = 1;

        this.MenuFileOpenXML.Shortcut = CType(resources.GetObject("MenuFileOpenXML.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileOpenXML.ShowShortcut = CType(resources.GetObject("MenuFileOpenXML.ShowShortcut"), bool);

        this.MenuFileOpenXML.Text = resources.GetString("MenuFileOpenXML.Text");

        this.MenuFileOpenXML.Visible = CType(resources.GetObject("MenuFileOpenXML.Visible"), bool);

        //

        //MenuFileSave

        //

        this.MenuFileSave.Enabled = CType(resources.GetObject("MenuFileSave.Enabled"), bool);

        this.MenuFileSave.Index = 2;

        this.MenuFileSave.Shortcut = CType(resources.GetObject("MenuFileSave.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileSave.ShowShortcut = CType(resources.GetObject("MenuFileSave.ShowShortcut"), bool);

        this.MenuFileSave.Text = resources.GetString("MenuFileSave.Text");

        this.MenuFileSave.Visible = CType(resources.GetObject("MenuFileSave.Visible"), bool);

        //

        //MainSplitter

        //

        this.MainSplitter.AccessibleDescription = CType(resources.GetObject("MainSplitter.AccessibleDescription"), string);

        this.MainSplitter.AccessibleName = CType(resources.GetObject("MainSplitter.AccessibleName"), string);

        this.MainSplitter.Anchor = CType(resources.GetObject("MainSplitter.Anchor"), System.Windows.Forms.AnchorStyles);

        this.MainSplitter.BackgroundImage = CType(resources.GetObject("MainSplitter.BackgroundImage"), System.Drawing.Image);

        this.MainSplitter.Dock = CType(resources.GetObject("MainSplitter.Dock"), System.Windows.Forms.DockStyle);

        this.MainSplitter.Enabled = CType(resources.GetObject("MainSplitter.Enabled"), bool);

        this.MainSplitter.Font = CType(resources.GetObject("MainSplitter.Font"), System.Drawing.Font);

        this.MainSplitter.ImeMode = CType(resources.GetObject("MainSplitter.ImeMode"), System.Windows.Forms.ImeMode);

        this.MainSplitter.Location = CType(resources.GetObject("MainSplitter.Location"), System.Drawing.Point);

        this.MainSplitter.MinExtra = CType(resources.GetObject("MainSplitter.MinExtra"), int);

        this.MainSplitter.MinSize = CType(resources.GetObject("MainSplitter.MinSize"), int);

        this.MainSplitter.Name = "MainSplitter";

        this.MainSplitter.RightToLeft = CType(resources.GetObject("MainSplitter.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.MainSplitter.Size = CType(resources.GetObject("MainSplitter.Size"), System.Drawing.Size);

        this.MainSplitter.TabIndex = CType(resources.GetObject("MainSplitter.TabIndex"), int);

        this.MainSplitter.TabStop = false;

        this.MainSplitter.Visible = CType(resources.GetObject("MainSplitter.Visible"), bool);

        //

        //MainMenu

        //

        this.MainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuFile, this.MenuEdit, this.MenuView, this.MenuWindow, this.MenuHelpMenu});

        this.MainMenu.RightToLeft = CType(resources.GetObject("MainMenu.RightToLeft"), System.Windows.Forms.RightToLeft);

        //

        //MenuFile

        //

        this.MenuFile.Enabled = CType(resources.GetObject("MenuFile.Enabled"), bool);

        this.MenuFile.Index = 0;

        this.MenuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuFileOpen, this.MenuFileOpenXML, this.MenuFileSave, this.MenuFileSaveAs, this.MenuFileSplitter, this.MenuFileExit});

        this.MenuFile.Shortcut = CType(resources.GetObject("MenuFile.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFile.ShowShortcut = CType(resources.GetObject("MenuFile.ShowShortcut"), bool);

        this.MenuFile.Text = resources.GetString("MenuFile.Text");

        this.MenuFile.Visible = CType(resources.GetObject("MenuFile.Visible"), bool);

        //

        //MenuFileOpen

        //

        this.MenuFileOpen.Enabled = CType(resources.GetObject("MenuFileOpen.Enabled"), bool);

        this.MenuFileOpen.Index = 0;

        this.MenuFileOpen.Shortcut = CType(resources.GetObject("MenuFileOpen.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileOpen.ShowShortcut = CType(resources.GetObject("MenuFileOpen.ShowShortcut"), bool);

        this.MenuFileOpen.Text = resources.GetString("MenuFileOpen.Text");

        this.MenuFileOpen.Visible = CType(resources.GetObject("MenuFileOpen.Visible"), bool);

        //

        //MenuFileSplitter

        //

        this.MenuFileSplitter.Enabled = CType(resources.GetObject("MenuFileSplitter.Enabled"), bool);

        this.MenuFileSplitter.Index = 4;

        this.MenuFileSplitter.Shortcut = CType(resources.GetObject("MenuFileSplitter.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuFileSplitter.ShowShortcut = CType(resources.GetObject("MenuFileSplitter.ShowShortcut"), bool);

        this.MenuFileSplitter.Text = resources.GetString("MenuFileSplitter.Text");

        this.MenuFileSplitter.Visible = CType(resources.GetObject("MenuFileSplitter.Visible"), bool);

        //

        //MenuView

        //

        this.MenuView.Enabled = CType(resources.GetObject("MenuView.Enabled"), bool);

        this.MenuView.Index = 2;

        this.MenuView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuViewExpandAll, this.MenuViewCollapseAll, this.MenuViewSplitter, this.MenuViewShowErrorWindow, this.MenuViewVersionInfo});

        this.MenuView.Shortcut = CType(resources.GetObject("MenuView.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuView.ShowShortcut = CType(resources.GetObject("MenuView.ShowShortcut"), bool);

        this.MenuView.Text = resources.GetString("MenuView.Text");

        this.MenuView.Visible = CType(resources.GetObject("MenuView.Visible"), bool);

        //

        //MenuViewExpandAll

        //

        this.MenuViewExpandAll.Enabled = CType(resources.GetObject("MenuViewExpandAll.Enabled"), bool);

        this.MenuViewExpandAll.Index = 0;

        this.MenuViewExpandAll.Shortcut = CType(resources.GetObject("MenuViewExpandAll.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuViewExpandAll.ShowShortcut = CType(resources.GetObject("MenuViewExpandAll.ShowShortcut"), bool);

        this.MenuViewExpandAll.Text = resources.GetString("MenuViewExpandAll.Text");

        this.MenuViewExpandAll.Visible = CType(resources.GetObject("MenuViewExpandAll.Visible"), bool);

        //

        //MenuViewCollapseAll

        //

        this.MenuViewCollapseAll.Enabled = CType(resources.GetObject("MenuViewCollapseAll.Enabled"), bool);

        this.MenuViewCollapseAll.Index = 1;

        this.MenuViewCollapseAll.Shortcut = CType(resources.GetObject("MenuViewCollapseAll.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuViewCollapseAll.ShowShortcut = CType(resources.GetObject("MenuViewCollapseAll.ShowShortcut"), bool);

        this.MenuViewCollapseAll.Text = resources.GetString("MenuViewCollapseAll.Text");

        this.MenuViewCollapseAll.Visible = CType(resources.GetObject("MenuViewCollapseAll.Visible"), bool);

        //

        //MenuViewSplitter

        //

        this.MenuViewSplitter.Enabled = CType(resources.GetObject("MenuViewSplitter.Enabled"), bool);

        this.MenuViewSplitter.Index = 2;

        this.MenuViewSplitter.Shortcut = CType(resources.GetObject("MenuViewSplitter.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuViewSplitter.ShowShortcut = CType(resources.GetObject("MenuViewSplitter.ShowShortcut"), bool);

        this.MenuViewSplitter.Text = resources.GetString("MenuViewSplitter.Text");

        this.MenuViewSplitter.Visible = CType(resources.GetObject("MenuViewSplitter.Visible"), bool);

        //

        //MenuViewShowErrorWindow

        //

        this.MenuViewShowErrorWindow.Enabled = CType(resources.GetObject("MenuViewShowErrorWindow.Enabled"), bool);

        this.MenuViewShowErrorWindow.Index = 3;

        this.MenuViewShowErrorWindow.Shortcut = CType(resources.GetObject("MenuViewShowErrorWindow.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuViewShowErrorWindow.ShowShortcut = CType(resources.GetObject("MenuViewShowErrorWindow.ShowShortcut"), bool);

        this.MenuViewShowErrorWindow.Text = resources.GetString("MenuViewShowErrorWindow.Text");

        this.MenuViewShowErrorWindow.Visible = CType(resources.GetObject("MenuViewShowErrorWindow.Visible"), bool);

        //

        //MenuViewVersionInfo

        //

        this.MenuViewVersionInfo.Enabled = CType(resources.GetObject("MenuViewVersionInfo.Enabled"), bool);

        this.MenuViewVersionInfo.Index = 4;

        this.MenuViewVersionInfo.Shortcut = CType(resources.GetObject("MenuViewVersionInfo.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuViewVersionInfo.ShowShortcut = CType(resources.GetObject("MenuViewVersionInfo.ShowShortcut"), bool);

        this.MenuViewVersionInfo.Text = resources.GetString("MenuViewVersionInfo.Text");

        this.MenuViewVersionInfo.Visible = CType(resources.GetObject("MenuViewVersionInfo.Visible"), bool);

        //

        //MenuWindow

        //

        this.MenuWindow.Enabled = CType(resources.GetObject("MenuWindow.Enabled"), bool);

        this.MenuWindow.Index = 3;

        this.MenuWindow.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuWindowCascade, this.MenuWindowTile, this.MenuWindowArrangeIcons, this.MenuWindowCloseAll});

        this.MenuWindow.Shortcut = CType(resources.GetObject("MenuWindow.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuWindow.ShowShortcut = CType(resources.GetObject("MenuWindow.ShowShortcut"), bool);

        this.MenuWindow.Text = resources.GetString("MenuWindow.Text");

        this.MenuWindow.Visible = CType(resources.GetObject("MenuWindow.Visible"), bool);

        //

        //MenuWindowCascade

        //

        this.MenuWindowCascade.Enabled = CType(resources.GetObject("MenuWindowCascade.Enabled"), bool);

        this.MenuWindowCascade.Index = 0;

        this.MenuWindowCascade.Shortcut = CType(resources.GetObject("MenuWindowCascade.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuWindowCascade.ShowShortcut = CType(resources.GetObject("MenuWindowCascade.ShowShortcut"), bool);

        this.MenuWindowCascade.Text = resources.GetString("MenuWindowCascade.Text");

        this.MenuWindowCascade.Visible = CType(resources.GetObject("MenuWindowCascade.Visible"), bool);

        //

        //MenuWindowTile

        //

        this.MenuWindowTile.Enabled = CType(resources.GetObject("MenuWindowTile.Enabled"), bool);

        this.MenuWindowTile.Index = 1;

        this.MenuWindowTile.Shortcut = CType(resources.GetObject("MenuWindowTile.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuWindowTile.ShowShortcut = CType(resources.GetObject("MenuWindowTile.ShowShortcut"), bool);

        this.MenuWindowTile.Text = resources.GetString("MenuWindowTile.Text");

        this.MenuWindowTile.Visible = CType(resources.GetObject("MenuWindowTile.Visible"), bool);

        //

        //MenuWindowArrangeIcons

        //

        this.MenuWindowArrangeIcons.Enabled = CType(resources.GetObject("MenuWindowArrangeIcons.Enabled"), bool);

        this.MenuWindowArrangeIcons.Index = 2;

        this.MenuWindowArrangeIcons.Shortcut = CType(resources.GetObject("MenuWindowArrangeIcons.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuWindowArrangeIcons.ShowShortcut = CType(resources.GetObject("MenuWindowArrangeIcons.ShowShortcut"), bool);

        this.MenuWindowArrangeIcons.Text = resources.GetString("MenuWindowArrangeIcons.Text");

        this.MenuWindowArrangeIcons.Visible = CType(resources.GetObject("MenuWindowArrangeIcons.Visible"), bool);

        //

        //MenuWindowCloseAll

        //

        this.MenuWindowCloseAll.Enabled = CType(resources.GetObject("MenuWindowCloseAll.Enabled"), bool);

        this.MenuWindowCloseAll.Index = 3;

        this.MenuWindowCloseAll.Shortcut = CType(resources.GetObject("MenuWindowCloseAll.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuWindowCloseAll.ShowShortcut = CType(resources.GetObject("MenuWindowCloseAll.ShowShortcut"), bool);

        this.MenuWindowCloseAll.Text = resources.GetString("MenuWindowCloseAll.Text");

        this.MenuWindowCloseAll.Visible = CType(resources.GetObject("MenuWindowCloseAll.Visible"), bool);

        //

        //MenuHelpMenu

        //

        this.MenuHelpMenu.Enabled = CType(resources.GetObject("MenuHelpMenu.Enabled"), bool);

        this.MenuHelpMenu.Index = 4;

        this.MenuHelpMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuHelp, this.MenuHelpAbout});

        this.MenuHelpMenu.Shortcut = CType(resources.GetObject("MenuHelpMenu.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuHelpMenu.ShowShortcut = CType(resources.GetObject("MenuHelpMenu.ShowShortcut"), bool);

        this.MenuHelpMenu.Text = resources.GetString("MenuHelpMenu.Text");

        this.MenuHelpMenu.Visible = CType(resources.GetObject("MenuHelpMenu.Visible"), bool);

        //

        //MenuHelp

        //

        this.MenuHelp.Enabled = CType(resources.GetObject("MenuHelp.Enabled"), bool);

        this.MenuHelp.Index = 0;

        this.MenuHelp.Shortcut = CType(resources.GetObject("MenuHelp.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuHelp.ShowShortcut = CType(resources.GetObject("MenuHelp.ShowShortcut"), bool);

        this.MenuHelp.Text = resources.GetString("MenuHelp.Text");

        this.MenuHelp.Visible = CType(resources.GetObject("MenuHelp.Visible"), bool);

        //

        //MenuHelpAbout

        //

        this.MenuHelpAbout.Enabled = CType(resources.GetObject("MenuHelpAbout.Enabled"), bool);

        this.MenuHelpAbout.Index = 1;

        this.MenuHelpAbout.Shortcut = CType(resources.GetObject("MenuHelpAbout.Shortcut"), System.Windows.Forms.Shortcut);

        this.MenuHelpAbout.ShowShortcut = CType(resources.GetObject("MenuHelpAbout.ShowShortcut"), bool);

        this.MenuHelpAbout.Text = resources.GetString("MenuHelpAbout.Text");

        this.MenuHelpAbout.Visible = CType(resources.GetObject("MenuHelpAbout.Visible"), bool);

        //

        //ErrorPanel

        //

        this.ErrorPanel.AccessibleDescription = CType(resources.GetObject("ErrorPanel.AccessibleDescription"), string);

        this.ErrorPanel.AccessibleName = CType(resources.GetObject("ErrorPanel.AccessibleName"), string);

        this.ErrorPanel.Anchor = CType(resources.GetObject("ErrorPanel.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorPanel.AutoScroll = CType(resources.GetObject("ErrorPanel.AutoScroll"), bool);

        this.ErrorPanel.AutoScrollMargin = CType(resources.GetObject("ErrorPanel.AutoScrollMargin"), System.Drawing.Size);

        this.ErrorPanel.AutoScrollMinSize = CType(resources.GetObject("ErrorPanel.AutoScrollMinSize"), System.Drawing.Size);

        this.ErrorPanel.BackgroundImage = CType(resources.GetObject("ErrorPanel.BackgroundImage"), System.Drawing.Image);

        this.ErrorPanel.Controls.AddRange(new System.Windows.Forms.Control[] {this.ErrorWindow});

        this.ErrorPanel.Dock = CType(resources.GetObject("ErrorPanel.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorPanel.Enabled = CType(resources.GetObject("ErrorPanel.Enabled"), bool);

        this.ErrorPanel.Font = CType(resources.GetObject("ErrorPanel.Font"), System.Drawing.Font);

        this.ErrorPanel.ImeMode = CType(resources.GetObject("ErrorPanel.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorPanel.Location = CType(resources.GetObject("ErrorPanel.Location"), System.Drawing.Point);

        this.ErrorPanel.Name = "ErrorPanel";

        this.ErrorPanel.RightToLeft = CType(resources.GetObject("ErrorPanel.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorPanel.Size = CType(resources.GetObject("ErrorPanel.Size"), System.Drawing.Size);

        this.ErrorPanel.TabIndex = CType(resources.GetObject("ErrorPanel.TabIndex"), int);

        this.ErrorPanel.Text = resources.GetString("ErrorPanel.Text");

        this.ErrorPanel.Visible = CType(resources.GetObject("ErrorPanel.Visible"), bool);

        //

        //ErrorWindow

        //

        this.ErrorWindow.AccessibleDescription = CType(resources.GetObject("ErrorWindow.AccessibleDescription"), string);

        this.ErrorWindow.AccessibleName = CType(resources.GetObject("ErrorWindow.AccessibleName"), string);

        this.ErrorWindow.Anchor = CType(resources.GetObject("ErrorWindow.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorWindow.AutoScroll = CType(resources.GetObject("ErrorWindow.AutoScroll"), bool);

        this.ErrorWindow.AutoScrollMargin = CType(resources.GetObject("ErrorWindow.AutoScrollMargin"), System.Drawing.Size);

        this.ErrorWindow.AutoScrollMinSize = CType(resources.GetObject("ErrorWindow.AutoScrollMinSize"), System.Drawing.Size);

        this.ErrorWindow.BackgroundImage = CType(resources.GetObject("ErrorWindow.BackgroundImage"), System.Drawing.Image);

        this.ErrorWindow.Dock = CType(resources.GetObject("ErrorWindow.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorWindow.Enabled = CType(resources.GetObject("ErrorWindow.Enabled"), bool);

        this.ErrorWindow.Font = CType(resources.GetObject("ErrorWindow.Font"), System.Drawing.Font);

        this.ErrorWindow.ImeMode = CType(resources.GetObject("ErrorWindow.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorWindow.Location = CType(resources.GetObject("ErrorWindow.Location"), System.Drawing.Point);

        this.ErrorWindow.Name = "ErrorWindow";

        this.ErrorWindow.RightToLeft = CType(resources.GetObject("ErrorWindow.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorWindow.Size = CType(resources.GetObject("ErrorWindow.Size"), System.Drawing.Size);

        this.ErrorWindow.TabIndex = CType(resources.GetObject("ErrorWindow.TabIndex"), int);

        this.ErrorWindow.Visible = CType(resources.GetObject("ErrorWindow.Visible"), bool);

        //

        //ErrorPanelSplitter

        //

        this.ErrorPanelSplitter.AccessibleDescription = CType(resources.GetObject("ErrorPanelSplitter.AccessibleDescription"), string);

        this.ErrorPanelSplitter.AccessibleName = CType(resources.GetObject("ErrorPanelSplitter.AccessibleName"), string);

        this.ErrorPanelSplitter.Anchor = CType(resources.GetObject("ErrorPanelSplitter.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorPanelSplitter.BackgroundImage = CType(resources.GetObject("ErrorPanelSplitter.BackgroundImage"), System.Drawing.Image);

        this.ErrorPanelSplitter.Dock = CType(resources.GetObject("ErrorPanelSplitter.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorPanelSplitter.Enabled = CType(resources.GetObject("ErrorPanelSplitter.Enabled"), bool);

        this.ErrorPanelSplitter.Font = CType(resources.GetObject("ErrorPanelSplitter.Font"), System.Drawing.Font);

        this.ErrorPanelSplitter.ImeMode = CType(resources.GetObject("ErrorPanelSplitter.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorPanelSplitter.Location = CType(resources.GetObject("ErrorPanelSplitter.Location"), System.Drawing.Point);

        this.ErrorPanelSplitter.MinExtra = CType(resources.GetObject("ErrorPanelSplitter.MinExtra"), int);

        this.ErrorPanelSplitter.MinSize = CType(resources.GetObject("ErrorPanelSplitter.MinSize"), int);

        this.ErrorPanelSplitter.Name = "ErrorPanelSplitter";

        this.ErrorPanelSplitter.RightToLeft = CType(resources.GetObject("ErrorPanelSplitter.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorPanelSplitter.Size = CType(resources.GetObject("ErrorPanelSplitter.Size"), System.Drawing.Size);

        this.ErrorPanelSplitter.TabIndex = CType(resources.GetObject("ErrorPanelSplitter.TabIndex"), int);

        this.ErrorPanelSplitter.TabStop = false;

        this.ErrorPanelSplitter.Visible = CType(resources.GetObject("ErrorPanelSplitter.Visible"), bool);

        //

        //ErrorWindowTitle

        //

        this.ErrorWindowTitle.AccessibleDescription = CType(resources.GetObject("ErrorWindowTitle.AccessibleDescription"), string);

        this.ErrorWindowTitle.AccessibleName = CType(resources.GetObject("ErrorWindowTitle.AccessibleName"), string);

        this.ErrorWindowTitle.Anchor = CType(resources.GetObject("ErrorWindowTitle.Anchor"), System.Windows.Forms.AnchorStyles);

        this.ErrorWindowTitle.AutoSize = CType(resources.GetObject("ErrorWindowTitle.AutoSize"), bool);

        this.ErrorWindowTitle.Dock = CType(resources.GetObject("ErrorWindowTitle.Dock"), System.Windows.Forms.DockStyle);

        this.ErrorWindowTitle.Enabled = CType(resources.GetObject("ErrorWindowTitle.Enabled"), bool);

        this.ErrorWindowTitle.Font = CType(resources.GetObject("ErrorWindowTitle.Font"), System.Drawing.Font);

        this.ErrorWindowTitle.ForeColor = System.Drawing.Color.FromArgb(CType(192, Byte), CType(25, Byte), CType(37, Byte));

        this.ErrorWindowTitle.Image = CType(resources.GetObject("ErrorWindowTitle.Image"), System.Drawing.Image);

        this.ErrorWindowTitle.ImageAlign = CType(resources.GetObject("ErrorWindowTitle.ImageAlign"), System.Drawing.ContentAlignment);

        this.ErrorWindowTitle.ImageIndex = CType(resources.GetObject("ErrorWindowTitle.ImageIndex"), int);

        this.ErrorWindowTitle.ImeMode = CType(resources.GetObject("ErrorWindowTitle.ImeMode"), System.Windows.Forms.ImeMode);

        this.ErrorWindowTitle.Location = CType(resources.GetObject("ErrorWindowTitle.Location"), System.Drawing.Point);

        this.ErrorWindowTitle.Name = "ErrorWindowTitle";

        this.ErrorWindowTitle.RightToLeft = CType(resources.GetObject("ErrorWindowTitle.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.ErrorWindowTitle.Size = CType(resources.GetObject("ErrorWindowTitle.Size"), System.Drawing.Size);

        this.ErrorWindowTitle.TabIndex = CType(resources.GetObject("ErrorWindowTitle.TabIndex"), int);

        this.ErrorWindowTitle.Text = resources.GetString("ErrorWindowTitle.Text");

        this.ErrorWindowTitle.TextAlign = CType(resources.GetObject("ErrorWindowTitle.TextAlign"), System.Drawing.ContentAlignment);

        this.ErrorWindowTitle.Visible = CType(resources.GetObject("ErrorWindowTitle.Visible"), bool);

        //

        //MainWindow

        //

        this.AccessibleDescription = CType(resources.GetObject("$this.AccessibleDescription"), string);

        this.AccessibleName = CType(resources.GetObject("$this.AccessibleName"), string);

        this.Anchor = CType(resources.GetObject("$this.Anchor"), System.Windows.Forms.AnchorStyles);

        this.AutoScaleBaseSize = CType(resources.GetObject("$this.AutoScaleBaseSize"), System.Drawing.Size);

        this.AutoScroll = CType(resources.GetObject("$this.AutoScroll"), bool);

        this.AutoScrollMargin = CType(resources.GetObject("$this.AutoScrollMargin"), System.Drawing.Size);

        this.AutoScrollMinSize = CType(resources.GetObject("$this.AutoScrollMinSize"), System.Drawing.Size);

        this.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image);

        this.ClientSize = CType(resources.GetObject("$this.ClientSize"), System.Drawing.Size);

        this.Controls.AddRange(new System.Windows.Forms.Control[] {this.ErrorPanelSplitter, this.ErrorPanel, this.MainSplitter, this.TreeView, this.MainStatusBar, this.MdiClient});

        this.Dock = CType(resources.GetObject("$this.Dock"), System.Windows.Forms.DockStyle);

        this.Enabled = CType(resources.GetObject("$this.Enabled"), bool);

        this.Font = CType(resources.GetObject("$this.Font"), System.Drawing.Font);

        this.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon);

        this.ImeMode = CType(resources.GetObject("$this.ImeMode"), System.Windows.Forms.ImeMode);

        this.IsMdiContainer = true;

        this.Location = CType(resources.GetObject("$this.Location"), System.Drawing.Point);

        this.MaximumSize = CType(resources.GetObject("$this.MaximumSize"), System.Drawing.Size);

        this.Menu = this.MainMenu;

        this.MinimumSize = CType(resources.GetObject("$this.MinimumSize"), System.Drawing.Size);

        this.Name = "MainWindow";

        this.RightToLeft = CType(resources.GetObject("$this.RightToLeft"), System.Windows.Forms.RightToLeft);

        this.StartPosition = CType(resources.GetObject("$this.StartPosition"), System.Windows.Forms.FormStartPosition);

        this.Text = resources.GetString("$this.Text");

        this.Visible = CType(resources.GetObject("$this.Visible"), bool);

        CType(this.StatusPanelAction, System.ComponentModel.ISupportInitialize).EndInit();

        CType(this.StatusPanelErrorInformation, System.ComponentModel.ISupportInitialize).EndInit();

        this.ErrorPanel.ResumeLayout(false);

        this.ResumeLayout(false);

    }

#endregion

    public () {

        

        // This call is required by the Windows Form Designer.

        InitializeComponent();

        InitializeSetUp();

    }

    private void InitializeSetUp();

        //The error window is initially hidden.

        HideErrorWindow();

        TreeView.Font = MemberNode.EditedFont;

        //Load the image strip from an embedded resource.

        this.ImageList.Images.AddStrip(Image.FromStream(Reflection.Assembly.GetExecutingAssembly.GetManifestResourceStream("XMLDocumentationTool.imagestrip.bmp")));

        //if we have a list of previously opened files in the registry, add them to the file menu.

        CheckForHistoryFiles();

        if Microsoft.VisualBasic.Command != "" {

            OpenNamedAssembly(Microsoft.VisualBasic.Command);

        }

    }

    private void CheckForHistoryFiles();

        // Check to see if the registry contains information about recently

        // opened files, and if so, add them to the file menu.

        strFileName string;

        strFileName = GetSetting(APPNAME, HISTORYFILES, "1");

        if strFileName = "" { return;

        // add a separator bar

        newItem MenuItem;

        newItem = new MenuItem("-");

        this.MenuFile.MenuItems.Add(this.MenuFile.MenuItems.Count - 1, newItem);

        // add the history files

        newItem = new MenuItem("&1 " + strFileName, new EventHandler(AddressOf OpenHistoryFile));

        this.MenuFile.MenuItems.Add(this.MenuFile.MenuItems.Count - 2, newItem);

        strFileName = GetSetting(APPNAME, HISTORYFILES, "2");

        if strFileName = "" { return;

        newItem = new MenuItem("&2 " + strFileName, new EventHandler(AddressOf OpenHistoryFile));

        this.MenuFile.MenuItems.Add(this.MenuFile.MenuItems.Count - 2, newItem);

        strFileName = GetSetting(APPNAME, HISTORYFILES, "3");

        if strFileName = "" { return;

        newItem = new MenuItem("&3 " + strFileName, new EventHandler(AddressOf OpenHistoryFile));

        this.MenuFile.MenuItems.Add(this.MenuFile.MenuItems.Count - 2, newItem);

        strFileName = GetSetting(APPNAME, HISTORYFILES, "4");

        if strFileName = "" { return;

        newItem = new MenuItem("&4 " + strFileName, new EventHandler(AddressOf OpenHistoryFile));

        this.MenuFile.MenuItems.Add(this.MenuFile.MenuItems.Count - 2, newItem);

    }

    private void ShiftHistoryFiles(newestFile string);

        //Move each file in the history list down one, and add a new entry at the top.  if the

        //new entry appears in the list, remove it from that point in the list and move it to

        //the top.

        strFileName string;

        // first make sure the file doesn't already appear in our history in at least position 3

        if GetSetting(APPNAME, HISTORYFILES, "1") != newestFile {

            if GetSetting(APPNAME, HISTORYFILES, "2") = newestFile {

                strFileName = GetSetting(APPNAME, HISTORYFILES, "1");

                SaveSetting(APPNAME, HISTORYFILES, "2", strFileName);

                SaveSetting(APPNAME, HISTORYFILES, "1", newestFile);

            } else if ( GetSetting(APPNAME, HISTORYFILES, "3") = newestFile {

                strFileName = GetSetting(APPNAME, HISTORYFILES, "2");

                SaveSetting(APPNAME, HISTORYFILES, "3", strFileName);

                strFileName = GetSetting(APPNAME, HISTORYFILES, "1");

                SaveSetting(APPNAME, HISTORYFILES, "2", strFileName);

                SaveSetting(APPNAME, HISTORYFILES, "1", newestFile);

            else {

                strFileName = GetSetting(APPNAME, HISTORYFILES, "3");

                SaveSetting(APPNAME, HISTORYFILES, "4", strFileName);

                strFileName = GetSetting(APPNAME, HISTORYFILES, "2");

                SaveSetting(APPNAME, HISTORYFILES, "3", strFileName);

                strFileName = GetSetting(APPNAME, HISTORYFILES, "1");

                SaveSetting(APPNAME, HISTORYFILES, "2", strFileName);

                SaveSetting(APPNAME, HISTORYFILES, "1", newestFile);

            }

        }

        // let's also update the File Menu at this point.  First, we want

        // to verify that there are old history files to remove from the

        // menu.

        iBars int = 0;

        item MenuItem;

        foreach(item In this.MenuFile.MenuItems;

            if item.Text = "-" { iBars += 1;

        }

        if iBars < 2 { return;

        // then, remove the old ones, and call CheckForHistoryFiles to 

        // repopulate them.

        iBars = 1;

        Do;

            iBars += 1;

            if this.MenuFile.MenuItems(iBars).Text = "-" { Exit Do;

        Loop;

        Do;

            this.MenuFile.MenuItems.RemoveAt(iBars);

        Loop Until this.MenuFile.MenuItems(iBars).Text = "-";

        CheckForHistoryFiles();

    }

    private void MenuFileOpen_Click(object sender, System.EventArgs e) MenuFileOpen.Click;

        // Open an Assembly file and, optionally, an associated XML

        // Documentation file.

        if ! m_Doc == null {

            if FileDirty {

                //Ask the user to save the current XML Documentation file.

                switch( MessageBox.Show(SAVE_MESSAGE, MsgBoxStyle.YesNoCancel);

                    case MsgBoxResult.Yes;

                        if SaveXMLDocumentationFile() = FileResult.Failure {

                            return;

                        }

                    case MsgBoxResult.Cancel;

                        return;

                }

            }

        }

        fileOpen OpenFiles = new OpenFiles();

        if fileOpen.ShowDialog = DialogResult.OK {

            //Import the Assembly.

            OpenNamedAssembly(fileOpen.AssemblyPath, fileOpen.XMLDocumentationPath);

        }

    }

    private void OpenHistoryFile(object sender, System.EventArgs e);

        // Open an assembly file from the recently opened files

        WhichFile string;

        try {

            WhichFile = Mid(CType(sender, MenuItem).Text, 2, 1);

            if CInt(WhichFile) = 0 { return;

        Catch;

            return;

        }

        AsmPath string;

        AsmPath = GetSetting(APPNAME, HISTORYFILES, WhichFile);

        //Import the Assembly.

        OpenNamedAssembly(AsmPath);

    }

    private DATA_TYPE_HERE OpenNamedAssembly(Asmpath string, Optional XMLPath string = "") FileResult;

        //This is a subsidiary function to actually open the assembly once we've worked out

        //what assembly we want to open via one of the different ways available to the user

        //via the UI.

        //if an assembly file has been opened before we need to do some cleanup.

        if ! m_Doc == null {

            if FileDirty {

                //Ask the user to save the current XML Documentation file.

                switch( MessageBox.Show(SAVE_MESSAGE, MsgBoxStyle.YesNoCancel);

                    case MsgBoxResult.Yes;

                        if SaveXMLDocumentationFile() = FileResult.Failure {

                            return FileResult.Failure;

                            Exit Function;

                        }

                    case MsgBoxResult.Cancel;

                        return FileResult.Failure;

                        Exit Function;

                }

            }

            //Do some clean up before opening a new document.

            TreeView.Nodes.Clear();

            this.StatusPanelErrorInformation.Text = "";

            m_Doc = null;

            m_AsmFileName = null;

            m_XMLFileName = null;

            ErrorWindow.ClearAllItems();

            this.ErrorPanel.Hide();

            this.ErrorPanelSplitter.Hide();

            MenuWindowCloseAll_Click(null, null);

        }

        //Import the Assembly.

        if OpenAssemblyFile(Asmpath) = FileResult.Success {

            this.Text = WINDOW_TITLE + " - " + m_AsmFileName;

            if XMLPath = "" {

                //Attempt to guess the right XML file

                XMLPath = IO.Path.GetDirectoryName(Asmpath) + "\" + IO.Path.GetFileNameWithoutExtension(Asmpath) + ".xml";

            }

            //Open the XML Documentation file if the user supplied one.

            if IO.File.Exists(XMLPath) {

                OpenXMLDocumentationFile(XMLPath);

            }

            //Update the file history

            ShiftHistoryFiles(m_AsmFileName);

            //Reset dirty flag

            FileDirty = false;

            return FileResult.Success;

        else {

            return FileResult.Failure;

        }

    }

    private void MenuFileOpenXML_Click(object sender, System.EventArgs e) MenuFileOpenXML.Click;

        openDialog OpenFileDialog = new OpenFileDialog();

        openDialog.FileName = GetXMLFileName();

        openDialog.Filter = XML_FILE_FILTER;

        if openDialog.ShowDialog = DialogResult.OK {

            OpenXMLDocumentationFile(openDialog.FileName);

        }

    }

    private void MenuFileSave_Click(object sender, System.EventArgs e) MenuFileSave.Click;

        SaveXMLDocumentationFile();

    }

    private void MenuFileSaveAs_Click(object sender, System.EventArgs e) MenuFileSaveAs.Click;

        SaveWithnewFileName();

    }

    private enum FileResult;

        Failure;

        Success;

    }

    private DATA_TYPE_HERE OpenAssemblyFile(filename string) FileResult;

        // Construct the main document and then import the assembly.

        try {

            StatusMessage("Loading Assembly...");

            Update()  'Fixes some redrawing annoyances.;

            m_Doc = new MainDoc(this, Reflection.Assembly.LoadFrom(filename));

            m_Doc.ImportAssembly();

            ErrorWindow.AdjustColumns()     'To make the error window columns fit any error messages.;

            m_AsmFileName = filename;

            return FileResult.Success;

       } catch( e Exception;

            MessageBox.Show(GetErrorMessage(ErrorID.ErrorDuringLoad3, "the assembly file", filename, e.Message));

            return FileResult.Failure;

        Finally;

            StatusMessageReady();

        }

    }

    private DATA_TYPE_HERE OpenXMLDocumentationFile(filename string) FileResult;

        try {

            StatusMessage("Loading XML...");

            ErrorWindow.BeginUpdate();

            m_Doc.OpenXML(filename);

            ErrorWindow.AdjustColumns()     'To make the error window columns fit any error messages.;

            m_XMLFileName = filename;

            return FileResult.Success;

       } catch( e Exception;

            MessageBox.Show(GetErrorMessage(ErrorID.ErrorDuringLoad3, "the XML Documentation file", filename, e.Message));

            return FileResult.Failure;

        Finally;

            StatusMessageReady();

            ErrorWindow.EndUpdate();

        }

    }

    private DATA_TYPE_HERE SaveXMLDocumentationFile() FileResult;

        if m_XMLFileName = "" {

            return SaveWithnewFileName();

        else {

            return SaveWithFileName();

        }

    }

    private DATA_TYPE_HERE SaveWithFileName() FileResult;

        try {

            StatusMessage("Saving...");

            m_Doc.SaveXML(m_XMLFileName);

            FileDirty = false;

            return FileResult.Success;

       } catch( e Exception;

            MessageBox.Show(GetErrorMessage(ErrorID.ErrorDuringSave3, "the XML Documentation file", m_XMLFileName, e.Message));

            return FileResult.Failure;

        Finally;

            StatusMessageReady();

        }

    }

    private DATA_TYPE_HERE SaveWithnewFileName() FileResult;

        // Ask the user for a new XML Documentation filename and then save

        // using that new name.

        saveDialog SaveFileDialog = new SaveFileDialog();

        saveDialog.FileName = GetXMLFileName();

        saveDialog.Filter = XML_FILE_FILTER;

        if DialogResult.OK = saveDialog.ShowDialog {

            m_XMLFileName = saveDialog.FileName;

            return SaveWithFileName();

        }

    }

    private void MenuFileExit_Click(object sender, System.EventArgs e) MenuFileExit.Click;

        this.Close()  'This will fire the Closing event which will take care of notifying the user to save their file.;

    }

    private void MenuEditCut_Click(object sender, System.EventArgs e) MenuEditCut.Click;

        if ! this.ActiveMdiChild == null {

            Holder UserControl = CType(this.ActiveMdiChild.ActiveControl, UserControl);

            if ! Holder.ActiveControl == null {

                box DescriptoBox = CType(Holder.ActiveControl, DescriptoBox);

                if box.SelectionLength > 0 {

                    box.Cut();

                    FileDirty = true;

                }

            }

        }

    }

    private void MenuEditCopy_Click(object sender, System.EventArgs e) MenuEditCopy.Click;

        if ! this.ActiveMdiChild == null AndAlso this.ActiveControl == this.ActiveMdiChild {

            Holder UserControl = CType(this.ActiveMdiChild.ActiveControl, UserControl);

            if ! Holder.ActiveControl == null {

                box DescriptoBox = CType(Holder.ActiveControl, DescriptoBox);

                if box.SelectionLength > 0 {

                    box.Copy();

                }

            }

        } else if ( this.ActiveControl == TreeView {

            source MemberNode = CType(TreeView.SelectedNode, MemberNode);

            m_CopyNode = CType(source.Clone, MemberNode);

        }

        if ! this.ActiveControl == TreeView {

            m_CopyNode = null;

        }

    }

    private void MenuEditPaste_Click(object sender, System.EventArgs e) MenuEditPaste.Click;

        if ! this.ActiveMdiChild == null AndAlso this.ActiveControl == this.ActiveMdiChild {

            Holder UserControl = CType(this.ActiveMdiChild.ActiveControl, UserControl);

            if ! Holder.ActiveControl == null {

                box DescriptoBox = CType(Holder.ActiveControl, DescriptoBox);

                box.Paste(DataFormats.GetFormat(DataFormats.Text));

                FileDirty = true;

            }

        } else if ( this.ActiveControl == TreeView {

            target MemberNode = CType(TreeView.SelectedNode, MemberNode);

            TreeView.CopyNode(m_CopyNode, target);

            FileDirty = true;

        }

    }

    private void MenuEditFind_Click(object sender, System.EventArgs e) MenuEditFind.Click;

        // Display the Find dialog.

        // We need to consider three situations:

        //         Find Dialog is ! constructed yet

        //         Find Dialog is hidden

        //         Find Dialog is already visible

        if m_FindDialog == null {

            m_FindDialog = new FindDialog(this);

            m_FindDialog.Show();

        } else if ( m_FindDialog.Visible = false {

            m_FindDialog.Reset();

            m_FindDialog.Show();

        else {

            m_FindDialog.BringToFront();

        }

    }

    private void MenuEditClearNode_Click(object sender, System.EventArgs e) MenuEditClearNode.Click, TreeViewContextMenuClear.Click;

        // Delete the selected node, including its children.

        memberNode MemberNode = CType(TreeView.SelectedNode(), MemberNode);

        if memberNode.GetNodeCount(false) > 0 {

            if vbNo = MessageBox.Show("This will clear all the children of this node. " + _;

                    Environment.NewLine + "Are you sure you want to continue?", MsgBoxStyle.YesNo) {

                Return;

            }

        }

        StatusMessage("Deleting...");

        ClearNode(memberNode, true);

        FileDirty = true;

        StatusMessageReady();

    }

    private void MenuEditDeleteAllXMLNodes_Click(object sender, System.EventArgs e) MenuEditDeleteAllXMLNodes.Click;

        // Delete all nodes which appear only in the XML Documentation file.

        // First we recurse the Assembly tree to build a list of nodes to

        // delete.  { given this list, we delete each one.  Since we recurse

        // depth-first, children are guaranteed to be deleted before their

        // parents (which keeps the tree from getting garbled we delete

        // nodes).

        if vbNo = MessageBox.Show("This will delete all the nodes found only in the XML Documentation file. Continue?", MsgBoxStyle.YesNo) {

            Return;

        }

        StatusMessage("Deleting...");

        memberNode MemberNode;

        DeletionList new ArrayList(8);

        foreach(memberNode In TreeView.Nodes;

            RecursiveCheck(memberNode, DeletionList);

        }

        foreach(memberNode In DeletionList;

            DeleteNode(memberNode, false)   'The children are already in the list, so don't do a recursive delete.;

        }

        FileDirty = true;

        StatusMessageReady();

    }

    private void RecursiveCheck(memberNode MemberNode, DeletionList ArrayList);

        // Builds a deletion list of all nodes which are found only in the

        // XML Documentation file.  We do a depth-first search to make sure

        // children appear before their parents in the deletion list.

        childNode MemberNode;

        foreach(childNode In memberNode.Nodes;

            RecursiveCheck(childNode, DeletionList);

        }

        if memberNode.Source = NodeSource.XML {

            DeletionList.Add(memberNode);

        }

    }

    private void DoDeleteNode(memberNode MemberNode, IncludeChildren bool);

        // Delete the member node from the XML Documentation tree.  if

        // necessary, recursively delete the children well.  Remove the

        // deleted node's errors from the error list, close its window (if

        // open), and remove it from consideration in the Find window's state.

        if IncludeChildren {

            //Delete the children first.

            child MemberNode;

            foreach(child In memberNode.Nodes;

                DoDeleteNode(child, true);

            }

        }

        //Remove all the errors for this node.

        if memberNode.HasErrors {

            m_Doc.DeleteNodeFromErrorList(memberNode);

        }

        //if this node has a editor window open, close it.

        form NodeWindow;

        foreach(form In MdiClient.MdiChildren;

            if form.Text = memberNode.FriendlySignatureWithPathAndModifiers {

                form.Close();

                break;

            }

        }

        if ! m_FindDialog == null AndAlso m_FindDialog.Visible {

            m_FindDialog.SkipDeletedNode(memberNode);

        }

    }

    private void ClearNode(memberNode MemberNode, IncludeChildren bool);

        // Clear the member node from the XML Documentation tree.  if

        // necessary, recursively delete the children well.  Remove

        // any errors and update the document window.

        if IncludeChildren {

            //Clear the children first.

            child MemberNode;

            foreach(child In memberNode.Nodes;

                ClearNode(child, true);

            }

        }

        //Remove all the errors for this node.

        if memberNode.HasErrors {

            m_Doc.DeleteNodeFromErrorList(memberNode);

        }

        //Clear all the stored values

        ClearValues(memberNode.m_Params);

        ClearValues(memberNode.m_PropertyValue);

        ClearValues(memberNode.m_Remarks);

        ClearValues(memberNode.m_ReturnValue);

        ClearValues(memberNode.m_Summary);

        //if this node has an editor window open, refresh the controls.

        form NodeWindow;

        foreach(form In MdiClient.MdiChildren;

            if form.Text = memberNode.FriendlySignatureWithPathAndModifiers {

                Holder UserControl = CType(form.ActiveControl, UserControl);

                Holder.Controls.Clear();

                form.BuildFormMembers();

                break;

            }

        }

        memberNode.UpdateEditState();

    }

    private void ClearValues(Values ArrayList);

        // Clear valid values from the arraylist.  Delete errors.

        i int;

        param ParameterDescriptor;

        content ContentDescriptor;

        For i = 0 To Values.Count - 1;

            if Values(i).GetType == typeof(ParameterDescriptor) {

                param = CType(Values(i), ParameterDescriptor);

                if param.Errors == null {

                    param.Content = "";

                else {

                    Values.Remove(param);

                }

            } else if ( Values(i).GetType == typeof(ContentDescriptor) {

                content = CType(Values(i), ContentDescriptor);

                if content.Errors == null {

                    content.Content = "";

                else {

                    Values.Remove(content);

                }

            }

        }

    }

    private void DeleteNode(memberNode MemberNode, IncludeChildren bool);

        DoDeleteNode(memberNode, IncludeChildren);

        //Remove the top-level node from the Assembly tree.  if children are included, they

        //will be removed well.

        TreeView.Nodes.Remove(memberNode);

    }

    private void MenuViewExpandAll_Click(object sender, System.EventArgs e) MenuViewExpandAll.Click;

        StatusMessage("Expanding...");

        TreeView.BeginUpdate();

        TreeView.ExpandAll();

        // Make sure we are on top of the tree

        if TreeView.GetNodeCount(false) > 0 {

            TreeView.Nodes(0).EnsureVisible();

        }

        TreeView.EndUpdate();

        StatusMessageReady();

    }

    private void MenuViewCollapseAll_Click(object sender, System.EventArgs e) MenuViewCollapseAll.Click;

        StatusMessage("Collapsing...");

        TreeView.CollapseAll();

        StatusMessageReady();

    }

    private void MenuViewShowErrorWindow_Click(object sender, System.EventArgs e) MenuViewShowErrorWindow.Click;

        this.MenuViewShowErrorWindow.Checked = ! MenuViewShowErrorWindow.Checked;

        if this.MenuViewShowErrorWindow.Checked {

            ShowErrorWindow();

        else {

            HideErrorWindow();

        }

    }

    private void MenuViewVersionInfo_Click(object sender, System.EventArgs e) MenuViewVersionInfo.Click;

        // Build the version properties objects and display the version info

        // dialog.

        AsmData VersionData = new VersionData(m_Doc.m_Asm.GetName);

        versionProperties VersionProperties;

        if ! m_Doc.m_XMLData == null {

            versionProperties = new VersionProperties(AsmData, m_Doc.m_XMLData);

        else {

            versionProperties = new VersionProperties(AsmData);

        }

        verinfo VersionInfo = new VersionInfo(versionProperties);

        verinfo.ShowDialog();

    }

    private void MenuWindowCascade_Click(object sender, System.EventArgs e) MenuWindowCascade.Click;

        MdiClient.LayoutMdi(MdiLayout.Cascade);

    }

    private void MenuWindowTile_Click(object sender, System.EventArgs e) MenuWindowTile.Click;

        MdiClient.LayoutMdi(MdiLayout.TileVertical);

    }

    private void MenuWindowArrangeIcons_Click(object sender, System.EventArgs e) MenuWindowArrangeIcons.Click;

        MdiClient.LayoutMdi(MdiLayout.ArrangeIcons);

    }

    private void MenuWindowCloseAll_Click(object sender, System.EventArgs e) MenuWindowCloseAll.Click;

        form NodeWindow;

        foreach(form In MdiClient.MdiChildren;

            form.Close();

        }

    }

    private void MenuHelp_Click(object sender, System.EventArgs e) MenuHelp.Click;

        try {

            System.Diagnostics.Process.Start("Readthis.htm");

       } catch( ex1 System.ComponentModel.Win32Exception;

            try {

                System.Diagnostics.Process.Start("..\Readthis.htm");

           } catch( ex2 Exception;

                MessageBox.Show("Error loading 'Readthis.htm': " + ex2.Message);

            }

        }

    }

    private void MenuFile_Popup(object sender, System.EventArgs e) MenuFile.Popup;

        // Disables SaveAs, Save, and OpenXML if no assembly is open.  OpenXML

        // is disabled if an XML Documentation file is already open.

        this.MenuFileSaveAs.Enabled = ! m_Doc == null;

        this.MenuFileSave.Enabled = ! m_Doc == null;

        this.MenuFileOpenXML.Enabled = m_XMLFileName = "" AndAlso ! m_Doc == null;

    }

    private void MenuEdit_Popup(object sender, System.EventArgs e) MenuEdit.Popup;

        // Enable Cut, Copy and Paste depending on the current item selected.

        this.MenuEditCut.Enabled = false;

        this.MenuEditCopy.Enabled = false;

        this.MenuEditPaste.Enabled = false;

        this.MenuEditDeleteError.Enabled = false;

        //Cut, Copy and Paste only works for DescriptoBoxes.

        if ! this.ActiveMdiChild == null AndAlso this.ActiveControl == this.ActiveMdiChild {

            Holder UserControl = CType(this.ActiveMdiChild.ActiveControl, UserControl);

            if ! Holder.ActiveControl == null {

                box DescriptoBox = CType(Holder.ActiveControl, DescriptoBox);

                if box.SelectionLength > 0 {

                    MenuEditCut.Enabled = ! box.ReadOnly;

                    MenuEditCopy.Enabled = true;

                }

                if typeof(System.Windows.Forms.Clipboard.GetDataObject.GetData(DataFormats.Text) == string {

                    this.MenuEditPaste.Enabled = ! box.ReadOnly;

                }

                if box.Descriptor.HasErrors {

                    this.MenuEditDeleteError.Enabled = true;

                }

            }

        } else if ( this.ActiveControl == TreeView {

            MenuEditCopy.Enabled = true;

            if ! m_CopyNode == null {

                MenuEditPaste.Enabled = true;

            }

        }

        this.MenuEditFind.Enabled = ! m_Doc == null;

        this.MenuEditDeleteAllXMLNodes.Enabled = ! m_Doc == null AndAlso this.ErrorWindow.ErrorCount > 0;

        //Can only delete a node if the treeview is active and has a node selected.

        this.MenuEditClearNode.Enabled = ! m_Doc == null AndAlso this.ActiveControl == TreeView AndAlso ! TreeView.SelectedNode() == null;

    }

    private void MenuView_Popup(object sender, System.EventArgs e) MenuView.Popup;

        this.MenuViewExpandAll.Enabled = ! m_Doc == null;

        this.MenuViewCollapseAll.Enabled = ! m_Doc == null;

        if ! m_Doc == null AndAlso this.ErrorWindow.ErrorCount > 0 {

            this.MenuViewShowErrorWindow.Checked = ErrorWindow.Visible;

            this.MenuViewShowErrorWindow.Enabled = true;

        else {

            this.MenuViewShowErrorWindow.Checked = false;

            this.MenuViewShowErrorWindow.Enabled = false;

        }

        this.MenuViewVersionInfo.Enabled = ! m_Doc == null AndAlso ! m_Doc.m_Asm == null;

    }

    private void MenuWindow_Popup(object sender, System.EventArgs e) MenuWindow.Popup;

        this.MenuWindowArrangeIcons.Enabled = MdiClient.HasChildren;

        this.MenuWindowCascade.Enabled = MdiClient.HasChildren;

        this.MenuWindowCloseAll.Enabled = MdiClient.HasChildren;

        this.MenuWindowTile.Enabled = MdiClient.HasChildren;

    }

    private void TreeViewContextMenu_Popup(object sender, System.EventArgs e) TreeViewContextMenu.Popup;

        //A right-click selects the node at the location of the right-click.

        TreeView.SelectedNode = TreeView.GetNodeAt(TreeView.PointToClient(Control.MousePosition));

        this.TreeViewContextMenuClear.Enabled = ! TreeView == null AndAlso ! TreeView.SelectedNode == null;

        this.TreeViewContextMenuOpen.Enabled = ! TreeView == null AndAlso ! TreeView.SelectedNode == null;

    }

    private void MainStatusBar_PanelClick(object sender, e System.Windows.Forms.StatusBarPanelClickEventArgs) MainStatusBar.PanelClick;

        // double clicking the Error Status Panel displays all error nodes in

        // the Assembly tree.

        if e.StatusBarPanel == this.StatusPanelErrorInformation AndAlso e.Clicks = 2 AndAlso ! m_Doc == null AndAlso this.ErrorWindow.ErrorCount > 0 {

            ShowAllErrorNode();

        }

    }

    public void StatusMessage(message string);

        // Displays status messages tasks are performed.  This function

        // is used in conjunction with StatusMessageReady to change the mouse

        // cursor.

        Debug.Assert(message != READY_STATUS, "Wrong sub called to set the ready status.");

        //The program is doing something, so set the wait cursor.

        this.Cursor = Cursors.WaitCursor;

        this.MainStatusBar.Panels(0).Text = message;

        this.MainStatusBar.Update();

    }

    public const READY_STATUS string = "Ready"   'The string for the Ready status.;

    public void StatusMessageReady();

        // Set the status back to "Ready".  This function is used in

        // conjunction with StatusMessage to change the mouse cursor.

        //set {the mouse back to the default pointer.

        this.Cursor = Cursors.Default;

        this.MainStatusBar.Panels(0).Text = READY_STATUS;

        this.MainStatusBar.Update();

    }

    public void UpdateErrorStatus();

        // Update the Error Status Panel to display the correct number of

        // errors.

        errorNumber int = this.ErrorWindow.ErrorCount;

        if errorNumber != 0 {

            if errorNumber > 1 {

                this.StatusPanelErrorInformation.Text = CStr(errorNumber) + " Errors";

            else {

                this.StatusPanelErrorInformation.Text = CStr(errorNumber) + " Error";

            }

            if this.ErrorWindow.Visible = false {  'A simple optimization.;

                this.ShowErrorWindow();

                this.StatusPanelErrorInformation.ToolTipText = "Double click to view all error nodes";

            }

        else {

            this.StatusPanelErrorInformation.Text = "No Error";

            this.HideErrorWindow();

        }

    }

    private void OpenNodeWindow(member MemberNode) ErrorWindow.ErrorListDoubleClick;

        // The window for a member needs to be opened, so first see if it's

        // already open.  if it is, bring the form to the front.  Otherwise,

        // constuct a new node window, hookup the cut, copy, paste events, and

        // show it.

        if member == null {

            return;

        }

        form NodeWindow;

        foreach(form In MdiClient.MdiChildren;

            // We use full signature the title for each node window so we will search

            // based on it since it is unique.

            if form.Text = member.FriendlySignatureWithPathAndModifiers {

                form.BringToFront();

                Return;

            }

        }

        //At this point, the form is ! found in the MdiChildren so build a new one.

        form = new NodeWindow(member, m_Doc, Me);

        AddHandler form.MenuCutClicked, AddressOf MenuEditCut_Click;

        AddHandler form.MenuCopyClicked, AddressOf MenuEditCopy_Click;

        AddHandler form.MenuPasteClicked, AddressOf MenuEditPaste_Click;

        form.MdiParent = Me;

        form.Show();

        TreeView.SelectedNode = member;

    }

    private void OpenNodeWindow(object sender, System.EventArgs e) TreeView.DoubleClick, TreeViewContextMenuOpen.Click;

        // This overloaded sub maps more events onto OpenNodeWindow.

        OpenNodeWindow(CType(TreeView.SelectedNode, MemberNode));

    }

    private void FindDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e) m_FindDialog.Closing;

        //Hide the dialog instead of closing it to preserve its state.

        e.Cancel = true;

        m_FindDialog.Hide();

    }

    private void UpdateNodeWindow(destination MemberNode) TreeView.NodeCopied;

        //Update the destination's NodeWindow if it is opened.

        form NodeWindow;

        foreach(form In MdiClient.MdiChildren;

            if form.Text = destination.FriendlySignatureWithPathAndModifiers {

                Holder UserControl = CType(form.ActiveControl, UserControl);

                Holder.Controls.Clear();

                form.BuildFormMembers();

                break;

            }

        }

    }

    private void ShowAllErrorNode();

        // Expand the Assembly tree so that all member nodes with errors are

        // visible.

        StatusMessage("Expanding...");

        TreeView.BeginUpdate();

        //Recursively walk the tree looking for nodes with errors.

        node MemberNode;

        foreach(node In TreeView.Nodes;

            TraverseTree(node);

        }

        //After the expansion is complete, seek back to the top of the tree.

        TreeView.SelectedNode = null;

        TreeView.Nodes(0).EnsureVisible();

        TreeView.EndUpdate();

        StatusMessageReady();

    }

    private void TraverseTree(node MemberNode);

        // Recursively display all the error nodes.

        if node.HasErrors {

            //Selecting the node automatically expands the tree to that node.

            TreeView.SelectedNode = node;

        }

        //if the current node has children, look at each of them.

        if node.GetNodeCount(false) > 0 {

            subnode MemberNode;

            foreach(subnode In node.Nodes;

                TraverseTree(subnode);

            }

        }

    }

    private DATA_TYPE_HERE GetXMLFileName() string;

        // return the XML Documentation file's name.  The default name is the

        // name of the Assembly file with the extension replaced with "xml".

        if m_XMLFileName = "" {

            try {

                if Microsoft.VisualBasic.Right(m_AsmFileName, 4) = ".exe" {

                    return m_AsmFileName.Replace(".exe", ".xml");

                }

            Catch;

            }

            return m_AsmFileName.Replace(".dll", ".xml");

        else {

            return m_XMLFileName;

        }

    }

    public void ShowErrorWindow();

        this.ErrorPanelSplitter.Show();

        this.ErrorPanel.Show();

        this.ErrorWindow.Show();

    }

    public void HideErrorWindow() ErrorWindow.OnCloseButtonClicked;

        this.ErrorPanelSplitter.Hide();

        this.ErrorPanel.Hide();

        this.ErrorWindow.Hide();

    }

    private void OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e) base.Closing;

        // if the program is closing and an Assembly file has been opened, then

        // we need to prompt the user to save.

        if ! m_Doc == null {

            if FileDirty {

                //Ask the user to save.

                switch( MessageBox.Show(SAVE_MESSAGE, MsgBoxStyle.YesNoCancel);

                    case MsgBoxResult.Yes;

                        if SaveXMLDocumentationFile() = FileResult.Failure {

                            e.Cancel = true;

                        }

                    case MsgBoxResult.Cancel;

                        e.Cancel = true;

                    case MsgBoxResult.No;

                        //Do nothing to shut down.

                }

            }

        }

    }

    private void MenuEditDeleteError_Click(object sender, System.EventArgs e) MenuEditDeleteError.Click;

        form NodeWindow = CType(this.ActiveMdiChild, NodeWindow);

        Holder UserControl = CType(form.ActiveControl, UserControl);

        box DescriptoBox = CType(Holder.ActiveControl, DescriptoBox);

        form.RemoveDescriptor(box.Descriptor);

        FileDirty = true;

    }

    private void MenuHelpAbout_Click(object sender, System.EventArgs e) MenuHelpAbout.Click;

        // Open the About form in Dialog Mode

        frmAbout frm = new frmAbout();

        frm.ShowDialog(this);

        frm.Dispose();

    }

}

