using Domain.Products;
using FluentAssertions;

namespace Unit.Product;

public class ProductTagsTests
{
    [Fact]
    public void Create_ValidProductTags_ReturnsSuccess()
    {
        var tags = new List<string> { "nike", "sneakers", "running" };

        var result = ProductTags.Create(tags);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Count.Should().Be(3);
    }

    [Fact]
    public void Create_EmptyTags_ReturnsSuccess()
    {
        var result = ProductTags.Create([]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().BeEmpty();
    }

    [Fact]
    public void Create_ProductTagsExceedingMaxTags_ReturnsFailure()
    {
        var tags = Enumerable.Range(1, ProductTags.MaxTags + 1).Select(x => $"tag-{x}").ToList();

        var result = ProductTags.Create(tags);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_DuplicateTags_RemovesDuplicates()
    {
        var tags = new List<string> { "nike", "nike", "running", "running" };

        var result = ProductTags.Create(tags);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Count.Should().Be(2);
        result.Value.Value.Should().Contain(["nike", "running"]);
    }

    [Fact]
    public void Create_WhitespaceTags_RemovesInvalidTags()
    {
        var tags = new List<string> { "nike", "", "   ", "running" };

        var result = ProductTags.Create(tags);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Count.Should().Be(2);
        result.Value.Value.Should().Contain(["nike", "running"]);
    }

    [Fact]
    public void Empty_ReturnsEmptyProductTags()
    {
        var result = ProductTags.Empty();

        result.Value.Should().BeEmpty();
    }
}
