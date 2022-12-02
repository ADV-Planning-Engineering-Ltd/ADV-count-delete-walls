﻿/////////////////////////////////////////////////////////////////////
// Copyright 2022 Autodesk Inc
// Written by Develope Advocacy and Support
//

// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using System.Collections.Generic;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;

namespace DeleteElements
{
    /// <summary>
    /// Delete elements depends on the input parameters
    /// </summary>
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   public class DeleteElementsApp : IExternalDBApplication
   {
      public ExternalDBApplicationResult OnStartup(Autodesk.Revit.ApplicationServices.ControlledApplication app)
      {
         DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
         return ExternalDBApplicationResult.Succeeded;
      }

      public ExternalDBApplicationResult OnShutdown(Autodesk.Revit.ApplicationServices.ControlledApplication app)
      {
         return ExternalDBApplicationResult.Succeeded;
      }

      public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
      {
         e.Succeeded = true;
         DeleteAllElements(e.DesignAutomationData);
      }

        /// <summary>
        /// delete elements depends on the input params.json file
        /// </summary>
        /// <param name="data"></param>
        public static void DeleteAllElements(DesignAutomationData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            Application rvtApp = data.RevitApp;
            if (rvtApp == null) throw new InvalidDataException(nameof(rvtApp));

            string modelPath = data.FilePath;
            if (String.IsNullOrWhiteSpace(modelPath)) throw new InvalidDataException(nameof(modelPath));

            // If the input revit model passed is a workshared revit file then by default the Design Automation 
            // bridge will open the model detached from central, opting DetachAndPreserveWorsets option.
            // Non-worshared revit file will be load as is.
            Document doc = data.RevitDoc;
            if (doc == null) throw new InvalidOperationException("Could not open document.");


            // For CountIt workItem: If RvtParameters is null, count all types
            DeleteElementsParams deleteElementsParams = DeleteElementsParams.Parse("params.json");

            using (Transaction transaction = new Transaction(doc))
            {
                transaction.Start("Delete Elements");
                if (deleteElementsParams.walls)
                {
                    FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Wall));
                    doc.Delete(col.ToElementIds());

                }
                if (deleteElementsParams.floors)
                {
                    FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Floor));
                    doc.Delete(col.ToElementIds());
                }
                if (deleteElementsParams.doors)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    ICollection<ElementId> collection = collector.OfClass(typeof(FamilyInstance))
                                                       .OfCategory(BuiltInCategory.OST_Doors)
                                                       .ToElementIds();
                    doc.Delete(collection);
                }
                if (deleteElementsParams.windows)
                {
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    ICollection<ElementId> collection = collector.OfClass(typeof(FamilyInstance))
                                                       .OfCategory(BuiltInCategory.OST_Windows)
                                                       .ToElementIds();
                    doc.Delete(collection);
                }
                transaction.Commit();
            }

            ModelPath path = ModelPathUtils.ConvertUserVisiblePathToModelPath("result.rvt");
            // If a worshared file is opened as a part of this addin then the new file will be 
            // saved as central.
            SaveAsOptions opts = new SaveAsOptions();
            if (doc.IsWorkshared)
            {
                opts.SetWorksharingOptions(new WorksharingSaveAsOptions { SaveAsCentral = true });
                WorksharingUtils.RelinquishOwnership(doc, new RelinquishOptions(true), new TransactWithCentralOptions());
            }
            doc.SaveAs(path, new SaveAsOptions());
        }
   }
}
