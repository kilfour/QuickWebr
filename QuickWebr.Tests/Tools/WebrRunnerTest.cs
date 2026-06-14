using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood;

namespace QuickWebr.Tests.Tools;

public abstract class WebrRunnerTest<T> : WebrBaseTest<T>
{
    protected Journalist TheJournalist = new();

    [StackTraceHidden]
    protected void Document(
        Action run,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        try { run(); } catch (FalsifiableException) { }
        var article = TheJournalist.GetArticle();
        ProcessArticle(article, callerPath);
        verifier(article);
    }

    [StackTraceHidden]
    protected void Document(
        ConfiguredCheckr checkr,
        Action<ConfiguredCheckr> runCheckr,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        var article = Journalist.Publish(checkr, runCheckr);
        ProcessArticle(article, callerPath);
        verifier(article);
    }
}
