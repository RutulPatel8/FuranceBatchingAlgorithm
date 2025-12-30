namespace TPMS.FurnacesBatching
{
    // =====================================================
    // DOMAIN MODELS
    // =====================================================

    public record SalesOrderLine(
        string SoLineId,
        string MaterialId,
        int RemainingQuantity,
        DateTime OriginalCommittedTimeStamp,
        int PriorityNo,
        DateTime EarliestAvailableFullkitDate
    );

    public class Material
    {
        public string MaterialId { get; set; }
        public double KgPerUnitFinishWeight { get; set; }
        public double MaterialYieldPercent { get; set; }
    }

    public class Resource
    {
        public string ResourceId { get; set; }
        public bool IsBatchType { get; set; }
        public int MinBatchQty { get; set; }
        public int MaxBatchQty { get; set; }
        public int NPerCycle { get; set; }
        public List<string> EligibleMaterials { get; set; } = new();
    }

    // =====================================================
    // INTERNAL BATCHING MODELS
    // =====================================================

    public class Demand
    {
        public string SoLineId;
        public string MaterialId;
        public int BatchQtyKg;
        public DateTime FullKitDate;
        public int Priority;
    }

    public class BatchCandidate
    {
        public string MaterialId;
        public List<Demand> Demands = new();
        public int TotalQty => Demands.Sum(d => d.BatchQtyKg);
    }

    // =====================================================
    // WORK ORDER MODELS
    // =====================================================
    public class WorkOrder
    {
        public string WoId;
        public string ResourceId;
        public string MaterialId;
        public int TotalQty;
        public string Status;
        public DateTime BatchEarliestFullkitDate;
        public List<WorkOrderDetail> Details = new();
    }

    public class WorkOrderDetail
    {
        public string WoId;
        public string SoLineId;
        public int AllocatedQty;
    }
}
