using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace RewardingRentals.Server
{
    public struct GoogleSpreadsheetInfo
    {
        public ResultEnum Code;
        public string Message;
        public RentalInformation Rental;
    };

    public class GoogleSpreadsheetManager
    {
        public static GoogleSpreadsheetManager Instance { get; } = new GoogleSpreadsheetManager();

        private SheetsService m_sheetService;

        private string m_sheetId = "1lO0KAZ12UP_9qIZUAOcMQtuspEyQdn4mjRD8Dtj57pU";
        private string m_sheetReference = "Availability!";

        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";

        static Dictionary<string, Dictionary<long, string>> m_botColumnMap;

        private GoogleSpreadsheetManager()
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            m_sheetService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            m_botColumnMap = new Dictionary<string, Dictionary<long, string>>();
            InstantiateBotColumnMap();
            GetTestData();
            //var rowNumber = GetDayRow(DateTime.Now);
            //Console.WriteLine(rowNumber);
        }

        public void GetTestData()
        {
            // Define request parameters.
            String title = m_sheetReference + "B1";
            String range = m_sheetReference + "B137:B140";

            SpreadsheetsResource.ValuesResource.GetRequest titleRequest =
                    m_sheetService.Spreadsheets.Values.Get(m_sheetId, title);

            SpreadsheetsResource.ValuesResource.GetRequest request =
                    m_sheetService.Spreadsheets.Values.Get(m_sheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute();
            ValueRange titleResponse = titleRequest.Execute();
            IList<IList<Object>> titleValue = titleResponse.Values;
            IList<IList<Object>> values = response.Values;
            if (titleValue != null && titleValue.Count == 1)
            {
                Console.WriteLine(titleValue[0][0]);

            }

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    Console.WriteLine($"{row[0]}");
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }

        public void InstantiateBotColumnMap()
        {
            SpreadsheetsResource.ValuesResource.GetRequest columnRequest =
                    m_sheetService.Spreadsheets.Values.Get(m_sheetId, $"{m_sheetReference}A1:BD1");
            ValueRange columnResponse = columnRequest.Execute();

            foreach (var row in columnResponse.Values)
            {
                int colNumber = 1;
                foreach (var column in row)
                {
                    var data = column as string;
                    if (data.Contains('#'))
                    {
                        var parts = data.Split('#');
                        string botName = parts[0].Trim();
                        long botNumber = Convert.ToInt64(parts[1].Trim());
                        string botColumn = GetColNameFromIndex(colNumber);

                        //new Dictionary<long, string>
                        if (!m_botColumnMap.ContainsKey(botName))
                        {
                            m_botColumnMap.Add(botName, new Dictionary<long, string>());
                            m_botColumnMap[botName] = new Dictionary<long, string>();
                        }

                        m_botColumnMap[botName].Add(botNumber, botColumn);
                    }

                    colNumber++;
                    //m_botColumnMap
                }
            }
        }

        // (1 = A, 2 = B...27 = AA...703 = AAA...)
        public static string GetColNameFromIndex(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public int GetDayRow(DateTime requestDay)
        {
            SpreadsheetsResource.ValuesResource.GetRequest columnRequest =
            m_sheetService.Spreadsheets.Values.Get(m_sheetId, $"{m_sheetReference}A:A");
            ValueRange dateResponse = columnRequest.Execute();
            int rowNumber = 1;
            foreach (var rowList in dateResponse.Values)
            {
                var row = rowList[0];
                var data = row as string;
                if (data.Contains('/'))
                {
                    var parts = data.Split('/');
                    var month = Convert.ToInt32(parts[0]);
                    var day = Convert.ToInt32(parts[1]);
                    var year = Convert.ToInt32(parts[2]);

                    DateTime date = new DateTime(year, month, day);

                    if (requestDay.Month == month && requestDay.Day == day && requestDay.Year == year)
                    {
                        return rowNumber;
                    }
                }

                rowNumber++;
            }

            throw new Exception("GetDayRow failed. Check spreadsheet to make sure date you're looking for exists in row A");
        }

        // Pass in your data as a list of a list (2-D lists are equivalent to the 2-D spreadsheet structure)
        /*public string UpdateData(List<IList<object>> data, string botName, long botNumber)
        {
            var titleSearch = $"{botName} #{botNumber}";

            String range = $"{m_sheetReference}{m_botColumnMap[botName][botNumber]}";
            string valueInputOption = "USER_ENTERED";


            // The new values to apply to the spreadsheet.
            List<Data.ValueRange> updateData = new List<Data.ValueRange>();
            var dataValueRange = new Data.ValueRange();
            dataValueRange.Range = range;
            dataValueRange.Values = data;
            updateData.Add(dataValueRange);

            Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest();
            requestBody.ValueInputOption = valueInputOption;
            requestBody.Data = updateData;

            var request = m_sheetService.Spreadsheets.Values.BatchUpdate(requestBody, _spreadsheetId);

            Data.BatchUpdateValuesResponse response = request.Execute();
            // Data.BatchUpdateValuesResponse response = await request.ExecuteAsync(); // For async 

            return JsonConvert.SerializeObject(response);
        }*/
    }
}
