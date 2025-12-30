using ClosedXML.Excel;

namespace TPMS.FurnacesBatching
{
    public class AutoBatchingExcelReport
    {
        private readonly XLWorkbook _workbook = new();

        public void AddTestCaseSheet(
            string testCaseName,
            List<Resource> resources,
            Dictionary<string, Material> materials,
            List<SalesOrderLine> salesOrders,
            List<WorkOrder> workOrders)
        {
            var ws = _workbook.Worksheets.Add(testCaseName);
            int row = 1;

            row = WriteResources(ws, resources, row);
            row += 2;

            row = WriteMaterials(ws, materials.Values.ToList(), row);
            row += 2;

            row = WriteSalesOrders(ws, salesOrders, row);
            row += 2;

            WriteWorkOrders(ws, workOrders, row);

            ws.Columns().AdjustToContents();
        }

        public void Save(string path) => _workbook.SaveAs(path);

        private int WriteResources(IXLWorksheet ws, List<Resource> resources, int row)
        {
            ws.Cell(row++, 1).Value = "RESOURCES";
            ws.Range(row - 1, 1, row - 1, 6).Merge().Style.Font.Bold = true;

            string[] headers =
            {
            "ResourceId", "IsBatch", "MinBatchQty", "MaxBatchQty",
            "NPerCycle", "EligibleMaterials"
        };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            row++;

            foreach (var r in resources)
            {
                ws.Cell(row, 1).Value = r.ResourceId;
                ws.Cell(row, 2).Value = r.IsBatchType;
                ws.Cell(row, 3).Value = r.MinBatchQty;
                ws.Cell(row, 4).Value = r.MaxBatchQty;
                ws.Cell(row, 5).Value = r.NPerCycle;
                ws.Cell(row, 6).Value = string.Join(",", r.EligibleMaterials);
                row++;
            }

            return row;
        }


        private int WriteMaterials(IXLWorksheet ws, List<Material> materials, int row)
        {
            ws.Cell(row++, 1).Value = "MATERIALS";
            ws.Range(row - 1, 1, row - 1, 4).Merge().Style.Font.Bold = true;

            string[] headers = { "MaterialId", "KgPerUnit", "Yield %" };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            row++;

            foreach (var m in materials)
            {
                ws.Cell(row, 1).Value = m.MaterialId;
                ws.Cell(row, 2).Value = m.KgPerUnitFinishWeight;
                ws.Cell(row, 3).Value = m.MaterialYieldPercent;
                row++;
            }

            return row;
        }

        private int WriteSalesOrders(IXLWorksheet ws, List<SalesOrderLine> salesOrders, int row)
        {
            ws.Cell(row++, 1).Value = "SALES ORDER LINES";
            ws.Range(row - 1, 1, row - 1, 6).Merge().Style.Font.Bold = true;

            string[] headers =
            {
            "SO Line", "Material", "Qty", "DueDate",
            "Priority", "FullKitDate"
        };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            row++;

            foreach (var so in salesOrders)
            {
                ws.Cell(row, 1).Value = so.SoLineId;
                ws.Cell(row, 2).Value = so.MaterialId;
                ws.Cell(row, 3).Value = so.RemainingQuantity;
                ws.Cell(row, 4).Value = "";
                ws.Cell(row, 5).Value = so.PriorityNo;
                ws.Cell(row, 6).Value = so.EarliestAvailableFullkitDate;
                row++;
            }

            return row;
        }

        private void WriteWorkOrders(IXLWorksheet ws, List<WorkOrder> workOrders, int row)
        {
            ws.Cell(row++, 1).Value = "WORK ORDERS";
            ws.Range(row - 1, 1, row - 1, 6).Merge().Style.Font.Bold = true;

            string[] headers =
            {
            "WO", "Resource", "Material",
            "Total Qty", "Status", "FullKitDate"
        };

            for (int i = 0; i < headers.Length; i++)
                ws.Cell(row, i + 1).Value = headers[i];

            ws.Range(row, 1, row, headers.Length).Style.Font.Bold = true;
            row++;

            foreach (var wo in workOrders)
            {
                ws.Cell(row, 1).Value = wo.WoId;
                ws.Cell(row, 2).Value = wo.ResourceId;
                ws.Cell(row, 3).Value = wo.MaterialId;
                ws.Cell(row, 4).Value = wo.TotalQty;
                ws.Cell(row, 5).Value = wo.Status;
                ws.Cell(row, 6).Value = wo.BatchEarliestFullkitDate;
                row++;

                foreach (var d in wo.Details)
                {
                    ws.Cell(row, 2).Value = "→ " + d.SoLineId;
                    ws.Cell(row, 4).Value = d.AllocatedQty;
                    row++;
                }
            }
        }
    }

}

