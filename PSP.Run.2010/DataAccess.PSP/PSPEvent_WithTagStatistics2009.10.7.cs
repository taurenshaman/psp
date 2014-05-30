using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml.Linq;

namespace DataAccess.PSP {

  /// <summary>
  /// 每一条单独的事情记录
  /// </summary>
  public class PSPEvent {
    public Guid guid  { get; set; }
    /// <summary>
    /// 当日日期
    /// </summary>
    public DateTime date { get; set; }
    /// <summary>
    /// 从PSP开始的第几周
    /// </summary>
    public int week_number { get; set; }
    /// <summary>
    /// Time：开始时间
    /// </summary>
    public DateTime time_start { get; set; }
    /// <summary>
    /// Time：结束时间
    /// </summary>
    public DateTime time_end { get; set; }
    /// <summary>
    /// 持续时间，单位：分钟
    /// </summary>
    public int time_duration { get; set; }
    /// <summary>
    /// 做的事情；记录
    /// </summary>
    public string record { get; set; }
    /// <summary>
    /// 所属模块
    /// </summary>
    public PSPModule module { get; set; }
    public List<TagReference> tags { get; set; }

    public PSPEvent() {
      guid = Guid.NewGuid();
      ResetWeekNumber();
    }

    /// <summary>
    /// 重置week_number，将其设为从PSP开始记录后，本周所属的第几周 -- 未完成
    /// </summary>
    public void ResetWeekNumber() {
      
    }

    /// <summary>
    /// 在标签列表中查找，并返回索引
    /// </summary>
    /// <param name="tr"></param>
    /// <returns></returns>
    public int GetTagIndex( TagReference tr ) {
      int index = -1;
      if ( tr != null && tags != null && tags.Count > 0 ) {
        for ( int i = 0; i < tags.Count; i++ ) {
          TagReference trTmp = tags[i];
          if ( tr.guid.Equals( trTmp.guid ) && tr.name.Equals( trTmp.name ) ) {
            index = i;
            break;
          }

        }

      }

      return index;
    }

    public XElement ToXElement() {
      XElement xroot = new XElement( "event",
        new XAttribute( "date", date.ToLongDateString() ),
        new XAttribute( "guid", guid.ToString() ),
        new XAttribute( "week_number", week_number ),
        new XAttribute( "time_start", time_start.ToShortTimeString() ),
        new XAttribute( "time_end", time_end.ToShortTimeString() ),
        new XAttribute( "time_duration", time_duration ) );
      xroot.Add( new XElement( "record", new XCData( record ) ) );
      if ( module != null )
        xroot.Add( module.ToSimpleXElement() );
      xroot.Add( TagReference.GetXElementOfTags( tags, "tags" ) );

      return xroot;
    }

    public string ToXMLString( bool withXMLHeader ) {
      XElement xroot = ToXElement();
      return withXMLHeader ? "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + xroot.ToString() : xroot.ToString();
    }

