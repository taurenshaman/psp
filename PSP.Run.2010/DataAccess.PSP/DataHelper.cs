using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml.Linq;

namespace DataAccess.PSP {
  public class DataHelper {
    const string xml_header = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";

    static string current_WeekDataFile = string.Empty;
    /// <summary>
    /// 当前打开的周数据文件名. Readonly.
    /// </summary>
    public static string CurrentWeekDataFile {
      get {
        return current_WeekDataFile;
      }
    }

    /// <summary>
    /// 第几周
    /// </summary>
    public static int WeekNumber { get; set; }

    static List<PSPModule> psp_modules = null;
    public static List<PSPModule> PSP_Modules {
      get {
        if ( psp_modules == null )
          psp_modules = LoadModules( Configuration.PSP_Data_Path );
        return psp_modules;
      }
      set {
        psp_modules = value;
      }
    }

    static List<TagReference> psp_tags = null;
    public static List<TagReference> PSP_Tags {
      get {
        if ( psp_tags == null )
          psp_tags = LoadTags( Configuration.PSP_Data_Path );
        return psp_tags;
      }
      set {
        psp_tags = value;
      }
    }

    static PSPWeek psp_week = null;
    public static PSPWeek PSP_Week {
      get {
        if ( psp_week == null )
          psp_week = LoadThisWeekData( Configuration.PSP_Data_Path );
        return psp_week;
      }
      set {
        psp_week = value;
      }
    }

    #region Module

    public static List<PSPModule> LoadModules( string pspDataPath ) {
      if ( !File.Exists( pspDataPath + "\\" + Configuration.PSP_Modules_File ) ) {
        return new List<PSPModule>();
      }

      List<PSPModule> modules = new List<PSPModule>();
      string xml = File.ReadAllText( pspDataPath + "\\" + Configuration.PSP_Modules_File, Encoding.UTF8 );
      try {
        XElement xroot = XElement.Parse( xml );
        if ( xroot == null )
          return null;
        foreach ( XElement xe in xroot.Elements( "module" ) ) {
          PSPModule pm = PSPModule.Parse( xe );
          if ( pm != null )
            modules.Add( pm );
        }

      }
      catch ( Exception ex ) {
        return null;
      }

      return modules;
    }

    public static List<PSPModule> LoadModules2( string fileName ) {
      if ( !File.Exists( fileName ) ) {
        return new List<PSPModule>();
      }

      List<PSPModule> modules = new List<PSPModule>();
      string xml = File.ReadAllText( fileName, Encoding.UTF8 );
      try {
        XElement xroot = XElement.Parse( xml );
        if ( xroot == null )
          return null;
        foreach ( XElement xe in xroot.Elements( "module" ) ) {
          PSPModule pm = PSPModule.Parse( xe );
          if ( pm != null )
            modules.Add( pm );
        }

      } catch ( Exception ex ) {
        return null;
      }

      return modules;
    }

    public static PSPModule GetModule( Guid guid ) {
      if ( Guid.Equals( guid, Guid.Empty ) )
        return null;
      foreach ( PSPModule m in PSP_Modules )
        if ( Guid.Equals( m.guid, guid ) )
          return m;
      return null;
    }

    public static int GetModuleIndex( Guid guid ) {
      int index = -1;
      if ( Guid.Equals( guid, Guid.Empty ) )
        return index;
      int count = PSP_Modules.Count;
      for ( int i = 0; i < count; i++ ) {
        if ( Guid.Equals( psp_modules[i].guid, guid ) ) {
          index = i;
          break;
        }
      }
      return index;
    }

    /// <summary>
    /// 可以是新创建，也可以使更新
    /// </summary>
    /// <param name="pm"></param>
    public static void SaveModule( PSPModule pm ) {
      if ( pm == null )
        return;
      int index = -1;
      for ( int i = 0; i < PSP_Modules.Count; i++ ) {
        if ( Guid.Equals( pm.guid, PSP_Modules[i].guid ) ) {
          index = i;
          break;
        }
      }
      if ( index >= 0 )
        PSP_Modules[index] = pm;
      else
        PSP_Modules.Add( pm );
      // save
      SaveModules( Configuration.PSP_Data_Path, PSP_Modules );
    }

