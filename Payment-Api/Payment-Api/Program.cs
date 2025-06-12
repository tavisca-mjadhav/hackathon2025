using Microsoft.EntityFrameworkCore;
using Amazon.CloudWatchLogs;
using PaymentApi.Services;
using PaymentApi.Repositories;
using PaymentApi.Interfaces;
using Amazon.CloudWatchLogs.Model;
using PaymentApi.Log;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using OrderAPI.Log;

var builder = WebApplication.CreateBuilder(args);
var logGroupName = "PaymentApiLogs";
var logStreamName = $"Stream-PaymentApi-{DateTime.UtcNow:yyyyMMddHHmmss}";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var cloudWatchClient = new AmazonCloudWatchLogsClient(
    configuration["AWS:AccessKey"],
    configuration["AWS:SecretKey"],
    Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
);
var dynamoDbClient = new AmazonDynamoDBClient(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"], Amazon.RegionEndpoint.USEast1);

// Register services
builder.Services.AddSingleton<IAmazonDynamoDB>(dynamoDbClient);
builder.Services.AddSingleton<IDynamoDBContext>(new DynamoDBContext(dynamoDbClient));
builder.Services.AddSingleton<AmazonClient>();
await EnsureLogGroupAndStream(cloudWatchClient, logGroupName, logStreamName);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register your services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<IAmazonCloudWatchLogs>(cloudWatchClient);
builder.Services.AddSingleton(new CloudWatchLogContext
{
    LogGroupName = logGroupName,
    LogStreamName = logStreamName
});
builder.Services.AddSingleton<ICloudWatchLogger, CloudWatchLogger>();

var app = builder.Build();

// 4. Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

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