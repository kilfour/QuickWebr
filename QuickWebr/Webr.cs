using QuickWebr.Bolts.WebrBuilders;

namespace QuickWebr;



public static class Webr
{
    public static WebrNamed Named(string name) => new(name);
}