    public static void DeleteModule( PSPModule pm ) {
      if ( pm == null )
        return;
      int index = -1;
      int count = PSP_Modules.Count;
      for ( int i = 0; i < count; i++ ) {
        if ( index < 0 && Guid.Equals( pm.guid, psp_modules[i].guid ) ) {
          index = i;
          break;
        }
      }
      if ( index < 0 )
        return;

      for ( int i = 0; i < count; i++ ) {
        // 删除将pm作为parent module的链接
        if ( Guid.Equals( pm.guid, psp_modules[i].parent_module ) ) {
          psp_modules[i].parent_module = Guid.Empty;
        }
        // 删除将pm作为child module的链接
        psp_modules[i].RemoveChild( pm.guid );

      }

      // remove
      psp_modules.RemoveAt( index );
      // save
      SaveModules( Configuration.PSP_Data_Path, psp_modules );
    }

    public static bool SaveModules( string pspDataPath, List<PSPModule> modules ) {
      if ( pspDataPath == null || pspDataPath.Length < 0 )
        return false;
      XElement xroot = new XElement( "psp_modules" );
      foreach ( PSPModule pm in modules ) {
        XElement xe = pm.ToXElement();
        if ( xe != null )
          xroot.Add( xe );
      }
      try {
        File.WriteAllText( pspDataPath + "\\" + Configuration.PSP_Modules_File, xml_header + xroot.ToString(), System.Text.Encoding.UTF8 );
      }
      catch ( Exception ex ) {
        return false;
      }

      // reload
      psp_modules = LoadModules( pspDataPath );

      return true;
    }

    /// <summary>
    /// 将从外界导入的模块数据，追加到当前的数据中
    /// </summary>
    /// <param name="importedModules"></param>
    /// <returns></returns>
    public static bool AppendModules( List<PSPModule> importedModules ) {
      if ( importedModules == null ) return true;
      foreach ( PSPModule pm in importedModules ) {
        int index = GetModuleIndex( pm.guid );
        if ( index < 0 )
          psp_modules.Add( pm );
      }
      // save
      return SaveModules( Configuration.PSP_Data_Path, PSP_Modules );
    }

    ///// <summary>
    ///// 2009.9.21
    ///// </summary>
    //public static void Update() {
    //  var vm = PSP_Modules;
    //  var vt = PSP_Tags;
    //  var vw = PSP_Week;
    //}

    #endregion Module

    #region Tag

    public static List<TagReference> LoadTags( string pspDataPath ) {
      if ( !File.Exists( pspDataPath + "\\" + Configuration.PSP_Tags_File ) ) {
        return new List<TagReference>();
      }

      List<TagReference> tags = new List<TagReference>();
      string xml = File.ReadAllText( pspDataPath + "\\" + Configuration.PSP_Tags_File, Encoding.UTF8 );
      try {
        XElement xroot = XElement.Parse( xml );
        if ( xroot == null )
          return null;
        foreach ( XElement xe in xroot.Elements( "tag" ) ) {
          TagReference tr = TagReference.ParseSingle( xe );
          if ( tr != null )
            tags.Add( tr );
        }

      }
      catch ( Exception ex ) {
        return null;
      }

      return tags;
    }

    public static int GetTagIndex( string name, Guid guid ) {
      int count = PSP_Tags.Count;
      for ( int i = 0; i < count; i++ ) {
        TagReference tr = psp_tags[i];
        if ( Guid.Equals( tr.guid, guid ) && string.Equals( tr.name, name ) )
          return i;
      }
      return -1;
    }

    /// <summary>
    /// 可以是新创建，也可以使更新
    /// </summary>
    /// <param name="pm"></param>
    public static void SaveTag( TagReference pm ) {
      if ( pm == null )
        return;
      int index = GetTagIndex( pm.name, pm.guid );
      if ( index >= 0 )
        PSP_Tags[index] = pm;
      else
        PSP_Tags.Add( pm );
      // save
      SaveTags( Configuration.PSP_Data_Path, PSP_Tags );
    }

    public static void DeleteTag( TagReference tag ) {
      if ( tag == null )
        return;
      int index = GetTagIndex( tag.name, tag.guid );
      if ( index >= 0 ) {
        // remove
        PSP_Tags.RemoveAt( index );
        // save
        SaveTags( Configuration.PSP_Data_Path, PSP_Tags );
      }
    }

