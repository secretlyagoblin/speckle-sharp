using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopUI2;
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using System.Timers;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

using SAP2000v1;

using Speckle.Core.Logging;
using Speckle.ConnectorSAP2000.Util;
using Speckle.ConnectorSAP2000.UI;
using System.Reflection;
using System.IO;

namespace SpeckleConnectorSAP2000
{
  public class cPlugin
  {
    public static cPluginCallback pluginCallback { get; set; }
    public static bool isSpeckleClosed { get; set; } = false;
    public Timer SelectionTimer;
    public static cSapModel model { get; set; }

    public static Window MainWindow { get; private set; }

    public static ConnectorBindingsSAP2000 Bindings { get; set; }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<DesktopUI2.App>()
    .UsePlatformDetect()
    .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
    .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
    .LogToTrace()
    .UseReactiveUI();



    public static void CreateOrFocusSpeckle()
    {
      if (MainWindow == null)
      {

        BuildAvaloniaApp().Start(AppMain, null);
      }

      MainWindow.Show();
      MainWindow.Activate();


    }

    private static void AppMain(Application app, string[] args)
    {
      var viewModel = new MainWindowViewModel();
      MainWindow = new MainWindow
      {
        DataContext = viewModel
      };

      app.Run(MainWindow);
      //Task.Run(() => app.Run(MainWindow));
    }

    public static void OpenOrFocusSpeckle(cSapModel model)
    {
      Bindings = new ConnectorBindingsSAP2000(model);
      CreateOrFocusSpeckle();

      //try
      //{
      //    Setup.Init("ConnectorETABS");
      //    if (Bootstrapper != null)
      //    {
      //        Bootstrapper.ShowRootView();
      //        return;
      //    }

      //    Bootstrapper = new Bootstrapper()
      //    {
      //        Bindings = new ConnectorBindingsETABS(model)
      //    };

      //    if (Application.Current != null)
      //        new StyletAppLoader() { Bootstrapper = Bootstrapper };
      //    else
      //        new App(Bootstrapper);



      var processes = Process.GetProcesses();
      IntPtr ptr = IntPtr.Zero;
      foreach (var process in processes)
      {
        if (process.ProcessName.ToLower().Contains("sap2000"))
        {
          ptr = process.MainWindowHandle;
          break;
        }
      }
      if (ptr != IntPtr.Zero)
      {
        //Application.Current.MainWindow.Closed += SpeckleWindowClosed;
        MainWindow.Closed += SpeckleWindowClosed;
        MainWindow.Closing += SpeckleWindowClosed;
      }
      //}
      //catch
      //{
      //    Bootstrapper = null;
      //}
    }

    private static void SpeckleWindowClosed(object sender, EventArgs e)
    {
      isSpeckleClosed = true;
      pluginCallback.Finish(0);
    }

    private void SelectionTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if (isSpeckleClosed == true)
      {
        pluginCallback.Finish(0);
      }
    }


    public int Info(ref string Text)
    {
      Text = "This is a Speckle plugin for SAP2000";
      return 0;
    }

    static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
      Assembly a = null;
      var name = args.Name.Split(',')[0];
      string path = Path.GetDirectoryName(typeof(cPlugin).Assembly.Location);

      string assemblyFile = Path.Combine(path, name + ".dll");

      if (File.Exists(assemblyFile))
        a = Assembly.LoadFrom(assemblyFile);

      return a;
    }



    public void Main(ref cSapModel SapModel, ref cPluginCallback ISapPlugin)
    {
      model = SapModel;
      pluginCallback = ISapPlugin;
      AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
      Setup.Init(ConnectorSAP2000Utils.SAP2000AppName);
      try
      {
        cHelper helper = new Helper();
        var SAP2000Object = helper.GetObject("CSI.SAP2000.API.SapObject");
        model = SAP2000Object.SapModel;
      }
      catch
      {
        ISapPlugin.Finish(0);
        return;
      }

      try
      {
        OpenOrFocusSpeckle(model);
        SelectionTimer = new Timer(2000) { AutoReset = true, Enabled = true };
        SelectionTimer.Elapsed += SelectionTimer_Elapsed;
        SelectionTimer.Start();
      }

      catch (Exception e)
      {

        ISapPlugin.Finish(0);
        return;
      }
    }
  }
}