    public static PSPEvent ParseOne( XElement xroot ) {
      if ( xroot == null )
        return null;

      PSPEvent psp_event = new PSPEvent();

      XAttribute xaTmp = xroot.Attribute( "date" );
      psp_event.date = xaTmp == null ? DateTime.Today : Convert.ToDateTime( xaTmp.Value );

      xaTmp = xroot.Attribute( "guid" );
      psp_event.guid = xaTmp == null ? Guid.NewGuid() : new Guid( xaTmp.Value );

      xaTmp = xroot.Attribute( "week_number" );
      psp_event.week_number = xaTmp == null ? 0 : Convert.ToInt32( xaTmp.Value );

      xaTmp = xroot.Attribute( "time_start" );
      psp_event.time_start = xaTmp == null ? DateTime.Today : Convert.ToDateTime( xaTmp.Value );

      xaTmp = xroot.Attribute( "time_end" );
      psp_event.time_end = xaTmp == null ? DateTime.Today : Convert.ToDateTime( xaTmp.Value );

      xaTmp = xroot.Attribute( "time_duration" );
      psp_event.time_duration = xaTmp == null ? 0 : Convert.ToInt32( xaTmp.Value );

      XElement xeTmp = xroot.Element( "record" );
      psp_event.record = xeTmp == null ? string.Empty : xeTmp.Value;

      xeTmp = xroot.Element( "module" );
      if ( xeTmp != null ) {
        xaTmp = xeTmp.Attribute( "guid" );
        try {
          Guid gModule = new Guid( xaTmp.Value );
          // Get PSPModule instance...
          psp_event.module = DataHelper.GetModule( gModule );
        }
        catch ( Exception ex ) {
        }
      }

      xeTmp = xroot.Element( "tags" );
      if ( xeTmp != null ) {
        psp_event.tags = TagReference.Parse( xeTmp );
      }

      return psp_event;
    }
    public static List<PSPEvent> ParseMany( XElement xroot ) {
      if ( xroot == null )
        return null;

      List<PSPEvent> psp_events = new List<PSPEvent>();
      var eventsElement = xroot.Elements( "event" );
      if ( eventsElement == null )
        return null;
      foreach ( XElement ee in eventsElement ) {
        PSPEvent pe = PSPEvent.ParseOne( ee );
        if ( pe != null )
          psp_events.Add( pe );
      }

      return psp_events;
    }

    public static XElement ToXElement( List<PSPEvent> psp_events, string elementName ) {
      if ( psp_events == null || elementName == null || elementName.Length == 0 )
        return null;

      XElement xroot = new XElement( elementName );
      if ( psp_events != null && psp_events.Count > 0 ) {
        foreach ( PSPEvent pe in psp_events )
          xroot.Add( pe.ToXElement() );
      }

      return xroot;
    }

  }

  public class PSPModule {
    public Guid guid { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public Guid parent_module { get; set; }
    public List<TagReference> tags { get; set; }
    public List<ModuleReference> children_modules { get; set; }
    /// <summary>
    /// 模块的使用/引用次数
    /// </summary>
    public long referenced_times { get; set; }

    public PSPModule() {
      guid = Guid.NewGuid();
    }
    public PSPModule( string name ) {
      this.name = name;
      guid = Guid.NewGuid();
    }

    /// <summary>
    ///  只获取name和guid
    /// </summary>
    /// <returns></returns>
    public XElement ToSimpleXElement() {
      XElement xroot = new XElement( "module", new XAttribute( "name", name ), new XAttribute( "guid", guid.ToString() ), new XAttribute( "parent_module", parent_module.ToString() ) );
      return xroot;
    }
    public XElement ToXElement() {
      XElement xroot = new XElement( "module",
        new XAttribute( "name", name ),
        new XAttribute( "guid", guid.ToString() ),
        new XAttribute( "parent_module", parent_module.ToString() ),
        new XAttribute( "referenced_times", referenced_times.ToString() )
      );
      if ( description != null && description.Trim().Length > 0 )
        xroot.Add( new XElement( "description", new XCData( description.Trim() ) ) );
      if ( tags != null && tags.Count > 0 )
        xroot.Add( TagReference.GetXElementOfTags( tags, "tags" ) );
      if ( children_modules != null && children_modules.Count > 0 )
        xroot.Add( ModuleReference.GetXElementOfModules( children_modules, "children_modules" ) );

      return xroot;
    }

    /// <summary>
    /// 检查是否存在某个子模块。存在，返回索引；否则返回-1
    /// </summary>
    /// <param name="gChild"></param>
    /// <returns></returns>
    public int ChildExists( Guid gChild ) {
      int index = -1;
      if ( children_modules != null && children_modules.Count > 0 ) {
        for ( int i = 0; i < children_modules.Count; i++ )
          if ( Guid.Equals( gChild, children_modules[i].guid ) ) {
            index = i;
            break;
          }
      }
      return index;
    }
    public void RemoveChild( Guid gChild ) {
      int index = ChildExists( gChild );
      if ( index >= 0 )
        children_modules.RemoveAt( index );
    }

    /// <summary>
    /// 只获取name和guid
    /// </summary>
    /// <param name="modules"></param>
    /// <returns></returns>
    public static XElement GetSimpleXElementOfModules( List<PSPModule> modules ) {
      XElement xroot = null;
      if ( modules != null && modules.Count > 0 )
        xroot = new XElement( "modules", from m in modules
                                      select new XElement( "item", new XAttribute( "name", m.name ), new XAttribute( "guid", m.guid.ToString() ) ) );
      return xroot;
    }

    public static PSPModule Parse( XElement xroot ) {
      if ( xroot == null )
        return null;

      PSPModule psp_module = new PSPModule();
      XAttribute xaTmp = xroot.Attribute( "name" );
      psp_module.name = xaTmp == null ? string.Empty : xaTmp.Value;

      xaTmp = xroot.Attribute( "guid" );
      psp_module.guid = xaTmp == null ? Guid.Empty : new Guid( xaTmp.Value.Trim() );

      xaTmp = xroot.Attribute( "parent_module" );
      psp_module.parent_module = xaTmp == null ? Guid.Empty : new Guid( xaTmp.Value.Trim() );

      xaTmp = xroot.Attribute( "referenced_times" );
      psp_module.referenced_times = xaTmp == null ? 0 : Convert.ToInt64( xaTmp.Value.Trim() );

      XElement xeTmp = xroot.Element( "description" );
      psp_module.description = xeTmp == null ? string.Empty : xeTmp.Value;

      xeTmp = xroot.Element( "tags" );
      psp_module.tags = xeTmp == null ? null : TagReference.Parse( xeTmp );

      xeTmp = xroot.Element( "children_modules" );
      psp_module.children_modules = xeTmp == null ? null : ModuleReference.Parse( xeTmp );

      return psp_module;
    }

  }

