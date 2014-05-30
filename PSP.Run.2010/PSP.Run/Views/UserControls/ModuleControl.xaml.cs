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
using System.Windows.Navigation;
using System.Windows.Shapes;

using DataAccess.PSP;

namespace PSP.Run.Views.UserControls {
  /// <summary>
  /// ModuleControl.xaml 的交互逻辑
  /// </summary>
  public partial class ModuleControl : UserControl {
    PSPModule psp_module = null;
    public PSPModule PSP_Module {
      get {
        if ( psp_module == null )
          psp_module = new PSPModule();
        psp_module.name = tbName.Text.Trim();
        psp_module.description = tbDescription.Text.Trim();
        // parent module
        if ( tblkParentModule.Text != "未设置" && tblkParentModule.Tag != null ) {
          Guid g = (Guid)( tblkParentModule.Tag );
          if ( !Guid.Equals( Guid.Empty, g ) ) {
            psp_module.parent_module = g;
          }
        }
        // children modules
        if ( psp_module.children_modules != null )
          psp_module.children_modules.Clear();
        else
          psp_module.children_modules = new List<ModuleReference>();
        foreach ( UIElement uie in dpChildrenModules.Children ) {
          TextBlock tblk = uie as TextBlock;
          if ( tblk == null )
            continue;
          ModuleReference mr = new ModuleReference( tblk.Text.Trim(), (Guid)( tblk.Tag ) );
          psp_module.children_modules.Add( mr );
        }
        // tags
        if ( psp_module.tags != null )
          psp_module.tags.Clear();
        else
          psp_module.tags = new List<TagReference>();
        foreach ( UIElement uie in dpTags.Children ) {
          TextBlock tblk = uie as TextBlock;
          if ( tblk == null )
            continue;
          TagReference tr = new TagReference( tblk.Text.Trim(), (Guid)( tblk.Tag ) );
          psp_module.tags.Add( tr );
        }
        
        return psp_module;
      }
      set{
        psp_module = value;
        // 设置界面
        tbName.Text = psp_module.name;
        tblkGuid.ToolTip = psp_module.guid.ToString();
        tbDescription.Text = psp_module.description;
        // parent module
        PSPModule pmParent = DataHelper.GetModule( psp_module.parent_module );
        if ( pmParent == null ) {
          tblkParentModule.Text = "未设置";
          tblkParentModule.Tag = null;
          tblkParentModule.ToolTip = null;
        }
        else {
          tblkParentModule.Text = pmParent.name;
          tblkParentModule.Tag = pmParent.guid;
          tblkParentModule.ToolTip = pmParent.description;
        }
        // children modules
        dpChildrenModules.Children.Clear();
        if ( psp_module.children_modules != null ) {
          foreach ( ModuleReference mrChild in psp_module.children_modules ) {
            AddChildModuleInContainer( mrChild );
          }
        }
        // tags
        dpTags.Children.Clear();
        if ( psp_module.tags != null ) {
          foreach ( TagReference tr in psp_module.tags ) {
            AddTagInContainer( tr );
          }
        }
      }
    }

    private void AddTagInContainer( TagReference tr ) {
      TextBlock tblkTag = new TextBlock();
      tblkTag.Text = tr.name;
      tblkTag.Margin = new Thickness( 3 );
      if ( !Guid.Equals( tr.guid, Guid.Empty ) ) {
        tblkTag.Tag = tr.guid;
        tblkTag.ToolTip = "GUID: " + tr.guid.ToString() + "\n鼠标右键点击，删除此项";
      }
      else {
        tblkTag.Tag = Guid.Empty;
        tblkTag.ToolTip = "普通标签\n鼠标右键点击，删除此项";
      }
      tblkTag.SetValue( DockPanel.DockProperty, Dock.Top );
      tblkTag.MouseRightButtonUp += new MouseButtonEventHandler( tblkTag_MouseRightButtonUp );
      dpTags.Children.Insert( 0, tblkTag );
    }

