namespace TPMS.FurnacesBatching
{
    public class TestCases
    {
        public static TestCaseResult Case1()
        {
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
                new SalesOrderLine(
                    "SO-1", "STEEL-A", 10,
                    DateTime.Parse("2025-01-05"), 1,
                    DateTime.Parse("2025-01-03")),

                new SalesOrderLine(
                    "SO-2", "STEEL-A", 8,
                    DateTime.Parse("2025-01-06"), 2,
                    DateTime.Parse("2025-01-07")),

                new SalesOrderLine(
                    "SO-3", "STEEL-A", 6,
                    DateTime.Parse("2025-01-10"), 3,
                    DateTime.Parse("2025-01-20"))
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
            var workOrders = service.Run(salesOrders, materials, resources);

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

            return new TestCaseResult
            {
                TestCaseName = "Case1",
                Resources = resources,
                Materials = materials,
                SalesOrders = salesOrders,
                WorkOrders = workOrders
            };
        }

        public static TestCaseResult Case2()
        {
            Console.WriteLine("=== Auto Batching Case 2 (2 Materials, 6 SO Lines) ===");

            var materials = new Dictionary<string, Material>
            {
                ["STEEL-A"] = new Material
                {
                    MaterialId = "STEEL-A",
                    KgPerUnitFinishWeight = 100,
                    MaterialYieldPercent = 80
                },
                ["STEEL-B"] = new Material
                {
                    MaterialId = "STEEL-B",
                    KgPerUnitFinishWeight = 120,
                    MaterialYieldPercent = 85
                }
            };

            var salesOrders = new List<SalesOrderLine>
            {
                new SalesOrderLine("SO-1", "STEEL-A", 5,  DateTime.Parse("2025-01-05"), 1, DateTime.Parse("2025-01-03")),
                new SalesOrderLine("SO-2", "STEEL-A", 7,  DateTime.Parse("2025-01-06"), 2, DateTime.Parse("2025-01-04")),
                new SalesOrderLine("SO-3", "STEEL-A", 4,  DateTime.Parse("2025-01-08"), 3, DateTime.Parse("2025-01-06")),

                new SalesOrderLine("SO-4", "STEEL-B", 6,  DateTime.Parse("2025-01-07"), 1, DateTime.Parse("2025-01-05")),
                new SalesOrderLine("SO-5", "STEEL-B", 9,  DateTime.Parse("2025-01-09"), 2, DateTime.Parse("2025-01-08")),
                new SalesOrderLine("SO-6", "STEEL-B", 3,  DateTime.Parse("2025-01-12"), 3, DateTime.Parse("2025-01-10"))
            };

            var resources = new List<Resource>
            {
                new Resource
                {
                    ResourceId = "FURNACE-1",
                    IsBatchType = true,
                    MinBatchQty = 800,
                    MaxBatchQty = 2000,
                    NPerCycle = 2,
                    EligibleMaterials = new List<string> { "STEEL-A", "STEEL-B" }
                }
            };

            var service = new AutoBatchingService();
            var workOrders = service.Run(salesOrders, materials, resources);

            Console.WriteLine("\n=== WORK ORDERS ===");
            foreach (var wo in workOrders)
            {
                Console.WriteLine($"WO={wo.WoId}, Resource={wo.ResourceId}, Material={wo.MaterialId}, Qty={wo.TotalQty}");
                foreach (var d in wo.Details)
                    Console.WriteLine($"   -> SO={d.SoLineId}, Qty={d.AllocatedQty}");
            }

            return new TestCaseResult
            {
                TestCaseName = "Case1",
                Resources = resources,
                Materials = materials,
                SalesOrders = salesOrders,
                WorkOrders = workOrders
            };
        }

