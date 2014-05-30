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

using System.IO;
using System.Net;
using System.Web;
using System.Threading;
using System.ComponentModel;
using System.Windows.Media.Animation;
using DataAccess.PSP;

namespace PSP.Run.Views.UserControls {
  /// <summary>
  /// Interaction logic for ConceptControl.xaml
  /// </summary>
  public partial class ConceptControl : UserControl {
    string InfoBasePrefix = "http://www.witchhat.cn/infobase/select/Default.aspx?name=";
    /// <summary>
    /// 查询结果
    /// </summary>
    List<InfoBase.Concept> concepts = null;
    InfoBase.Concept cSelected = null;

    List<TagReference> recentTags = null;

    /// <summary>
    /// SynchronizationContext - 同步上下文管理类
    /// </summary>
    SynchronizationContext _syncContext;
    /// <summary>
    /// exception info
    /// </summary>
    string _exception = string.Empty;
    public static ManualResetEvent allDone = new ManualResetEvent( false );
    const int BUFFER_SIZE = 1024;
    const int DefaultTimeout = 2 * 60 * 1000; // 2 minutes timeout

    public ConceptControl() {
      InitializeComponent();

      this.Loaded += new RoutedEventHandler( ConceptControl_Loaded );
    }

    void ConceptControl_Loaded( object sender, RoutedEventArgs e ) {
      // SynchronizationContext.Current - 当前线程的同步上下文
      _syncContext = SynchronizationContext.Current;
      // 用最近使用的标签初始化lbRecentTags
      if ( recentTags == null )
        recentTags = new List<TagReference>();
      var tagdata = DataHelper.PSP_Tags;
      if ( tagdata != null )
        recentTags.AddRange( tagdata );
      lbRecentTags.ItemsSource = recentTags;
      lbRecentTags.Items.SortDescriptions.Add(
        new SortDescription( "referenced_times", ListSortDirection.Descending ) );
    }

    void initializeRecentTags( string prefix ) {
      if ( recentTags != null )
        recentTags.Clear();
      else
        recentTags = new List<TagReference>();

      lbRecentTags.ItemsSource = null;
      if ( prefix == null || prefix.Length == 0 ) // 所有最近使用的标签
        recentTags.AddRange( DataHelper.PSP_Tags );
      else { // 过滤
        var tmp = DataHelper.PSP_Tags.Where( p => p.name.Contains( prefix ) );
        if ( tmp != null )
          recentTags.AddRange( tmp );
      }

      lbRecentTags.ItemsSource = recentTags;
    }

    // 查询
    private void tbTagName_KeyUp( object sender, KeyEventArgs e ) {
      if ( e.Key == Key.Enter ) { // 按Enter键，向Info-base查询
        btnQuery_Click( btnQuery, null );
      }
      else { // 在最近用过的标签中过滤
        initializeRecentTags( tbTagName.Text.Trim() );
      }
    }

    // 查询
    private void btnQuery_Click( object sender, RoutedEventArgs e ) {
      string queryString = tbTagName.Text.Trim();
      if ( queryString.Length <= 0 ) {
        MessageBox.Show( "请输入数据先……", "注意！", MessageBoxButton.OK );
        return;
      }

      lbConcepts.Items.Clear();
      if ( concepts != null )
        concepts.Clear();
      cSelected = null;
      tblkSelectedConcept.Text = "";

      // ui
      btnAddSimpleTag.Visibility = Visibility.Collapsed;
      btnQuery.Visibility = Visibility.Collapsed;

      // 参考：http://msdn.microsoft.com/zh-cn/library/system.net.httpwebrequest.begingetresponse(VS.80).aspx
      HttpWebRequest hwrQuery = HttpWebRequest.CreateDefault( new Uri( InfoBasePrefix + System.Web.HttpUtility.UrlEncode( queryString ) ) ) as HttpWebRequest;
      hwrQuery.AllowAutoRedirect = true;
      RequestState myRequestState = new RequestState();
      myRequestState.request = hwrQuery;
      // Start the asynchronous request.
      IAsyncResult result =(IAsyncResult)hwrQuery.BeginGetResponse( new AsyncCallback( RespCallback ), myRequestState );
      // this line implements the timeout, if there is a timeout, the callback fires and the request becomes aborted
      ThreadPool.RegisterWaitForSingleObject( result.AsyncWaitHandle, new WaitOrTimerCallback( TimeoutCallback ), hwrQuery, DefaultTimeout, true );
      // The response came in the allowed time. The work processing will happen in the 
      // callback function.
      allDone.WaitOne();

    }

    #region Http

    // Abort the request if the timer fires.
    private void TimeoutCallback( object state, bool timedOut ) {
      if ( timedOut ) {
        HttpWebRequest request = state as HttpWebRequest;
        if ( request != null ) {
          request.Abort();
        }
      }
    }

    private void RespCallback( IAsyncResult asynchronousResult ) {
      try {
        // State of request is asynchronous.
        RequestState myRequestState = (RequestState)asynchronousResult.AsyncState;
        HttpWebRequest myHttpWebRequest = myRequestState.request;
        myRequestState.response = (HttpWebResponse)myHttpWebRequest.EndGetResponse( asynchronousResult );

        // Read the response into a Stream object.
        Stream responseStream = myRequestState.response.GetResponseStream();
        myRequestState.streamResponse = responseStream;

        // Begin the Reading of the contents of the HTML page and print it to the console.
        IAsyncResult asynchronousInputRead = responseStream.BeginRead( myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback( ReadCallBack ), myRequestState );
        return;
      }
      catch ( WebException e ) {
      }
      allDone.Set();
    }