    public static bool SaveTags( string pspDataPath, List<TagReference> tags ) {
      if ( pspDataPath == null || pspDataPath.Length < 0 )
        return false;
      XElement xroot = new XElement( "psp_tags" );
      foreach ( TagReference pm in tags ) {
        XElement xe = pm.ToXElement( "tag" );
        if ( xe != null )
          xroot.Add( xe );
      }
      try {
        File.WriteAllText( pspDataPath + "\\" + Configuration.PSP_Tags_File, xml_header + xroot.ToString(), System.Text.Encoding.UTF8 );
      }
      catch ( Exception ex ) {
        return false;
      }

      // reload
      psp_tags = LoadTags( pspDataPath );

      return true;
    }

    #endregion Tag

    #region Week

    /// <summary>
    /// 取某个日期所在的指定周的PSP数据，若不存在，创建
    /// </summary>
    /// <param name="pspDataPath"></param>
    /// <param name="dtTheDay"></param>
    /// <returns></returns>
    public static PSPWeek LoadWeekData( string pspDataPath, DateTime dtTheDay ) {
      DateTime monday = GetMondayDate( dtTheDay );
      current_WeekDataFile = Configuration.PSP_WeekFile + monday.ToString( "yyyy-MM-dd" ) + ".psp";
      string fileName = pspDataPath + "\\" + current_WeekDataFile;

      // 不存在 本周 的PSP数据，新建
      if ( !File.Exists( fileName ) ) {
        // 计算已经存在了多少个PSP周数据文件
        string[] weekFiles = Directory.GetFiles( pspDataPath, Configuration.PSP_WeekFile + "*.psp" );
        if ( weekFiles == null )
          WeekNumber = 0;
        else
          WeekNumber = weekFiles.Length;

        WeekNumber++;
        psp_week = new PSPWeek( WeekNumber, monday );
        return psp_week;
      }
      // 存在 本周 的PSP数据，导入
      string xml = File.ReadAllText( fileName, Encoding.UTF8 );
      try {
        XElement xroot = XElement.Parse( xml );
        if ( xroot == null )
          return null;
        psp_week = PSPWeek.Parse( xroot );
      }
      catch ( Exception ex ) {
        return null;
      }

      return psp_week;
    }

    /// <summary>
    /// 取某个日期所在的指定周的PSP数据，若不存在，不创建，返回null
    /// </summary>
    /// <param name="pspDataPath"></param>
    /// <param name="dtTheDay"></param>
    /// <returns></returns>
    public static PSPWeek LoadWeekData2( string pspDataPath, DateTime dtTheDay ) {
      DateTime monday = GetMondayDate( dtTheDay );
      current_WeekDataFile = Configuration.PSP_WeekFile + monday.ToString( "yyyy-MM-dd" ) + ".psp";
      string fileName = pspDataPath + "\\" + current_WeekDataFile;

      // 不存在 本周 的PSP数据，新建
      if ( !File.Exists( fileName ) ) {
        return null;
      }
      // 存在 本周 的PSP数据，导入
      string xml = File.ReadAllText( fileName, Encoding.UTF8 );
      try {
        XElement xroot = XElement.Parse( xml );
        if ( xroot == null )
          return null;
        psp_week = PSPWeek.Parse( xroot );
      } catch ( Exception ex ) {
        return null;
      }

      return psp_week;
    }

    /// <summary>
    /// 获取 本周 的PSP数据
    /// </summary>
    /// <returns></returns>
    public static PSPWeek LoadThisWeekData( string pspDataPath ) {
      return LoadWeekData( pspDataPath, DateTime.Today );
    }

    /// <summary>
    /// 保存当前打开的 周数据 到文件系统
    /// </summary>
    /// <param name="pspDataPath"></param>
    /// <returns></returns>
    public static bool SaveWeekData( string pspDataPath ) {
      if ( pspDataPath == null || pspDataPath.Length < 0 || psp_week == null )
        return false;
      XElement xroot = psp_week.ToXElement();
      try {
        File.WriteAllText( pspDataPath + "\\" + CurrentWeekDataFile, xml_header + xroot.ToString(), System.Text.Encoding.UTF8 );
      }
      catch ( Exception ex ) {
        return false;
      }

      // reload
      DateTime dt = psp_week.DateOfMonday;
      psp_week = LoadWeekData( pspDataPath, dt.Date );

      return true;
    }

