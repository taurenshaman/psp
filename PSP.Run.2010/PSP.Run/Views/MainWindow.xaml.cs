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

using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Taskbar;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using DataAccess.PSP;

namespace PSP.Run.Views {
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window {
    Configuration config = null;
    Views.UserControls.StatisticsItemsLineSeries silsWeek = null,
      silsDay = null;
    Storyboard sbRotateX = null,
      sbRotateY = null;

    DateTime dtLastMonday = DateTime.Today;

    #region Windows 7

    private const string appId = "PSP.Run";
    private const string progId = "PSP.Run";
    /// <summary>
    /// 注册*.psp文件类型的对话框，若系统支持，确定按钮带有UAC标记
    /// </summary>
    TaskDialog tdRegisterPSPFilesDialog;
    /// <summary>
    /// 程序的执行目路径：目录+xx.exe
    /// </summary>
    private string executablePath;
    private string executableFolder;

    /// <summary>
    /// 通过ThumbnailToolbarButton，每次旋转10度
    /// </summary>
    double minRotatingDegree = 10;

    /// <summary>
    /// 控制3D物体向左旋转minRotatingDegree
    /// </summary>
    private ThumbnailToolbarButton ttbtnRotateLeft;
    private ThumbnailToolbarButton ttbtnRotateRight;
    private ThumbnailToolbarButton ttbtnRotateClockwise;
    private ThumbnailToolbarButton ttbtnRotateAntiClockwise;

    //////// ==== JumpList ==== ////////

    /// <summary>
    /// 今日涉及的模块
    /// </summary>
    private JumpListCustomCategory jlccModulesUsedToday;
    private JumpList jumpList;

    #endregion Windows 7

    public MainWindow() {
      InitializeComponent();

      this.Loaded += new RoutedEventHandler( MainWindow_Loaded );
    }

    void MainWindow_Loaded( object sender, RoutedEventArgs e ) {
      executablePath = System.Reflection.Assembly.GetEntryAssembly().Location;
      executableFolder = System.IO.Path.GetDirectoryName( executablePath );
      config = Configuration.GetConfigration( executableFolder );
      // 第一次允许，设置PSP Data的路径信息
      if ( Configuration.PSP_Data_Path == null || Configuration.PSP_Data_Path.Length == 0 ) {
        NeedTo_SetPSP_Data_Path( "注意！", "配置文件中没有存放PSP数据的文件夹信息，您可能是第一次访问，要进行设置吗？" );
      }
      else if ( !System.IO.Directory.Exists( Configuration.PSP_Data_Path ) ) {
        NeedTo_SetPSP_Data_Path( "注意！", "存放PSP数据的文件夹不存在，您要重新选择吗？" );
      }
      else { // 正常访问

        #region 升级--具体的升级方法中判断是否进行该次升级

        DataHelper.Update_30Sept2009(); // 2009.9.30

        #endregion 升级--具体的升级方法中判断是否进行该次升级

        AfterLoaded();

      }

    }

    void tbdTrackball_LayoutUpdated( object sender, EventArgs e ) {
      // Windows7
      if ( TaskbarManager.IsPlatformSupported ) {
        // Get the offset for picturebox
        Vector v = VisualTreeHelper.GetOffset( tbdTrackball );
        // Set the thumbnail clip
        TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip( ( new WindowInteropHelper( this ) ).Handle,
          new System.Drawing.Rectangle( (int)v.X, (int)v.Y, (int)tbdTrackball.RenderSize.Width, (int)tbdTrackball.RenderSize.Height ) );
      }
    }

    private void AfterLoaded() {
      tiSomeDay.Header = DateTime.Today.ToLongDateString();

      silsWeek = this.Resources["silsWeek"] as Views.UserControls.StatisticsItemsLineSeries;
      silsDay = this.Resources["silsDay"] as Views.UserControls.StatisticsItemsLineSeries;
      sbRotateX = this.Resources["sbRotateX"] as Storyboard;
      sbRotateY = this.Resources["sbRotateY"] as Storyboard;

      silsWeek.SetTitle( "周统计信息" );
      silsDay.SetTitle( "日统计信息" );

      pdSomeDay.tdvcModules.pieSeries.MouseLeftButtonUp += new MouseButtonEventHandler( pieSeries_MouseLeftButtonUp );
      silsWeek.lineSeries.MouseLeftButtonUp += new MouseButtonEventHandler( lineSeries_MouseLeftButtonUp );

      tbdTrackball.LayoutUpdated += new EventHandler( tbdTrackball_LayoutUpdated );

      // 检查*.psp文件是否已经注册
      CheckPSPFileRegistration();

      #region Windows 7

      // Taskbar
      if ( TaskbarManager.IsPlatformSupported ) {
        TaskbarManager.Instance.ApplicationId = appId;

        #region ThumbnailToolbarButtons

        // ThumbnailToolbarButton
        ttbtnRotateLeft = new ThumbnailToolbarButton( Properties.Resources.left, "向左旋转" );
        ttbtnRotateRight = new ThumbnailToolbarButton( Properties.Resources.right, "向右旋转" );
        ttbtnRotateClockwise = new ThumbnailToolbarButton( Properties.Resources.up, "顺时针旋转" );
        ttbtnRotateAntiClockwise = new ThumbnailToolbarButton( Properties.Resources.down, "逆时针旋转" );

        ttbtnRotateLeft.Click += new EventHandler<ThumbnailButtonClickedEventArgs>( ttbtnRotateLeft_Click );
        ttbtnRotateRight.Click += new EventHandler<ThumbnailButtonClickedEventArgs>( ttbtnRotateRight_Click );
        ttbtnRotateClockwise.Click += new EventHandler<ThumbnailButtonClickedEventArgs>( ttbtnRotateClockwise_Click );
        ttbtnRotateAntiClockwise.Click += new EventHandler<ThumbnailButtonClickedEventArgs>( ttbtnRotateAntiClockwise_Click );

        ttbtnRotateLeft.Enabled = false;
        ttbtnRotateRight.Enabled = false;
        ttbtnRotateClockwise.Enabled = false;
        ttbtnRotateAntiClockwise.Enabled = false;

        // 下面方法的参数为UIElement的重载不能正常运行
        TaskbarManager.Instance.ThumbnailToolbars.AddButtons( new WindowInteropHelper( this ).Handle, ttbtnRotateLeft, ttbtnRotateClockwise, ttbtnRotateAntiClockwise, ttbtnRotateRight );

        #endregion ThumbnailToolbarButtons

        #region JumpList

        jlccModulesUsedToday = new JumpListCustomCategory( "今日涉及的模块" );
        jumpList = JumpList.CreateJumpList();

        jumpList.AddCustomCategories( jlccModulesUsedToday );
        updateJumpListUsingModulesUsedToday();

        //TaskbarManager.Instance.SetProgressState( TaskbarProgressBarState.NoProgress );
        
        #endregion JumpList

      }

      #endregion Windows 7

      // 通过参数传入周数据，初始化七个饼图
      string[] args = Environment.GetCommandLineArgs();
      if ( args.Length > 2 && args[1] == "/weekfile" ) {
        // 获取文件名
        string fileName = string.Join( " ", args, 2, args.Length - 2 );
        // 获取周一所属日期 psp_week_[date_of_monday].psp
        int start = fileName.LastIndexOf( "psp_week_" ); // 9
        if ( start < 0 )
          return;
        string d = fileName.Substring( start + 9, fileName.Length - start - 13 ); // 13= 9+4
        try {
          // 更新
          calendaDay.SelectedDate = DateTime.Parse( d );
          // 转向
          tiWeek.IsSelected = true;
          // 使窗体最大化
          this.WindowState = WindowState.Maximized;
        }
        catch ( Exception ex ) {
        }

      }
      // 使用 今天 所在的周数据，初始化七个饼图
      else {
        DateTime dtMonday = DataHelper.GetMondayDate( DateTime.Today );
        updateWeekDiagram( dtMonday, true );
      }

    }

    #region Windows 7

    void updateJumpListUsingModulesUsedToday() {
      if ( !TaskbarManager.IsPlatformSupported )
        return;

      string[] weekFiles = System.IO.Directory.GetFiles( Configuration.PSP_Data_Path, Configuration.PSP_WeekFile + "*.psp" );
      if ( weekFiles == null || weekFiles.Length == 0 )
        return;
      int len = Configuration.PSP_WeekFile.Length + Configuration.PSP_Data_Path.Length;
      foreach ( string wf in weekFiles ) {
        JumpListLink jll = new JumpListLink( wf, wf.Substring( len + 1, wf.Length - len - 1 - 4 ) );
        jll.IconReference = new IconReference( System.IO.Path.Combine( executableFolder, "PSP.Run.exe" ), 0 );
        jll.Arguments = "module";
        jll.ShowCommand = WindowShowCommand.Show;
        jumpList.AddUserTasks( jll );
      }
      jumpList.Refresh();

    }

    void ttbtnRotateAntiClockwise_Click( object sender, ThumbnailButtonClickedEventArgs e ) {
      sbRotateX.Children[0].SetValue( DoubleAnimation.FromProperty, rotateX.Angle );
      sbRotateX.Children[0].SetValue( DoubleAnimation.ToProperty, ( rotateX.Angle - minRotatingDegree ) );
      sbRotateX.Duration = new Duration( new TimeSpan( 0, 0, 0, 0, 300 ) );
      sbRotateX.Begin();
    }

    void ttbtnRotateClockwise_Click( object sender, ThumbnailButtonClickedEventArgs e ) {
      sbRotateX.Children[0].SetValue( DoubleAnimation.FromProperty, rotateX.Angle );
      sbRotateX.Children[0].SetValue( DoubleAnimation.ToProperty, ( rotateX.Angle + minRotatingDegree ) );
      sbRotateX.Duration = new Duration( new TimeSpan( 0, 0, 0, 0, 300 ) );
      sbRotateX.Begin();
    }

    void ttbtnRotateRight_Click( object sender, ThumbnailButtonClickedEventArgs e ) {
      sbRotateY.Children[0].SetValue( DoubleAnimation.FromProperty, rotateY.Angle );
      sbRotateY.Children[0].SetValue( DoubleAnimation.ToProperty, ( rotateY.Angle + minRotatingDegree ) );
      sbRotateY.Duration = new Duration( new TimeSpan( 0, 0, 0, 0, 300 ) );
      sbRotateY.Begin();
    }

    void ttbtnRotateLeft_Click( object sender, ThumbnailButtonClickedEventArgs e ) {
      sbRotateY.Children[0].SetValue( DoubleAnimation.FromProperty, rotateY.Angle );
      sbRotateY.Children[0].SetValue( DoubleAnimation.ToProperty, ( rotateY.Angle - minRotatingDegree ) );
      sbRotateY.Duration = new Duration( new TimeSpan( 0, 0, 0, 0, 300 ) );
      sbRotateY.Begin();
    }

    #endregion Windows 7

    void NeedTo_SetPSP_Data_Path( string title, string info ) {
      MessageBoxResult mbr = MessageBox.Show( info, title, MessageBoxButton.YesNo );
      if ( mbr == MessageBoxResult.No ) {
        this.Close();
        return;
      }

      // windows 7
      if ( CommonOpenFileDialog.IsPlatformSupported ) {
        CommonOpenFileDialog cofd = new CommonOpenFileDialog();
        cofd.Multiselect = false;
        cofd.AllowNonFileSystemItems = false;
        cofd.IsFolderPicker = true; // 只允许选择文件夹
        CommonFileDialogResult cfdr = cofd.ShowDialog();
        if ( cfdr == CommonFileDialogResult.OK ) {
          ShellContainer selectedSC = null;
          try {
            // Try to get a valid selected item
            selectedSC = cofd.FileAsShellObject as ShellContainer;
            Configuration.PSP_Data_Path = selectedSC.ParsingName;
            Configuration.Save( config, Configuration.AppExcuteFilePath );
            AfterLoaded();
          }
          catch {
            MessageBox.Show( "选取的文件夹无效，原因未知……囧" );
            NeedTo_SetPSP_Data_Path( "注意！", "您没有设置存放PSP数据的文件夹，现在要进行设置吗？" );
          }

        }
        else {
          NeedTo_SetPSP_Data_Path( "注意！", "您没有设置存放PSP数据的文件夹，现在要进行设置吗？" );
        }

      }
      // others
      else {
        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
        fbd.Description = "请选择数据存放的文件夹，建议放到系统盘之外";
        fbd.ShowNewFolderButton = true;
        System.Windows.Forms.DialogResult dr = fbd.ShowDialog();
        if ( dr == System.Windows.Forms.DialogResult.OK ) {
          Configuration.PSP_Data_Path = fbd.SelectedPath;
          Configuration.Save( config, Configuration.AppExcuteFilePath );
          AfterLoaded();
        }
        else {
          NeedTo_SetPSP_Data_Path( "注意！", "您没有设置存放PSP数据的文件夹，现在要进行设置吗？" );
        }

      }

    }

    // 默认：今天。更改日期，将更改PSPDay控件中的所有日期，并会重新载入数据
    private void calendaDay_SelectedDatesChanged( object sender, SelectionChangedEventArgs e ) {
      // 更改PSPDay控件中的所有日期，重新载入数据
      DateTime dt = (DateTime)( calendaDay.SelectedDate );
      pdSomeDay.UpdateDate( dt );
      tiSomeDay.Header = dt.ToLongDateString();
      
      // 更新七个饼图
      updateWeekDiagram( DataHelper.GetMondayDate( dt ), false );
    }

    // 显示 模块管理 窗体
    private void imageShowModuleMgtWindow_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      ModuleManagementWindow moduleMgtWindow = new ModuleManagementWindow();
      moduleMgtWindow.Show();
    }

