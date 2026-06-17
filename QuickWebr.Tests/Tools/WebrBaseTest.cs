using QuickCheckr.Authoring;

namespace QuickWebr.Tests.Tools;

public abstract class WebrBaseTest<T> : QuickCheckrTest<T>
{
    protected override bool WriteAllReportsToDisk => false;

    protected class DocWebrHeaderAttribute() :
        DocBoldHeaderAttribute("The Webr");

    protected class DocMethodsHeaderAttribute() :
        DocBoldHeaderAttribute("The Methods");
}