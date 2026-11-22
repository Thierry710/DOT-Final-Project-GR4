using TraceLinq.Data.Models;

namespace TraceLinq.Data;

// Rules for which batch status can change to which.
public static class BatchWorkflow
{
    // Returns true if a batch is allowed to move from one status to another.
    public static bool CanMove(BatchStatus from, BatchStatus to)
    {
        if (from == BatchStatus.Registered)
            return to == BatchStatus.Passed || to == BatchStatus.Failed;

        if (from == BatchStatus.Passed)
            return to == BatchStatus.InStorage;

        // Failed and InStorage batches cannot be changed by the inspector.
        return false;
    }

    // Only batches that passed inspection can go to the warehouse.
    public static bool CanStore(BatchStatus status)
    {
        return status == BatchStatus.Passed;
    }
}