  public class PSPProject {
    public Guid guid { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public List<PSPModule> modules { get; set; }
    public List<TagReference> tags { get; set; }

    public XElement ToXElement() {
      XElement xroot = new XElement( "project", new XAttribute( "name", name ), new XAttribute( "guid", guid.ToString() ) );
      if ( description != null && description.Trim().Length > 0 )
        xroot.Add( new XElement( "description", new XCData( description.Trim() ) ) );
      if ( tags != null && tags.Count > 0 )
        xroot.Add( TagReference.GetXElementOfTags( tags, "tags" ) );

      return xroot;
    }

  }

  public class TagReference {
    public string name { get; set; }
    public Guid guid { get; set; }
    /// <summary>
    /// 模块的使用/引用次数
    /// </summary>
    public long referenced_times { get; set; }

    public TagReference( string name ) {
      this.name = name;
      this.guid = Guid.Empty;
      referenced_times = 0;
    }
    public TagReference( string name, Guid guid ) {
      this.name = name;
      this.guid = guid;
      referenced_times = 0;
    }

    public XElement ToXElement( string elementName ) {
      return new XElement( elementName,
        new XAttribute( "name", name ),
        new XAttribute( "guid", guid.ToString() ),
        new XAttribute( "referenced_times", referenced_times.ToString() )
      );
    }

    public static XElement GetXElementOfTags( List<TagReference> tags, string elementName ) {
      XElement xroot = null;
      if ( tags != null && tags.Count > 0 )
        xroot = new XElement( elementName, from t in tags
                                         select new XElement( "item",
                                           new XAttribute( "name", t.name ),
                                           new XAttribute( "guid", t.guid.ToString() ),
                                           new XAttribute( "referenced_times", t.referenced_times.ToString() ) ) );
      return xroot;
    }

