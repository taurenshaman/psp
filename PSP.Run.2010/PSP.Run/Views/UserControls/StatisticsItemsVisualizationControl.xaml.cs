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
using System.Windows.Controls.DataVisualization.Charting;

namespace PSP.Run.Views.UserControls {
  /// <summary>
  /// Interaction logic for TagDataVisualizationControl.xaml
  /// </summary>
  public partial class StatisticsItemsVisualizationControl : UserControl {
    List<DataAccess.PSP.StatisticsItem> data = null;

    public StatisticsItemsVisualizationControl() {
      InitializeComponent();
      
      //this.Loaded += new RoutedEventHandler( StatisticsItemsVisualizationControl_Loaded );
    }

    void StatisticsItemsVisualizationControl_Loaded( object sender, RoutedEventArgs e ) {
    }

    /// <summary>
    /// 使用某天的PSP统计数据更新数据图
    /// </summary>
    /// <param name="dt"></param>
    public void UpdateWithStatisticsData( List<DataAccess.PSP.StatisticsItem> StatisticsData ) {
      pieSeries.ItemsSource = null;
      data = StatisticsData;
      pieSeries.ItemsSource = data;
    }

    /// <summary>
    /// 设置chartingToolkit:Chart的标题
    /// </summary>
    /// <param name="title"></param>
    public void SetTitle( string title ) {
      chart.Title = title;
    }

    private void pieSeries_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      // Test--Okay
      DataAccess.PSP.StatisticsItem si = (DataAccess.PSP.StatisticsItem)pieSeries.SelectedItem;
      if ( si == null ) return;
      chart.Title = si.name;
    }

  }
}
