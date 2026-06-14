using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;

namespace QuickWebr.Tests.Tools;

public abstract class WebrTest<T> : WebrBaseTest<T>
{
    protected class DocWebrAttribute() :
        DocExampleAttribute(typeof(T), nameof(GetWebr));

    [StackTraceHidden]
    protected void Document(
        Action<ConfiguredCheckr> runCheckr,
        Action<Article> verifier,
        [CallerFilePath] string callerPath = "")
    {
        var article = Journalist.Publish(GetWebr(), runCheckr);
        ProcessArticle(article, callerPath);
        verifier(article);
    }

    protected abstract ConfiguredCheckr GetWebr();
    //protected abstract void Verify(Article article);
}
