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

namespace PSP.Run.Views {
  /// <summary>
  /// SelectTagWindow.xaml 的交互逻辑
  /// </summary>
  public partial class SelectTagWindow : Window {
    public static TagReference tagSelected = null;

    public SelectTagWindow() {
      InitializeComponent();

      this.Loaded += new RoutedEventHandler( SelectTagWindow_Loaded );
    }

    void SelectTagWindow_Loaded( object sender, RoutedEventArgs e ) {
      ccTags.lbConcepts.ToolTip = null;
      ccTags.lbRecentTags.ToolTip = null;
      
      //ccTags.btnAddSimpleTag.Content = "普通标签";
      //ccTags.btnAddSimpleTag.ToolTip = "未找到相关的语义标签数据，将其作为 普通标签 使用";
      ccTags.btnAddSimpleTag.Click += new RoutedEventHandler( btnAddSimpleTag_Click );

      ccTags.lbConcepts.MouseLeftButtonUp += new MouseButtonEventHandler( lbConcepts_MouseLeftButtonUp );
      ccTags.lbRecentTags.MouseLeftButtonUp += new MouseButtonEventHandler( lbRecentTags_MouseLeftButtonUp );

      tagSelected = null;
   }

    void btnAddSimpleTag_Click( object sender, RoutedEventArgs e ) {
      tagSelected = new TagReference( ccTags.tbTagName.Text.Trim(), Guid.Empty );
      tblkSelectedTag.Text = tagSelected.name;
      tblkSelectedTag.ToolTip = "普通标签";

      btnSelectIt.IsEnabled = true;
    }

    void lbRecentTags_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      tblkSelectedTag.Text = "";
      tblkSelectedTag.ToolTip = null;

      if ( ccTags.lbRecentTags.SelectedIndex < 0 )
        return;
      //ListBoxItem lbi = ccTags.lbRecentTags.SelectedItem as ListBoxItem;
      //if ( lbi == null )
      //  return;
      //tagSelected = new TagReference( lbi.Content as string, (Guid)( lbi.Tag ) );
      
      tagSelected = ccTags.lbRecentTags.SelectedItem as TagReference;
      if ( tagSelected == null )
        return;

      tblkSelectedTag.Text = tagSelected.name;
      if ( Guid.Equals( Guid.Empty, tagSelected.guid ) )
        tblkSelectedTag.ToolTip = "普通标签";
      else
        tblkSelectedTag.ToolTip = "GUID: " + tagSelected.guid.ToString();
      
      btnSelectIt.IsEnabled = true;
    }

    void lbConcepts_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      tblkSelectedTag.Text = "";
      tblkSelectedTag.ToolTip = null;

      if ( ccTags.lbConcepts.SelectedIndex < 0 )
        return;
      ListBoxItem lbi = ccTags.lbConcepts.SelectedItem as ListBoxItem;
      if ( lbi == null )
        return;

      tagSelected = new TagReference( lbi.Content as string, (Guid)( lbi.Tag ) );
      tblkSelectedTag.Text = tagSelected.name;
      tblkSelectedTag.ToolTip = "GUID: " + tagSelected.guid.ToString();
      
      btnSelectIt.IsEnabled = true;
    }

    private void btnSelectIt_Click( object sender, RoutedEventArgs e ) {
      this.Close();
    }

    private void btnCancel_Click( object sender, RoutedEventArgs e ) {
      tagSelected = null;
      this.Close();
    }

    private void Window_KeyUp( object sender, KeyEventArgs e ) {
      if ( e.Key == Key.Escape )
        btnCancel_Click( btnCancel, null );
    }

  }

}