    /// <summary>
    /// 从周数据中，获取某天的PSP数据
    /// --如果查询日期不属于本周数据，会重新载入符合查询日期的周数据
    /// </summary>
    /// <param name="dt"></param>
    public static PSPDay GetDayData( DateTime dt ) {
      if( psp_week == null)
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );

      PSPDay pday = psp_week.Get_PSPDay( dt );
      // 如果所查询的日期不在 本周的PSP数据中
      if ( pday == null && !psp_week.DateOfMonday.Date.Equals( GetMondayDate( dt.Date ).Date ) ) {
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );
        pday = psp_week.Get_PSPDay( dt );
      }

      return pday;
    }
    /// <summary>
    /// 从周数据中，获取某天的PSP数据的索引
    /// --如果查询日期不属于本周数据，会重新载入符合查询日期的周数据
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static int GetDayData_Index( DateTime dt ) {
      if ( psp_week == null )
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );

      int index = psp_week.Get_PSPDay_Index( dt );
      // 如果所查询的日期不在 本周的PSP数据中
      if ( index == 0 && !psp_week.DateOfMonday.Date.Equals( GetMondayDate( dt.Date ).Date ) ) {
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );
        index = GetDayData_Index2( dt.Date );
      }

      return index;
    }
    /// <summary>
    /// 从周数据中，获取某天的PSP数据
    /// --如果查询日期不属于本周数据，不会 重新载入符合查询日期的周数据
    /// </summary>
    /// <param name="dt"></param>
    public static PSPDay GetDayData2( DateTime dt ) {
      if ( psp_week == null )
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );

      return psp_week.Get_PSPDay( dt );
    }
    /// <summary>
    /// 从周数据中，获取某天的PSP数据的索引
    /// --如果查询日期不属于本周数据，不会 重新载入符合查询日期的周数据
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static int GetDayData_Index2( DateTime dt ) {
      if ( psp_week == null )
        psp_week = LoadWeekData( Configuration.PSP_Data_Path, dt.Date );

      return psp_week.Get_PSPDay_Index( dt );
    }

    public static bool AddPSPEvent( PSPEvent pevent, DateTime dt ) {
      if ( pevent == null )
        return false;

      int index = GetDayData_Index( dt.Date );
      PSPDay pday = null;
      if ( index < 0 ) {
        pday = new PSPDay();
        pday.Date = dt;
      }
      else
        pday = psp_week.PSP_Week_Days[index];

      pday.AddEvent( pevent );
      // 模块
      if ( pevent.module != null ) {
        PSPModule pm = GetModule( pevent.module.guid );
        // 统计+1
        pm.referenced_times++;
        SaveModule( pm );
      }
      // 标签
      if ( pevent.tags != null && pevent.tags.Count > 0 ) {
        foreach ( TagReference tr in pevent.tags ) {
          int tagIndex = GetTagIndex( tr.name, tr.guid );
          if ( tagIndex < 0 ) {
            tr.referenced_times = 1;
            psp_tags.Add( tr );
          }
          else {
            // 统计+1
            tr.referenced_times = psp_tags[tagIndex].referenced_times + 1;
            psp_tags[tagIndex] = tr;
          }
        }
        // 保存标签
        SaveTags( Configuration.PSP_Data_Path, psp_tags );
      }

      //// 保存标签
      //if ( pevent.tags != null ) {
      //  bool toSave = false;
      //  foreach ( TagReference tr in pevent.tags ) {
      //    int tagIndex = GetTagIndex( tr.name, tr.guid );
      //    if ( tagIndex < 0 ) {
      //      PSP_Tags.Add( tr );
      //      toSave = true;
      //    }
      //  }
      //  // save
      //  if ( toSave )
      //    SaveTags( Configuration.PSP_Data_Path, psp_tags );
      //}

      // save
      return UpdateDayData( pday, index );
    }

    public static bool RemovePSPEvent( PSPEvent pevent ) {
      if ( pevent == null )
        return false;

      int index = GetDayData_Index( pevent.date.Date );
      if ( index < 0 ) {
        return false;
      }
      PSPDay pday = psp_week.PSP_Week_Days[index];
      pday.RemoveEvent( pevent );
      psp_week.PSP_Week_Days[index] = pday;
      // save to datastore
      return SaveWeekData( Configuration.PSP_Data_Path );
    }

    /// <summary>
    /// 更新某一天的PSP数据
    /// </summary>
    /// <param name="pday"></param>
    /// <returns></returns>
    public static bool UpdateDayData( PSPDay pday ) {
      if ( pday == null )
        return false;

      int index = GetDayData_Index( pday.Date.Date );
      return UpdateDayData( pday, index );
    }
    public static bool UpdateDayData( PSPDay pday, int index ) {
      if ( pday == null )
        return false;

      if ( index < 0 )
        psp_week.PSP_Week_Days.Add( pday );
      else
        psp_week.PSP_Week_Days[index] = pday;

      // save to datastore
      return SaveWeekData( Configuration.PSP_Data_Path );
    }

    /// <summary>
    /// 获取某周的PSP数据备份的数目
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static int BackupFilesCount( DateTime dt ) {
      int count = 0;
      DateTime monday = GetMondayDate( dt );
      string fileNamePrefix = Configuration.PSP_WeekFile + monday.ToString( "yyyy-MM-dd" ) + "_at";
      string bakPath = Configuration.PSP_Data_Path + "\\" + Configuration.PSP_Data_BackupFolder;
      if ( System.IO.Directory.Exists( bakPath ) ) {
        string[] files = Directory.GetFiles( bakPath, fileNamePrefix + "*.psp" );
        if ( files == null )
          count = 0;
        else
          count = files.Length;
      }
      else {
        System.IO.Directory.CreateDirectory( bakPath );
      }

      return count;
    }

    /// <summary>
    /// 备份某周的PSP数据至[PSP_Data]/bak
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static bool BackupWeekData( DateTime dt ) {
      bool result = true;
      DateTime monday = GetMondayDate( dt );
      string bakPath = Configuration.PSP_Data_Path + "\\" + Configuration.PSP_Data_BackupFolder;
      string fileNamePrefix = Configuration.PSP_WeekFile + monday.ToString( "yyyy-MM-dd" );
      string bakFilePath = bakPath + "\\" + fileNamePrefix + "_at" + DateTime.Now.ToString( "HHmmss" ) + ".psp",
        srcFilePath = Configuration.PSP_Data_Path + "\\" + fileNamePrefix + ".psp";

      try {
        System.IO.File.Copy( srcFilePath, bakFilePath );
      }
      catch ( Exception ex ) {
        result = false;
      }

      return result;
    }

    #endregion Week

    #region DateTime

    /// <summary>
    /// 根据指定日期，返回该周的周一所在日期
    /// </summary>
    /// <param name="dtTheDay"></param>
    /// <returns></returns>
    public static DateTime GetMondayDate( DateTime dtTheDay ) {
      DateTime dateOfMonday = dtTheDay;
      DayOfWeek dow = dtTheDay.DayOfWeek;
      switch ( dow ) {
        case DayOfWeek.Monday:
          break;
        case DayOfWeek.Tuesday:
          dateOfMonday = dateOfMonday.AddDays( -1 );
          break;
        case DayOfWeek.Wednesday:
          dateOfMonday = dateOfMonday.AddDays( -2 );
          break;
        case DayOfWeek.Thursday:
          dateOfMonday = dateOfMonday.AddDays( -3 );
          break;
        case DayOfWeek.Friday:
          dateOfMonday = dateOfMonday.AddDays( -4 );
          break;
        case DayOfWeek.Saturday:
          dateOfMonday = dateOfMonday.AddDays( -5 );
          break;
        case DayOfWeek.Sunday:
          dateOfMonday = dateOfMonday.AddDays( -6 );
          break;
      }
      return dateOfMonday.Date;
    }

    #endregion DateTime

    #region Statistics

    /// <summary>
    /// 获取指定日期的某模块的最近四周（不包括本周）的统计数据
    /// </summary>
    /// <returns></returns>
    public static List<StatisticsItem> Statistics_RecentFourWeeks( DateTime dt, string moduleName, Guid gModule ) {
      // Monday
      DateTime dtThisWeek = GetMondayDate( dt );
      DateTime dtWeekAgo1 = dtThisWeek.AddDays( -7 ),
        dtWeekAgo2 = dtThisWeek.AddDays( -14 ),
        dtWeekAgo3 = dtThisWeek.AddDays( -21 ),
        dtWeekAgo4 = dtThisWeek.AddDays( -28 );
      // Get week data
      PSPWeek weekData0 = LoadWeekData2( Configuration.PSP_Data_Path, dtThisWeek ),
        weekData1 = LoadWeekData2( Configuration.PSP_Data_Path, dtWeekAgo1 ),
        weekData2 = LoadWeekData2( Configuration.PSP_Data_Path, dtWeekAgo2 ),
        weekData3 = LoadWeekData2( Configuration.PSP_Data_Path, dtWeekAgo3 ),
        weekData4 = LoadWeekData2( Configuration.PSP_Data_Path, dtWeekAgo4 );

      List<StatisticsItem> results = new List<StatisticsItem>();
      if ( weekData0 != null ) {
        StatisticsItem si = new StatisticsItem();
        si.name = weekData0.DateOfMonday.ToString( "yy-MM-dd" );
        si.date = weekData0.DateOfMonday;
        si.guid = gModule;
        si.total_time = weekData0.GetWeekStatisticsOfModule2( moduleName, gModule );
        results.Add( si );
      }
      if ( weekData1 != null ) {
        StatisticsItem si = new StatisticsItem();
        si.name = weekData1.DateOfMonday.ToString( "yy-MM-dd" );
        si.date = weekData1.DateOfMonday;
        si.guid = gModule;
        si.total_time = weekData1.GetWeekStatisticsOfModule2( moduleName, gModule );
        results.Add( si );
      }
      if ( weekData2 != null ) {
        StatisticsItem si = new StatisticsItem();
        si.name = weekData2.DateOfMonday.ToString( "yy-MM-dd" );
        si.date = weekData2.DateOfMonday;
        si.guid = gModule;
        si.total_time = weekData2.GetWeekStatisticsOfModule2( moduleName, gModule );
        results.Add( si );
      }
      if ( weekData3 != null ) {
        StatisticsItem si = new StatisticsItem();
        si.name = weekData3.DateOfMonday.ToString( "yy-MM-dd" );
        si.date = weekData3.DateOfMonday;
        si.guid = gModule;
        si.total_time = weekData3.GetWeekStatisticsOfModule2( moduleName, gModule );
        results.Add( si );
      }
      if ( weekData4 != null ) {
        StatisticsItem si = new StatisticsItem();
        si.name = weekData4.DateOfMonday.ToString( "yy-MM-dd" );
        si.date = weekData4.DateOfMonday;
        si.guid = gModule;
        si.total_time = weekData4.GetWeekStatisticsOfModule2( moduleName, gModule );
        results.Add( si );
      }
      return results;
    }

    /// <summary>
    /// 获取指定日期的某模块的所在一周的统计数据
    /// </summary>
    /// <returns></returns>
    public static List<StatisticsItem> ModuleStatistics_SomeWeek( DateTime dt, string moduleName, Guid gModule ) {
      DateTime monday = GetMondayDate( dt );
      PSPWeek weekData = LoadWeekData2( Configuration.PSP_Data_Path, monday );
      if ( weekData == null ) return null;
      return weekData.GetWeekStatisticsOfModule( moduleName, gModule );
    }

    /// <summary>
    /// 获取某天的模块统计信息
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public static List<StatisticsItem> GetModulesStatistics( DateTime dt ) {
      PSPDay pday = GetDayData( dt );
      if ( pday == null || pday.Statistics == null || pday.Statistics.ModulesStatistics == null )
        return null;
      return pday.Statistics.ModulesStatistics;
    }

    #endregion Statistics

    #region Update

    /// <summary>
    /// 2009.9.30升级：将PSP数据文件（指psp_week_[date_of_monday].psp）的后缀由.xml更改为.psp
    /// </summary>
    /// <returns></returns>
    public static bool Update_30Sept2009() {
      bool state = true;
      // 查找旧的以.xml为后缀的文件，若不存在，则不需要升级
      string[] weekFiles = Directory.GetFiles( Configuration.PSP_Data_Path, Configuration.PSP_WeekFile + "*.xml" );
      if ( weekFiles == null || weekFiles.Length == 0 )
        return state;

      try {
        foreach ( string fn in weekFiles ) {
          // copy
          File.Copy( fn, fn.Substring( 0, fn.Length - 4 ) + ".psp" );
          // delete
          File.Delete( fn );
        }

      }
      catch ( Exception ex ) {
        state = false;
      }

      return state;
    }

    #endregion Update

  }
}
