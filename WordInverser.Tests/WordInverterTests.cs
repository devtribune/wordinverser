using Microsoft.VisualStudio.TestTools.UnitTesting;
using WordInverser.Business.Services;

namespace WordInverser.Tests.Business;

[TestClass]
public class WordInverterTests
{
    [TestMethod]
    public void InverseWord_SimpleWord_ReturnsReversed()
    {
        // Arrange
        var word = "hello";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("olleh", result);
    }

    [TestMethod]
    public void InverseWord_WordWithLeadingSpecialChar_PreservesSpecialChar()
    {
        // Arrange
        var word = "!hello";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("!olleh", result);
    }

    [TestMethod]
    public void InverseWord_WordWithTrailingSpecialChar_PreservesSpecialChar()
    {
        // Arrange
        var word = "hello!";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("olleh!", result);
    }

    [TestMethod]
    public void InverseWord_WordWithBothSpecialChars_PreservesBoth()
    {
        // Arrange
        var word = "!hello?";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("!olleh?", result);
    }

    [TestMethod]
    public void InverseWord_WordWithEmbeddedSpecialChars_ReversesAll()
    {
        // Arrange
        var word = "hel-lo";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("ol-leh", result);
    }

    [TestMethod]
    public void InverseWord_EmptyString_ReturnsEmpty()
    {
        // Arrange
        var word = "";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void InverseWord_OnlySpecialChars_ReturnsUnchanged()
    {
        // Arrange
        var word = "!!!";

        // Act
        var result = WordInverter.InverseWord(word);

        // Assert
        Assert.AreEqual("!!!", result);
    }

    [TestMethod]
    public void GetNormalizedCoreWord_SimpleWord_ReturnsLowercase()
    {
        // Arrange
        var word = "Hello";

        // Act
        var result = WordInverter.GetNormalizedCoreWord(word);

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void GetNormalizedCoreWord_WithSpecialChars_RemovesBoundaryChars()
    {
        // Arrange
        var word = "!Hello?";

        // Act
        var result = WordInverter.GetNormalizedCoreWord(word);

        // Assert
        Assert.AreEqual("hello", result);
    }

    [TestMethod]
    public void GetNormalizedCoreWord_WithEmbeddedSpecialChars_PreservesEmbedded()
    {
        // Arrange
        var word = "hel-lo";

        // Act
        var result = WordInverter.GetNormalizedCoreWord(word);

        // Assert
        Assert.AreEqual("hel-lo", result);
    }

    [TestMethod]
    public void InverseSentence_MultiplWords_InversesEach()
    {
        // Arrange
        var sentence = "hello world";

        // Act
        var result = WordInverter.InverseSentence(sentence);

        // Assert
        Assert.AreEqual("olleh dlrow", result);
    }

    [TestMethod]
    public void InverseSentence_WithSpecialChars_PreservesPosition()
    {
        // Arrange
        var sentence = "!hello world!";

        // Act
        var result = WordInverter.InverseSentence(sentence);

        // Assert
        Assert.AreEqual("!olleh dlrow!", result);
    }
}
