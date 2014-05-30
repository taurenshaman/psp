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

using System.Windows.Media.Animation;
using DataAccess.PSP;

namespace PSP.Run.Views.UserControls {
  /// <summary>
  /// Interaction logic for PSPDay.xaml
  /// </summary>
  public partial class PSPDay : UserControl {
    DataAccess.PSP.PSPDay pspToday = null;
    Storyboard sbEventNewCollapse = null,
      sbEventNewVisible = null,
      sbEventNewItemVisible = null;

    /// <summary>
    /// Current PSPEvent in ecNew
    /// </summary>
    PSPEvent psp_event = null;
    EventControl ecNewItem = null;

    ///// <summary>
    ///// 忽略日期的更新
    ///// </summary>
    //bool IngoreUpdate = false;

    public PSPDay() {
      InitializeComponent();
      this.Loaded += new RoutedEventHandler( PSPDay_Loaded );
      //lbDayEvents.MouseLeftButtonUp += new MouseButtonEventHandler( lbDayEvents_MouseLeftButtonUp );
    }

    void PSPDay_Loaded( object sender, RoutedEventArgs e ) {
      dpDay.SelectedDate = DateTime.Now;
      // 初始化 今天 的PSP数据
      initializeData( DateTime.Today );

      ecNew.IsInListContainer = false;
      //tdvcTags.SetTitle( "标签 时间分布" );
      tdvcModules.SetTitle( "模块 时间分布" );

      //test();

      sbEventNewCollapse = this.Resources["sbEventNewCollapse"] as Storyboard;
      sbEventNewVisible = this.Resources["sbEventNewVisible"] as Storyboard;
      sbEventNewItemVisible = this.Resources["sbEventNewItemVisible"] as Storyboard;

      sbEventNewCollapse.Completed += new EventHandler( sbEventNewCollapse_Completed );
      sbEventNewVisible.Completed += new EventHandler( sbEventNewVisible_Completed );

      tblkBakInfo.Text = "已经备份：" + DataHelper.BackupFilesCount( DateTime.Today ).ToString();
    }

    //public void initializeData() {
    //  pspToday = DataHelper.PSP_Week.Get_PSPDay( DateTime.Today );
    //  if ( pspToday == null )
    //    pspToday = new DataAccess.PSP.PSPDay();

    //}

    //void lbDayEvents_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
    //  if ( lbDayEvents.SelectedIndex < 0 )
    //    return;
    //  EventControl ec = lbDayEvents.SelectedItem as EventControl;
    //  if ( ec == null )
    //    return;

    //  //ec.HighlightTheControl();
    //}

    /// <summary>
    /// 载入某天的数据
    /// </summary>
    /// <param name="day"></param>
    void initializeData( DateTime day ) {
      pspToday = DataHelper.GetDayData( day );
      if ( pspToday == null ) {
        pspToday = new DataAccess.PSP.PSPDay();
        // 更新图表控件
        //tdvcTags.UpdateWithStatisticsData( pspToday.Statistics.TagsStatistics );
        tdvcModules.UpdateWithStatisticsData( pspToday.Statistics.ModulesStatistics );
        return;
      }
      // 更新图表控件
      //tdvcTags.UpdateWithStatisticsData( pspToday.Statistics.TagsStatistics );
      tdvcModules.UpdateWithStatisticsData( pspToday.Statistics.ModulesStatistics );

      // 使用数据初始化dpDayEvents控件
      InitializeEventList( ref dpDayEvents, pspToday );
      // 更新当日的已统计时间
      UpdateTimeCount( pspToday );
    }

    private void Expander_Expanded( object sender, RoutedEventArgs e ) {
      //Expander expad = sender as Expander;
    }

