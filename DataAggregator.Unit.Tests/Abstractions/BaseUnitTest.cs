using FluentAssertions;

namespace DataAggregator.Unit.Tests.Abstractions
{
    public abstract class BaseUnitTest<TTestClass, TBaseClass>
        where TTestClass : BaseUnitTest<TTestClass, TBaseClass>
    {
        public TBaseClass _base = default!;
        public Exception _exception = default!;

        protected BaseUnitTest()
        {
            SetupClassReference()
                .GetAwaiter()
                .GetResult();
        }

        protected abstract Task SetupClassReference();

        public TTestClass Arrange()
        {
            return (TTestClass)this;
        }

        public ArrangeContext<TTestClass, TBaseClass, TRequest, TResult> Arrange<TRequest, TResult>(
            Action<TRequest> arrangeRequest,
            Action<TResult>? arrangeExpected = null)
            where TRequest : new()
            where TResult : new()
        {
            var request = new TRequest();
            arrangeRequest(request);

            var expected = default(TResult?);
            if (arrangeExpected is not null)
            {
                expected = new TResult();
                arrangeExpected(expected);
            }

            return new ArrangeContext<TTestClass, TBaseClass, TRequest, TResult>(
                (TTestClass)this, request, expected);
        }

        public TTestClass Act(Action<TBaseClass> arrangeAct)
        {
            try
            {
                arrangeAct(_base);
            }
            catch (Exception ex)
            {
                _exception = ex;
            }

            return (TTestClass)this;
        }

        public void Assert(Action? assertion = null)
        {
            if (_exception is not null)
            {
                throw new InvalidOperationException($"Test method threw {_exception?.GetType().Name}. " +
                    $"Did you mean to call {nameof(AssertThrows)} instead of {nameof(Assert)}?");
            }

            if (assertion is not null)
            {
                assertion();
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
