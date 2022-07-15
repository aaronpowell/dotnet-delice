module LicenseCache

open System.Text.RegularExpressions

// This is a lookup against a number of standard projects and their license type.
// The NuGet versions here use the legacy license format, so the URL is the only
// thing that will tell you what the license is. This cache means we don't have
// to query the URL each run.
type LicenseCache =
    { Expression: string
      Packages: Map<string, string list> }

let knownLicenseCache =
    Map.ofList
        [ ("https://github.com/Microsoft/visualfsharp/blob/master/License.txt",
           { Expression = "MIT"
             Packages = Map.ofList [ ("fsharp.core", [ "4.5.0"; "4.5.1"; "4.5.2" ]) ] })
          ("http://go.microsoft.com/fwlink/?LinkId=329770",
           { Expression = "Microsoft Software License"
             Packages =
                 Map.ofList
                     [ ("microsoft.csharp", [ "4.0.1" ])
                       ("microsoft.win32.primitives", [ "4.3.0" ])
                       ("microsoft.win32.registry", [ "4.3.0" ])
                       ("runtime.native.system", [ "4.3.0" ])
                       ("system.collections", [ "4.3.0" ])
                       ("system.diagnostics.debug", [ "4.3.0" ])
                       ("system.diagnostics.process", [ "4.3.0" ])
                       ("system.diagnostics.tools", [ "4.0.1" ])
                       ("system.dynamic.runtime", [ "4.3.0" ])
                       ("system.globalization", [ "4.3.0" ])
                       ("system.io", [ "4.3.0" ])
                       ("system.io.filesystem", [ "4.3.0" ])
                       ("system.io.filesystem.primitives", [ "4.3.0" ])
                       ("system.linq", [ "4.3.0" ])
                       ("system.linq.expressions", [ "4.3.0" ])
                       ("system.objectmodel", [ "4.3.0" ])
                       ("system.reflection", [ "4.3.0" ])
                       ("system.reflection.emit", [ "4.3.0" ])
                       ("system.reflection.emit.ilgeneration", [ "4.3.0" ])
                       ("system.reflection.emit.lightweight", [ "4.3.0" ])
                       ("system.reflection.extensions", [ "4.3.0" ])
                       ("system.reflection.primitives", [ "4.3.0" ])
                       ("system.reflection.typeextensions", [ "4.3.0" ])
                       ("system.resources.resourcemanager", [ "4.3.0" ])
                       ("system.runtime", [ "4.3.0" ])
                       ("system.runtime.extensions", [ "4.3.0" ])
                       ("system.runtime.handles", [ "4.3.0" ])
                       ("system.runtime.interopservices", [ "4.3.0" ])
                       ("system.runtime.serialization.primitives", [ "4.1.1" ])
                       ("system.security.cryptography.primitives", [ "4.3.0" ])
                       ("system.security.cryptography.protecteddata", [ "4.3.0" ])
                       ("system.text.encoding", [ "4.3.0" ])
                       ("system.text.encoding.extensions", [ "4.3.0" ])
                       ("system.text.regularexpressions", [ "4.1.0" ])
                       ("system.threading", [ "4.3.0" ])
                       ("system.threading.tasks", [ "4.3.0" ])
                       ("system.threading.tasks.extensions", [ "4.0.0" ])
                       ("system.threading.thread", [ "4.3.0" ])
                       ("system.threading.threadpool", [ "4.3.0" ])
                       ("system.xml.readerwriter", [ "4.0.11" ])
                       ("system.xml.xdocument", [ "4.0.11" ]) ] })
          ("http://go.microsoft.com/fwlink/?LinkId=529443",
           { Expression = "Microsoft Software License"
             Packages =
                 Map.ofList
                     [ ("Microsoft.CodeAnalysis.FxCopAnalyzers", [ "2.9.4" ])
                       ("Microsoft.CodeAnalysis.VersionCheckAnalyzer", [ "2.9.4" ])
                       ("Microsoft.CodeQuality.Analyzers", [ "2.9.4" ])
                       ("Microsoft.NetCore.Analyzers", [ "2.9.4" ]) ] })
          ("https://github.com/dotnet/core-setup/blob/master/LICENSE.TXT",
           { Expression = "MIT"
             Packages =
                 Map.ofList
                     [ ("Microsoft.NETCore.App", [ "2.2.0" ])
                       ("Microsoft.NETCore.DotNetAppHost", [ "2.2.0" ])
                       ("Microsoft.NETCore.DotNetHostPolicy", [ "2.2.0" ])
                       ("Microsoft.NETCore.DotNetHostResolver", [ "2.2.0" ]) ] })
          ("https://github.com/dotnet/corefx/blob/master/LICENSE.TXT",
           { Expression = "MIT"
             Packages =
                 Map.ofList
                     [ ("Microsoft.NETCore.Platforms", [ "2.2.0" ])
                       ("Microsoft.NETCore.Targets", [ "2.0.0" ])
                       ("System.ComponentModel.Annotations", [ "4.4.1" ]) ] })
          ("https://github.com/dotnet/standard/blob/master/LICENSE.TXT",
           { Expression = "MIT"
             Packages = Map.ofList [ ("NETStandard.Library", [ "2.0.3" ]) ] })
          ("https://raw.github.com/JamesNK/Newtonsoft.Json/master/LICENSE.md",
           { Expression = "MIT"
             Packages = Map.ofList [ ("Newtonsoft.Json", [ "9.0.1" ]) ] })
          ("https://raw.githubusercontent.com/aspnet/AspNetCore/2.0.0/LICENSE.txt",
           { Expression = "Apache-2.0"
             Packages =
                 Map.ofList
                     [ ("Microsoft.AspNetCore", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Antiforgery", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication.Cookies", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication.Core", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication.JwtBearer", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authentication.OAuth", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Authorization", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Connections.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Cors", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Cryptography.Internal", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.DataProtection", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.DataProtection.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Diagnostics", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Diagnostics.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.HostFiltering", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Hosting", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Hosting.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Hosting.Server.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Http", [ "2.2.2" ])
                       ("Microsoft.AspNetCore.Http.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Http.Extensions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Http.Features", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.HttpOverrides", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.ResponseCaching", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.ResponseCaching.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.ResponseCompression", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Rewrite", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Routing", [ "2.2.2" ])
                       ("Microsoft.AspNetCore.Routing.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Server.IIS", [ "2.2.2" ])
                       ("Microsoft.AspNetCore.Server.IISIntegration", [ "2.2.1" ])
                       ("Microsoft.AspNetCore.Server.Kestrel", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Server.Kestrel.Core", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Server.Kestrel.Https", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets", [ "2.2.1" ])
                       ("Microsoft.AspNetCore.Session", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.StaticFiles", [ "2.2.0" ])
                       ("Microsoft.AspNetCore.WebUtilities", [ "2.2.0" ])
                       ("Microsoft.Extensions.Caching.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Caching.Memory", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration.Binder", [ "2.2.4" ])
                       ("Microsoft.Extensions.Configuration.CommandLine", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration.EnvironmentVariables", [ "2.2.4" ])
                       ("Microsoft.Extensions.Configuration.FileExtensions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration.Json", [ "2.2.0" ])
                       ("Microsoft.Extensions.Configuration.UserSecrets", [ "2.2.0" ])
                       ("Microsoft.Extensions.DependencyInjection", [ "2.2.0" ])
                       ("Microsoft.Extensions.DependencyInjection.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.FileProviders.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.FileProviders.Physical", [ "2.2.0" ])
                       ("Microsoft.Extensions.FileSystemGlobbing", [ "2.2.0" ])
                       ("Microsoft.Extensions.Hosting.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging.Abstractions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging.Configuration", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging.Console", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging.Debug", [ "2.2.0" ])
                       ("Microsoft.Extensions.Logging.EventSource", [ "2.2.0" ])
                       ("Microsoft.Extensions.ObjectPool", [ "2.2.0" ])
                       ("Microsoft.Extensions.Options", [ "2.2.0" ])
                       ("Microsoft.Extensions.Options.ConfigurationExtensions", [ "2.2.0" ])
                       ("Microsoft.Extensions.Primitives", [ "2.2.0" ])
                       ("Microsoft.Extensions.WebEncoders", [ "2.2.0" ])
                       ("Microsoft.Net.Http.Headers", [ "2.2.0" ]) ] })
          ("https://go.microsoft.com/fwlink/?linkid=2028464",
           { Expression = "Microsoft Software License"
             Packages =
                 Map.ofList
                     [ ("Microsoft.Azure.WebJobs", [ "3.0.6" ])
                       ("Microsoft.Azure.WebJobs.Core", [ "3.0.6" ])
                       ("Microsoft.Azure.WebJobs.Extensions", [ "3.0.2" ])
                       ("Microsoft.Azure.WebJobs.Extensions.EventHubs", [ "3.0.4" ])
                       ("Microsoft.Azure.WebJobs.Extensions.Http", [ "3.0.2" ])
                       ("Microsoft.Azure.WebJobs.Extensions.Storage", [ "3.0.5" ])
                       ("Microsoft.Azure.WebJobs.Host.Storage", [ "3.0.6" ])
                       ("Microsoft.Azure.WebJobs.Script.ExtensionsMetadataGenerator", [ "1.1.0" ]) ] })
          ("http://aws.amazon.com/apache2.0/",
           { Expression = "Apache-2.0"
             Packages =
                 Map.ofList
                     [
                        ("AWSSDK.SecretsManager.Caching", [ "1.0.3" ])
                        ("AWSSDK.AccessAnalyzer", [ "3.7.2.1" ])
                        ("AWSSDK.AlexaForBusiness", [ "3.7.0.105" ])
                        ("AWSSDK.APIGateway", [ "3.7.2.51" ])
                        ("AWSSDK.AppConfig", [ "3.7.1.4" ])
                        ("AWSSDK.Appflow", [ "3.7.4.23" ])
                        ("AWSSDK.AppIntegrationsService", [ "3.7.1.33" ])
                        ("AWSSDK.AppStream", [ "3.7.3.3" ])
                        ("AWSSDK.Athena", [ "3.7.0.105" ])
                        ("AWSSDK.AugmentedAIRuntime", [ "3.7.0.106" ])
                        ("AWSSDK.Chime", [ "3.7.14.4" ])
                        ("AWSSDK.ChimeSDKIdentity", [ "3.7.1.19" ])
                        ("AWSSDK.ChimeSDKMeetings", [ "3.7.3.2" ])
                        ("AWSSDK.ChimeSDKMessaging", [ "3.7.2.19" ])
                        ("AWSSDK.CloudDirectory", [ "3.7.0.106" ])
                        ("AWSSDK.CloudFront", [ "3.7.4.15" ])
                        ("AWSSDK.CloudHSM", [ "3.7.1.101" ])
                        ("AWSSDK.CloudSearch", [ "3.7.2.25" ])
                        ("AWSSDK.CloudSearchDomain", [ "3.7.0.106" ])
                        ("AWSSDK.CloudWatch", [ "3.7.3.4" ])
                        ("AWSSDK.ApplicationInsights", [ "3.7.2.3" ])
                        ("AWSSDK.CloudWatchEvents", [ "3.7.4.50" ])
                        ("AWSSDK.CloudWatchEvidently", [ "3.7.0.2" ])
                        ("AWSSDK.CloudWatchLogs", [ "3.7.1.80" ])
                        ("AWSSDK.CodeGuruProfiler", [ "3.7.0.105" ])
                        ("AWSSDK.CodeGuruReviewer", [ "3.7.4.42" ])
                        ("AWSSDK.CognitoIdentity", [ "3.7.0.105" ])
                        ("AWSSDK.CognitoIdentityProvider", [ "3.7.1.77" ])
                        ("AWSSDK.Comprehend", [ "3.7.4.37" ])
                        ("AWSSDK.ConnectContactLens", [ "3.7.0.105" ])
                        ("AWSSDK.CustomerProfiles", [ "3.7.4.2" ])
                        ("AWSSDK.ConnectParticipant", [ "3.7.2.14" ])
                        ("AWSSDK.Connect", [ "3.7.10.2" ])
                        ("AWSSDK.ConnectWisdomService", [ "3.7.0.33" ])
                        ("AWSSDK.DLM", [ "3.7.1.51" ])
                        ("AWSSDK.Detective", [ "3.7.2.92" ])
                        ("AWSSDK.DevOpsGuru", [ "3.7.6" ])
                        ("AWSSDK.DocDB", [ "3.7.2.77" ])
                        ("AWSSDK.DynamoDBv2", [ "3.7.2" ])
                        ("AWSSDK.DAX", [ "3.7.1.71" ])
                        ("AWSSDK.ECR", [ "3.7.2.2" ])
                        ("AWSSDK.ECS", [ "3.7.4.12" ])
                        ("AWSSDK.ElasticInference", [ "3.7.0.105" ])
                        ("AWSSDK.EBS", [ "3.7.0.106" ])
                        ("AWSSDK.EC2", [ "3.7.49" ])
                        ("AWSSDK.ECRPublic", [ "3.7.0.105" ])
                        ("AWSSDK.ElasticFileSystem", [ "3.7.3.31" ])
                        ("AWSSDK.EKS", [ "3.7.11.2" ])
                        ("AWSSDK.ElasticTranscoder", [ "3.7.0.105" ])
                        ("AWSSDK.ElastiCache", [ "3.7.4.3" ])
                        ("AWSSDK.Elasticsearch", [ "3.7.3.2" ])
                        ("AWSSDK.ElasticMapReduce", [ "3.7.4.42" ])
                        ("AWSSDK.EMRContainers", [ "3.7.2.19" ])
                        ("AWSSDK.EventBridge", [ "3.7.4.50" ])
                        ("AWSSDK.ForecastQueryService", [ "3.7.0.105" ])
                        ("AWSSDK.ForecastService", [ "3.7.6.4" ])
                        ("AWSSDK.FraudDetector", [ "3.7.5.26" ])
                        ("AWSSDK.FSx", [ "3.7.6.1" ])
                        ("AWSSDK.GameLift", [ "3.7.1.18" ])
                        ("AWSSDK.Glacier", [ "3.7.0.105" ])
                        ("AWSSDK.GuardDuty", [ "3.7.0.105" ])
                        ("AWSSDK.HealthLake", [ "3.7.1.63" ])
                        ("AWSSDK.Honeycode", [ "3.7.0.105" ])
                        ("AWSSDK.Snowball", [ "3.7.3.1" ])
                        ("AWSSDK.Inspector", [ "3.7.0.105" ])
                        ("AWSSDK.IVS", [ "3.7.3.4" ])
                        ("AWSSDK.Kinesis", [ "3.7.1.1" ])
                        ("AWSSDK.KinesisAnalytics", [ "3.7.0.105" ])
                        ("AWSSDK.KinesisAnalyticsV2", [ "3.7.5.24" ])
                        ("AWSSDK.KinesisFirehose", [ "3.7.2.27" ])
                        ("AWSSDK.KinesisVideoSignalingChannels", [ "3.7.0.105" ])
                        ("AWSSDK.KinesisVideo", [ "3.7.0.105" ])
                        ("AWSSDK.KinesisVideoArchivedMedia", [ "3.7.0.105" ])
                        ("AWSSDK.KinesisVideoMedia", [ "3.7.0.105" ])
                        ("AWSSDK.LexModelBuildingService", [ "3.7.4.46" ])
                        ("AWSSDK.LexModelsV2", [ "3.7.9" ])
                        ("AWSSDK.Lex", [ "3.7.1.99" ])
                        ("AWSSDK.LexRuntimeV2", [ "3.7.3.3" ])
                        ("AWSSDK.Lightsail", [ "3.7.4.16" ])
                        ("AWSSDK.LocationService", [ "3.7.6" ])
                        ("AWSSDK.LookoutEquipment", [ "3.7.1.42" ])
                        ("AWSSDK.LookoutMetrics", [ "3.7.4.75" ])
                        ("AWSSDK.LookoutforVision", [ "3.7.0.105" ])
                        ("AWSSDK.MachineLearning", [ "3.7.1.101" ])
                        ("AWSSDK.Macie", [ "3.7.0.105" ])
                        ("AWSSDK.Macie2", [ "3.7.6.15" ])
                        ("AWSSDK.ManagedBlockchain", [ "3.7.1.76" ])
                        ("AWSSDK.ManagedGrafana", [ "3.7.0.27" ])
                        ("AWSSDK.MTurk", [ "3.7.0.106" ])
                        ("AWSSDK.MemoryDB", [ "3.7.0.52" ])
                        ("AWSSDK.MobileAnalytics", [ "3.7.0.105" ])
                        ("AWSSDK.Neptune", [ "3.7.2.16" ])
                        ("AWSSDK.OpenSearchService", [ "3.7.2.2" ])
                        ("AWSSDK.Personalize", [ "3.7.6.2" ])
                        ("AWSSDK.PersonalizeEvents", [ "3.7.1.76" ])
                        ("AWSSDK.PersonalizeRuntime", [ "3.7.1.2" ])
                        ("AWSSDK.Pinpoint", [ "3.7.4.2" ])
                        ("AWSSDK.PinpointEmail", [ "3.7.0.105" ])
                        ("AWSSDK.PinpointSMSVoice", [ "3.7.0.105" ])
                        ("AWSSDK.Polly", [ "3.7.3.48" ])
                        ("AWSSDK.PrometheusService", [ "3.7.2.32" ])
                        ("AWSSDK.QLDB", [ "3.7.2.61" ])
                        ("AWSSDK.QLDBSession", [ "3.7.0.105" ])
                        ("AWSSDK.QuickSight", [ "3.7.9.2" ])
                        ("AWSSDK.RecycleBin", [ "3.7.0.2" ])
                        ("AWSSDK.Redshift", [ "3.7.10.2" ])
                        ("AWSSDK.Rekognition", [ "3.7.6" ])
                        ("AWSSDK.RDS", [ "3.7.8.2" ])
                        ("AWSSDK.Route53", [ "3.7.2" ])
                        ("AWSSDK.Route53Domains", [ "3.7.0.105" ])
                        ("AWSSDK.Route53Resolver", [ "3.7.2.20" ])
                        ("AWSSDK.S3Outposts", [ "3.7.1.61" ])
                        ("AWSSDK.SagemakerEdgeManager", [ "3.7.0.105" ])
                        ("AWSSDK.SageMakerFeatureStoreRuntime", [ "3.7.1.76" ])
                        ("AWSSDK.SageMakerRuntime", [ "3.7.2" ])
                        ("AWSSDK.SageMaker", [ "3.7.22" ])
                        ("AWSSDK.SimpleEmail", [ "3.7.0.105" ])
                        ("AWSSDK.SimpleEmailV2", [ "3.7.1.32" ])
                        ("AWSSDK.SimpleNotificationService", [ "3.7.3.5" ])
                        ("AWSSDK.SQS", [ "3.7.2.2" ])
                        ("AWSSDK.S3", [ "3.7.7.1" ])
                        ("AWSSDK.SimpleSystemsManagement", [ "3.7.11.2" ])
                        ("AWSSDK.SimpleWorkflow", [ "3.7.0.106" ])
                        ("AWSSDK.SimpleDB", [ "3.7.0.105" ])
                        ("AWSSDK.Textract", [ "3.7.3.2" ])
                        ("AWSSDK.TimestreamQuery", [ "3.7.1.2" ])
                        ("AWSSDK.TimestreamWrite", [ "3.7.1.2" ])
                        ("AWSSDK.TranscribeService", [ "3.7.9.17" ])
                        ("AWSSDK.Translate", [ "3.7.3.2" ])
                        ("AWSSDK.VoiceID", [ "3.7.0.33" ])
                        ("AWSSDK.WorkDocs", [ "3.7.0.105" ])
                        ("AWSSDK.WorkLink", [ "3.7.0.105" ])
                        ("AWSSDK.WorkMail", [ "3.7.4.24" ])
                        ("AWSSDK.WorkMailMessageFlow", [ "3.7.0.105" ])
                        ("AWSSDK.WorkSpaces", [ "3.7.2.32" ])
                        ("AWSSDK.WorkSpacesWeb", [ "3.7.0.1" ])
                        ("AWSSDK.ApiGatewayManagementApi", [ "3.7.0.105" ])
                        ("AWSSDK.ApiGatewayV2", [ "3.7.1.54" ])
                        ("AWSSDK.MQ", [ "3.7.1.68" ])
                        ("AWSSDK.MWAA", [ "3.7.1.78" ])
                        ("AWSSDK.NimbleStudio", [ "3.7.2.15" ])
                        ("AWSSDK.AmplifyBackend", [ "3.7.4.5" ])
                        ("AWSSDK.ApplicationAutoScaling", [ "3.7.2.30" ])
                        ("AWSSDK.Mgn", [ "3.7.2.2" ])
                        ("AWSSDK.AutoScaling", [ "3.7.8.3" ])
                        ("AWSSDK.Account", [ "3.7.0.31" ])
                        ("AWSSDK.Amplify", [ "3.7.0.105" ])
                        ("AWSSDK.AmplifyUIBuilder", [ "3.7.0" ])
                        ("AWSSDK.AppMesh", [ "3.7.1.76" ])
                        ("AWSSDK.AppRunner", [ "3.7.1.30" ])
                        ("AWSSDK.AppConfigData", [ "3.7.0.5" ])
                        ("AWSSDK.ApplicationCostProfiler", [ "3.7.0.84" ])
                        ("AWSSDK.ApplicationDiscoveryService", [ "3.7.0.105" ])
                        ("AWSSDK.AppSync", [ "3.7.3" ])
                        ("AWSSDK.AuditManager", [ "3.7.7.4" ])
                        ("AWSSDK.AutoScalingPlans", [ "3.7.0.105" ])
                        ("AWSSDK.Backup", [ "3.7.5.2" ])
                        ("AWSSDK.BackupGateway", [ "3.7.0.1" ])
                        ("AWSSDK.Batch", [ "3.7.3.12" ])
                        ("AWSSDK.Budgets", [ "3.7.0.105" ])
                        ("AWSSDK.CertificateManager", [ "3.7.1.63" ])
                        ("AWSSDK.ACMPCA", [ "3.7.3.46" ])
                        ("AWSSDK.CloudControlApi", [ "3.7.0.31" ])
                        ("AWSSDK.ServiceDiscovery", [ "3.7.3.70" ])
                        ("AWSSDK.Cloud9", [ "3.7.2.53" ])
                        ("AWSSDK.CloudFormation", [ "3.7.7.2" ])
                        ("AWSSDK.CloudHSMV2", [ "3.7.0.105" ])
                        ("AWSSDK.CloudTrail", [ "3.7.1.6" ])
                        ("AWSSDK.CodeBuild", [ "3.7.7.30" ])
                        ("AWSSDK.CodeCommit", [ "3.7.0.105" ])
                        ("AWSSDK.CodeDeploy", [ "3.7.0.105" ])
                        ("AWSSDK.CodePipeline", [ "3.7.0.105" ])
                        ("AWSSDK.CodeStar", [ "3.7.0.105" ])
                        ("AWSSDK.CodeStarconnections", [ "3.7.1.94" ])
                        ("AWSSDK.CodeStarNotifications", [ "3.7.0.105" ])
                        ("AWSSDK.ComprehendMedical", [ "3.7.1.94" ])
                        ("AWSSDK.ComputeOptimizer", [ "3.7.4.2" ])
                        ("AWSSDK.ConfigService", [ "3.7.5.24" ])
                        ("AWSSDK.CostAndUsageReport", [ "3.7.0.105" ])
                        ("AWSSDK.CostExplorer", [ "3.7.3.53" ])
                        ("AWSSDK.DataExchange", [ "3.7.2.23" ])
                        ("AWSSDK.DataPipeline", [ "3.7.1.101" ])
                        ("AWSSDK.DatabaseMigrationService", [ "3.7.6.2" ])
                        ("AWSSDK.DataSync", [ "3.7.3.14" ])
                        ("AWSSDK.DeviceFarm", [ "3.7.1.77" ])
                        ("AWSSDK.DirectConnect", [ "3.7.4" ])
                        ("AWSSDK.DirectoryService", [ "3.7.1.53" ])
                        ("AWSSDK.EC2InstanceConnect", [ "3.7.1.102" ])
                        ("AWSSDK.ElasticBeanstalk", [ "3.7.0.105" ])
                        ("AWSSDK.MediaConvert", [ "3.7.10.8" ])
                        ("AWSSDK.MediaLive", [ "3.7.7.3" ])
                        ("AWSSDK.MediaPackage", [ "3.7.4.23" ])
                        ("AWSSDK.MediaPackageVod", [ "3.7.4.23" ])
                        ("AWSSDK.MediaStore", [ "3.7.0.105" ])
                        ("AWSSDK.MediaStoreData", [ "3.7.0.105" ])
                        ("AWSSDK.FIS", [ "3.7.0.105" ])
                        ("AWSSDK.GlobalAccelerator", [ "3.7.0.105" ])
                        ("AWSSDK.Glue", [ "3.7.10.1" ])
                        ("AWSSDK.GlueDataBrew", [ "3.7.5.4" ])
                        ("AWSSDK.Greengrass", [ "3.7.0.105" ])
                        ("AWSSDK.GroundStation", [ "3.7.1.92" ])
                        ("AWSSDK.AWSHealth", [ "3.7.1.62" ])
                        ("AWSSDK.IdentityManagement", [ "3.7.2.85" ])
                        ("AWSSDK.ImportExport", [ "3.7.0.105" ])
                        ("AWSSDK.IoT", [ "3.7.8.1" ])
                        ("AWSSDK.IoT1ClickDevicesService", [ "3.7.0.105" ])
                        ("AWSSDK.IoT1ClickProjects", [ "3.7.0.105" ])
                        ("AWSSDK.IoTAnalytics", [ "3.7.2.60" ])
                        ("AWSSDK.IoTDeviceAdvisor", [ "3.7.2.3" ])
                        ("AWSSDK.IotData", [ "3.7.1.51" ])
                        ("AWSSDK.IoTEvents", [ "3.7.1.77" ])
                        ("AWSSDK.IoTEventsData", [ "3.7.1.77" ])
                        ("AWSSDK.IoTFleetHub", [ "3.7.0.105" ])
                        ("AWSSDK.GreengrassV2", [ "3.7.3.11" ])
                        ("AWSSDK.IoTJobsDataPlane", [ "3.7.0.105" ])
                        ("AWSSDK.IoTSecureTunneling", [ "3.7.0.105" ])
                        ("AWSSDK.IoTSiteWise", [ "3.7.9.2" ])
                        ("AWSSDK.IoTThingsGraph", [ "3.7.0.105" ])
                        ("AWSSDK.IoTTwinMaker", [ "3.7.0.1" ])
                        ("AWSSDK.IoTWireless", [ "3.7.8.2" ])
                        ("AWSSDK.KeyManagementService", [ "3.7.2.50" ])
                        ("AWSSDK.LakeFormation", [ "3.7.2.1" ])
                        ("AWSSDK.Lambda", [ "3.7.8.2" ])
                        ("AWSSDK.LicenseManager", [ "3.7.5.34" ])
                        ("AWSSDK.MarketplaceCatalog", [ "3.7.1.89" ])
                        ("AWSSDK.AWSMarketplaceCommerceAnalytics", [ "3.7.0.105" ])
                        ("AWSSDK.MarketplaceEntitlementService", [ "3.7.0.105" ])
                        ("AWSSDK.MediaConnect", [ "3.7.3.75" ])
                        ("AWSSDK.MediaTailor", [ "3.7.6.25" ])
                        ("AWSSDK.MigrationHub", [ "3.7.0.105" ])
                        ("AWSSDK.MigrationHubConfig", [ "3.7.0.105" ])
                        ("AWSSDK.MigrationHubRefactorSpaces", [ "3.7.0.2" ])
                        ("AWSSDK.Mobile", [ "3.7.0.105" ])
                        ("AWSSDK.NetworkFirewall", [ "3.7.1.31" ])
                        ("AWSSDK.NetworkManager", [ "3.7.2" ])
                        ("AWSSDK.OpsWorks", [ "3.7.0.105" ])
                        ("AWSSDK.OpsWorksCM", [ "3.7.0.105" ])
                        ("AWSSDK.Organizations", [ "3.7.0.106" ])
                        ("AWSSDK.Outposts", [ "3.7.5.1" ])
                        ("AWSSDK.Panorama", [ "3.7.0.23" ])
                        ("AWSSDK.PI", [ "3.7.1.76" ])
                        ("AWSSDK.Pricing", [ "3.7.1.101" ])
                        ("AWSSDK.Proton", [ "3.7.2.2" ])
                        ("AWSSDK.RDSDataService", [ "3.7.0.106" ])
                        ("AWSSDK.ResilienceHub", [ "3.7.0.11" ])
                        ("AWSSDK.RAM", [ "3.7.2" ])
                        ("AWSSDK.ResourceGroups", [ "3.7.0.105" ])
                        ("AWSSDK.ResourceGroupsTaggingAPI", [ "3.7.0.106" ])
                        ("AWSSDK.RoboMaker", [ "3.7.5.23" ])
                        ("AWSSDK.Route53RecoveryControlConfig", [ "3.7.0.60" ])
                        ("AWSSDK.Route53RecoveryReadiness", [ "3.7.0.60" ])
                        ("AWSSDK.S3Control", [ "3.7.4.2" ])
                        ("AWSSDK.SavingsPlans", [ "3.7.2.59" ])
                        ("AWSSDK.SecretsManager", [ "3.7.1.58" ])
                        ("AWSSDK.SecurityToken", [ "3.7.1.96" ])
                        ("AWSSDK.SecurityHub", [ "3.7.7.23" ])
                        ("AWSSDK.ServerMigrationService", [ "3.7.0.105" ])
                        ("AWSSDK.ServiceCatalog", [ "3.7.1.76" ])
                        ("AWSSDK.AppRegistry", [ "3.7.1.47" ])
                        ("AWSSDK.Shield", [ "3.7.2" ])
                        ("AWSSDK.Signer", [ "3.7.0.105" ])
                        ("AWSSDK.SSO", [ "3.7.0.105" ])
                        ("AWSSDK.SSOAdmin", [ "3.7.0.106" ])
                        ("AWSSDK.SnowDeviceManagement", [ "3.7.0.55" ])
                        ("AWSSDK.IdentityStore", [ "3.7.0.106" ])
                        ("AWSSDK.SSOOIDC", [ "3.7.0.105" ])
                        ("AWSSDK.StepFunctions", [ "3.7.0.105" ])
                        ("AWSSDK.StorageGateway", [ "3.7.4.1" ])
                        ("AWSSDK.AWSSupport", [ "3.7.0.106" ])
                        ("AWSSDK.SSMIncidents", [ "3.7.2.18" ])
                        ("AWSSDK.SSMContacts", [ "3.7.2.58" ])
                        ("AWSSDK.Transfer", [ "3.7.4.7" ])
                        ("AWSSDK.WAF", [ "3.7.0.105" ])
                        ("AWSSDK.WAFRegional", [ "3.7.0.105" ])
                        ("AWSSDK.WAFV2", [ "3.7.8.7" ])
                        ("AWSSDK.WellArchitected", [ "3.7.2.2" ])
                        ("AWSSDK.XRay", [ "3.7.0.105" ])
                        ("AWSSDK.Kendra", [ "3.7.11" ])
                        ("AWSSDK.AWSMarketplaceMetering", [ "3.7.0.105" ])
                        ("AWSSDK.ServerlessApplicationRepository", [ "3.7.0.105" ])
                        ("AWSSDK.Braket", [ "3.7.2.2" ])
                        ("AWSSDK.CloudWatchRUM", [ "3.7.0.2" ])
                        ("AWSSDK.CodeArtifact", [ "3.7.0.105" ])
                        ("AWSSDK.Core", [ "3.7.5.4" ])
                        ("AWSSDK.Imagebuilder", [ "3.7.4.2" ])
                        ("AWSSDK.Drs", [ "3.7.0.5" ])
                        ("AWSSDK.ElasticLoadBalancing", [ "3.7.0.105" ])
                        ("AWSSDK.ElasticLoadBalancingV2", [ "3.7.3.24" ])
                        ("AWSSDK.Extensions.CrtIntegration", [ "3.7.0.2" ])
                        ("AWSSDK.Extensions.NETCore.Setup", [ "3.7.1" ])
                        ("AWSSDK.FinSpaceData", [ "3.7.1.3" ])
                        ("AWSSDK.Finspace", [ "3.7.1.14" ])
                        ("AWSSDK.FMS", [ "3.7.3.50" ])
                        ("AWSSDK.Inspector2", [ "3.7.0.2" ])
                        ("AWSSDK.Kafka", [ "3.7.5.1" ])
                        ("AWSSDK.KafkaConnect", [ "3.7.0.38" ])
                        ("AWSSDK.MigrationHubStrategyRecommendations", [ "3.7.0.7" ])
                        ("AWSSDK.RedshiftDataAPIService", [ "3.7.5.1" ])
                        ("AWSSDK.Route53RecoveryCluster", [ "3.7.0.60" ])
                        ("AWSSDK.Schemas", [ "3.7.2.27" ])
                        ("AWSSDK.ServiceQuotas", [ "3.7.0.105" ])
                        ("AWSSDK.Synthetics", [ "3.7.2.30" ])
                        ("AWSSDK.DAX.Client", [ "2.1.0" ]) ] })
          ("https://raw.githubusercontent.com/xunit/xunit/master/license.txt",
           { Expression = "Apache-2.0 AND MIT"
             Packages =
                 Map.ofList
                     [
                       ("xunit", [ "2.4.1 " ])
                       ("xunit.abstractions", [ "2.0.3" ])
                       ("xunit.assert", [ "2.4.1 " ])
                       ("xunit.core", [ "2.4.1 " ])
                       ("xunit.extensibility.core", [ "2.4.1 " ])
                       ("xunit.extensibility.execution", [ "2.4.1 " ]) ] }) ]

open FSharp.Data

type LicenseResponse = JsonProvider<""" { "license": { "key": "", "name": "", "spdx_id": "" } } """>

let mutable private dynamicLicenseCache = Map.empty<string, LicenseCache>
let mutable private githubNoAssertion = List.empty<string>

let (|CompiledMatch|_|) pattern input =
    if isNull input then
        None
    else
        let m = Regex.Match(input, pattern, RegexOptions.Compiled)
        if m.Success then
            Some [ for x in m.Groups -> x ]
        else
            None

let checkLicenseViaGitHub token licenseUrl =
    match dynamicLicenseCache.TryFind licenseUrl with
    | Some cache -> Some cache
    | None ->
        if githubNoAssertion |> List.contains licenseUrl then
            None
        else
            match licenseUrl with
            | CompiledMatch ".*(github|githubusercontent)\.com\/([\w\-]*)\/([\w\-_\.]*)\/.*"
              [ _; _; orgGroup; repoGroup ] ->
                let org = orgGroup.Value
                let repo = repoGroup.Value

                let headers =
                    [ "User-Agent", "dotnet-delice" ]
                    |> List.append
                        (match token with
                         | null
                         | "" -> []
                         | _ -> [ "Authorization", sprintf "token %s" token ])

                let res =
                    try
                        Http.RequestString(sprintf "https://api.github.com/repos/%s/%s/license" org repo, headers = headers)
                        |> LicenseResponse.Parse
                        |> Some
                    with _ ->
                        None

                match res with
                | Some res when res.License.SpdxId.JsonValue.AsString() = "NOASSERTION" ->
                    // this is what the GitHub API returns if it can't determine the license of a repository
                    githubNoAssertion <- licenseUrl :: githubNoAssertion
                    None
                | Some res ->
                    let lc =
                        { Expression = res.License.SpdxId.JsonValue.AsString()
                          Packages = Map.ofList [ (repo, []) ] }
                    dynamicLicenseCache <- dynamicLicenseCache.Add(licenseUrl, lc)
                    Some lc
                | None ->
                    None
            | _ -> None

let checkProjectAndLicenseViaGitHub token licenseUrl projectUrl =
    match checkLicenseViaGitHub token licenseUrl with
    | Some result -> Some result
    | None ->
        match checkLicenseViaGitHub token projectUrl with
        | Some projectLicenseResult -> Some projectLicenseResult
        | None ->
            None

open CommonLicenseDescriptions

// adapted from https://lucidjargon.wordpress.com/2010/11/29/dices-coefficient-in-f/
let bigrams (s: string) =
    s.ToLower().ToCharArray().[0..s.Length - 2]
    |> Array.fold (fun (set: Set<string>, i) c -> set.Add(c.ToString() + s.[i + 1].ToString()), i + 1) (Set.empty, 0)
    |> fst

let diceCoefficient (a: string) (b: string) =
    let f str = str |> Array.fold (fun fset arr -> Set.union fset (bigrams arr)) Set.empty
    let s1 = a.Split(' ') |> f
    let s2 = b.Split(' ') |> f
    2. * float ((Set.intersect s1 s2).Count) / float (s1.Count + s2.Count)

let mutable private failedUrls = List.empty<string>

let private convertIfGitHub (licenseUrl: string) =
    if Regex.IsMatch(licenseUrl, "^https?:\/\/github\.com") then
        Regex.Replace(licenseUrl, "^https?:\/\/github\.com", "https://raw.githubusercontent.com").Replace("/blob/", "/")
    else licenseUrl

let findMatchingLicense similarity licenseContents =
    descriptions |> Map.tryFindKey (fun _ licenseTemplate -> diceCoefficient licenseTemplate licenseContents > similarity)

let checkLicenseContents similarity packageName licenseUrl =
    match dynamicLicenseCache.TryFind licenseUrl with
    | Some lc -> Some lc
    | None ->
        if failedUrls |> List.contains licenseUrl then
            None
        else
            try
                let licenseContents = convertIfGitHub licenseUrl |> Http.RequestString
                match findMatchingLicense similarity licenseContents with
                | Some key ->
                    let lc =
                        { Expression = key
                          Packages = Map.ofList [ (packageName, []) ] }
                    dynamicLicenseCache <- dynamicLicenseCache.Add(licenseUrl, lc)
                    Some lc
                | None ->
                    failedUrls <- licenseUrl :: failedUrls
                    None
            with _ ->
                failedUrls <- licenseUrl :: failedUrls
                None