    private void btnNew_Click( object sender, RoutedEventArgs e ) {
      // 判断：可能没有设置结束时间
      if ( ( ecNew.tbEnd.Text.Equals( "00:00:00" ) || ecNew.tbEnd.Text.Equals( "00:00" ) )
        && ecNew.tbDuration.Text.Equals( "0" )
        && MessageBox.Show( "您可能没有设置结束时间，要继续吗？", "注意", MessageBoxButton.YesNo ) == MessageBoxResult.No ) {
        return;
      }

      psp_event = ecNew.PSP_Event;
      psp_event.date = (DateTime)( dpDay.SelectedDate );
      psp_event.week_number = DataHelper.WeekNumber;

      if ( psp_event == null ) {
        MessageBox.Show( "时间格式有误，应该是00:00", "注意", MessageBoxButton.OK );
        return;
      }
      if ( psp_event.time_duration < 0 ) {
        MessageBox.Show( "时间统计有误……", "注意", MessageBoxButton.OK );
        return;
      }
      // 标签与记录，二选一或二选二
      if ( psp_event.tags == null || psp_event.tags.Count == 0 ) {
        if ( psp_event.record == null || psp_event.record.Length == 0 ) {
          MessageBox.Show( "未记录刚刚做的事情……\n您可以添加标签，也可以填写简单的记录。", "注意", MessageBoxButton.OK );
          return;
        }
      }
      // 模块
      if ( psp_event.module == null ) {
        MessageBoxResult mbr = MessageBox.Show( "您没有设置所属模块，要继续吗？若继续，将影响统计效果。", "注意", MessageBoxButton.YesNo );
        if ( mbr == MessageBoxResult.No ) return;
      }

      ecNewItem = new EventControl();
      ecNewItem.PSP_Event = psp_event;
      ecNewItem.Height = 0;
      ecNewItem.Tag = Guid.NewGuid().ToString();
      ecNewItem.imageDelete.Tag = ecNewItem.Tag;
      ecNewItem.imageDelete.IsEnabled = true;
      ecNewItem.imageDelete.MouseLeftButtonUp += new MouseButtonEventHandler( imageDelete_MouseLeftButtonUp );

      // * Solution 1 -- ListBox
      //lbDayEvents.Items.Insert( 0, ec );

      // * Solution 1.5 -- DockPanel
      ecNewItem.SetValue( DockPanel.DockProperty, Dock.Top );
      dpDayEvents.Children.Insert( 0, ecNewItem );

      // * Solution 2 -- DataGrid
      //dgDayEvents.ItemsSource = null;
      //day_events.Add( psp_event );
      //dgDayEvents.ItemsSource = day_events;

      // Save
      DataHelper.AddPSPEvent( psp_event, psp_event.date.Date );

      // 更新统计图表
      pspToday = DataHelper.GetDayData( psp_event.date );
      if ( pspToday != null ) {
        //tdvcTags.UpdateWithStatisticsData( pspToday.Statistics.TagsStatistics );
        tdvcModules.UpdateWithStatisticsData( pspToday.Statistics.ModulesStatistics );
      }
      // 更新当日的已统计时间
      UpdateTimeCount( pspToday );

      // Animation
      sbEventNewCollapse.Begin();
      sbEventNewItemVisible.Begin( ecNewItem );

      btnNew.IsEnabled = false;
    }

    // 删除事件
    void imageDelete_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      Image image = sender as Image;
      if ( image == null )
        return;
      MessageBoxResult mbr = MessageBox.Show( "注意：此操作不可恢复！", "确认删除？", MessageBoxButton.YesNo );
      if ( mbr == MessageBoxResult.No )
        return;
      int ecIndex2Del = GetEventControlByTag( image.Tag as string );
      if ( ecIndex2Del < 0 ) {
        MessageBox.Show( "删除操作失败，原因未知。", "未知错误", MessageBoxButton.OK );
        return;
      }
      PSPEvent pe2Del = ( (EventControl)dpDayEvents.Children[ecIndex2Del] ).PSP_Event;
      // remove
      dpDayEvents.Children.RemoveAt( ecIndex2Del );
      pspToday.RemoveEvent( pe2Del );
      // Update to file
      DataHelper.UpdateDayData( pspToday );
      // 更新当日的已统计时间
      UpdateTimeCount( pspToday );
    }