    public static List<TagReference> Parse( XElement xroot ) {
      if ( xroot == null )
        return null;
      var c = xroot.Elements( "item" );
      if ( c == null || c.Count() == 0 )
        return null;
      
      List<TagReference> tags = new List<TagReference>();
      foreach ( XElement i in c ) {
        TagReference tr = new TagReference( i.Attribute( "name" ).Value, new Guid( i.Attribute( "guid" ).Value ) );
        XAttribute xa = i.Attribute( "referenced_times" );
        tr.referenced_times = xa == null ? 0 : Convert.ToInt64( xa.Value );
        tags.Add( tr );
      }

      return tags;
    }

    public static TagReference ParseSingle( XElement xroot ) {
      if ( xroot == null )
        return null;
      XAttribute xaName = xroot.Attribute( "name" ),
        xaGuid = xroot.Attribute( "guid" );
      return new TagReference( xaName.Value, new Guid( xaGuid.Value ) );
    }

  }

  public class ModuleReference {
    public string name { get; set; }
    public Guid guid { get; set; }

    public ModuleReference( string name ) {
      this.name = name;
      this.guid = Guid.Empty;
    }
    public ModuleReference( string name, Guid guid ) {
      this.name = name;
      this.guid = guid;
    }

    public static XElement GetXElementOfModules( List<ModuleReference> modules, string elementName ) {
      XElement xroot = null;
      if ( modules != null && modules.Count > 0 )
        xroot = new XElement( elementName, from t in modules
                                      select new XElement( "item", new XAttribute( "name", t.name ), new XAttribute( "guid", t.guid.ToString() ) ) );
      return xroot;
    }

    public static List<ModuleReference> Parse( XElement xroot ) {
      if ( xroot == null )
        return null;
      var c = xroot.Elements( "item" );
      if ( c == null || c.Count() == 0 )
        return null;

      List<ModuleReference> modules = new List<ModuleReference>();
      foreach ( XElement i in c ) {
        ModuleReference tr = new ModuleReference( i.Attribute( "name" ).Value, new Guid( i.Attribute( "guid" ).Value ) );
        modules.Add( tr );
      }

      return modules;
    }

  }

  public class StatisticsItem {
    /// <summary>
    /// 时间统计
    /// </summary>
    public int total_time { get; set; }
    /// <summary>
    /// Guid
    /// </summary>
    public Guid guid = Guid.Empty;
    public string name { get; set; }
    /// <summary>
    /// 统计日期
    /// </summary>
    public DateTime date { get; set; }

    public StatisticsItem() {
      total_time = 0;
      guid = Guid.NewGuid();
      name = string.Empty;
      date = DateTime.Today;
    }

    /// <summary>
    /// XElement Name: item
    /// </summary>
    /// <returns></returns>
    public XElement ToItemXElement() {
      XElement xroot = new XElement( "item",
        new XAttribute( "name", name ),
        new XAttribute( "guid", guid.ToString() ),
        new XAttribute( "date", date.ToLongDateString() ),
        new XAttribute( "total_time", total_time ) );
      return xroot;
    }

    public static StatisticsItem Parse( XElement xroot ) {
      if ( xroot == null )
        return null;
      StatisticsItem si = new StatisticsItem();
      XAttribute xaTmp = xroot.Attribute( "name" );
      si.name = xaTmp == null ? string.Empty : xaTmp.Value;

      xaTmp = xroot.Attribute( "guid" );
      si.guid = xaTmp == null ? Guid.Empty : new Guid( xaTmp.Value );

      xaTmp = xroot.Attribute( "date" );
      si.date = xaTmp == null ? DateTime.Today : Convert.ToDateTime( xaTmp.Value );

      xaTmp = xroot.Attribute( "total_time" );
      si.total_time = xaTmp == null ? 0 : Convert.ToInt32( xaTmp.Value );

      return si;
    }

  }

  public class PSPDayStatistics {
    public List<StatisticsItem> TagsStatistics { get; set; }
    public List<StatisticsItem> ModulesStatistics { get; set; }
    //public List<StatisticsItem> ProjectsStatistics { get; set; }

