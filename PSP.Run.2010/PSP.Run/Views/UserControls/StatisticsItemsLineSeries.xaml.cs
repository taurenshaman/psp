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

namespace PSP.Run.Views.UserControls {
  /// <summary>
  /// StatisticsItemsLineSeries.xaml 的交互逻辑
  /// </summary>
  public partial class StatisticsItemsLineSeries : UserControl {
    List<DataAccess.PSP.StatisticsItem> data = null;

    public StatisticsItemsLineSeries() {
      InitializeComponent();

      //this.Loaded += new RoutedEventHandler( StatisticsItemsLineSeries_Loaded );
    }

    void StatisticsItemsLineSeries_Loaded( object sender, RoutedEventArgs e ) {
      
    }

    /// <summary>
    /// 使用某天的PSP统计数据更新数据图
    /// </summary>
    /// <param name="dt"></param>
    public void UpdateWithStatisticsData( List<DataAccess.PSP.StatisticsItem> StatisticsData ) {
      lineSeries.ItemsSource = null;
      data = StatisticsData;
      lineSeries.ItemsSource = data;
    }

    /// <summary>
    /// 设置chartingToolkit:Chart的标题
    /// </summary>
    /// <param name="title"></param>
    public void SetTitle( string title ) {
      chart.Title = title;
    }

  }
}
