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
  /// Interaction logic for EventControl.xaml
  /// </summary>
  public partial class EventControl : UserControl {
    DataAccess.PSP.PSPEvent psp_event = null;
    SolidColorBrush scbGray = new SolidColorBrush( Colors.Gray ),
      scbLightBlue = new SolidColorBrush( Colors.LightBlue ),
      scbSteelBlue = new SolidColorBrush( Colors.SteelBlue );
    //Storyboard sbBorderDisappear, sbBorderOccur;

    bool isInListContainer = true;
    /// <summary>
    /// 是否在一个List容器中，如ListBox/DockPanel...
    /// </summary>
    public bool IsInListContainer {
      get {
        return isInListContainer;
      }
      set {
        isInListContainer = value;
        if ( !isInListContainer ) {
          imageDelete.Opacity = 1;
          imageInfo.Opacity = 1;
          imageTag.Opacity = 1;
        }

      }
    }

    public DataAccess.PSP.PSPEvent PSP_Event {
      get {
        try {
          // 预处理：将用户输入的中文冒号替换为英文冒号
          tbStart.Text = tbStart.Text.Replace( "：", ":" );
          tbEnd.Text = tbEnd.Text.Replace( "：", ":" );
          // 预处理：替换０１２３４５６７８９ 为0123456789
          tbStart.Text = replaceDigits( tbStart.Text );
          tbEnd.Text = replaceDigits( tbEnd.Text );
          tbDuration.Text = replaceDigits( tbDuration.Text );

          psp_event.time_start = Convert.ToDateTime( tbStart.Text.Trim() );
          psp_event.time_end = Convert.ToDateTime( tbEnd.Text.Trim() );
          psp_event.time_duration = Convert.ToInt32( tbDuration.Text );
        }
        catch ( Exception ex ) {
          return null;
        }
        // module -- 主要适用于PSPDay.xaml.cs中的btnUpdate_Click事件
        if ( psp_event.module == null || psp_event.module.name == null || psp_event.module.name.Length == 0 ) {
          Guid g = (Guid)btnModule.Tag;
          if ( !Guid.Equals( g, Guid.Empty ) ) {
            PSPModule pm = DataHelper.GetModule( g );
            psp_event.module = pm;

          } // !Guid...
        } // if

        // tags -- 主要适用于PSPDay.xaml.cs中的btnUpdate_Click事件
        if ( ( psp_event.tags == null || psp_event.tags.Count == 0 ) &&
           dpTags.Children.Count > 0 ) {
          foreach ( UIElement uie in dpTags.Children ) {
            TextBlock tblk = uie as TextBlock;
            if ( tblk == null )
              continue;
            Guid g = (Guid)tblk.Tag;
            TagReference tr = new TagReference( tblk.Text.Trim(), g );
            psp_event.tags.Add( tr );

          } // for
        } // if

        psp_event.record = tbRecord.Text.Trim();
        psp_event.date = DateTime.Today;
        
        return psp_event;
      }
      set {
        psp_event = value;

        tbStart.Text = psp_event.time_start.ToLongTimeString();
        tbEnd.Text = psp_event.time_end.ToLongTimeString();
        tbDuration.Text = psp_event.time_duration.ToString();
        tbRecord.Text = psp_event.record;
        // module
        if ( psp_event.module != null ) {
          btnModule.Content = psp_event.module.name;
          btnModule.Tag = psp_event.module.guid;
          btnModule.ToolTip = "右键点击，移除所属模块\n模块：" + psp_event.module.name + "\n描述：" + psp_event.module.description;
        }
        else {
          btnModule.Content = "所属模块";
          btnModule.Tag = null;
          btnModule.ToolTip = "未设置";
        }
        // tags
        dpTags.Children.Clear();
        if ( psp_event.tags != null && psp_event.tags.Count > 0 ) {
          foreach ( TagReference tr in psp_event.tags ) {
            AddTagInContainer( tr.name, tr.guid, false );
          }
        }

      }
    }

    /// <summary>
    /// 向容器中添加标签控件及数据
    /// </summary>
    /// <param name="name"></param>
    /// <param name="guid"></param>
    private void AddTagInContainer( string name, Guid guid, bool addToDataList ) {
      TextBlock tblkTag = new TextBlock();
      tblkTag.Margin = new Thickness( 2 );
      tblkTag.Background = Brushes.LightBlue;

      tblkTag.Text = name;
      if ( !Guid.Equals( guid, Guid.Empty ) ) {
        tblkTag.Tag = guid;
        tblkTag.ToolTip = "GUID: " + guid.ToString() + "\n鼠标右键点击，删除此项";
        if( addToDataList)
          psp_event.tags.Add( new DataAccess.PSP.TagReference( name, guid ) );
      }
      else {
        tblkTag.Tag = null;
        tblkTag.ToolTip = "普通标签\n鼠标右键点击，删除此项";
        if( addToDataList)
          psp_event.tags.Add( new DataAccess.PSP.TagReference( name, Guid.Empty ) );
      }
      tblkTag.MouseRightButtonUp += new MouseButtonEventHandler( tblkTag_MouseRightButtonUp );

      tblkTag.SetValue( DockPanel.DockProperty, Dock.Left );
      dpTags.Children.Insert( 0, tblkTag );
    }

    public EventControl() {
      InitializeComponent();

      psp_event = new DataAccess.PSP.PSPEvent();
      //this.DataContext = psp_event;
      //psp_event.module = null;

      //sbBorderDisappear = this.Resources["sbBorderDisappear"] as Storyboard;
      //sbBorderOccur = this.Resources["sbBorderOccur"] as Storyboard;
    }

    private void imageStartEnd_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      Image image = sender as Image;
      if ( image == null )
        return;

      if ( image.Name.EndsWith( "Start" ) ) {
        tbStart.Text = DateTime.Now.ToLongTimeString();

        psp_event.time_duration = 0;
        tbDuration.Text = "0";

        imageEnd.IsEnabled = true;
        imageEnd.OpacityMask = null;
        imageEnd.Opacity = 1;
      }
      else if ( image.Name.EndsWith( "End" ) ) {
        tbEnd.Text = DateTime.Now.ToLongTimeString();

        try {
          psp_event.time_start = Convert.ToDateTime( tbStart.Text.Trim() );
          psp_event.time_end = Convert.ToDateTime( tbEnd.Text.Trim() );
          psp_event.time_duration = Convert.ToInt32( tbDuration.Text );

          TimeSpan ts = psp_event.time_end - psp_event.time_start;
          psp_event.time_duration = ts.Hours * 60 + ts.Minutes;
          if ( ts.Seconds > 30 || psp_event.time_duration == 0 )
            psp_event.time_duration++;

          tbDuration.Text = psp_event.time_duration.ToString();
        }
        catch ( Exception ex ) {
        }

      }
    }

    private void btnModule_MouseRightButtonUp( object sender, MouseButtonEventArgs e ) {
      SelectModuleWindow.moduleSelected = null;
      psp_event.module = null;
      btnModule.Content = "所属模块";
      btnModule.ToolTip = "未设置";
    }

    private void btnModule_Click( object sender, RoutedEventArgs e ) {
      SelectModuleWindow smwSelectModule = new SelectModuleWindow();
      smwSelectModule.Closed += new EventHandler( smwSelectModule_Closed );
      smwSelectModule.ShowDialog();
    }

    // 选择模块 窗体即将关闭
    void smwSelectModule_Closed( object sender, EventArgs e ) {
      SelectModuleWindow smwSelectModule = sender as SelectModuleWindow;
      if ( smwSelectModule == null || SelectModuleWindow.moduleSelected == null )
        return;
      if ( psp_event == null )
        psp_event = new PSPEvent();
      psp_event.module = SelectModuleWindow.moduleSelected;
      btnModule.Content = psp_event.module.name;
      btnModule.Tag = psp_event.module.guid;
      btnModule.ToolTip = "右键点击，移除所属模块\n模块：" + psp_event.module.name + "\n描述：" + psp_event.module.description;
    }

    private void imageTag_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      SelectTagWindow stwSelectTag = new SelectTagWindow();
      stwSelectTag.Closed += new EventHandler( stwSelectTag_Closed );
      stwSelectTag.ShowDialog();
    }

    // 选择标签 窗体即将关闭
    void stwSelectTag_Closed( object sender, EventArgs e ) {
      SelectTagWindow stwSelectTag = sender as SelectTagWindow;
      if ( stwSelectTag == null || SelectTagWindow.tagSelected == null )
        return;
      TagReference trSelected = SelectTagWindow.tagSelected;
      if ( psp_event == null )
        psp_event = new PSPEvent();
      if ( psp_event.tags == null )
        psp_event.tags = new List<DataAccess.PSP.TagReference>();
      int index = psp_event.GetTagIndex( trSelected );
      if ( index < 0 ) {
        AddTagInContainer( trSelected.name, trSelected.guid, true );
      }
      else
        MessageBox.Show( "已经存在这个标签……", "提示", MessageBoxButton.OK );
    }

    void ResetTags() {
      if( psp_event.tags != null )
        psp_event.tags.Clear();

      dpTags.Children.Clear();
    }

    private void ResetModule() {
      psp_event.module = null;
      btnModule.Tag = null;
      btnModule.Content = "所属模块";
      btnModule.ToolTip = "未设置";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isConcept">是否是语义标签。</param>
    /// <param name="g">isConcept=True：g!=Guid.Empty；isConcept=False：g=Guid.Empty</param>
    public void AddTag( string name, bool isConcept, Guid g ) {
      if ( isConcept && Guid.Equals( g, Guid.Empty ) ) {
        MessageBox.Show( "无法确定是普通标签，还是语义标签……", "注意", MessageBoxButton.OK );
        return;
      }

      if ( psp_event == null )
        psp_event = new DataAccess.PSP.PSPEvent();
      if ( psp_event.tags == null )
        psp_event.tags = new List<DataAccess.PSP.TagReference>();

      AddTagInContainer( name, g, true );
    }

    private void tblkTag_MouseRightButtonUp( object sender, MouseButtonEventArgs e ) {
      TextBlock tblk = sender as TextBlock;
      if ( tblk == null )
        return;

      // Remove from
      Guid guid = Guid.Empty;
      if ( tblk.Tag != null )
        guid = (Guid)( tblk.Tag );
      for ( int i = 0; i < psp_event.tags.Count; i++ ) {
        DataAccess.PSP.TagReference tr = psp_event.tags[i];
        if ( Guid.Equals( tr.guid, guid ) && string.Equals( tblk.Text, tr.name ) ) {
          psp_event.tags.RemoveAt( i );
          break;
        } // if
      } // for

      dpTags.Children.Remove( tblk );
    }

    public void Reset() {
      imageEnd.IsEnabled = false;
      imageEnd.OpacityMask = scbGray;
      imageEnd.Opacity = 0.5;

      tbStart.Text = "00:00:00";
      tbEnd.Text = "00:00:00";
      tbDuration.Text = "0";

      tbRecord.Text = "";
      psp_event.ResetWeekNumber();

      ResetModule();
      ResetTags();
    }

    /// <summary>
    /// 暗化控件
    /// </summary>
    public void FadeTheControl( double opacity ) {
      if ( opacity < 0 || opacity > 1 ) {
        this.Opacity = 0.5;
        return;
      }

      this.Opacity = opacity;
    }
    /// <summary>
    /// 高亮控件
    /// </summary>
    public void HighlightTheControl() {
      this.Opacity = 1;
    }

    public void UpdateDate( DateTime date ) {
      psp_event.date = date;
    }

    /// <summary>
    /// 替换０１２３４５６７８９ 为0123456789
    /// </summary>
    /// <param name="old"></param>
    /// <returns></returns>
    string replaceDigits( string old ) {
      StringBuilder sb = new StringBuilder( old );
      sb = sb.Replace( "０", "0" );
      sb = sb.Replace( "１", "1" );
      sb = sb.Replace( "２", "2" );

      sb = sb.Replace( "３", "3" );
      sb = sb.Replace( "４", "4" );
      sb = sb.Replace( "５", "5" );

      sb = sb.Replace( "６", "6" );
      sb = sb.Replace( "７", "7" );
      sb = sb.Replace( "８", "8" );
      sb = sb.Replace( "９", "9" );
      return sb.ToString();
    }

    // 鼠标进入控件
    private void userControl_MouseEnter( object sender, MouseEventArgs e ) {
      if ( !IsInListContainer )
        return;
      border.BorderBrush = scbSteelBlue;
      imageDelete.Opacity = 1;
      imageInfo.Opacity = 1;
      imageTag.Opacity = 1;
    }

    // 鼠标离开控件
    private void userControl_MouseLeave( object sender, MouseEventArgs e ) {
      if ( !IsInListContainer )
        return;
      border.BorderBrush = scbLightBlue;
      imageDelete.Opacity = 0.3;
      imageInfo.Opacity = 0.3;
      imageTag.Opacity = 0.3;
    }

    //private void UserControl_MouseEnter( object sender, MouseEventArgs e ) {
    //  if ( !IsInListContainer )
    //    return;
    //  Storyboard sb = (Storyboard)this.FindResource( "sbHighlight" );
    //  if ( sb == null )
    //    return;
    //  sb.Begin( this );
    //}

    //private void UserControl_MouseLeave( object sender, MouseEventArgs e ) {
    //  if ( !IsInListContainer )
    //    return;
    //  Storyboard sb = (Storyboard)this.FindResource( "sbFade" );
    //  if ( sb == null )
    //    return;
    //  sb.Begin( this );
    //}

  }
}
