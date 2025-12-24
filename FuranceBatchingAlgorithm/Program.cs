
    Console.WriteLine("=== Auto Batching (Greedy v1) ===");
    
    // ---------------------------
    // SAMPLE INPUT DATA
    // ---------------------------
    
    var materials = new Dictionary<string, Material>
            {
                ["STEEL-A"] = new Material
                {
                    MaterialId = "STEEL-A",
                    KgPerUnitFinishWeight = 100,
                    MaterialYieldPercent = 80
                }
            };
    
    var salesOrders = new List<SalesOrderLine>
            {
                new SalesOrderLine("SO-1", "STEEL-A", 10, DateTime.Parse("2025-01-05"), 1),
                new SalesOrderLine("SO-2", "STEEL-A", 8,  DateTime.Parse("2025-01-06"), 2),
                new SalesOrderLine("SO-3", "STEEL-A", 6,  DateTime.Parse("2025-01-10"), 3)
            };
    
    var fullKitDates = new Dictionary<string, DateTime>
            {
                ["SO-1"] = DateTime.Parse("2025-01-03"),
                ["SO-2"] = DateTime.Parse("2025-01-07"),
                ["SO-3"] = DateTime.Parse("2025-01-20")
            };
    
    var resources = new List<Resource>
            {
                new Resource
                {
                    ResourceId = "FURNACE-1",
                    IsBatchType = true,
                    MinBatchQty = 1000,
                    MaxBatchQty = 2000,
                    NPerCycle = 2,
                    EligibleMaterials = new List<string> { "STEEL-A" }
                },
                new Resource
                {
                    ResourceId = "FURNACE-2",
                    IsBatchType = true,
                    MinBatchQty = 1000,
                    MaxBatchQty = 2000,
                    NPerCycle = 1,
                    EligibleMaterials = new List<string> { "STEEL-A" }
                }
            };
    
    // ---------------------------
    // RUN AUTO-BATCHING
    // ---------------------------
    
    var service = new AutoBatchingService();
    var workOrders = service.Run(
        salesOrders,
        materials,
        fullKitDates,
        resources
    );
    
    // ---------------------------
    // OUTPUT
    // ---------------------------
    
    Console.WriteLine("\n=== WORK ORDERS ===");
    foreach (var wo in workOrders)
    {
        Console.WriteLine(
            $"WO={wo.WoId}, Resource={wo.ResourceId}, " +
            $"Material={wo.MaterialId}, Qty={wo.TotalQty}, " +
            $"Status={wo.Status}, FK={wo.BatchEarliestFullkitDate:d}"
        );
    
        foreach (var d in wo.Details)
        {
            Console.WriteLine($"   -> SO={d.SoLineId}, Qty={d.AllocatedQty}");
        }
    }


    // =====================================================
    // DOMAIN MODELS
    // =====================================================

    record SalesOrderLine(
        string SoLineId,
        string MaterialId,
        int RemainingQuantity,
        DateTime OriginalCommittedTimeStamp,
        int PriorityNo
    );

    class Material
    {
        public string MaterialId { get; set; }
        public double KgPerUnitFinishWeight { get; set; }
        public double MaterialYieldPercent { get; set; }
    }

    class Resource
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

    class Demand
    {
        public string SoLineId;
        public string MaterialId;
        public int BatchQtyKg;
        public DateTime FullKitDate;
        public int Priority;
    }

    class BatchCandidate
    {
        public string MaterialId;
        public List<Demand> Demands = new();
        public int TotalQty => Demands.Sum(d => d.BatchQtyKg);
    }

    // =====================================================
    // WORK ORDER MODELS
    // =====================================================

    class WorkOrder
    {
        public string WoId;
        public string ResourceId;
        public string MaterialId;
        public int TotalQty;
        public string Status;
        public DateTime BatchEarliestFullkitDate;
        public List<WorkOrderDetail> Details = new();
    }

    class WorkOrderDetail
    {
        public string WoId;
        public string SoLineId;
        public int AllocatedQty;
    }

    // =====================================================
    // AUTO-BATCHING SERVICE
    // =====================================================

    class AutoBatchingService
    {
        public List<WorkOrder> Run(
            List<SalesOrderLine> soLines,
            Dictionary<string, Material> materials,
            Dictionary<string, DateTime> fkDates,
            List<Resource> resources)
        {
            // 1. Normalize demand
            var demands = soLines
                .OrderBy(s => s.MaterialId)
                .ThenBy(s => s.PriorityNo)
                .ThenBy(s => s.OriginalCommittedTimeStamp)
                .ThenBy(s => s.SoLineId)
                .Select(so => Normalize(so, materials[so.MaterialId], fkDates[so.SoLineId]))
                .ToList();

            var workOrders = new List<WorkOrder>();

            // 2. Group by material
            foreach (var materialGroup in demands.GroupBy(d => d.MaterialId))
            {
                var eligibleResources = resources
                    .Where(r => r.IsBatchType &&
                                r.EligibleMaterials.Contains(materialGroup.Key))
                    .OrderBy(r => r.ResourceId)
                    .ToList();

                if (!eligibleResources.Any())
                    continue;

                // 3. Build batches
                var batches = BuildBatches(
                    materialGroup.ToList(),
                    eligibleResources.First().MinBatchQty,
                    eligibleResources.First().MaxBatchQty
                );

                // 4. Assign resources (rotation + nPerCycle)
                var rotator = new ResourceRotator();

                foreach (var batch in batches)
                {
                    var resource = rotator.Assign(eligibleResources);
                    if (resource == null)
                        continue;

                    workOrders.Add(CreateWorkOrder(batch, resource));
                }
            }

            return workOrders;
        }

        // ---------------------------
        // Helpers
        // ---------------------------

        Demand Normalize(SalesOrderLine so, Material mat, DateTime fk)
        {
            var finishKg = so.RemainingQuantity * mat.KgPerUnitFinishWeight;
            var batchKg = (int)Math.Ceiling(
                finishKg / (mat.MaterialYieldPercent / 100.0)
            );

            return new Demand
            {
                SoLineId = so.SoLineId,
                MaterialId = so.MaterialId,
                BatchQtyKg = batchKg,
                FullKitDate = fk,
                Priority = so.PriorityNo
            };
        }

        List<BatchCandidate> BuildBatches(
            List<Demand> demands,
            int minBatch,
            int maxBatch)
        {
            var result = new List<BatchCandidate>();
            var current = new BatchCandidate { MaterialId = demands.First().MaterialId };

            foreach (var d in demands)
            {
                if (current.TotalQty + d.BatchQtyKg > maxBatch)
                {
                    result.Add(current);
                    current = new BatchCandidate { MaterialId = d.MaterialId };
                }

                current.Demands.Add(d);
            }

            if (current.TotalQty > 0)
                result.Add(current);

            return result;
        }

        WorkOrder CreateWorkOrder(BatchCandidate batch, Resource r)
        {
            var wo = new WorkOrder
            {
                WoId = Guid.NewGuid().ToString(),
                ResourceId = r.ResourceId,
                MaterialId = batch.MaterialId,
                TotalQty = batch.TotalQty,
                Status = batch.TotalQty >= r.MinBatchQty
                    ? "Ready"
                    : "OnHold-Underfill",
                BatchEarliestFullkitDate =
                    batch.Demands.Max(d => d.FullKitDate)
            };

            wo.Details = batch.Demands.Select(d =>
                new WorkOrderDetail
                {
                    WoId = wo.WoId,
                    SoLineId = d.SoLineId,
                    AllocatedQty = d.BatchQtyKg
                }).ToList();

            return wo;
        }
    }

    // =====================================================
    // RESOURCE ROTATION + FAIRNESS
    // =====================================================

    class ResourceRotator
    {
        private readonly Dictionary<string, int> _usage = new();
        private int _pointer = 0;

        public Resource Assign(List<Resource> resources)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                var r = resources[_pointer];
                _pointer = (_pointer + 1) % resources.Count;

                if (!_usage.ContainsKey(r.ResourceId))
                    _usage[r.ResourceId] = 0;

                if (_usage[r.ResourceId] < r.NPerCycle)
                {
                    _usage[r.ResourceId]++;
                    return r;
                }
            }

            return null;
        }
    }

