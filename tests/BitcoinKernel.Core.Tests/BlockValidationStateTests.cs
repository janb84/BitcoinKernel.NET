using BitcoinKernel.Core.Abstractions;
using BitcoinKernel.Interop.Enums;
using Xunit;

namespace BitcoinKernel.Core.Tests;

public class BlockValidationStateTests
{
    [Fact]
    public void Create_ShouldCreateValidState()
    {
        using var state = new BlockValidationState();
        
        Assert.NotNull(state);
    }

    [Fact]
    public void ValidationMode_NewState_ShouldReturnValid()
    {
        using var state = new BlockValidationState();
        
        var mode = state.ValidationMode;
        
        Assert.Equal(ValidationMode.VALID, mode);
    }

    [Fact]
    public void ValidationResult_NewState_ShouldReturnUnset()
    {
        using var state = new BlockValidationState();
        
        var result = state.ValidationResult;
        
        Assert.Equal(Interop.Enums.BlockValidationResult.UNSET, result);
    }

    [Fact]
    public void Copy_ShouldCreateIndependentCopy()
    {
        using var original = new BlockValidationState();
        using var copy = original.Copy();
        
        Assert.NotNull(copy);
        Assert.Equal(original.ValidationMode, copy.ValidationMode);
        Assert.Equal(original.ValidationResult, copy.ValidationResult);
    }

    [Fact]
    public void Dispose_ShouldAllowMultipleCalls()
    {
        var state = new BlockValidationState();
        
        state.Dispose();
        state.Dispose(); // Should not throw
    }

    [Fact]
    public void AccessAfterDispose_ShouldThrowObjectDisposedException()
    {
        var state = new BlockValidationState();
        state.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => state.ValidationMode);
    }

    [Fact]
    public void Copy_AfterDispose_ShouldThrowObjectDisposedException()
    {
        var state = new BlockValidationState();
        state.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => state.Copy());
    }

    [Fact]
    public void ValidationResult_AfterDispose_ShouldThrowObjectDisposedException()
    {
        var state = new BlockValidationState();
        state.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => state.ValidationResult);
    }
}
