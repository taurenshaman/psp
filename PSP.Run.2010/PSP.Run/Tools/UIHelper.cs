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

using DataAccess.PSP;

namespace PSP.Run.Tools {
  public class UIHelper {

    /// <summary>
    /// 使用TagReference列表初始化容器, lbi.Content = name;lbi.Tag = guid;
    /// </summary>
    /// <param name="lbContainer"></param>
    /// <param name="tag_list"></param>
    public static void InitializeTagList( ListBox lbContainer, List<TagReference> tag_list ) {
      if ( lbContainer == null )
        return;
      lbContainer.Items.Clear();
      if ( tag_list == null || tag_list.Count == 0 )
        return;

      foreach ( TagReference tr in tag_list ) {
        ListBoxItem lbi = new ListBoxItem();
        lbi.Content = tr.name;
        lbi.Tag = tr.guid;
        if ( tr.guid.Equals( Guid.Empty ) ) {
          lbi.ToolTip = "普通标签";
        }
        else {
          lbi.ToolTip = "语义标签\nGUID: " + tr.guid.ToString();
        }
        lbContainer.Items.Add( lbi );
      }

    }

    /// <summary>
    /// 使用PSPModule列表初始化容器, lbi.Content = name;lbi.Tag = guid;lbi.ToolTip = description;
    /// </summary>
    /// <param name="lbContainer"></param>
    /// <param name="module_list"></param>
    public static void InitializeModuleList( ListBox lbContainer, List<PSPModule> module_list ) {
      if ( lbContainer == null )
        return;
      lbContainer.Items.Clear();
      if ( module_list == null || module_list.Count == 0 )
        return;

      foreach ( PSPModule pm in module_list ) {
        ListBoxItem lbi = new ListBoxItem();
        lbi.Content = pm.name;
        lbi.Tag = pm.guid;
        lbi.ToolTip = pm.description;
        lbContainer.Items.Add( lbi );
      }
    }

    // 未完成
    public static void InitializeModuleList( TreeView tvContainer, List<PSPModule> module_list ) {
      if ( tvContainer == null )
        return;
      tvContainer.Items.Clear();
      if ( module_list == null || module_list.Count == 0 )
        return;

      foreach ( PSPModule pm in module_list ) {
        ListBoxItem lbi = new ListBoxItem();

        lbi.Content = pm.name;
        lbi.Tag = pm.guid;
        lbi.ToolTip = pm.description;
        //lbContainer.Items.Add( lbi );
      }
    }

    /// <summary>
    /// 使用PSPModule列表初始化容器, lbi.Content = name; lbi.Tag = pspModule
    /// </summary>
    /// <param name="lbContainer"></param>
    /// <param name="module_list"></param>
    public static void InitializeModuleList2( ListBox lbContainer, List<PSPModule> module_list ) {
      if ( lbContainer == null )
        return;
      lbContainer.Items.Clear();
      if ( module_list == null || module_list.Count == 0 )
        return;

      foreach ( PSPModule pm in module_list ) {
        ListBoxItem lbi = new ListBoxItem();

        lbi.Content = pm.name;
        lbi.Tag = pm;
        lbContainer.Items.Add( lbi );
      }
    }



  }
}
