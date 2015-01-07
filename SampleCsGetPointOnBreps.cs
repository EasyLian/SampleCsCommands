﻿using System;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace SampleCsCommands
{
  /// <summary>
  /// SampleCsGetPointOnBreps
  /// </summary>
  [System.Runtime.InteropServices.Guid("3dfc74e4-8cbb-4670-b490-0aeafab6fdc7")]
  public class SampleCsGetPointOnBreps : Command
  {
    public SampleCsGetPointOnBreps()
    {
    }

    public override string EnglishName
    {
      get { return "SampleCsGetPointOnBreps"; }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var go = new GetObject();
      go.SetCommandPrompt("Select surfaces and polysurfaces");
      go.GeometryFilter = ObjectType.Surface | ObjectType.PolysrfFilter;
      go.SubObjectSelect = false;
      go.GetMultiple(1, 0);
      if (go.CommandResult() != Result.Success)
        return go.CommandResult();

      var gp = new GetPointOnBreps();
      gp.SetCommandPrompt("Point on surface or polysurface");

      foreach (var obj_ref in go.Objects())
        gp.Breps.Add(obj_ref.Brep());

      gp.Get();
      if (gp.CommandResult() != Result.Success)
        return gp.CommandResult();

      var point = gp.Point();

      // One final calculation
      var closest_point = gp.CalculateClosestPoint(point);
      if (closest_point.IsValid)
      {
        doc.Objects.AddPoint(closest_point);
        doc.Views.Redraw();
      }

      return Result.Success;
    }
  }


  /// <summary>
  /// GetPointOnBreps
  /// </summary>
  class GetPointOnBreps : GetPoint
  {
    public readonly List<Brep> Breps;
    public Point3d ClosestPoint;

    public GetPointOnBreps()
    {
      Breps = new List<Brep>();
      ClosestPoint = Point3d.Unset;
    }

    public Point3d CalculateClosestPoint(Point3d point)
    {
      var closest_point = Point3d.Unset;
      var minimum_distance = Double.MaxValue;
      foreach (var brep in Breps)
      {
        var brep_point = brep.ClosestPoint(point);
        if (brep_point.IsValid)
        {
          double distance = brep_point.DistanceTo(point);
          if (distance < minimum_distance)
          {
            minimum_distance = distance;
            closest_point = brep_point;
          }
        }
      }
      return closest_point;
    }

    protected override void OnMouseMove(GetPointMouseEventArgs e)
    {
      ClosestPoint = CalculateClosestPoint(e.Point);
      base.OnMouseMove(e);
    }

    protected override void OnDynamicDraw(GetPointDrawEventArgs e)
    {
      if (ClosestPoint.IsValid)
        e.Display.DrawPoint(ClosestPoint, AppearanceSettings.DefaultObjectColor);

      // Do not call base class...
      //base.OnDynamicDraw(e);
    }
  }
}
