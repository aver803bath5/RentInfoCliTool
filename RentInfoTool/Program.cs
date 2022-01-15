// See https://aka.ms/new-console-template for more information

using Microsoft.Data.Sqlite;

async Task<int> GetLastElectricUsageAsync()
{
    var connectionString = new SqliteConnectionStringBuilder()
    {
        DataSource = @"E:\lab\RentInfoTool\identifier.sqlite"
    }.ToString();

    await using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText =
        @"
   SELECT t.*
      FROM ElectricConsumptionHistory t
ORDER BY DateTime DESC
    ";

    await using var reader = command.ExecuteReader();
    await reader.ReadAsync();
    var electricUsageString = reader.GetString(1);

    var result = int.Parse(electricUsageString);

    return result;
}


async Task InsertCurrentElectricUsageAsync(int currentConsumption)
{
    var connectionString = new SqliteConnectionStringBuilder()
    {
        DataSource = @"E:\lab\RentInfoTool\identifier.sqlite"
    }.ToString();

    await using var connection = new SqliteConnection(connectionString);

    connection.Open();

    var command = connection.CreateCommand();

    command.CommandText =
        @"
insert into ElectricConsumptionHistory (Consumption, DateTime)
values (@Consumption, datetime('now'));
    ";

    command.Parameters.AddWithValue("@Consumption", currentConsumption.ToString());

    command.ExecuteScalar();
}


var currentElectricUsage = 0;

if (Environment.GetCommandLineArgs().Length.Equals(1))
{
    Console.Write("請輸入本次用電量：");
    while (int.TryParse(Console.ReadLine() ?? string.Empty, out currentElectricUsage).Equals(false))
    {
        Console.Write("請輸入本次用電量：");
    }
}
else
{
    int.TryParse(Environment.GetCommandLineArgs()[1], out currentElectricUsage);
}

// 從資料庫取得最新上次用電量
var lastElectricUsage = await GetLastElectricUsageAsync();

// 將使用者輸入的電量寫進資料庫
await InsertCurrentElectricUsageAsync(currentElectricUsage);

// 每度電費
const int electricUnitFee = 6;

const string landlordName = "林小姐";

// 房租
const int rent = 12000;

// 總用電量
var electricUsage = currentElectricUsage - lastElectricUsage;

// 總電費
var totalElectronicFee = (currentElectricUsage - lastElectricUsage) * electricUnitFee;

var total = rent + totalElectronicFee;

var output = $@"
{landlordName}您好，這是這次的房租加電費總共是 {total} 元，請查收，謝謝您！

電費算法為：({currentElectricUsage} - {lastElectricUsage}) * {electricUnitFee} = {electricUsage} * {electricUnitFee} = {totalElectronicFee}
";

Console.WriteLine(output);