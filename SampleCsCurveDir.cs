﻿using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace SampleCsCommands
{
  /// <summary>
  /// SampleCsCurveDir command
  /// </summary>
  [System.Runtime.InteropServices.Guid("08d7d1bf-c7b7-4508-9de4-00a4aedb289e")]
  public class SampleCsCurveDir : Command
  {
    public SampleCsCurveDir()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsCurveDir"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      GetObject go = new GetObject();
      go.SetCommandPrompt("Select curves for direction display");
      go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      go.SubObjectSelect = false;
      go.GetMultiple(1, 0);
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      doc.Objects.UnselectAll();

      SampleCsCurveDirConduit conduit = new SampleCsCurveDirConduit(go);
      conduit.Enabled = true;
      doc.Views.Redraw();

      GetString gs = new GetString();
      gs.SetCommandPrompt("Press <Enter> to continue");
      gs.AcceptNothing(true);
      gs.Get();

      conduit.Enabled = false;

      if (go.ObjectsWerePreselected)
      {
        for (int i = 0; i < go.ObjectCount; i++)
          go.Object(i).Object().Select(true);
      }

      doc.Views.Redraw();

      return Result.Success;
    }
  }

  /// <summary>
  /// SampleCsCurveDirConduit display conduit
  /// </summary>
  public class SampleCsCurveDirConduit : Rhino.Display.DisplayConduit
  {
    private const int CURVE_ARROW_COUNT = 3;
    private List<Curve> m_curves;

    public SampleCsCurveDirConduit(GetObject go)
    {
      m_curves = new List<Curve>(go.ObjectCount);
     
      for (int i = 0; i < go.ObjectCount; i++)
      {
        Curve curve = go.Object(i).Curve();
        if (null != curve)
          m_curves.Add(curve);
      }
    }

    protected override void DrawOverlay(DrawEventArgs e)
    {
      System.Drawing.Color color = Rhino.ApplicationSettings.AppearanceSettings.TrackingColor;
      for (int i = 0; i < m_curves.Count; i++)
      {
        Curve curve = m_curves[i];
        if (null == curve)
          continue;

        for (double j = 0.0; j < CURVE_ARROW_COUNT; j += 1.0 )
        {
          double d = j / (CURVE_ARROW_COUNT - 1.0);
          double t = curve.Domain.ParameterAt(d);
          Point3d p = curve.PointAt(t);
          Vector3d dir = curve.TangentAt(t);
          e.Display.DrawDirectionArrow(p, dir, color);
        }

      }
    }
  }
}
