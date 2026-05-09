using Domain.Product;

namespace Unit.Product;

public class ProductTagsTests
{
    [Fact]
    public void Create_ValidProductTags_ReturnsSuccess()
    {
        var tags = new List<string> { "nike", "sneakers", "running" };

        var result = ProductTags.Create(tags);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Value.Count);
    }

    [Fact]
    public void Create_EmptyTags_ReturnsSuccess()
    {
        var result = ProductTags.Create([]);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Value);
    }

    [Fact]
    public void Create_ProductTagsExceedingMaxTags_ReturnsFailure()
    {
        var tags = Enumerable.Range(1, ProductTags.MaxTags + 1).Select(x => $"tag-{x}").ToList();

        var result = ProductTags.Create(tags);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_DuplicateTags_RemovesDuplicates()
    {
        var tags = new List<string> { "nike", "nike", "running", "running" };

        var result = ProductTags.Create(tags);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Value.Count);
    }

    [Fact]
    public void Create_WhitespaceTags_RemovesInvalidTags()
    {
        var tags = new List<string> { "nike", "", "   ", "running" };

        var result = ProductTags.Create(tags);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Value.Count);
        Assert.Contains("nike", result.Value.Value);
        Assert.Contains("running", result.Value.Value);
    }

    [Fact]
    public void Empty_ReturnsEmptyProductTags()
    {
        var result = ProductTags.Empty();

        Assert.Empty(result.Value);
    }
}
