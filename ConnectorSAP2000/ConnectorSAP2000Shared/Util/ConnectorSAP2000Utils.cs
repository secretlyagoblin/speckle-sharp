using System;
using System.Collections.Generic;
using System.Text;
using Speckle.Core.Kits;
using Speckle.Core.Logging;
using System.Linq;
using Speckle.ConnectorSAP2000.UI;
using SAP2000v1;

namespace Speckle.ConnectorSAP2000.Util
{
  public class ConnectorSAP2000Utils
  {
#if SAP2000V23
    public static string SAP2000AppName = Applications.SAP2000v23;
#else
    public static string SAP2000AppName = Applications.SAP2000;
#endif

    public static Dictionary<string, (string, string)> ObjectIDsTypesAndNames { get; set; }

    public List<SpeckleException> ConversionErrors { get; set; }

    public static void GetObjectIDsTypesAndNames(cSapModel model)
    {
      ObjectIDsTypesAndNames = new Dictionary<string, (string, string)>();
      foreach (var objectType in Enum.GetNames(typeof(SAP2000APIUsableTypes)))
      {
        var names = new List<string>();
        try
        {
          names = GetAllNamesOfObjectType(model, objectType);
        }
        catch { }
        if (names.Count > 0)
        {
          foreach (string name in names)
          {
            ObjectIDsTypesAndNames.Add(string.Concat(objectType, ": ", name), (objectType, name));
          }
        }
      }
    }

    public static bool IsTypeSAP2000APIUsable(string type)
    {
      return Enum.GetNames(typeof(SAP2000APIUsableTypes)).Contains(type);
    }

    public static List<string> GetAllNamesOfObjectType(cSapModel model, string objectType)
    {
      switch (objectType)
      {
        case "Point":
          return GetAllPointNames(model);
        case "Frame":
          return GetAllFrameNames(model);
        case "Area":
          return GetAllAreaNames(model);
        case "Links":
          return GetAllLinkNames(model);
        case "Tendon":
          return GetAllTendonNames(model);
       
        case "LoadPattern":
          return GetAllLoadPatternNames(model);
        case "FrameLoading":
          return GetAllFrameNames(model);
        case "AreaLoading":
          return GetAllAreaNames(model);
        case "NodeLoading":
          return GetAllPointNames(model);
        case "Model":
          var names = new string[] { };
          names.Append(model.GetModelFilename());
          return names.ToList();
        case "AnalysisResults":
          return GetAllElementNames(model);
        default:
          return null;
      }
    }
    #region Get List Names
    public static List<string> GetAllPointNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PointObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }

    }
    public static List<string> GetAllFrameNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.FrameObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }




    public static List<string> GetAllElementNames(cSapModel model)
    {
      var elementNames = new List<string>();
      elementNames.AddRange(GetAllFrameNames(model));

      return elementNames;
    }

    public static List<string> GetAllTendonNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.TendonObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllAreaNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.AreaObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }

    public static List<string> GetAllLinkNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.LinkObj.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllPropMaterialNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PropMaterial.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllPropRebarNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PropRebar.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllPropFrameNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PropFrame.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllLoadCaseNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.LoadCases.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllGroupNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.GroupDef.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllComboNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.RespCombo.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllConstraintNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.ConstraintDef.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllLoadPatternNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.LoadPatterns.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllSteelDesignNames(cSapModel model)
    {
      var name = "";
      try
      {
        model.DesignSteel.GetCode(ref name);
        return new List<string>() { name };
      }
      catch { return null; }
    }
    public static List<string> GetAllConcreteDesignNames(cSapModel model)
    {
      var name = "";
      try
      {
        model.DesignConcrete.GetCode(ref name);
        return new List<string>() { name };
      }
      catch { return null; }
    }
    public static List<string> GetAllLineNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.LineElm.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
 
 
   
  
 
    public static List<string> GetAllPropTendonNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PropTendon.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }
    public static List<string> GetAllPropLinkNames(cSapModel model)
    {
      int num = 0;
      var names = new string[] { };
      try
      {
        model.PropLink.GetNameList(ref num, ref names);
        return names.ToList();
      }
      catch { return null; }
    }

    #endregion

    public static List<(string, string)> SelectedObjects(cSapModel model)
    {
      int num = 0;
      var types = new int[] { };
      var names = new string[] { };
      model.SelectObj.GetSelected(ref num, ref types, ref names);
      var typesAndNames = new List<(string, string)>();
      if (num < 1)
      {
        return null;
      }
      for (int i = 0; i < num; i++)
      {
        switch (types[i])
        {
          case 1:
            typesAndNames.Add(("Point", names[i]));
            break;
          case 2:
            typesAndNames.Add(("Frame", names[i]));
            break;
          case 3:
            typesAndNames.Add(("Cable", names[i]));
            break;
          case 4:
            typesAndNames.Add(("Tendon", names[i]));
            break;
          case 5:
            typesAndNames.Add(("Area", names[i]));
            break;
          case 6:
            typesAndNames.Add(("Solid", names[i]));
            break;
          case 7:
            typesAndNames.Add(("Link", names[i]));
            break;
          default:
            break;
        }
      }
      return typesAndNames;
    }

    public enum SAP2000APIUsableTypes
    {
      Point, // cPointObj
      Frame, // cFrameObj
      Beam,
      Column,
      Brace,
      Area,
      Wall,
      Spandrel,
      Pier,
      Floor,
      Grids,
      Links,
      Tendon,
      LoadPattern,
      Model,
      BeamLoading,
      ColumnLoading,
      BraceLoading,
      FrameLoading,
      FloorLoading,
      AreaLoading,
      WallLoading,
      NodeLoading,


      //ColumnResults,
      //BeamResults,
      //BraceResults,
      //PierResults,
      //SpandrelResults,
      //AnalysisResults
    }

    /// <summary>
    /// same as ObjectType in SAP2000 cSelect.GetSelected API function
    /// </summary>
    public enum SAP2000ViewSelectableTypes
    {
      Point = 1,
      Frame = 2,
      Area = 4
    }
  }
}

