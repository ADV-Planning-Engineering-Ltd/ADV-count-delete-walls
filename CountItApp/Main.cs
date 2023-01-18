/////////////////////////////////////////////////////////////////////
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using DesignAutomationFramework;
using Newtonsoft.Json;
using AdvObjects.Bundle;


namespace CountIt
{
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class CountIt : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication app)
        {
            DesignAutomationBridge.DesignAutomationReadyEvent += HandleDesignAutomationReadyEvent;
            return ExternalDBApplicationResult.Succeeded;
        }

         public ExternalDBApplicationResult OnShutdown(ControlledApplication app)
         {
            return ExternalDBApplicationResult.Succeeded;
         }

         public void HandleDesignAutomationReadyEvent(object sender, DesignAutomationReadyEventArgs e)
         {  
            e.Succeeded = CountElementsInModel(e.DesignAutomationData.RevitApp, e.DesignAutomationData.FilePath, e.DesignAutomationData.RevitDoc);
      }

        internal static List<Document> GetHostAndLinkDocuments(Document revitDoc)
        {
            List<Document> docList = new List<Document>();
            docList.Add(revitDoc);

            // Find RevitLinkInstance documents
            FilteredElementCollector elemCollector = new FilteredElementCollector(revitDoc);
            elemCollector.OfClass(typeof(RevitLinkInstance));
            foreach (Element curElem in elemCollector)
            {
                RevitLinkInstance revitLinkInstance = curElem as RevitLinkInstance;
                if (null == revitLinkInstance)
                   continue;

                Document curDoc = revitLinkInstance.GetLinkDocument();
                if (null == curDoc) // Link is unloaded.
                   continue;
                
                // When one linked document has more than one RevitLinkInstance in the
                // host document, then 'docList' will contain the linked document multiple times.

                docList.Add(curDoc);
            }

            return docList;
        }

        /// <summary>
        /// Count the element in each file
        /// </summary>
        /// <param name="revitDoc"></param>
        /// <param name="countItParams"></param>
        /// <param name="results"></param>
        internal static void CountElements(Document revitDoc, CountItParams countItParams, ref CountItResults results)
        {
            if (countItParams.walls)
            {
                FilteredElementCollector elemCollector = new FilteredElementCollector(revitDoc);
                elemCollector.OfClass(typeof(Wall));
                var adskWalls = elemCollector.ToElements().ToList();

                foreach(var wall in adskWalls)
                {
                    var returnWall = new AdvWall()
                    {
                        Property1 = wall.Name,
                        Property2 = wall.Pinned.ToString(),
                        Property3 = wall.WorksetId.ToString(),
                        Property4 = null,
                        Property5 = string.Empty,
                        Property6 = wall.LevelId.ToString(),     
                    };
                    results.Walls.Add(returnWall);
                }
            }

            if (countItParams.floors)
            {
                FilteredElementCollector elemCollector = new FilteredElementCollector(revitDoc);
                elemCollector.OfClass(typeof(Floor));
                var adskFloors = elemCollector.ToElements().ToList();

                foreach (var floor in adskFloors)
                {
                    var returnFloor = new AdvFloor()
                    {
                        Property1 = floor.Name,
                        Property2 = floor.Pinned.ToString(),
                        Property3 = floor.WorksetId.ToString(),
                        Property4 = floor.ViewSpecific.ToString(),
                        Property5 = floor.UniqueId,
                        Property6 = floor.LevelId.ToString(),
                        Property7 = null,
                        Property8 = string.Empty,
                    };
                    results.Floors.Add(returnFloor);
                }
            }

            if (countItParams.doors)
            {
                FilteredElementCollector collector = new FilteredElementCollector(revitDoc);
                ICollection<Element> collection = collector.OfClass(typeof(FamilyInstance))
                                                   .OfCategory(BuiltInCategory.OST_Doors)
                                                   .ToElements();

                var adskDoors = collection.ToList();

                foreach (var door in adskDoors)
                {
                    var returnDoor = new AdvDoor()
                    {
                        Property1 = door.Name,
                        Property2 = door.Pinned.ToString(),
                        Property3 = string.Empty,
                        //Property4 = null Property is not set to test if possible

                    };
                    results.Doors.Add(returnDoor);
                }
            }

        }

        /// <summary>
        /// count the elements depends on the input parameter in params.json
        /// </summary>
        /// <param name="rvtApp"></param>
        /// <param name="inputModelPath"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static bool CountElementsInModel(Application rvtApp, string inputModelPath, Document doc)
        {
            if (rvtApp == null)
               return false;

            if (!File.Exists(inputModelPath))
               return false;
            
            if (doc == null)
               return false;
            
            // For CountIt workItem: If RvtParameters is null, count all types
            CountItParams countItParams = CountItParams.Parse("params.json");
            CountItResults results = new CountItResults();

            List<Document> allDocs = GetHostAndLinkDocuments(doc);
            foreach(Document curDoc in allDocs)
            {
               CountElements(curDoc, countItParams, ref results);
            }
            
            using (StreamWriter sw = File.CreateText("result.txt"))
            {
               sw.WriteLine(JsonConvert.SerializeObject(results));
               sw.Close();
            }

            return true;
        }    
    }
}
