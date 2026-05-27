namespace ProcesadorCorreosPYS.Application.Configuration;

public sealed class ProcessingOptions
{
    public string BaseFolder { get; init; } = "C:/Emails";
    public string BcsFolder { get; init; } = "C:/BCS_IMG";
    public string WindowStart { get; init; } = "04:00";
    public string WindowEnd { get; init; } = "23:59";
    public int PollIntervalMinutes { get; init; } = 15;
    public int MaxEmailsPerRun { get; init; } = 300;
    public string WkHtmlToPdfPath { get; init; } = string.Empty;
}
