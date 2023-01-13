// /******************************************************************************
//  * File: QCHTTimeProfiler.cs
//  * Copyright (c) 2021 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace QCHT.Core
{
    public static class QCHTTimeProfiler
    {
        struct ProfilingData {
            public double start;
            public double minTime;
            public double maxTime;
            public double totalTime;
            public int updateCount;
            public double averageTimePerCall;
            public double averageTimePerFrame;
        }
        
        private static Dictionary<string, ProfilingData> data = new Dictionary<string, ProfilingData>();
        private static int maxCount = 30;
        private static int count = 0;
        
        public static void StartTimeProfiling(string id)
        {
            ProfilingData profilingData = GetData(id);
            profilingData.start = Time.realtimeSinceStartupAsDouble;
            data[id] = profilingData;
        }

        public static void StopTimeProfiling(string id)
        {
            ProfilingData profData = GetData(id);
            double time = Time.realtimeSinceStartupAsDouble - profData.start;
            if (time < profData.minTime) profData.minTime = time;
            if (time > profData.maxTime) profData.maxTime = time;
            profData.totalTime += time;
            profData.updateCount++;
            data[id] = profData;
        }

        public static void DumpProfilingData()
        {
            if (++count % maxCount == 0)
            {
                foreach (var kv in data)
                {
                    string id = kv.Key;
                    ProfilingData profilingData = kv.Value;

                    profilingData.averageTimePerCall =
                        profilingData.updateCount > 0 ? profilingData.totalTime / profilingData.updateCount : 0;
                    profilingData.averageTimePerFrame = profilingData.totalTime / maxCount;

                    if (profilingData.updateCount > 0)
                    {
                        Debug.LogWarning(
                            $"{id}: mean={profilingData.averageTimePerCall:0.00}ms frame={profilingData.averageTimePerFrame:0.00}ms min={profilingData.minTime:0.00}ms max={profilingData.maxTime:0.00}ms");
                    }

                    profilingData.minTime = 0;
                    profilingData.maxTime = 0;
                    profilingData.totalTime = 0;
                    profilingData.updateCount = 0;
                }
            }
        }
        
        private static ProfilingData GetData(string id)
        {
            ProfilingData profilingData;
            if (!data.ContainsKey(id))
            {
                profilingData = new ProfilingData();
            }
            else
            {
                profilingData = data[id];
            }
            return profilingData;
        }
    }
}
