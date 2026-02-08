using System.Diagnostics;

namespace AgroSolutions.SharedKernel.Observability;

/// <summary>
/// Helper para criar spans customizados de tracing
/// </summary>
public class TracingHelper
{
    private readonly ActivitySource _activitySource;

    public TracingHelper()
    {
        _activitySource = OpenTelemetryConfiguration.CreateActivitySource();
    }

    /// <summary>
    /// Executa uma operação com tracing automático
    /// </summary>
    public async Task<T> TraceAsync<T>(
        string operationName,
        Func<Task<T>> operation,
        Dictionary<string, object>? tags = null)
    {
        using var activity = _activitySource.StartActivity(operationName);

        try
        {
            // Adicionar tags personalizadas
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }

            var result = await operation();

            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddTag("exception.message", ex.Message);
            activity?.AddTag("exception.type", ex.GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Executa uma operação void com tracing automático
    /// </summary>
    public async Task TraceAsync(
        string operationName,
        Func<Task> operation,
        Dictionary<string, object>? tags = null)
    {
        using var activity = _activitySource.StartActivity(operationName);

        try
        {
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity?.SetTag(tag.Key, tag.Value);
                }
            }

            await operation();
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddTag("exception.message", ex.Message);
            activity?.AddTag("exception.type", ex.GetType().FullName);
            throw;
        }
    }

    /// <summary>
    /// Adiciona evento ao span atual
    /// </summary>
    public void AddEvent(string eventName, Dictionary<string, object>? attributes = null)
    {
        var activity = Activity.Current;
        if (activity != null)
        {
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    activity.AddTag($"event.{eventName}.{attr.Key}", attr.Value);
                }
            }
            else
            {
                activity.AddTag($"event.{eventName}", true);
            }
        }
    }
}
