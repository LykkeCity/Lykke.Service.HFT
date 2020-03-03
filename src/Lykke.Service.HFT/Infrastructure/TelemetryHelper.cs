using System;
using System.Diagnostics;
using Common;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lykke.Service.HFT.Infrastructure
{
    internal static class TelemetryHelper
    {
        private static readonly TelemetryClient Telemetry = new TelemetryClient();

        internal static IOperationHolder<RequestTelemetry> InitTelemetryOperation(
            string name,
            string id,
            string parentId = null)
        {
            Console.WriteLine($"Info: {new { Activity.Current.RootId, Activity.Current.Id, Activity.Current}}");
            var requestTelemetry = new RequestTelemetry { Id = id, Name = name };

            requestTelemetry.Context.Operation.Id = id;

            requestTelemetry.Context.Operation.ParentId = parentId ?? Activity.Current?.RootId;

            var operation = Telemetry.StartOperation(requestTelemetry);

            var json = new
            {
                operationId = operation.Telemetry.Context.Operation.Id,
                parentId = operation.Telemetry.Context.Operation.ParentId,
                name = operation.Telemetry.Name,
            }.ToJson();

            Console.WriteLine($"sending request telemetry: {json}");

            return operation;
        }

        internal static void SubmitException(IOperationHolder<RequestTelemetry> telemtryOperation, Exception e)
        {
            telemtryOperation.Telemetry.Success = false;
            Telemetry.TrackException(e);
        }

        internal static void SubmitOperationResult(IOperationHolder<RequestTelemetry> telemtryOperation)
        {
            Telemetry.StopOperation(telemtryOperation);
        }
    }
}
