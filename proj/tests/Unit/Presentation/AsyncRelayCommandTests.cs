using MapEditor.Presentation.Commands;

namespace MapEditor.Tests.Presentation
{
    public class AsyncRelayCommandTests
    {
        [Fact]
        public void Execute_WithValidExecute_CallsExecuteFunction()
        {
            // Arrange
            var executeCalled = false;
            var command = new AsyncRelayCommand(async _ =>
            {
                await Task.Delay(10);
                executeCalled = true;
            });

            // Act
            command.Execute(null);

            // Assert - give it time to execute
            System.Threading.Thread.Sleep(100);
            Assert.True(executeCalled);
        }

        [Fact]
        public void CanExecute_InitiallyTrue_WhenCanExecutePredicateIsNull()
        {
            // Arrange
            var command = new AsyncRelayCommand(async _ => await Task.CompletedTask);

            // Act
            var result = command.CanExecute(null);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanExecute_ReturnsFalse_WhenCanExecutePredicateReturnsFalse()
        {
            // Arrange
            var command = new AsyncRelayCommand(
                async _ => await Task.CompletedTask,
                _ => false);

            // Act
            var result = command.CanExecute(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanExecute_PreventsConcurrentExecution()
        {
            // Arrange
            var executionCount = 0;
            var command = new AsyncRelayCommand(async _ =>
            {
                executionCount++;
                await Task.Delay(50);
                executionCount--;
            });

            // Act
            command.Execute(null);
            var canExecuteWhileRunning = command.CanExecute(null);

            // Assert
            Assert.False(canExecuteWhileRunning);
        }

        [Fact]
        public void Execute_WithValidCanExecutePredicate_ExecutesWhenPredicateReturnsTrue()
        {
            // Arrange
            var executeCalled = false;
            var command = new AsyncRelayCommand(
                async _ =>
                {
                    await Task.Delay(10);
                    executeCalled = true;
                },
                _ => true);

            // Act
            command.Execute(null);

            // Assert
            System.Threading.Thread.Sleep(100);
            Assert.True(executeCalled);
        }

        [Fact]
        public void Execute_WithInvalidCanExecutePredicate_DoesNotExecuteWhenPredicateReturnsFalse()
        {
            // Arrange
            var executeCalled = false;
            var command = new AsyncRelayCommand(
                async _ =>
                {
                    await Task.Delay(10);
                    executeCalled = true;
                },
                _ => false);

            // Act
            command.Execute(null);

            // Assert
            System.Threading.Thread.Sleep(100);
            Assert.False(executeCalled);
        }

        [Fact]
        public async Task Execute_WithException_AllowsSubsequentExecution()
        {
            // Arrange
            var executionCount = 0;
            var command = new AsyncRelayCommand(async _ =>
            {
                executionCount++;
                await Task.CompletedTask;
                if (executionCount == 1)
                    throw new InvalidOperationException("Test exception");
            });

            // Act
            command.Execute(null);
            await Task.Delay(50);
            var canExecuteAfterException = command.CanExecute(null);
            command.Execute(null);
            await Task.Delay(50);

            // Assert
            Assert.True(canExecuteAfterException);
            Assert.Equal(2, executionCount);
        }
    }
}
