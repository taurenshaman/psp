using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Dialogs;
using DataAccess.PSP;

namespace PSP.Run.Views {
  /// <summary>
  /// ModuleManagementWindow.xaml 的交互逻辑
  /// </summary>
  public partial class ModuleManagementWindow : Window {
    PSPModule currentModule = null,
      selectedInChildrenModules = null;

    public ModuleManagementWindow() {
      InitializeComponent();
      this.Loaded += new RoutedEventHandler( ModuleManagementWindow_Loaded );
    }

    void ModuleManagementWindow_Loaded( object sender, RoutedEventArgs e ) {
      mcModule.tblkParentModule.MouseLeftButtonUp += new MouseButtonEventHandler( tblkParentModule_MouseLeftButtonUp );
      mcModule.tblkParentModule.MouseRightButtonUp += new MouseButtonEventHandler( tblkParentModule_MouseRightButtonUp );
      mcModule.imageSave.MouseLeftButtonUp += new MouseButtonEventHandler( imageSave_MouseLeftButtonUp );

      lbChildrenModules.MouseLeftButtonUp += new MouseButtonEventHandler( lbChildrenModules_MouseLeftButtonUp );
      lbModules.MouseLeftButtonUp += new MouseButtonEventHandler( lbModules_MouseLeftButtonUp );

      ccTags.btnAddSimpleTag.Click += new RoutedEventHandler( btnAddSimpleTag_Click );
      ccTags.lbConcepts.MouseDoubleClick += new MouseButtonEventHandler( lbConcepts_MouseDoubleClick );
      ccTags.lbRecentTags.MouseDoubleClick += new MouseButtonEventHandler( lbRecentTags_MouseDoubleClick );

      initializeModuleList();
    }

    #region Tags

    /// <summary>
    /// 将ListBox中的选中项作为标签数据添加，name=ListBoxItem.Content，guid=ListBoxItem.Tag
    /// </summary>
    /// <param name="lb"></param>
    private void addListBoxSelectedItemToTag( ListBox lb ) {
      if ( lb == null || lb.SelectedIndex < 0 )
        return;
      ListBoxItem lbi = lb.SelectedItem as ListBoxItem;
      if ( lbi == null )
        return;
      Guid g = (Guid)( lbi.Tag );
      TagReference tr = new TagReference( lbi.Content as string, g );
      mcModule.AddTag( tr );
      // 刷新 最近使用的标签 列表
      //ccTags.initializeRecentTags();
    }
    private void addListBoxSelectedItemToTag( TagReference tr ) {
      if ( tr == null )
        return;
      mcModule.AddTag( tr );
      // 刷新 最近使用的标签 列表
      //ccTags.initializeRecentTags();
    }

    // 双击，从最近使用的标签中 添加 标签，可能是语义标签，可能是普通标签
    void lbRecentTags_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
      //addListBoxSelectedItemToTag( ccTags.lbRecentTags );
      
      TagReference tagSelected = ccTags.lbRecentTags.SelectedItem as TagReference;
      if ( tagSelected == null )
        return;
      addListBoxSelectedItemToTag( tagSelected );
    }