    private void tabControl_SelectionChanged( object sender, SelectionChangedEventArgs e ) {
      if ( ti3D.IsSelected ) {
        // Windows7
        if ( TaskbarManager.IsPlatformSupported ) {
          ttbtnRotateLeft.Enabled = true;
          ttbtnRotateRight.Enabled = true;
          ttbtnRotateClockwise.Enabled = true;
          ttbtnRotateAntiClockwise.Enabled = true;
        }
      }
      else {
        // Windows7
        if ( TaskbarManager.IsPlatformSupported && ttbtnRotateAntiClockwise != null ) {
          ttbtnRotateLeft.Enabled = false;
          ttbtnRotateRight.Enabled = false;
          ttbtnRotateClockwise.Enabled = false;
          ttbtnRotateAntiClockwise.Enabled = false;
        }

      }

    }

    #region Statistics Diagram

    void pieSeries_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      // 在silsWeek上显示 某模块 的周统计数据
      System.Windows.Controls.DataVisualization.Charting.PieSeries pieSeries = sender as System.Windows.Controls.DataVisualization.Charting.PieSeries;
      if ( pieSeries == null ) return;
      DataAccess.PSP.StatisticsItem si = (DataAccess.PSP.StatisticsItem)pieSeries.SelectedItem;
      if ( si == null ) return;

