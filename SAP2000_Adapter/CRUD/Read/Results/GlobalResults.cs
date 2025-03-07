/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BH.oM.Structure.Results;
using BH.oM.Common;
using BH.oM.Structure.Elements;
using BH.Engine.SAP2000;
using BH.oM.Structure.Loads;
using BH.oM.Structure.Requests;
using BH.oM.Adapters.SAP2000;
using BH.oM.Geometry;
using BH.Engine.Geometry;
/*using SAP2000v19;*/
using BH.oM.Adapter;
using SAP2000v1;

namespace BH.Adapter.SAP2000
{
    /***************************************************/
    /**** Public method - Read override             ****/
    /***************************************************/

    public IEnumerable<IResult> ReadResults(GlobalResultRequest request,
                                            ActionConfig actionConfig = null)
    {
        CheckAndSetUpCases(request);

        switch (request.ResultType)
        {
            case GlobalResultType.Reactions:
                return GetGlobalReactions();
            case GlobalResultType.ModalDynamics:
                return GetModalParticipationMassRatios();
            default:
                Engine.Reflection.Compute.RecordError("Result extraction of type " + request.ResultType + " is not yet supported");
                return new List<IResult>();
        }
    }

    /***************************************************/
    /**** Private method - Extraction methods       ****/
    /***************************************************/

    private List<GlobalReactions> GetGlobalReactions()
    {
        List<GlobalReactions> globalReactions = new List<GlobalReactions>();

        int resultCount = 0;
        string[] loadcaseNames = null;
        string[] stepType = null;
        int[] stepNum = 0;

        double[] fx = null;
        double[] fy = null;
        double[] fz = null;
        double[] mx = null;
        double[] my = null;
        double[] mz = null;
        double gx = 0;
        double gy = 0;
        double gz = 0;

        int ret = m_model.Results.BaseReact(ref resultCount,
                                            ref loadcaseNames,
                                            ref stepType,
                                            ref stepNum,
                                            ref fx,
                                            ref fy,
                                            ref fz,
                                            ref mx,
                                            ref my,
                                            ref mz,
                                            ref gx,
                                            ref gy,
                                            ref gz);

        for (int i; i < resultCount; i++)
        {
            GlobalReactions g = new GlobalReactions()
            {
                ResultCase = loadcaseNames[i],
                FX = fx[i],
                FY = fy[i],
                FZ = fz[i],
                MX = mx[i],
                MY = my[i],
                MZ = mz[i],
                TimeStep = stepNum[i]
            };

            globalReactions.Add(g);
        }

        return globalReactions;
    }

    /***************************************************/

    private List<ModalDynamics> GetModalParticipationMassRatios()
    {
        List<ModalDynamics> partRatios = new List<ModalDynamics>();

        int resultCount = 0;
        string[] loadcaseNames = null;
        string[] stepType = null;
        int[] stepNum = null;
        double[] period = null;
        double[] ux = null;
        double[] uy = null;
        double[] uz = null;
        double[] sumUx = null;
        double[] sumUy = null;
        double[] sumUz = null;
        double[] rx = null;
        double[] ry = null;
        double[] rz = null;
        double[] sumRx = null;
        double[] sumRy = null;
        double[] sumRz = null;

        int ret = m_model.Results.ModalParticipatingMassRatios(ref resultCount,
                                                               ref loadcaseNames,
                                                               ref stepType,
                                                               ref stepNum,
                                                               ref period,
                                                               ref ux,
                                                               ref uy,
                                                               ref uz,
                                                               ref sumUx,
                                                               ref sumUy,
                                                               ref sumUz,
                                                               ref rx,
                                                               ref ry,
                                                               ref rz,
                                                               ref sumRx,
                                                               ref sumRy,
                                                               ref sumRz);

        if (ret != 0) Engine.Reflection.Compute.RecordError("Could not extract Modal information.");

        string previousModalCase = "";
        int modeNumber = 1; //makes up for stepnumber always = 0
        for (int i = 0; i < resultCount; i++)
        {
            if (loadcaseNames[i] != previousModalCase)
                modeNumber = 1;

            ModalDynamics mod = new ModalDynamics()
            {
                ResultCase = loadcaseNames[i],
                ModeNumber = modeNumber,
                Frequency = 1 / period[i],
                MassRatioX = ux[i],
                MassRatioY = uy[i],
                MassRatioZ = uz[i],
                InertiaRatioX = rx[i],
                InertiaRatioY = ry[i],
                InertiaRatioZ = rz[i]
            };

            modeNumber += 1;
            previousModalCase = loadcaseNames[i];

            partRatios.Add(mod);
        }

        return partRatios;
    }

        /***************************************************/
}