    /// <summary>
    /// 使用DataAccess.PSP.PSPDay中的事件初始化DockPanel
    /// </summary>
    /// <param name="dp"></param>
    /// <param name="pday"></param>
    void InitializeEventList( ref DockPanel dp, DataAccess.PSP.PSPDay pday ) {
      if ( dp == null )
        return;
      dp.Children.Clear();
      if ( pday == null )
        return;
      foreach ( PSPEvent pe in pday.PSP_Events ) {
        PSP.Run.Views.UserControls.EventControl ec = new PSP.Run.Views.UserControls.EventControl();
        ec.PSP_Event = pe;

        // for delete
        ec.Tag = Guid.NewGuid().ToString();
        ec.imageDelete.Tag = ec.Tag;
        ec.imageDelete.IsEnabled = true;
        ec.imageDelete.MouseLeftButtonUp += new MouseButtonEventHandler( imageDelete_MouseLeftButtonUp );

        ec.SetValue( DockPanel.DockProperty, Dock.Top );
        dp.Children.Insert( 0, ec );
      }
    }

    void sbEventNewCollapse_Completed( object sender, EventArgs e ) {
      // reset
      ecNew.Reset();

      // Visible animation
      sbEventNewVisible.Begin();
    }

    void sbEventNewVisible_Completed( object sender, EventArgs e ) {
      btnNew.IsEnabled = true;
    }

    /// <summary>
    /// 根据g==EventControl.Tag从dpDayEvents.Children中查找EventControl
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    int GetEventControlByTag( string g ) {
      if ( g == null || g.Length == 0 )
        return -1;
      
      EventControl ec = null;
      int index = -1;
      
      for ( int i = 0; i < dpDayEvents.Children.Count; i++ ) {
        ec = dpDayEvents.Children[i] as EventControl;
        if ( ec == null )
          continue;
        if ( string.Equals( ec.Tag as string, g ) ) {
          index = i;
          break;
        }

      }

      return index;
    }

    private void btnUpdate_Click( object sender, RoutedEventArgs e ) {
      // backup first
      DataHelper.BackupWeekData( (DateTime)dpDay.SelectedDate );
      tblkBakInfo.Text = "已经备份：" + DataHelper.BackupFilesCount( DateTime.Today ).ToString();

      pspToday.PSP_Events.Clear();
      for ( int i = 0; i < dpDayEvents.Children.Count; i++ ) {
        EventControl ec = dpDayEvents.Children[i] as EventControl;
        if ( ec == null )
          continue;
        ec.UpdateDate( (DateTime)( dpDay.SelectedDate ) );
        dpDayEvents.Children[i] = ec;
        PSPEvent pe = ec.PSP_Event;
        pe.date = (DateTime)( dpDay.SelectedDate );
        pspToday.PSP_Events.Add( pe );
      }

      // Update
      DataHelper.UpdateDayData( pspToday );
    }

    private void dpDay_SelectedDateChanged( object sender, SelectionChangedEventArgs e ) {
      //ecNew.UpdateDate( (DateTime)( dpDay.SelectedDate ) );
    }

    /// <summary>
    /// 更改日期，重新载入数据，也会更改新建PSPEvent的控件日期
    /// </summary>
    /// <param name="day"></param>
    public void UpdateDate( DateTime day ) {
      ecNew.UpdateDate( day );
      dpDay.SelectedDate = day;
      // clear
      dpDayEvents.Children.Clear();
      // reload
      initializeData( day );
      // 更新当日的已统计时间
      UpdateTimeCount( pspToday );
    }

    /// <summary>
    /// 更新当日的已统计时间
    /// </summary>
    void UpdateTimeCount( DataAccess.PSP.PSPDay pspToday ) {
      if ( pspToday == null ) {
        tblkTimeCount.Text = "暂无记录";
        return;
      }
      tblkTimeCount.Text = "已经统计：" + pspToday.TimeCount() + "分钟";
    }

    void test() {
      for ( int i = 0; i < 10; i++ ) {
        PSPEvent pe = new PSPEvent();
        pe.time_start = DateTime.Now;
        pe.time_end = DateTime.Now;
        pe.time_duration = i + 1;
        pe.record = i.ToString();

        EventControl ec = new EventControl();
        ec.PSP_Event = pe;
        ec.AddTag( "tag A" + i.ToString(), false, Guid.Empty );
        ec.AddTag( "tag B" + i.ToString(), false, Guid.Empty );
        ec.AddTag( "tag C" + i.ToString(), false, Guid.Empty );
        ec.AddTag( "tag D" + i.ToString(), false, Guid.Empty );

        ec.SetValue( DockPanel.DockProperty, Dock.Top );
        dpDayEvents.Children.Insert( 0, ec );
      }
    }

  }
}
