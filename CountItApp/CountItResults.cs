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
///

using AdvObjects.Bundle;
using System.Collections.Generic;

namespace CountIt
{
    /// <summary>
    /// CountItResults is used to save the count result into Json file
    /// </summary>
    /// 


    internal class CountItResults
    {
        public List<AdvFloor> Floors { get; set; } = new List<AdvFloor>();

        public List<AdvDoor> Doors { get; set; } = new List<AdvDoor>();

        public List<AdvWall> Walls { get; set; } = new List<AdvWall>();

    }

}