    private void AddChildModuleInContainer( ModuleReference mrChild ) {
      TextBlock tblkModule = new TextBlock();
      tblkModule.Text = mrChild.name;
      tblkModule.Tag = mrChild.guid;
      tblkModule.Margin = new Thickness( 3 );
      tblkModule.ToolTip = "鼠标右键点击，删除此项";
      tblkModule.SetValue( DockPanel.DockProperty, Dock.Top );
      tblkModule.MouseRightButtonUp += new MouseButtonEventHandler( tblkModule_MouseRightButtonUp );
      dpChildrenModules.Children.Insert( 0, tblkModule );
    }

    public ModuleControl() {
      InitializeComponent();

      this.Loaded += new RoutedEventHandler( ModuleControl_Loaded );
    }

    void ModuleControl_Loaded( object sender, RoutedEventArgs e ) {
      imageNew_MouseLeftButtonUp( null, null );
    }
    
    // 删除子模块
    void tblkModule_MouseRightButtonUp( object sender, MouseButtonEventArgs e ) {
      TextBlock tblk = sender as TextBlock;
      if ( tblk == null )
        return;

      Guid guid = (Guid)( tblk.Tag );
      for ( int i = 0; i < psp_module.children_modules.Count; i++ ) {
        ModuleReference mr = psp_module.children_modules[i];
        if ( Guid.Equals( mr.guid, guid ) ) {
          psp_module.children_modules.RemoveAt( i );
          break;
        } // if
      } // for

      dpChildrenModules.Children.Remove( tblk );
    }
    
    // 删除标签
    void tblkTag_MouseRightButtonUp( object sender, MouseButtonEventArgs e ) {
      TextBlock tblk = sender as TextBlock;
      if ( tblk == null )
        return;

      Guid guid = Guid.Empty;
      if ( tblk.Tag != null )
        guid = (Guid)( tblk.Tag );
      for ( int i = 0; i < psp_module.tags.Count; i++ ) {
        TagReference tr = psp_module.tags[i];
        if ( Guid.Equals( tr.guid, guid ) && string.Equals( tblk.Text, tr.name ) ) {
          psp_module.tags.RemoveAt( i );
          break;
        } // if
      } // for

      dpTags.Children.Remove( tblk );
    }

    // new
    private void imageNew_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      psp_module = new PSPModule();
      
      // 更新界面
      tbName.Text = "";
      tblkGuid.ToolTip = psp_module.guid.ToString();
      tbDescription.Text = "";
      // parent module
      SetParentModule( null );
      // children modules
      dpChildrenModules.Children.Clear();
      // tags
      dpTags.Children.Clear();
    }

    /// <summary>
    /// 设置 父模块
    /// </summary>
    /// <param name="pmodule"></param>
    public void SetParentModule( PSPModule pmodule ) {
      if ( psp_module == null )
        psp_module = new PSPModule();

      if ( pmodule == null ) {
        psp_module.parent_module = Guid.Empty;
        tblkParentModule.Text = "未设置";
        tblkParentModule.Tag = null;
        tblkParentModule.ToolTip = null;
      }
      else {
        psp_module.parent_module = pmodule.guid;
        tblkParentModule.Text = pmodule.name;
        tblkParentModule.Tag = pmodule.guid;
        tblkParentModule.ToolTip = pmodule.description;
      }

    }

    /// <summary>
    /// 添加 子模块
    /// </summary>
    /// <param name="pmodule"></param>
    public void AddChildModule( PSPModule pmodule ) {
      if ( pmodule == null || psp_module == null )
        return;
      if ( psp_module.children_modules == null )
        psp_module.children_modules = new List<ModuleReference>();

      ModuleReference mr = new ModuleReference( pmodule.name, pmodule.guid );
      psp_module.children_modules.Add( mr );
      AddChildModuleInContainer( mr );
    }

    /// <summary>
    /// 添加 标签
    /// </summary>
    /// <param name="tr"></param>
    public void AddTag( TagReference tr ) {
      if ( tr == null )
        return;
      if ( psp_module.tags == null )
        psp_module.tags = new List<TagReference>();

      psp_module.tags.Add( tr );
      AddTagInContainer( tr );
      // 添加到 最近使用的标签 列表
      DataHelper.SaveTag( tr );
    }

  }
}
