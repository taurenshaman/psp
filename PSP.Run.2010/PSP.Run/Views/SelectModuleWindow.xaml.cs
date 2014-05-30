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

using System.ComponentModel;
using DataAccess.PSP;

namespace PSP.Run.Views {
  /// <summary>
  /// SelectModuleWindow.xaml 的交互逻辑
  /// </summary>
  public partial class SelectModuleWindow : Window {
    /// <summary>
    /// 选中的模块
    /// </summary>
    public static PSPModule moduleSelected = null;

    List<PSPModule> modules = null;

    public SelectModuleWindow() {
      InitializeComponent();

      this.Loaded += new RoutedEventHandler( SelectModuleWindow_Loaded );
    }

    void SelectModuleWindow_Loaded( object sender, RoutedEventArgs e ) {
      lbConcepts.MouseLeftButtonUp += new MouseButtonEventHandler( lbConcepts_MouseLeftButtonUp );

      // 初始化模块列表
      //Tools.UIHelper.InitializeModuleList( tvConcepts, DataHelper.PSP_Modules );
      // binding
      modules = DataHelper.PSP_Modules;
      lbConcepts.ItemsSource = modules;
      lbConcepts.Items.SortDescriptions.Add(
        new SortDescription( "referenced_times", ListSortDirection.Descending ) );
      
      moduleSelected = null;
    }

    void lbConcepts_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( lbConcepts.SelectedIndex < 0 )
        return;

      //ListBoxItem lbi = lbConcepts.SelectedItem as ListBoxItem;
      //if ( lbi == null || lbi.Tag == null )
      //  return;
      //moduleSelected = (PSPModule)lbi.Tag;

      moduleSelected = lbConcepts.SelectedItem as PSPModule;
      if ( moduleSelected == null )
        return;

      if ( moduleSelected != null ) {
        btnSelectIt.IsEnabled = true;
        tblkSelected.Text = moduleSelected.description;
      }
      else {
        btnSelectIt.IsEnabled = false;
        tblkSelected.Text = "";
      }
    }

    private void btnSelectIt_Click( object sender, RoutedEventArgs e ) {
      this.Close();
    }

    private void btnCancel_Click( object sender, RoutedEventArgs e ) {
      moduleSelected = null;
      this.Close();
    }

    private void Window_KeyUp( object sender, KeyEventArgs e ) {
      if ( e.Key == Key.Escape )
        btnCancel_Click( btnCancel, null );
    }
  }
}