    public PSPDayStatistics() {
      TagsStatistics = new List<StatisticsItem>();
      ModulesStatistics = new List<StatisticsItem>();
      //ProjectsStatistics = new List<StatisticsItem>();
    }

    /// <summary>
    /// +: 更新标签的时间统计数据 -- 即 在增加 某个PSPEvent或Tag后，更新统计数据
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="minutes"></param>
    /// <param name="date"></param>
    public void UpdateTagsStatistics( TagReference tag, int minutes, DateTime date ) {
      if ( tag == null || minutes == 0 )
        return;

      int index = -1;
      for ( int i = 0; i < TagsStatistics.Count; i++ ) {
        StatisticsItem si = TagsStatistics[i];
        if ( si.date.Date.Equals( date.Date ) && si.name.Equals( tag.name ) && si.guid.Equals( tag.guid ) ) {
          index = i;
          break;
        }
      }
      if ( index >= 0 ) {
        StatisticsItem si = TagsStatistics[index];
        si.total_time += minutes;
        TagsStatistics[index] = si;
      }
      else {
        StatisticsItem si = new StatisticsItem();
        si.name = tag.name;
        si.guid = tag.guid;
        si.total_time += minutes;
        si.date = date;
        TagsStatistics.Add( si );
      }

    }
    /// <summary>
    /// +: 更新Module的时间统计数据 -- 即 在增加 某个PSPEvent或Module后，更新统计数据 -- 不处理parent_module和children_modules
    /// </summary>
    /// <param name="module"></param>
    /// <param name="minutes"></param>
    /// <param name="date"></param>
    public void UpdateModulesStatistics( PSPModule module, int minutes, DateTime date ) {
      if ( module == null || minutes == 0 )
        return;

      int index = -1;
      for ( int i = 0; i < ModulesStatistics.Count; i++ ) {
        StatisticsItem si = ModulesStatistics[i];
        if ( si.date.Date.Equals( date.Date ) && si.name.Equals( module.name ) && si.guid.Equals( module.guid ) ) {
          index = i;
          break;
        }
      }
      if ( index >= 0 ) {
        StatisticsItem si = ModulesStatistics[index];
        si.total_time += minutes;
        ModulesStatistics[index] = si;
      }
      else {
        StatisticsItem si = new StatisticsItem();
        si.name = module.name;
        si.guid = module.guid;
        si.total_time += minutes;
        si.date = date;
        ModulesStatistics.Add( si );
      }

    }

    ///// <summary>
    ///// 根据PSPModule，更新包含此PSPModule的所有Project的统计信息 -- 未完成
    ///// </summary>
    ///// <param name="module"></param>
    //public void UpdateProjectsStatistics( PSPModule module, int minutes, DateTime date ) {
    //  if ( module == null || minutes == 0 )
    //    return;

    //}

    /// <summary>
    /// -: 将某次更新取消 -- 即 在删除 某个PSPEvent或Tag后，更新统计数据
    /// </summary>
    /// <param name="tag"></param>
    public void UpdateTagsStatistics2( TagReference tag, int minutes, DateTime date ) {
      if ( tag == null || minutes == 0 || TagsStatistics.Count == 0 )
        return;

      for ( int i = 0; i < TagsStatistics.Count; i++ ) {
        StatisticsItem si = TagsStatistics[i];
        if ( si.date.Date.Equals( date.Date ) && si.name.Equals( tag.name ) && si.guid.Equals( tag.guid ) ) {
          si.total_time -= minutes;
          TagsStatistics[i] = si;
          break;
        }
      }

    }