      silsWeek.SetTitle( si.name + " - 周统计信息" );
      silsDay.SetTitle( si.name + " - 日统计信息" );
      ti3D.IsSelected = true;

      DateTime dt = DateTime.Today;
      if ( calendaDay.SelectedDate != null )
        dt = (DateTime)( calendaDay.SelectedDate );
      silsWeek.UpdateWithStatisticsData( DataHelper.Statistics_RecentFourWeeks( dt, si.name, si.guid ) );
    }

    void lineSeries_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      // 在silsDay上显示 某模块 的日统计数据
      System.Windows.Controls.DataVisualization.Charting.LineSeries lineSeries = sender as System.Windows.Controls.DataVisualization.Charting.LineSeries;
      if ( lineSeries == null ) return;
      DataAccess.PSP.StatisticsItem si = (DataAccess.PSP.StatisticsItem)lineSeries.SelectedItem;
      if ( si == null ) return;

      PSPModule pm = DataHelper.GetModule( si.guid );
      silsDay.UpdateWithStatisticsData( DataHelper.ModuleStatistics_SomeWeek( si.date, pm.name, pm.guid ) );

      sbRotateY.Children[0].SetValue( DoubleAnimation.FromProperty, rotateY.Angle );
      sbRotateY.Children[0].SetValue( DoubleAnimation.ToProperty, ( rotateY.Angle + 180 ) % 360 );
      sbRotateY.Duration = new Duration( new TimeSpan( 0, 0, 1 ) );
      sbRotateY.Begin();
    }

    /// <summary>
    /// 使用某周的数据初始化七个饼图
    /// </summary>
    /// <param name="dt">某个星期一所属日期</param>
    /// <param name="skipCompare">是否忽略dt与dtLastMonday的比较</param>
    void updateWeekDiagram( DateTime dt, bool skipCompare ) {
      if ( DateTime.Equals( dt.Date, dtLastMonday.Date ) && !skipCompare )
        return;
      dtLastMonday = dt;

      updateOneDayDiagram( DataHelper.GetDayData2( dt ), sivcMonday, "星期一" );
      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 1 ) ), sivcTuesday, "星期二" );

      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 2 ) ), sivcWednesday, "星期三" );
      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 3 ) ), sivcThursday, "星期四" );
      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 4 ) ), sivcFriday, "星期五" );

      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 5 ) ), sivcSaturday, "星期六" );
      updateOneDayDiagram( DataHelper.GetDayData2( dt.AddDays( 6 ) ), sivcSunday, "星期日" );
    }
    private void updateOneDayDiagram( PSPDay pday, UserControls.StatisticsItemsVisualizationControl sivc, string sivcTitle ) {
      if ( sivc == null )
        return;
      
      // 设置标题
      if( pday == null )
        sivc.SetTitle( sivcTitle );
      else
        sivc.SetTitle( sivcTitle + " (共计" + pday.TimeCount() + "分钟)" );

      // 更新统计图
      if ( pday == null || pday.Statistics == null )
        sivc.UpdateWithStatisticsData( null );
      else
        sivc.UpdateWithStatisticsData( pday.Statistics.ModulesStatistics );
    }

    #endregion Statistics Diagram

    #region System

    /// <summary>
    /// 检查*.psp文件是否已经注册
    /// </summary>
    private void CheckPSPFileRegistration() {
      bool registered = false;

      try {
        RegistryKey openWithKey = Registry.ClassesRoot.OpenSubKey( System.IO.Path.Combine( ".psp", "OpenWithProgIds" ) );
        if ( openWithKey != null ) { // 若openWithKey==null则表示还未注册
          string value = openWithKey.GetValue( progId, null ) as string;
          if ( value == null )
            registered = false;
          else
            registered = true;
        }

      }
      finally {
        // Windows7下的注册
        if ( !registered && TaskbarManager.IsPlatformSupported ) {
          RegistorPSPFiles_Windows7();
        }
        // 其它系统下的注册
        else if ( !registered ) {
          RegistorPSPFiles_OtherSystems();
        }
        // 已经注册
        else if( registered ) {
          afterRegisterPSPFiles();
        }

      }

    }

    /// <summary>
    /// 如果调用Win7中的TaskDialog失败，则通过RegistorPSPFiles_OtherSystems使用MessageBox提示用户
    /// </summary>
    private void RegistorPSPFiles_Windows7() {
      if ( !TaskbarManager.IsPlatformSupported )
        return;
      try {
        tdRegisterPSPFilesDialog = new TaskDialog();
        tdRegisterPSPFilesDialog.Text = "文件类型（*.psp）尚未注册";
        tdRegisterPSPFilesDialog.InstructionText = "程序PSP.Run需要注册*.psp类型的文件，以实现某些特性。";
        tdRegisterPSPFilesDialog.Icon = TaskDialogStandardIcon.Information;
        tdRegisterPSPFilesDialog.Cancelable = true;

        TaskDialogCommandLink btnRegisterPSPFiles = new TaskDialogCommandLink( "btnRegisterPSPFiles", "为该程序注册文件类型",
            "为该程序（PSP.Run）注册*.psp类型的文件，进行更好的体验。" );
        btnRegisterPSPFiles.Click += new EventHandler( btnRegisterPSPFiles_Click );
        // Show UAC sheild as this task requires elevation
        btnRegisterPSPFiles.ShowElevationIcon = true;

        tdRegisterPSPFilesDialog.Controls.Add( btnRegisterPSPFiles );
        TaskDialogResult tdr = tdRegisterPSPFilesDialog.Show();




        //TaskDialogResult tdr = TaskDialog.Show( "文件类型（*.psp）尚未注册", "程序PSP.Run需要注册*.psp类型的文件，以实现某些特性。", "注意" );
        //if( tdr == TaskDialogResult
      }
      catch ( Exception ex ) {
        // Windows7的对话窗体失败，使用一般的
        RegistorPSPFiles_OtherSystems();
      }

    }

    private void UnRegistorPSPFiles_Windows7() {
      if ( !TaskbarManager.IsPlatformSupported )
        return;
      try {
        tdRegisterPSPFilesDialog = new TaskDialog();
        tdRegisterPSPFilesDialog.Text = "文件类型（*.psp）已经注册";
        tdRegisterPSPFilesDialog.InstructionText = "程序PSP.Run已经注册*.psp类型的文件，您确认要 取消注册 吗？";
        tdRegisterPSPFilesDialog.Icon = TaskDialogStandardIcon.Information;
        tdRegisterPSPFilesDialog.Cancelable = true;

        TaskDialogCommandLink btnUnRegisterPSPFiles = new TaskDialogCommandLink( "btnUnRegisterPSPFiles", "取消注册" );
        btnUnRegisterPSPFiles.Click += new EventHandler( btnUnRegisterPSPFiles_Click );
        btnUnRegisterPSPFiles.ShowElevationIcon = true;

        tdRegisterPSPFilesDialog.Controls.Add( btnUnRegisterPSPFiles );
        TaskDialogResult tdr = tdRegisterPSPFilesDialog.Show();
      }
      catch ( Exception ex ) {
        // Windows7的对话窗体失败，使用一般的
        UnRegistorPSPFiles_OtherSystems();
      }

    }

    void btnUnRegisterPSPFiles_Click( object sender, EventArgs e ) {
      // unregister
      UnRegisterPSPFiles();
      // close the dialog
      tdRegisterPSPFilesDialog.Close();
    }

    /// <summary>
    /// 使用MessageBox提示用户是否注册*.psp类型的文件
    /// </summary>
    private void RegistorPSPFiles_OtherSystems() {
      MessageBoxResult mbr = MessageBox.Show( "为该程序（PSP.Run）注册*.psp类型的文件，进行更好的体验。", "文件类型（*.psp）尚未注册", MessageBoxButton.YesNo );
      if ( mbr == MessageBoxResult.Yes ) {
        // register
        RegisterPSPFiles();
      }
    }
    private void UnRegistorPSPFiles_OtherSystems() {
      MessageBoxResult mbr = MessageBox.Show( "您确认要 取消 为该程序（PSP.Run）注册*.psp类型的文件吗？", "文件类型（*.psp）已经未注册", MessageBoxButton.YesNo );
      if ( mbr == MessageBoxResult.Yes ) {
        // unregister
        UnRegisterPSPFiles();
      }
    }

    void btnRegisterPSPFiles_Click( object sender, EventArgs e ) {
      // register
      RegisterPSPFiles();
      // close the dialog
      tdRegisterPSPFilesDialog.Close();
    }

    /// <summary>
    /// 注册*.psp类型的文件
    /// </summary>
    void RegisterPSPFiles() {
      Tools.RegistrationHelper.RegisterFileAssociations(
                progId,
                false,
                appId,
                executablePath + " /weekfile %1",
                ".psp" );
      afterRegisterPSPFiles();
    }
    /// <summary>
    /// 取消注册*.psp类型的文件
    /// </summary>
    void UnRegisterPSPFiles() {
      Tools.RegistrationHelper.UnregisterFileAssociations(
                progId,
                false,
                appId,
                executablePath + " /weekfile %1",
                ".psp" );
      afterUnRegisterPSPFiles();
    }

    private void imageRegisterPSPFiles_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( imageRegisterPSPFiles.Tag == null )
        return;

      int t = (int)imageRegisterPSPFiles.Tag;
      // 取消注册*.psp类型的文件
      if ( t == 0 ) {
        if ( TaskbarManager.IsPlatformSupported )
          UnRegistorPSPFiles_Windows7();
        else
          UnRegistorPSPFiles_OtherSystems();

      }
      // 注册*.psp类型的文件
      else if ( t == 1 ) {
        if ( TaskbarManager.IsPlatformSupported )
          RegistorPSPFiles_Windows7();
        else
          RegistorPSPFiles_OtherSystems();

      }

    }

    // 注册之后
    void afterRegisterPSPFiles() {
      imageRegisterPSPFiles.Tag = 0;
      imageRegisterPSPFiles.ToolTip = "取消为PSP.Run注册*.psp类型的文件";
      imageRegisterPSPFiles.Source = new System.Windows.Media.Imaging.BitmapImage( new Uri( "/Resources/Images/256x256/UnRegisterPSPFiles.png", UriKind.Relative ) );
    }
    // 取消注册之后
    void afterUnRegisterPSPFiles() {
      imageRegisterPSPFiles.Tag = 1;
      imageRegisterPSPFiles.ToolTip = "为PSP.Run注册*.psp类型的文件";
      imageRegisterPSPFiles.Source = new System.Windows.Media.Imaging.BitmapImage( new Uri( "/Resources/Images/256x256/RegisterPSPFiles.png", UriKind.Relative ) );
    }

    #endregion System

  }
}
