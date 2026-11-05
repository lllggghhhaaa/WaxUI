namespace WaxUI.Utils;

public static class StringExtensionMethods
{
    public static string MergeClass(this string className, params string[] classes)
        => Khamsin.KhamsinMerger.Merge(
            className,
            string.Join(" ", classes.Where(c => !string.IsNullOrWhiteSpace(c))));
}