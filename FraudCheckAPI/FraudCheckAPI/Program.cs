using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using FraudCheckAPI.Log;
using FraudCheckAPI.Services;

var builder = WebApplication.CreateBuilder(args);
var logGroupName = "PaymentApiLogs";
var logStreamName = $"Stream-PaymentApiLogs-{DateTime.UtcNow:yyyyMMddHHmmss}";

var cloudWatchClient = new AmazonCloudWatchLogsClient("AKIARIQUO5RSXXN4J66S", "A6Tj8sxgxITkMEbfvx3va1YbF6XcqcCf2lrSzwa1", Amazon.RegionEndpoint.USEast1);
// Add services to the container.
await EnsureLogGroupAndStream(cloudWatchClient, logGroupName, logStreamName);

// Make CloudWatch client and log info available via DI
builder.Services.AddSingleton<IAmazonCloudWatchLogs>(cloudWatchClient);
builder.Services.AddSingleton(new CloudWatchLogContext
{
    LogGroupName = logGroupName,
    LogStreamName = logStreamName
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFraudCheckService, DummyFraudCheckService>();
builder.Services.AddSingleton<CloudWatchLogger>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
static async Task EnsureLogGroupAndStream(IAmazonCloudWatchLogs client, string groupName, string streamName)
{
    var groups = await client.DescribeLogGroupsAsync(new DescribeLogGroupsRequest());
    if (!groups.LogGroups.Any(g => g.LogGroupName == groupName))
    {
        await client.CreateLogGroupAsync(new CreateLogGroupRequest { LogGroupName = groupName });
    }

    var streams = await client.DescribeLogStreamsAsync(new DescribeLogStreamsRequest { LogGroupName = groupName });
    if (!streams.LogStreams.Any(s => s.LogStreamName == streamName))
    {
        await client.CreateLogStreamAsync(new CreateLogStreamRequest
        {
            LogGroupName = groupName,
            LogStreamName = streamName
        });
    }
}

// Optional: Create a context class to share log info
public class CloudWatchLogContext
{
    public string LogGroupName { get; set; }
    public string LogStreamName { get; set; }
}
