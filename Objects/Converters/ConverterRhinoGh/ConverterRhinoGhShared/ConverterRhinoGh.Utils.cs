﻿using Grasshopper.Kernel.Types;
using Objects.Geometry;
using Objects.Primitive;
using Objects.Other;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Speckle.Core.Kits;
using Speckle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Objects.Converter.RhinoGh
{
  public partial class ConverterRhinoGh
  {
    private RenderMaterial GetMaterial(RhinoObject o)
    {
      var material = o.GetMaterial(true);
      var renderMaterial = new RenderMaterial();

      // If it's a default material use the display color.
      if (!material.HasId && RhinoContext.HasDocument)
      {
        renderMaterial.diffuse = o.Attributes.DrawColor(RhinoContext.InnerDocument).ToArgb();
        return renderMaterial;
      } else if (!material.HasId)
      {
         return renderMaterial;
      }

      // Otherwise, extract what properties we can. 
      renderMaterial.name = material.Name;
      renderMaterial.diffuse = material.DiffuseColor.ToArgb();
      renderMaterial.emissive = material.EmissionColor.ToArgb();
      renderMaterial.opacity = 1 - material.Transparency;
      renderMaterial.metalness = material.Reflectivity;
      
      if (material.Name != null && material.Name.ToLower().Contains("glass") && renderMaterial.opacity == 0)
      {
        renderMaterial.opacity = 0.3;
      }

      return renderMaterial;
    }

    private DisplayStyle GetStyle(RhinoObject o)
    {
            if (!RhinoContext.HasDocument) throw new Exception("No Rhino Document in this context");
            var doc = RhinoContext.InnerDocument;

      var att = o.Attributes;
      var style = new DisplayStyle();

      // color
      style.color = att.DrawColor(doc).ToArgb();

      // linetype
      var lineType = doc.Linetypes[att.LinetypeIndex];
      if (lineType.HasName)
        style.linetype = lineType.Name;

      // lineweight
      style.lineweight = att.PlotWeight;

      return style;
    }

    private string GetSchema(RhinoObject obj, out string[] args)
    {
      args = null;

      // user string has format "DirectShape{[family], [type]}" if it is a directshape conversion
      // user string has format "AdaptiveComponent{[family], [type]}" if it is an adaptive component conversion
      // otherwise, it is just the schema type name
      string schema = obj.Attributes.GetUserString(SpeckleSchemaKey);

      if (schema == null)
        return null;

      string[] parsedSchema = schema.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
      if (parsedSchema.Length > 2) // there is incorrect formatting in the schema string!
        return null;
      if (parsedSchema.Length == 2)
        args = parsedSchema[1].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim()).ToArray();
      return parsedSchema[0].Trim();
    }

    private string GetCommitInfo()
    {
      var segments = RhinoContext.InnerDocument.Notes.Split(new string[] { "%%%" }, StringSplitOptions.None).ToList();
      return segments.Count > 1 ? segments[1] : "Unknown commit";
    }

        #region Units

    /// <summary>
    /// Computes the Speckle Units of the current Document. The Rhino document is passed as a reference, so it will always be up to date.
    /// </summary>    
    public string ModelUnits => RhinoContext.ModelUnits;

    private void SetUnits(Base geom)
    {
      geom["units"] = ModelUnits;
    }

    private double ScaleToNative(double value, string units)
    {
      var f = Units.GetConversionFactor(units, ModelUnits);
      return value * f;
    }


    #endregion

    #region Layers
    public static Layer GetLayer(RhinoDoc doc, string path, out int index, bool MakeIfNull = false)
    {
      index = doc.Layers.FindByFullPath(path, RhinoMath.UnsetIntIndex);
      Layer layer = doc.Layers.FindIndex(index);
      if (layer == null && MakeIfNull)
      {
        var layerNames = path.Split(new string[] { Layer.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

        Layer parent = null;
        string currentLayerPath = string.Empty;
        Layer currentLayer = null;
        for (int i = 0; i < layerNames.Length; i++)
        {
          currentLayerPath = (i == 0) ? layerNames[i] : $"{currentLayerPath}{Layer.PathSeparator}{layerNames[i]}";
          currentLayer = GetLayer(doc, currentLayerPath, out index);
          if (currentLayer == null)
            currentLayer = MakeLayer(doc, layerNames[i], out index, parent);
          if (currentLayer == null)
            break;
          parent = currentLayer;
        }
        layer = currentLayer;
      }
      return layer;
    }

    private static Layer MakeLayer(RhinoDoc doc, string name, out int index, Layer parentLayer = null)
    {
      index = -1;
      Layer newLayer = new Layer() { Color = System.Drawing.Color.White, Name = name };
      if (parentLayer != null)
        newLayer.ParentLayerId = parentLayer.Id;
      int newIndex = doc.Layers.Add(newLayer);
      if (newIndex < 0)
        return null;
      else
      {
        index = newIndex;
        return doc.Layers.FindIndex(newIndex);
      }
    }
    #endregion
  }
}
