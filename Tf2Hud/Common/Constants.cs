using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Lumina.Excel.GeneratedSheets;

namespace Tf2Hud.Common;

public abstract class Constants
{
    public static readonly IDictionary<uint, ClassJob> CombatJobs;

    static Constants()
    {
        CombatJobs = InitCombatJobs();
    }

    private static IDictionary<uint, ClassJob> InitCombatJobs()
    {
        // A combat job is one that has a JobIndex.
        if (Service.DataManager == null) throw new ApplicationException("DataManager not initialized!");

        var classJobSheet = Service.DataManager.GetExcelSheet<ClassJob>();
        if (classJobSheet == null) throw new ApplicationException("ClassJob sheet unavailable!");
        var jobList = new List<ClassJob>();
        for (var i = 0u; i < classJobSheet.RowCount; i++)
        {
            var classJob = classJobSheet.GetRow(i);
            if (classJob is null || classJob.JobIndex is 0) continue;
            jobList.Add(classJob);
        }

        return jobList.ToImmutableSortedDictionary(j => j.RowId, j => j);
    }
}
