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
  /// SimpleTimer.xaml 的交互逻辑
  /// </summary>
  public partial class SimpleTimer : UserControl {
    System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
    int hour = 0,
      minute = 0,
      second = 0;

    public SimpleTimer() {
      InitializeComponent();

      timer.Interval = new TimeSpan( 0, 0, 1 );
      timer.Tick += new EventHandler( timer_Tick );
    }

    void timer_Tick( object sender, EventArgs e ) {
      second++;
      if ( second >= 60 ) {
        second = 0;
        minute++;
        if ( minute >= 60 ) {
          minute = 0;
          hour++;
          //if ( hour >= 24 )
          //  hour = 1;
        } // if minute
      } // if second

    }

    public void Start() {
      timer.Start();
    }

    public void Stop() {
      timer.Stop();
    }

    public void Reset() {
      timer.Stop();

      hour = 0;
      minute = 0;
      second = 0;
    }

  }

}
