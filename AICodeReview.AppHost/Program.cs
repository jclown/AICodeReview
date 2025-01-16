var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AICodeReview_ApiService>("apiservice");

builder.AddProject<Projects.AICodeReview_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