    private void ReadCallBack( IAsyncResult asyncResult ) {
      try {
        RequestState myRequestState = (RequestState)asyncResult.AsyncState;
        Stream responseStream = myRequestState.streamResponse;

        // 并发
        int read = responseStream.EndRead( asyncResult );
        if ( read > 0 ) {
          myRequestState.requestData.Append( Encoding.UTF8.GetString( myRequestState.BufferRead, 0, read ) );
          if ( read >= BUFFER_SIZE ) {
            IAsyncResult asynchronousResult = responseStream.BeginRead( myRequestState.BufferRead, 0, BUFFER_SIZE, new AsyncCallback( ReadCallBack ), myRequestState );
            return;
          }
          else {
            _syncContext.Post( ProcessQueryResult, myRequestState );

            responseStream.Close();
            // Release the HttpWebResponse resource.
            myRequestState.response.Close();
          }
        }
        

        //// 非并发，不支持
        //int offset = 0;
        //while ( true ) {
        //  int read = responseStream.Read( myRequestState.BufferRead, offset, BUFFER_SIZE );
        //  if ( read <= 0 ) {
        //    _syncContext.Post( ProcessQueryResult, myRequestState );

        //    responseStream.Close();
        //    // Release the HttpWebResponse resource.
        //    myRequestState.response.Close();
        //    break;
        //  }
        //  myRequestState.requestData.Append( Encoding.UTF8.GetString( myRequestState.BufferRead ) );
        //  offset += read;
        //}

      }
      catch ( WebException e ) {
      }
      allDone.Set();

    }
    /// <summary>
    /// 由_syncContext调用，通知UI线程处理数据
    /// </summary>
    /// <param name="state"></param>
    private void ProcessQueryResult( object state ) {
      RequestState myRequestState = (RequestState)state;
      concepts = InfoBase.Concept.XML2Concepts( myRequestState.requestData.ToString() );

      btnQuery.Visibility = Visibility.Visible;

      if ( concepts == null || concepts.Count == 0 ) { // 没有相关的语义标签
        btnAddSimpleTag.Visibility = Visibility.Visible;
        return;
      }
      btnAddSimpleTag.Visibility = Visibility.Collapsed;
      // processing
      initializeConceptList( lbConcepts, concepts );
    }

    #endregion Http

    /// <summary>
    /// 使用concepts初始化lbConcepts
    /// </summary>
    void initializeConceptList( ListBox lb, List<InfoBase.Concept> clist ) {
      if ( clist == null )
        return;
      lb.Items.Clear();
      foreach ( InfoBase.Concept c in clist ) {
        ListBoxItem lbi = new ListBoxItem();
        lbi.Content = c.name;
        lbi.Tag = new Guid( c.guid );
        lbi.SetValue( ToolTipService.ToolTipProperty, "描述：" + c.description + "\n版本：" + c.versionNo.ToString() );
        lb.Items.Add( lbi );
      }
    }

    // 左键点击某个标签，显示相关信息
    private void lbConcepts_MouseLeftButtonUp( object sender, MouseButtonEventArgs e ) {
      if ( lbConcepts.Items.Count == 0 || lbConcepts.SelectedIndex < 0 ) {
        tblkSelectedConcept.Text = "";
        return;
      }
      ListBoxItem lbi = lbConcepts.SelectedItem as ListBoxItem;
      if ( lbi == null || lbi.Tag == null )
        return;
      string guid = ( (Guid)( lbi.Tag ) ).ToString();

      foreach ( InfoBase.Concept c in concepts ) {
        if ( string.Equals( guid, c.guid ) ) {
          cSelected = c;
          break;
        }
      }
      if ( cSelected == null )
        return;

      // 将Entities.Concept显示到tblkSelectedConcept中
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      // description
      if ( !string.IsNullOrEmpty( cSelected.description ) )
        sb.Append( "【描述(Description)】\n" + cSelected.description );
      // alsoKnownAs
      if ( cSelected.alsoKnownAs != null && cSelected.alsoKnownAs.Count > 0 ) {
        sb.Append( "\n\n【同义词(Also Known As)】\n" );
        foreach ( string s in cSelected.alsoKnownAs )
          sb.Append( s + "; " );
        sb = sb.Remove( sb.Length - 2, 2 );
      }
      // tags
      if ( cSelected.tags != null && cSelected.tags.Count > 0 ) {
        sb.Append( "\n\n【普通标签(Tags)】\n" );
        foreach ( string s in cSelected.tags )
          sb.Append( s + "; " );
      }
      // guid
      sb.Append( "\n\n[GUID] " + cSelected.guid );

      tblkSelectedConcept.Text = sb.ToString();
    }

  }



  public class RequestState {
    // This class stores the State of the request.
    const int BUFFER_SIZE = 1024;
    public StringBuilder requestData;
    public byte[] BufferRead;
    public HttpWebRequest request;
    public HttpWebResponse response;
    public Stream streamResponse;
    public RequestState() {
      BufferRead = new byte[BUFFER_SIZE];
      requestData = new StringBuilder( "" );
      request = null;
      streamResponse = null;
    }
  }


}
