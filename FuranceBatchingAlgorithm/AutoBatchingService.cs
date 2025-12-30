namespace TPMS.FurnacesBatching
{
    // =====================================================
    // AUTO-BATCHING SERVICE
    // =====================================================
    class AutoBatchingService
    {
        public List<WorkOrder> Run(
            List<SalesOrderLine> soLines,
            Dictionary<string, Material> materials,
            List<Resource> resources)
        {
            // 1. Normalize demand
            var demands = soLines
                .OrderBy(s => s.MaterialId)
                .ThenBy(s => s.PriorityNo)
                .ThenBy(s => s.OriginalCommittedTimeStamp)
                .ThenBy(s => s.SoLineId)
                .Select(so => Normalize(so, materials[so.MaterialId]))
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

                // 3. Build batches (physics only)
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

        Demand Normalize(SalesOrderLine so, Material mat)
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
                FullKitDate = so.EarliestAvailableFullkitDate,
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
}
