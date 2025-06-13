using Amazon.Extensions.NETCore.Setup; // Add this using directive for AddAWSService extension method

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.CloudWatchLogs;
using Serilog;
using Amazon.CloudWatchLogs.Model;
using OrderAPI.Log;
using Amazon.DynamoDBv2.DataModel;
using OrderAPI.Services;


var builder = WebApplication.CreateBuilder(args);
var logGroupName = "PaymentApiLogs";
var logStreamName = $"Stream-PaymentApiLogs-{DateTime.UtcNow:yyyyMMddHHmmss}";

var cloudWatchClient = new AmazonCloudWatchLogsClient("AKIARIQUO5RSXXN4J66S", "A6Tj8sxgxITkMEbfvx3va1YbF6XcqcCf2lrSzwa1", Amazon.RegionEndpoint.USEast1);
var dynamoDbClient = new AmazonDynamoDBClient("AKIARIQUO5RSXXN4J66S", "A6Tj8sxgxITkMEbfvx3va1YbF6XcqcCf2lrSzwa1", Amazon.RegionEndpoint.USEast1);

// Register services
builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
builder.Services.AddSingleton<IDynamoDBContext>(new DynamoDBContext(dynamoDbClient));
builder.Services.AddSingleton<AmazonClient>();
await EnsureLogGroupAndStream(cloudWatchClient, logGroupName, logStreamName);

// Make CloudWatch client and log info available via DI
builder.Services.AddSingleton<IAmazonCloudWatchLogs>(cloudWatchClient);
builder.Services.AddSingleton(new CloudWatchLogContext
{
    LogGroupName = logGroupName,
    LogStreamName = logStreamName
});

//Load AWS options from appsettings or environment
var awsOptions = builder.Configuration.GetAWSOptions();
awsOptions.Region = RegionEndpoint.USEast1; // explicitly set region (optional if in appsettings.json)

// Register AWS services
builder.Services.AddSingleton<CloudWatchLogger>();
// Add services
builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddHttpClient<IPaymentService, PaymentService>();
builder.Services.AddHttpClient<IFraudCheck, FraudCheck>();
// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
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