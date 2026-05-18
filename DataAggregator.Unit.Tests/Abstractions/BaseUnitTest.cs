using FluentAssertions;

namespace DataAggregator.Unit.Tests.Abstractions
{
    public abstract class BaseUnitTest<TTestClass, TBaseClass>
        where TTestClass : BaseUnitTest<TTestClass, TBaseClass>
    {
        protected TBaseClass _base = default!;
        protected Exception _exception = default!;

        protected BaseUnitTest()
        {
            SetupClassReference();
        }

        protected abstract void SetupClassReference();
        protected abstract void ActProcessor();

        public TTestClass Arrange<TRequest, TResult>(
            Action<TRequest>? arrangeRequest = null,
            Action<TResult>? arrangeResult = null)
        {
            return (TTestClass)this;
        }

        public TTestClass Act()
        {
            try
            {
                ActProcessor();
            }
            catch (Exception ex)
            {
                _exception = ex;
            }

            return (TTestClass)this;
        }

        public void Assert()
        {
            if (_exception is not null)
            {
                throw new InvalidOperationException($"Test method threw {_exception?.GetType().Name}. " +
                    $"Did you mean to call {nameof(AssertThrows)} instead of {nameof(Assert)}?");
            }
        }

        public void AssertThrows<TException>(Action<TException> assertion)
            where TException : Exception
        {
            if (_exception is not null)
            {
                throw new InvalidOperationException($"Expected output is defined in {nameof(Arrange)}. " +
                    $"Did you mean to call {nameof(Assert)} instead of {nameof(AssertThrows)}?");
            }

            _exception
                .Should()
                .NotBeNull();

            _exception
                .Should()
                .BeOfType<TException>();

            assertion((TException)_exception);
        }
    }
}