    /// <summary>
    /// -: 将某次更新取消 -- 即 在删除 某个PSPEvent或Module后，更新统计数据
    /// </summary>
    /// <param name="tag"></param>
    public void UpdateModulesStatistics2( PSPModule module, int minutes, DateTime date ) {
      if ( module == null || minutes == 0 || ModulesStatistics.Count == 0 )
        return;

      for ( int i = 0; i < ModulesStatistics.Count; i++ ) {
        StatisticsItem si = ModulesStatistics[i];
        if ( si.date.Date.Equals( date.Date ) && si.name.Equals( module.name ) && si.guid.Equals( module.guid ) ) {
          si.total_time -= minutes;
          ModulesStatistics[i] = si;
          break;
        }
      }

    }

    public static XElement ToXElement( List<StatisticsItem> siList, string elementName ) {
      if ( siList == null || elementName == null || elementName.Length == 0 )
        return null;

      XElement xroot = new XElement( elementName );
      foreach ( StatisticsItem si in siList )
        xroot.Add( si.ToItemXElement() );
      return xroot;
    }

    public static List<StatisticsItem> Parse( XElement xroot ) {
      if ( xroot == null )
        return null;
      List<StatisticsItem> results = new List<StatisticsItem>();
      var items = xroot.Elements( "item" );
      foreach ( XElement xe in items ) {
        StatisticsItem si = StatisticsItem.Parse( xe );
        if ( si != null )
          results.Add( si );
      }

      return results;
    }

    public static PSPDayStatistics Parse( XElement xroot, string tagsLabel, string modulesLabel, string projectsLabel ) {
      if ( xroot == null )
        return null;

      PSPDayStatistics result = new PSPDayStatistics();
      XElement xeTags = xroot.Element( tagsLabel ),
        xeModules = xroot.Element( modulesLabel ),
        xeProjects = xroot.Element( projectsLabel );
      if ( xeTags != null )
        result.TagsStatistics = PSPDayStatistics.Parse( xeTags );
      if ( xeModules != null )
        result.ModulesStatistics = PSPDayStatistics.Parse( xeModules );
      //if ( xeProjects != null )
      //  result.ProjectsStatistics = PSPDayStatistics.Parse( xeProjects );

      return result;
    }

  }

  public class PSPDay {
    public DateTime Date { get; set; }
    public List<PSPEvent> PSP_Events { get; set; }
    public PSPDayStatistics Statistics { get; set; }

    /// <summary>
    /// "day_xxxx_statistics"
    /// </summary>
    public const string dayTagsLabel = "day_tags_statistics",
      dayModulesLabel = "day_modules_statistics",
      dayProjectsLabel = "day_projects_statistics";

    public PSPDay() {
      Date = DateTime.Today;
      PSP_Events = new List<PSPEvent>();
      Statistics = new PSPDayStatistics();
    }

    /// <summary>
    /// 移除某个PSPEvent
    /// </summary>
    /// <param name="psp_event"></param>
    public void RemoveEvent( PSPEvent psp_event ) {
      if ( psp_event == null || PSP_Events == null || PSP_Events.Count == 0 )
        return;

      for ( int i = 0; i < PSP_Events.Count; i++ ) {
        if ( Guid.Equals( psp_event.guid, PSP_Events[i].guid ) ) {
          PSP_Events.RemoveAt( i );
          // 更新标签的统计数据
          if ( psp_event.tags != null && psp_event.tags.Count > 0 ) {
            foreach ( TagReference tr in psp_event.tags )
              Statistics.UpdateTagsStatistics2( tr, psp_event.time_duration, psp_event.date );
          }
          // 更新Module的统计数据
          if ( psp_event.module != null ) {
            Statistics.UpdateModulesStatistics2( psp_event.module, psp_event.time_duration, psp_event.date );
          }

          break;
        } // if
      } // for

    }

    /// <summary>
    /// 查找某个事件，并返回索引
    /// </summary>
    /// <param name="psp_event"></param>
    /// <returns></returns>
    public int EventIndex( PSPEvent psp_event ) {
      int index = -1;
      if ( psp_event == null || PSP_Events == null || PSP_Events.Count == 0 )
        return index;

      for ( int i = 0; i < PSP_Events.Count; i++ ) {
        if ( Guid.Equals( psp_event.guid, PSP_Events[i].guid ) ) {
          index = i;
          break;
        }
      }

      return index;
    }