    // 双击，从查询结果中 添加 语义标签
    void lbConcepts_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
      addListBoxSelectedItemToTag( ccTags.lbConcepts );
    }

    // 添加普通标签
    void btnAddSimpleTag_Click( object sender, RoutedEventArgs e ) {
      mcModule.AddTag( new TagReference( ccTags.tbTagName.Text.Trim(), Guid.Empty ) );
    }

    #endregion Tags

    #region Modules

    /// <summary>
    /// 初始化模块列表
    /// </summary>
    void initializeModuleList() {
      //lbModules.Items.Clear();
      //lbChildrenModules.Items.Clear();

      //foreach ( PSPModule pm in DataHelper.PSP_Modules ) {
      //  ListBoxItem lbi_a = new ListBoxItem(),
      //    lbi_b = new ListBoxItem();

      //  lbi_a.Content = pm.name;
      //  lbi_a.Tag = pm.guid;
      //  lbi_a.ToolTip = pm.description;
      //  lbModules.Items.Add( lbi_a );

      //  lbi_b.Content = pm.name;
      //  lbi_b.Tag = pm.guid;
      //  lbi_b.ToolTip = pm.description;
      //  lbChildrenModules.Items.Add( lbi_b );
      //}

      Tools.UIHelper.InitializeModuleList( lbModules, DataHelper.PSP_Modules );
      Tools.UIHelper.InitializeModuleList( lbChildrenModules, DataHelper.PSP_Modules );
    }

    private void imageImportModules_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      string fileName = string.Empty;
      // Windows 7
      if ( CommonOpenFileDialog.IsPlatformSupported ) {
        CommonOpenFileDialog cofd = new CommonOpenFileDialog();
        cofd.Multiselect = false;
        cofd.AllowNonFileSystemItems = false;
        cofd.IsFolderPicker = false; // 只允许选择文件
        cofd.Filters.Add( new CommonFileDialogFilter( "XML Files", "*.xml" ) );
        CommonFileDialogResult cfdr = cofd.ShowDialog();
        if ( cfdr == CommonFileDialogResult.OK ) {
          ShellObject selectedSO = null;
          try {
            // Try to get a valid selected item
            selectedSO = cofd.FileAsShellObject;
            fileName = selectedSO.ParsingName;
          }
          catch {
            MessageBox.Show( "选取的文件无效，原因未知……囧" );
            return;
          }

        }

      }
      // others
      else {
        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
        ofd.AutoUpgradeEnabled = true;
        ofd.Multiselect = false;
        ofd.Filter = "XML Files(*.xml)|*.xml";
        ofd.InitialDirectory = ".Resources/Modules";
        if ( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK )
          fileName = ofd.FileName;
        else
          return;
      }

      if ( fileName == null || fileName.Length == 0 ) return;
      var result = DataHelper.LoadModules2( fileName );
      if ( result == null || result.Count == 0 ) {
        MessageBox.Show( "没有数据，或数据格式不正确……", "提示", MessageBoxButton.OK );
        return;
      }
      // append
      DataHelper.AppendModules( result );
      // refresh
      initializeModuleList();
    }

    // 保存 模块
    void imageSave_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      PSPModule pm = mcModule.PSP_Module;
      if ( pm.name.Trim().Length == 0 ) {
        MessageBox.Show( "您没有填写模块名称……", "提示", MessageBoxButton.OK );
        return;
      }

      DataHelper.SaveModule( pm );
      // 保存之后，刷新Module列表
      initializeModuleList();
    }

    // 选中 模块
    void lbModules_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( lbModules.SelectedIndex < 0 ) {
        currentModule = null;
        return;
      }
      ListBoxItem lbi = lbModules.SelectedItem as ListBoxItem;
      if ( lbi == null )
        return;

      Guid g = (Guid)( lbi.Tag ); // new Guid( (string)( lbi.Tag ) );
      currentModule = DataHelper.GetModule( g );
      // 设置ModuleControl
      mcModule.PSP_Module = currentModule;
    }
    // 选中 模块
    void lbChildrenModules_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( lbChildrenModules.SelectedIndex < 0 ) {
        selectedInChildrenModules = null;
        return;
      }
      ListBoxItem lbi = lbChildrenModules.SelectedItem as ListBoxItem;
      if ( lbi == null )
        return;

      Guid g = (Guid)( lbi.Tag );
      selectedInChildrenModules = DataHelper.GetModule( g );
    }

    // 取消父模块
    void tblkParentModule_MouseRightButtonUp( object sender, MouseButtonEventArgs e ) {
      mcModule.SetParentModule( null );
    }
    // 设置父模块
    void tblkParentModule_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( selectedInChildrenModules == null ) {
        MessageBox.Show( "请先在右侧的模块列表中选择一项", "提示", MessageBoxButton.OK );
        return;
      }
      mcModule.SetParentModule( selectedInChildrenModules );
    }

    // 双击，添加 子模块
    private void lbChildrenModules_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
      if ( lbChildrenModules.SelectedIndex < 0 ) {
        selectedInChildrenModules = null;
        return;
      }
      ListBoxItem lbi = lbChildrenModules.SelectedItem as ListBoxItem;
      if ( lbi == null )
        return;
      Guid g = (Guid)( lbi.Tag );
      mcModule.AddChildModule( DataHelper.GetModule( g ) );
    }

    // 按Delete键，删除选中的模块
    private void lbModules_KeyUp( object sender, KeyEventArgs e ) {
      if ( currentModule == null ) {
        MessageBox.Show( "请先选中某个您要删除的模块", "提示", MessageBoxButton.OK );
        return;
      }
      // 按下了Delete键
      if ( e.Key == Key.Delete ) {
        MessageBoxResult mbr = MessageBox.Show( "确认删除？\n名称：" + currentModule.name, "提示", MessageBoxButton.YesNo );
        if ( mbr == MessageBoxResult.No )
          return;
        // delete
        DataHelper.DeleteModule( currentModule );
        // refresh
        initializeModuleList();
      }
    }

    #endregion Modules

  }
}
