using System.Diagnostics;
using System.Runtime.CompilerServices;
using QuickCheckr;
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickWebr.Bolts.WebrBuilders;

namespace QuickWebr.Tests.Tools;

public abstract class WebrRunTest<T> : QuickCheckrTest<T>
{
    protected class DocWebrHeaderAttribute() :
        DocBoldHeaderAttribute("The Webr");

    protected class DocWebrAttribute() :
        DocExampleAttribute(typeof(T), nameof(GetWebr));

    protected class DocMethodsHeaderAttribute() :
        DocBoldHeaderAttribute("The Methods");

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
