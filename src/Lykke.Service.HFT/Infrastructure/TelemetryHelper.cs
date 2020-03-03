using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lykke.Service.HFT.Infrastructure
{
    internal static class TelemetryHelper
    {
        private static readonly TelemetryClient Telemetry = new TelemetryClient();

        internal static IOperationHolder<DependencyTelemetry> InitTelemetryOperation(
            string name,
            string id,
            string parentId = null)
        {

            var dependencyTelemetry = new DependencyTelemetry
            {
                Id = id,
                Name = name
            };

            if (!string.IsNullOrEmpty(parentId))
            {
                dependencyTelemetry.Context.Operation.ParentId = parentId;
            }

            return Telemetry.StartOperation(dependencyTelemetry);
        }

        internal static void SubmitException(IOperationHolder<DependencyTelemetry> telemtryOperation, Exception e)
        {
            telemtryOperation.Telemetry.Success = false;
            Telemetry.TrackException(e);
        }

        internal static void SubmitOperationResult(IOperationHolder<DependencyTelemetry> telemtryOperation)
        {
            Telemetry.StopOperation(telemtryOperation);
        }
    }
}
