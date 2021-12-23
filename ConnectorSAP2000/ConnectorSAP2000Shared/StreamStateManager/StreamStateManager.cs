using System;
using Speckle.ConnectorSAP2000.Util;
using System.Collections.Generic;
using Speckle.Core.Logging;
using DesktopUI2.Models;
using Speckle.Newtonsoft.Json;
using System.IO;
using System.Text;
using SAP2000v1;

namespace ConnectorSAP2000.Storage
{
  public static class StreamStateManager
  {
    private static string _speckleFilePath;
    public static List<StreamState> ReadState(cSapModel model)
    {
      Tracker.TrackPageview(Tracker.DESERIALIZE);
      var strings = ReadSpeckleFile(model);
      if (strings == "")
      {
        return new List<StreamState>();
      }
      try
      {
        Tracker.TrackPageview(Tracker.DESERIALIZE);
        return JsonConvert.DeserializeObject<List<StreamState>>(strings);
      }
      catch
      {
        return new List<StreamState>();
      }
    }

    /// <summary>
    /// Writes the stream states to the <SAP2000ModelName>.txt file in speckle folder
    /// that exists or is created in the folder where the SAP2000 model exists.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="streamStates"></param>
    public static void WriteStreamStateList(cSapModel model, List<StreamState> streamStates)
    {
      if (_speckleFilePath == null) GetOrCreateSpeckleFilePath(model);
      FileStream fileStream = new FileStream(_speckleFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
      try
      {
        using (var streamWriter = new StreamWriter(fileStream))
        {
          streamWriter.Write(JsonConvert.SerializeObject(streamStates) as string);
          streamWriter.Flush();
          Tracker.TrackPageview(Tracker.SERIALIZE);
        }
      }
      catch { }
    }

    public static void ClearStreamStateList(cSapModel model)
    {
      if (_speckleFilePath == null) GetOrCreateSpeckleFilePath(model);
      FileStream fileStream = new FileStream(_speckleFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
      try
      {
        fileStream.SetLength(0);
      }
      catch { }
    }

    /// <summary>
    /// We need a folder in SAP2000 model folder named "speckle" and a file in it
    /// called "<SAP2000ModelName>.txt". This function create this file and folder if
    /// they doesn't exists and returns it, otherwise just returns the file path
    /// </summary>
    /// <param name="model"></param>
    private static void GetOrCreateSpeckleFilePath(cSapModel model)
    {
      string SAP2000ModelfilePath = model.GetModelFilename(true);
      if (SAP2000ModelfilePath == "")
      {
        // SAP2000 model is probably not saved, so speckle shouldn't do much
        _speckleFilePath = null;
        return;
      }
      string SAP2000FileName = Path.GetFileNameWithoutExtension(SAP2000ModelfilePath);
      string SAP2000ModelFolder = Path.GetDirectoryName(SAP2000ModelfilePath);
      string speckleFolderPath = Path.Combine(SAP2000ModelFolder, "speckle");
      string speckleFilePath = Path.Combine(SAP2000ModelFolder, "speckle", $"{SAP2000FileName}.txt");
      try
      {
        if (!Directory.Exists(speckleFolderPath))
        {
          Directory.CreateDirectory(speckleFolderPath);
        }
        if (!File.Exists(speckleFilePath))
        {
          File.CreateText(speckleFilePath);
        }
        _speckleFilePath = speckleFilePath;
      }
      catch
      {
        _speckleFilePath = null;
        return;
      }
    }

    /// <summary>
    /// Reads the "/speckle/<SAP2000ModelName>.txt" file and returns the string in it
    /// </summary>
    /// <param name="model"></param>
    private static string ReadSpeckleFile(cSapModel model)
    {
      if (_speckleFilePath == null)
        GetOrCreateSpeckleFilePath(model);

      if (_speckleFilePath == null) return "";
      FileStream fileStream = new FileStream(_speckleFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      try
      {
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
        {
          return streamReader.ReadToEnd();
        }
      }
      catch { return ""; }
    }

  }
}
