namespace OAuthClient
{
    using System.IO;

    public class TracePageAppender : log4net.Appender.AppenderSkeleton
    {
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            var stringWriter = new StringWriter(Logging.LogMessages);
            Layout.Format(stringWriter, loggingEvent);
        }
    }
}