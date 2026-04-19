using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace Foglaloplusz.Tests;

internal static class TestOutputAssert
{
    public static void Equal<T>(ITestOutputHelper output, T expected, T actual, string? label = null)
    {
        Assert.Equal(expected, actual);
        output.WriteLine(FormatPair(label ?? nameof(Assert.Equal), expected, actual));
    }

    public static void True(ITestOutputHelper output, bool condition, string? label = null, string? actualDetail = null)
    {
        Assert.True(condition);
        var extra = actualDetail != null ? $", detail: {actualDetail}" : "";
        output.WriteLine($"{label ?? nameof(Assert.True)}: expected = true, actual = true{extra}");
    }

    public static void False(ITestOutputHelper output, bool condition, string? label = null)
    {
        Assert.False(condition);
        output.WriteLine($"{label ?? nameof(Assert.False)}: expected = false, actual = false");
    }

    public static void NotEmpty(ITestOutputHelper output, string? value, string? label = null)
    {
        Assert.False(string.IsNullOrEmpty(value));
        output.WriteLine($"{label ?? "non-empty string"}: expected = non-empty, actual = length {value!.Length}");
    }

    public static void EnsureSuccessLog(this HttpResponseMessage response, ITestOutputHelper output, string? label = null)
    {
        response.EnsureSuccessStatusCode();
        output.WriteLine(
            $"{label ?? "HTTP"}: expected = 2xx success, actual = {(int)response.StatusCode} {response.StatusCode}");
    }

    private static string FormatPair<T>(string label, T expected, T actual) =>
        $"{label}: expected = {Format(expected)}, actual = {Format(actual)}";

    private static string Format<T>(T value) => value?.ToString() ?? "<null>";
}
