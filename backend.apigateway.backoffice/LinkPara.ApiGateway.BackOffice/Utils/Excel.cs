using System.Data;
using System.Globalization;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace LinkPara.ApiGateway.BackOffice.Utils
{
    public class Excel
    {
        private static readonly Lazy<Excel> Lazy = new(() => new Excel());
        public static Excel Instance => Lazy.Value;

        public byte[] CreateExcelDocument<T>(List<T> sourceList)
        {
            var dataSet = new DataSet();
            dataSet.Tables.Add(ListToDataTable(sourceList));
            return CreateExcelDocumentAsStream(dataSet);
        }

        private static byte[] CreateExcelDocumentAsStream(DataSet dataSet)
        {
            var stream = new MemoryStream();
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
            {
                WriteExcelFile(dataSet, document);
            }
            stream.Flush();
            stream.Position = 0;

            var data = new byte[stream.Length];

            stream.Read(data, 0, data.Length);
            stream.Close();
            return data;
        }

        private static DataTable ListToDataTable<T>(List<T> sourceList)
        {
            var dataTable = new DataTable();
            if (sourceList == null)
            {
                return dataTable;
            }
            var properties = typeof(T).GetProperties();

            foreach (var propertyInfo in properties)
            {
                dataTable.Columns.Add(new DataColumn(propertyInfo.Name, GetNullableType(propertyInfo.PropertyType)));
            }

            foreach (var item in sourceList)
            {
                var row = dataTable.NewRow();

                foreach (var propertyInfo in properties)
                {
                    if (!IsNullableType(propertyInfo.PropertyType))
                    {
                        row[propertyInfo.Name] = propertyInfo.GetValue(item, null);
                    }
                    else
                    {
                        row[propertyInfo.Name] = propertyInfo.GetValue(item, null) ?? DBNull.Value;
                    }
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        private static Type GetNullableType(Type type)
        {
            var returnType = type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                returnType = Nullable.GetUnderlyingType(type);
            }
            return returnType;
        }

        private static bool IsNullableType(Type type)
        {
            return type == typeof(string) ||
                   type.IsArray ||
                   (type.IsGenericType &&
                        type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private static void WriteExcelFile(DataSet ds, SpreadsheetDocument spreadsheet)
        {
            spreadsheet.AddWorkbookPart();
            spreadsheet.WorkbookPart.Workbook = new Workbook();

            var workbookView = new WorkbookView();
            var bookViews = new BookViews(workbookView);
            spreadsheet.WorkbookPart.Workbook.Append(bookViews);

            var workbookStylesPart = spreadsheet.WorkbookPart.AddNewPart<WorkbookStylesPart>("rIdStyles");
            var stylesheet = new Stylesheet();
            workbookStylesPart.Stylesheet = stylesheet;

            uint worksheetNumber = 1;
            foreach (DataTable dt in ds.Tables)
            {
                var newWorksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
                newWorksheetPart.Worksheet = new Worksheet();

                newWorksheetPart.Worksheet.AppendChild(new SheetData());

                WriteDataTableToExcelWorksheet(dt, newWorksheetPart);
                newWorksheetPart.Worksheet.Save();

                if (worksheetNumber == 1)
                    spreadsheet.WorkbookPart.Workbook.AppendChild(new Sheets());

                spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                {
                    Id = spreadsheet.WorkbookPart.GetIdOfPart(newWorksheetPart),
                    SheetId = worksheetNumber,
                    Name = dt.TableName
                });

                worksheetNumber++;
            }
            spreadsheet.WorkbookPart.Workbook.Save();
        }

        private static void WriteDataTableToExcelWorksheet(DataTable dataTable, WorksheetPart worksheetPart)
        {
            var worksheet = worksheetPart.Worksheet;
            var sheetData = worksheet.GetFirstChild<SheetData>();
            var numberOfColumns = dataTable.Columns.Count;
            var isNumericColumn = new bool[numberOfColumns];
            var excelColumnNames = new string[numberOfColumns];

            for (var n = 0; n < numberOfColumns; n++)
            {
                excelColumnNames[n] = GetExcelColumnName(n);
            }

            uint rowIndex = 1;
            var headerRow = new Row { RowIndex = rowIndex };

            sheetData.Append(headerRow);

            for (var colInx = 0; colInx < numberOfColumns; colInx++)
            {
                var col = dataTable.Columns[colInx];
                AppendTextCell(excelColumnNames[colInx] + "1", col.ColumnName, headerRow);
                isNumericColumn[colInx] = col.DataType.FullName == "System.Decimal" || col.DataType.FullName == "System.Int32";
            }

            foreach (DataRow dr in dataTable.Rows)
            {
                ++rowIndex;
                var newExcelRow = new Row { RowIndex = rowIndex };
                sheetData.Append(newExcelRow);
                for (var colInx = 0; colInx < numberOfColumns; colInx++)
                {
                    var cellValue = dr.ItemArray[colInx].ToString();

                    if (isNumericColumn[colInx])
                    {
                        if (!double.TryParse(cellValue, out double cellNumericValue)) continue;
                        cellValue = cellNumericValue.ToString(CultureInfo.InvariantCulture);
                        AppendNumericCell(excelColumnNames[colInx] + rowIndex, cellValue, newExcelRow);
                    }
                    else
                    {
                        AppendTextCell(excelColumnNames[colInx] + rowIndex, cellValue, newExcelRow);
                    }
                }
            }
        }

        private static void AppendTextCell(string cellReference, string cellStringValue, Row excelRow)
        {
            var cell = new Cell()
            {
                CellReference = cellReference,
                DataType = CellValues.String
            };
            var cellValue = new CellValue { Text = cellStringValue };
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private static void AppendNumericCell(string cellReference, string cellStringValue, Row excelRow)
        {
            var cell = new Cell()
            {
                CellReference = cellReference
            };
            var cellValue = new CellValue { Text = cellStringValue };
            cell.Append(cellValue);
            excelRow.Append(cell);
        }

        private static string GetExcelColumnName(int columnIndex)
        {
            string columnName;
            if (columnIndex < 26)
            {
                columnName = ((char)('A' + columnIndex)).ToString();
                return columnName;
            }
            var firstChar = (char)('A' + (columnIndex / 26) - 1);
            var secondChar = (char)('A' + (columnIndex % 26));
            columnName = $"{firstChar}{secondChar}";
            return columnName;
        }
    }
}