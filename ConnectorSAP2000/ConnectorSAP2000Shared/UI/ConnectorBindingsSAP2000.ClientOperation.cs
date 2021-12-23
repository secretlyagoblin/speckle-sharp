using System;
using System.Collections.Generic;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.ViewModels;
using Speckle.Core.Logging;
using System.Linq;
using System.Threading.Tasks;
using ConnectorSAP2000.Storage;


namespace Speckle.ConnectorSAP2000.UI
{
  public partial class ConnectorBindingsSAP2000
  {
    public override List<MenuItem> GetCustomStreamMenuItems()
    {
      return new List<MenuItem>();
    }

    public override void WriteStreamsToFile(List<StreamState> streams)
    {
      StreamStateManager.ClearStreamStateList(Model);

      foreach (var s in streams)
      {
        DocumentStreams.Add(s);
        WriteStateToFile();
      }
      //throw new NotImplementedException();
    }

    //public override void AddNewStream(StreamState state)
    //{
    //    Tracker.TrackPageview(Tracker.STREAM_CREATE);
    //    var index = DocumentStreams.FindIndex(b => b.Stream.id == state.Stream.id);
    //    if (index == -1)
    //    {
    //        DocumentStreams.Add(state);
    //        WriteStateToFile();
    //    }
    //}
    private void WriteStateToFile()
    {
      StreamStateManager.WriteStreamStateList(Model, DocumentStreams);
    }

    //public override void RemoveStreamFromFile(string streamId)
    //{
    //    var streamState = DocumentStreams.FirstOrDefault(s => s.Stream.id == streamId);
    //    if (streamState != null)
    //    {
    //        DocumentStreams.Remove(streamState);
    //        WriteStateToFile();
    //    }
    //}

    public override List<StreamState> GetStreamsInFile()
    {
      if (Model != null)
        DocumentStreams = StreamStateManager.ReadState(Model);

      return DocumentStreams;
    }
  }
}