    public void AddEvent( PSPEvent psp_event ) {
      if ( psp_event == null )
        return;
      // add
      if ( PSP_Events == null )
        PSP_Events = new List<PSPEvent>();
      PSP_Events.Add( psp_event );
      // 更新标签的统计数据
      if ( psp_event.tags != null && psp_event.tags.Count > 0 ) {
        foreach ( TagReference tr in psp_event.tags )
          Statistics.UpdateTagsStatistics( tr, psp_event.time_duration, psp_event.date );
      }
      // 更新Module的统计数据
      if ( psp_event.module != null ) {
        Statistics.UpdateModulesStatistics( psp_event.module, psp_event.time_duration, psp_event.date );
        //Statistics.UpdateProjectsStatistics( psp_event.module, psp_event.time_duration, psp_event.date );
      }

    }

    public XElement ToXElement() {
      XElement xroot = new XElement( "psp_day" );
      xroot.Add( new XAttribute( "date", Date.ToLongDateString() ) );

      if ( PSP_Events != null )
        xroot.Add( PSPEvent.ToXElement( PSP_Events, "day_events" ) );

      if ( Statistics != null ) {
        XElement xeStatistics = new XElement( "day_statistics" );
        xeStatistics.Add( PSPDayStatistics.ToXElement( Statistics.TagsStatistics, dayTagsLabel ) );
        xeStatistics.Add( PSPDayStatistics.ToXElement( Statistics.ModulesStatistics, dayModulesLabel ) );
        //xeStatistics.Add( PSPDayStatistics.ToXElement( Statistics.ProjectsStatistics, dayProjectsLabel ) );
        
        xroot.Add( xeStatistics );
      }

      return xroot;
    }

    public static PSPDay Parse( XElement xroot ) {
      if ( xroot == null )
        return null;

      PSPDay pday = new PSPDay();
      pday.Date = Convert.ToDateTime( xroot.Attribute( "date" ).Value );
      var eventsElement = xroot.Element( "day_events" );
      if ( eventsElement != null )
        pday.PSP_Events = PSPEvent.ParseMany( eventsElement );
      XElement statisticsElement = xroot.Element( "day_statistics" );
      pday.Statistics = PSPDayStatistics.Parse( statisticsElement, dayTagsLabel, dayModulesLabel, dayProjectsLabel );

      return pday;
    }

    /// <summary>
    /// 总计：当日的已统计时间
    /// </summary>
    /// <returns></returns>
    public int TimeCount() {
      int time = 0;
      foreach ( PSPEvent pe in PSP_Events ) {
        time += pe.time_duration;
      }
      return time;
    }

    /// <summary>
    /// 获取今日某模块的统计数据
    /// </summary>
    /// <param name="gModule"></param>
    /// <returns></returns>
    public int GetDayStatisticsOfModule( string moduleName, Guid gModule ) {
      if ( Guid.Equals( Guid.Empty, gModule ) ) return 0;

      if ( Statistics != null && Statistics.ModulesStatistics != null ) {
        foreach ( StatisticsItem si in Statistics.ModulesStatistics ) {
          if ( si.name.Equals( moduleName ) && si.guid.Equals( gModule ) )
            return si.total_time;
        }
      }

      return 0;
    }

  }