        public static TestCaseResult Case3()
        {
            Console.WriteLine("=== Auto Batching Case 3 (10 SO Lines, 3 Materials) ===");

            var materials = new Dictionary<string, Material>
            {
                ["STEEL-A"] = new Material { MaterialId = "STEEL-A", KgPerUnitFinishWeight = 100, MaterialYieldPercent = 80 },
                ["STEEL-B"] = new Material { MaterialId = "STEEL-B", KgPerUnitFinishWeight = 110, MaterialYieldPercent = 82 },
                ["ALU-C"] = new Material { MaterialId = "ALU-C", KgPerUnitFinishWeight = 60, MaterialYieldPercent = 90 }
            };

            var salesOrders = new List<SalesOrderLine>
            {
                new SalesOrderLine("SO-1",  "STEEL-A", 6, DateTime.Parse("2025-01-04"), 1, DateTime.Parse("2025-01-02")),
                new SalesOrderLine("SO-2",  "STEEL-A", 8, DateTime.Parse("2025-01-05"), 2, DateTime.Parse("2025-01-03")),
                new SalesOrderLine("SO-3",  "STEEL-A", 4, DateTime.Parse("2025-01-06"), 3, DateTime.Parse("2025-01-04")),

                new SalesOrderLine("SO-4",  "STEEL-B", 7, DateTime.Parse("2025-01-07"), 1, DateTime.Parse("2025-01-05")),
                new SalesOrderLine("SO-5",  "STEEL-B", 5, DateTime.Parse("2025-01-08"), 2, DateTime.Parse("2025-01-06")),
                new SalesOrderLine("SO-6",  "STEEL-B", 9, DateTime.Parse("2025-01-09"), 3, DateTime.Parse("2025-01-07")),

                new SalesOrderLine("SO-7",  "ALU-C",   10, DateTime.Parse("2025-01-06"), 1, DateTime.Parse("2025-01-04")),
                new SalesOrderLine("SO-8",  "ALU-C",   6,  DateTime.Parse("2025-01-07"), 2, DateTime.Parse("2025-01-05")),
                new SalesOrderLine("SO-9",  "ALU-C",   8,  DateTime.Parse("2025-01-10"), 3, DateTime.Parse("2025-01-08")),
                new SalesOrderLine("SO-10", "ALU-C",   4,  DateTime.Parse("2025-01-12"), 4, DateTime.Parse("2025-01-10"))
            };

            var resources = new List<Resource>
            {
                new Resource
                {
                    ResourceId = "FURNACE-STEEL",
                    IsBatchType = true,
                    MinBatchQty = 1000,
                    MaxBatchQty = 2500,
                    NPerCycle = 2,
                    EligibleMaterials = new List<string> { "STEEL-A", "STEEL-B" }
                },
                new Resource
                {
                    ResourceId = "FURNACE-ALU",
                    IsBatchType = true,
                    MinBatchQty = 600,
                    MaxBatchQty = 1500,
                    NPerCycle = 1,
                    EligibleMaterials = new List<string> { "ALU-C" }
                }
            };

            var service = new AutoBatchingService();
            var workOrders = service.Run(salesOrders, materials, resources);

            Console.WriteLine("\n=== WORK ORDERS ===");
            foreach (var wo in workOrders)
            {
                Console.WriteLine($"WO={wo.WoId}, Resource={wo.ResourceId}, Material={wo.MaterialId}, Qty={wo.TotalQty}");
                foreach (var d in wo.Details)
                    Console.WriteLine($"   -> SO={d.SoLineId}, Qty={d.AllocatedQty}");
            }

            return new TestCaseResult
            {
                TestCaseName = "Case1",
                Resources = resources,
                Materials = materials,
                SalesOrders = salesOrders,
                WorkOrders = workOrders
            };
        }

        public static TestCaseResult Case4()
        {
            Console.WriteLine("=== Auto Batching Case 4 (12 SO Lines, High Volume) ===");

            var materials = new Dictionary<string, Material>
            {
                ["STEEL-X"] = new Material { MaterialId = "STEEL-X", KgPerUnitFinishWeight = 95, MaterialYieldPercent = 78 },
                ["STEEL-Y"] = new Material { MaterialId = "STEEL-Y", KgPerUnitFinishWeight = 105, MaterialYieldPercent = 83 }
            };

            var salesOrders = new List<SalesOrderLine>();
            for (int i = 1; i <= 12; i++)
            {
                salesOrders.Add(
                    new SalesOrderLine(
                        $"SO-{i}",
                        i % 2 == 0 ? "STEEL-X" : "STEEL-Y",
                        5 + i,
                        DateTime.Parse("2025-01-05").AddDays(i),
                        i,
                        DateTime.Parse("2025-01-03").AddDays(i - 1)
                    )
                );
            }

            var resources = new List<Resource>
            {
                new Resource
                {
                    ResourceId = "FURNACE-MAIN",
                    IsBatchType = true,
                    MinBatchQty = 1200,
                    MaxBatchQty = 3000,
                    NPerCycle = 3,
                    EligibleMaterials = new List<string> { "STEEL-X", "STEEL-Y" }
                }
            };

            var service = new AutoBatchingService();
            var workOrders = service.Run(salesOrders, materials, resources);

            Console.WriteLine("\n=== WORK ORDERS ===");
            foreach (var wo in workOrders)
            {
                Console.WriteLine($"WO={wo.WoId}, Resource={wo.ResourceId}, Material={wo.MaterialId}, Qty={wo.TotalQty}");
                foreach (var d in wo.Details)
                    Console.WriteLine($"   -> SO={d.SoLineId}, Qty={d.AllocatedQty}");
            }

            return new TestCaseResult
            {
                TestCaseName = "Case1",
                Resources = resources,
                Materials = materials,
                SalesOrders = salesOrders,
                WorkOrders = workOrders
            };
        }

        public static void Main1()
        {

            var report = new AutoBatchingExcelReport();

            TestCaseResult testCaseResult = TestCases.Case1();
            report.AddTestCaseSheet(
                "Case1",
                testCaseResult.Resources,
                testCaseResult.Materials,
                testCaseResult.SalesOrders,
                testCaseResult.WorkOrders);

            testCaseResult = TestCases.Case2();

            report.AddTestCaseSheet(
                "Case2",
                testCaseResult.Resources,
                testCaseResult.Materials,
                testCaseResult.SalesOrders,
                testCaseResult.WorkOrders);

            testCaseResult = TestCases.Case3();

            report.AddTestCaseSheet(
                "Case3",
                testCaseResult.Resources,
                testCaseResult.Materials,
                testCaseResult.SalesOrders,
                testCaseResult.WorkOrders);

            testCaseResult = TestCases.Case4();

            report.AddTestCaseSheet(
                "Case4",
                testCaseResult.Resources,
                testCaseResult.Materials,
                testCaseResult.SalesOrders,
                testCaseResult.WorkOrders);

            report.Save(@"C:\Temp\AutoBatching_Report.xlsx");

        }
    }

    public class TestCaseResult
    {
        public string TestCaseName { get; set; }

        public List<Resource> Resources { get; set; }
        public Dictionary<string, Material> Materials { get; set; }
        public List<SalesOrderLine> SalesOrders { get; set; }
        public List<WorkOrder> WorkOrders { get; set; }
    }
}
