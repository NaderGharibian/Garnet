using Garnet;
using Garnet.server;

var builder = WebApplication.CreateBuilder(args);

try
{
    // Start the Garnet server in the background
    Task.Run(() =>
    {
        using var server = new Garnet.GarnetServer(args);
        // Optional: register custom extensions
        RegisterExtensions(server);
        server.Start();
        Thread.Sleep(Timeout.Infinite);
    });
}
catch (Exception ex)
{
    Console.WriteLine($"Unable to initialize Garnet server: {ex.Message}");
    // Handle startup failure appropriately (e.g., throw exception to prevent app start)
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
/// <summary>
/// Register new commands with the server. You can access these commands from clients using
/// commands such as db.Execute in StackExchange.Redis. Example:
///   db.Execute("SETIFPM", key, value, prefix);
/// </summary>
static void RegisterExtensions(GarnetServer server)
{
    // Register custom command on raw strings (SETIFPM = "set if prefix match")
    server.Register.NewCommand("SETIFPM", 2, CommandType.ReadModifyWrite, new SetIfPMCustomCommand());

    // Register custom command on raw strings (SETWPIFPGT = "set with prefix, if prefix greater than")
    server.Register.NewCommand("SETWPIFPGT", 2, CommandType.ReadModifyWrite, new SetWPIFPGTCustomCommand());

    // Register custom command on raw strings (DELIFM = "delete if value matches")
    server.Register.NewCommand("DELIFM", 1, CommandType.ReadModifyWrite, new DeleteIfMatchCustomCommand());

    // Register custom commands on objects
    var factory = new MyDictFactory();
    server.Register.NewCommand("MYDICTSET", 2, CommandType.ReadModifyWrite, factory);
    server.Register.NewCommand("MYDICTGET", 1, CommandType.Read, factory);

    // Register stored procedure to run a transactional command
    server.Register.NewTransactionProc("READWRITETX", 3, () => new ReadWriteTxn());

    // Register stored procedure to run a non-transactional command
    server.Register.NewTransactionProc("GETTWOKEYSNOTXN", 2, () => new GetTwoKeysNoTxn());

    // Register sample transactional procedures
    server.Register.NewTransactionProc("SAMPLEUPDATETX", 8, () => new SampleUpdateTxn());
    server.Register.NewTransactionProc("SAMPLEDELETETX", 5, () => new SampleDeleteTxn());
}