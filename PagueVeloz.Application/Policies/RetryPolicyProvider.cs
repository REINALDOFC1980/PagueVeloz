using Polly;
using Polly.Retry;
using Serilog;


public class RetryPolicyProvider
{
    public static AsyncRetryPolicy GetRetryPolicy()
    {
       
        return Policy
            .Handle<Exception>() // ou use SqlException se quiser limitar ao SQL
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Log.Warning("Tentativa {RetryCount} falhou. Esperando {TimeSpan} antes de tentar novamente. Erro: {ExceptionMessage}",
                        retryCount, timeSpan, exception.Message);
                });
    }
}