  public class PSPWeek {
    public List<PSPDay> PSP_Week_Days { get; set; }
    /// <summary>
    /// 指自开始记录PSP以来的第几周，大于等于1
    /// </summary>
    public int PSP_Week_Number { get; set; }
    /// <summary>
    /// 该周星期一的日期
    /// </summary>
    public DateTime DateOfMonday { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weekNum">指第几周，大于等于1</param>
    public PSPWeek( int weekNum ) {
      PSP_Week_Days = new List<PSPDay>();
      PSP_Week_Number = weekNum < 1 ? 1 : weekNum;
      DateOfMonday = DateTime.Today;
    }
    public PSPWeek( int weekNum, DateTime day ) {
      PSP_Week_Days = new List<PSPDay>();
      PSP_Week_Number = weekNum < 1 ? 1 : weekNum;
      DateOfMonday = day;
    }

    public PSPDay Get_PSPDay( DateTime dt ) {
      PSPDay pday = null;
      foreach ( PSPDay pd in PSP_Week_Days ) {
        if ( pd.Date.Date.Equals( dt.Date ) ) {
          pday = pd;
          break;
        }
      }

      return pday;
    }
    public int Get_PSPDay_Index( DateTime dt ) {
      int index = -1;
      if ( PSP_Week_Days == null || PSP_Week_Days.Count == 0 )
        return index;

      for ( int i = 0; i < PSP_Week_Days.Count; i++ ) {
        if ( PSP_Week_Days[i].Date.Date.Equals( dt.Date ) ) {
          index = i;
          break;
        }
      }

      return index;
    }

    /// <summary>
    /// 按天统计，即返回的List的大小是0--7
    /// </summary>
    public List<StatisticsItem> GetWeekStatisticsOfModule( string moduleName, Guid gModule ) {
      if ( Guid.Equals( Guid.Empty, gModule ) ) return null;

      List<StatisticsItem> results = new List<StatisticsItem>();
      foreach ( PSPDay pday in PSP_Week_Days ) {
        StatisticsItem si = new StatisticsItem();
        si.name = pday.Date.ToString( "yy-MM-dd" );
        si.guid = gModule;
        si.date = pday.Date;
        si.total_time = pday.GetDayStatisticsOfModule( moduleName, gModule );
        results.Add( si );
      }
      return results;
    }
    /// <summary>
    /// 获取某模块在某周的统计数据，一周七天的都加在一起
    /// </summary>
    public int GetWeekStatisticsOfModule2( string moduleName, Guid gModule ) {
      if ( Guid.Equals( Guid.Empty, gModule ) ) return 0;
      int result = 0;
      foreach ( PSPDay pday in PSP_Week_Days ) {
        result += pday.GetDayStatisticsOfModule( moduleName, gModule );
      }
      return result;
    }
    /// <summary>
    /// 获取某模块在某天的统计数据，若查询日期不在该周，则返回0
    /// </summary>
    public int GetDayStatisticsOfModule( string moduleName, Guid gModule, DateTime dt ) {
      if ( Guid.Equals( Guid.Empty, gModule ) ) return 0;
      PSPDay pday = Get_PSPDay( dt );
      if ( pday == null ) return 0;
      return pday.GetDayStatisticsOfModule( moduleName, gModule );
    }

    public XElement ToXElement() {
      XElement xroot = new XElement( "psp_week" );
      xroot.Add( new XAttribute( "week_number", PSP_Week_Number.ToString() ) );
      xroot.Add( new XAttribute( "date_of_monday", DateOfMonday.ToLongDateString() ) );
      if ( PSP_Week_Days != null ) {
        foreach ( PSPDay pd in PSP_Week_Days )
          xroot.Add( pd.ToXElement() );
      }

      return xroot;
    }

    public static PSPWeek Parse( XElement xroot ) {
      if ( xroot == null )
        return null;

      PSPWeek week = new PSPWeek( 1 );
      week.PSP_Week_Number = Convert.ToInt32( xroot.Attribute( "week_number" ).Value );
      week.DateOfMonday = Convert.ToDateTime( xroot.Attribute( "date_of_monday" ).Value );
      var days = xroot.Elements( "psp_day" );
      foreach ( XElement d in days ) {
        week.PSP_Week_Days.Add( PSPDay.Parse( d ) );
      }

      return week;
    }

  }

}
