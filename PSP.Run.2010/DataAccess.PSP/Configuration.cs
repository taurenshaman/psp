using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml.Linq;

namespace DataAccess.PSP {
  /// <summary>
  /// 软件配置相关，configration.xml位于和可执行文件同文件夹下
  /// </summary>
  public class Configuration {

    #region System

    /// <summary>
    /// 系统配置文件的文件名
    /// </summary>
    const string configFileName = "configration.xml";
    /// <summary>
    /// 系统可执行文件所在文件夹
    /// </summary>
    public static string AppExcuteFilePath = string.Empty;

    #endregion System

    #region PSP

    /// <summary>
    /// PSP数据存放的文件夹
    /// </summary>
    public static string PSP_Data_Path = string.Empty;
    /// <summary>
    /// 备份文件夹名称，需要与PSP_Data_Path结合使用
    /// </summary>
    public const string PSP_Data_BackupFolder = "bak";

    public const string PSP_Tags_File = "psp_tags.xml",
      PSP_Modules_File = "psp_modules.xml",
      PSP_Projects_File = "psp_projects.xml",
      PSP_Statistics_File = "psp_statistics.xml";
    /// <summary>
    /// psp_week_[date_of_monday].psp
    /// </summary>
    public const string PSP_WeekFile = "psp_week_";

    #endregion PSP

    #region public Methods

    public Configuration() {
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="appExcuteFilePath">系统可执行文件所在文件夹</param>
    public Configuration( string appExcuteFilePath ) {
      AppExcuteFilePath = appExcuteFilePath;
    }

    public XElement ToXElement() {
      XElement xroot = new XElement( "psp_run_configration" );
      xroot.Add( new XElement( "psp_data_path", PSP_Data_Path ) );

      return xroot;
    }

    public string ToXMLString( bool withXMLHeader ) {
      XElement xroot = ToXElement();
      return withXMLHeader ? "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" + xroot.ToString() : xroot.ToString();
    }

    #endregion public Methods

    #region Static Methods

    public static Configuration Parse( string xml ) {
      XElement xroot = null;
      try {
        xroot = XElement.Parse( xml );
      }
      catch ( Exception ex ) {
        return null;
      }

      if ( xroot == null )
        return null;

      Configuration c = new Configuration();
      XElement xeTmp = xroot.Element( "psp_data_path" );
      PSP_Data_Path = xeTmp == null ? string.Empty : xeTmp.Value.Trim();

      return c;
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="config"></param>
    /// <param name="filePath"></param>
    public static void Save( Configuration config, string filePath ) {
      if ( config == null || filePath == null || filePath.Length <= 0 )
        return;
      string data = config.ToXMLString( true );
      try {
        File.WriteAllText( filePath + "\\" + configFileName, data, Encoding.UTF8 );
      }
      catch ( Exception ex ) {
        throw ex;
      }
    }

    /// <summary>
    /// 判断是否存在配置文件
    /// </summary>
    /// <param name="ApplicationExcuteFilePath"></param>
    /// <returns></returns>
    public static bool ConfigrationFileExists( string ApplicationExcuteFilePath ) {
      return File.Exists( ApplicationExcuteFilePath + "\\" + configFileName );
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    /// <param name="ApplicationExcuteFilePath"></param>
    /// <returns></returns>
    public static Configuration GetConfigration( string ApplicationExcuteFilePath ) {
      if ( ApplicationExcuteFilePath == null || ApplicationExcuteFilePath.Length <= 0 )
        return null;
      string configFile = ApplicationExcuteFilePath + "\\" + configFileName;
      Configuration config = null;
      if ( !File.Exists( configFile ) ) {
        config = new Configuration( ApplicationExcuteFilePath );
      }
      else {
        try {
          string data = File.ReadAllText( configFile, Encoding.UTF8 );
          config = Parse( data );
          AppExcuteFilePath = ApplicationExcuteFilePath;
        }
        catch ( Exception ex ) {
          throw ex;
        }
      }

      return config;
    }

    #endregion Static Methods

  }

}
